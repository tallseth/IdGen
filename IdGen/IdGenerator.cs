using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            var id = CreateIdImpl(out var ex);
            if (ex != null)
                throw ex;
            return id;
        }

        public IEnumerable<long> CreateManyIds(int number)
        {
            return Enumerable.Range(0, number).Select(_ => CreateId());
        }

        /// <summary>
        /// Creates a new Id.
        /// </summary>
        /// <param name="exception">If any exceptions occur they will be returned in this argument.</param>
        /// <returns>
        /// Returns an Id based on the <see cref="IdGenerator"/>'s epoch, generatorid and sequence or
        /// a negative value when an exception occurred.
        /// </returns>
        /// <exception cref="InvalidSystemClockException">Thrown when clock going backwards is detected.</exception>
        /// <exception cref="SequenceOverflowException">Thrown when sequence overflows.</exception>
        private long CreateIdImpl(out Exception? exception)
        {
            lock (_genlock)
            {
                // Determine "timeslot" and make sure it's >= last timeslot (if any)
                var ticks = GetTicks();
                if (ticks >= Options.IdStructure.MaxIntervals)
                {
                    exception = new TimestampOverflowException();
                    return -1;
                }

                if (ticks < _lastgen || ticks < 0)
                {
                    exception = new InvalidSystemClockException($"Clock moved backwards or wrapped around. Refusing to generate id for {_lastgen - ticks} ticks");
                    return -1;
                }

                // If we're in the same "timeslot" as previous time we generated an Id, up the sequence number
                if (ticks == _lastgen)
                {
                    if (Options.SequenceGenerator.IsExhausted())
                    {
                        switch (Options.SequenceOverflowStrategy)
                        {
                            case SequenceOverflowStrategy.SpinWait:
                                SpinWait.SpinUntil(() => _lastgen != GetTicks());
                                return CreateIdImpl(out exception); // Try again
                            case SequenceOverflowStrategy.Throw:
                            default:
                                exception = new SequenceOverflowException("Sequence overflow. Refusing to generate id for rest of tick");
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
                    // If we made it here then no exceptions occurred; make sure we communicate that to the caller by setting `exception` to null
                    exception = null;
                    // Build id by shifting all bits into their place
                    return (ticks << SHIFT_TIME)
                           + (_generatorid << SHIFT_GENERATOR)
                           + Options.SequenceGenerator.GetNextValue();
                }
            }
        }

        /// <summary>
        /// Gets the number of ticks since the <see cref="ITimeSource"/>'s epoch.
        /// </summary>
        /// <returns>Returns the number of ticks since the <see cref="ITimeSource"/>'s epoch.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long GetTicks() => Options.TimeSource.GetTicks();
    }
}