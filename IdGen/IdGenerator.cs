using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IdGen.Exceptions;

namespace IdGen
{
    /// <summary>
    /// Generates Id's inspired by Twitter's (late) Snowflake project.
    /// </summary>
    public class IdGenerator : IIdGenerator
    {
        private readonly long _generatorid;
        private long _lastgen = -1;
        
        private readonly int SHIFT_TIME;
        private readonly int SHIFT_GENERATOR;
        
        // Object to lock() on while generating Id's
        private readonly object _genlock = new object();

        /// <summary>
        /// Gets the <see cref="IdGeneratorOptions"/>.
        /// </summary>
        public IdGeneratorOptions Options { get; }
        
        /// <summary>
        /// Gets the Id of the generator.
        /// </summary>
        public int Id => (int)_generatorid;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        public IdGenerator(int generatorId)
            : this(generatorId, new IdGeneratorOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class with the specified <see cref="IdGeneratorOptions"/>.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="options">The <see cref="IdGeneratorOptions"/> for the <see cref="IdGenerator"/></param>.
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public IdGenerator(int generatorId, IdGeneratorOptions options)
        {
            if (generatorId < 0)
                throw new ArgumentOutOfRangeException(nameof(generatorId), "GeneratorId must be larger than or equal to 0");
            _generatorid = generatorId;

            Options = options ?? throw new ArgumentNullException(nameof(options));

            if (_generatorid >= Options.IdStructure.MaxGenerators)
                throw new ArgumentOutOfRangeException(nameof(generatorId), $"GeneratorId must be between 0 and {Options.IdStructure.MaxGenerators - 1}.");

            // Precalculate some values
            SHIFT_TIME = options.IdStructure.GeneratorIdBits + options.IdStructure.SequenceBits;
            SHIFT_GENERATOR = options.IdStructure.SequenceBits;
        }

        /// <summary>
        /// Creates a new Id.
        /// </summary>
        /// <returns>Returns an Id based on the <see cref="IdGenerator"/>'s epoch, generatorid and sequence.</returns>
        /// <exception cref="InvalidSystemClockException">Thrown when clock going backwards is detected.</exception>
        /// <exception cref="SequenceOverflowException">Thrown when sequence overflows.</exception>
        /// <remarks>Note that this method MAY throw an one of the documented exceptions.</remarks>
        public long CreateId()
        {
            lock (_genlock)
            {
                // Determine "timeslot" and make sure it's >= last timeslot (if any)
                var ticks = Options.TimeSource.GetTicks();
                if (ticks >= Options.IdStructure.MaxIntervals)
                {
                    throw new TimestampOverflowException();
                }

                if (ticks < _lastgen || ticks < 0)
                {
                    throw new InvalidSystemClockException($"Clock moved backwards or wrapped around. Refusing to generate id for {_lastgen - ticks} ticks");
                }

                // If we're in the same "timeslot" as previous time we generated an Id, up the sequence number
                if (ticks == _lastgen)
                {
                    if (Options.SequenceGenerator.IsExhausted())
                    {
                        switch (Options.SequenceOverflowStrategy)
                        {
                            case SequenceOverflowStrategy.SpinWait:
                                SpinWait.SpinUntil(() => _lastgen != Options.TimeSource.GetTicks());
                                return CreateId(); // Try again
                            case SequenceOverflowStrategy.Throw:
                            default:
                                throw new SequenceOverflowException("Sequence overflow. Refusing to generate id for rest of tick");
                                return -1;
                        }
                    }
                }
                else // We're in a new(er) "timeslot", so we can reset the sequence and store the new(er) "timeslot"
                {
                    Options.SequenceGenerator.Reset();
                    _lastgen = ticks;
                }

                unchecked
                {
                    // Build id by shifting all bits into their place
                    return (ticks << SHIFT_TIME)
                           + (_generatorid << SHIFT_GENERATOR)
                           + Options.SequenceGenerator.GetNextValue();
                }
            }
        }

        /// <summary>
        /// Creates a group of Ids, given a number of desired ID's
        /// </summary>
        /// <param name="number">The number of ID's desired</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of ID's of length <param name="number"/></returns>
        public IEnumerable<long> CreateManyIds(int number)
        {
            return Enumerable.Range(0, number).Select(_ => CreateId());
        }
    }
}