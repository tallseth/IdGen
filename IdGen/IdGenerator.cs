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
        private readonly object _genlock = new object();
        private readonly long _generatorid;
        private readonly int SHIFT_TIME;
        private readonly int SHIFT_GENERATOR;
        
        private long _lastTimeslot = -1;
        
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
            Options = options ?? throw new ArgumentNullException(nameof(options));

            if (generatorId >= Options.IdStructure.MaxGenerators || generatorId < 0)
                throw new ArgumentOutOfRangeException(nameof(generatorId), $"GeneratorId must be between 0 and {Options.IdStructure.MaxGenerators - 1}.");
            
            _generatorid = generatorId;

            // Precalculate some values
            SHIFT_TIME = options.IdStructure.GeneratorIdBits + options.IdStructure.SequenceBits;
            SHIFT_GENERATOR = options.IdStructure.SequenceBits;
        }
        
        /// <summary>
        /// Gets the Id of the generator.
        /// </summary>
        public int Id => (int)_generatorid;

        /// <summary>
        /// Gets the <see cref="IdGeneratorOptions"/>.
        /// </summary>
        public IdGeneratorOptions Options { get; }

        /// <summary>
        /// Creates a new Id.
        /// </summary>
        /// <returns>Returns an Id based on the <see cref="IdGenerator"/>'s epoch, generatorid and sequence.</returns>
        /// <exception cref="InvalidSystemClockException">Thrown when clock going backwards is detected.</exception>
        /// <exception cref="TimestampOverflowException">Thrown when timestamp overflows allotted bits.</exception>
        /// <exception cref="SequenceOverflowException">Thrown when sequence overflows.</exception>
        /// <remarks>Note that this method MAY throw an one of the documented exceptions.</remarks>
        public long CreateId()
        {
            lock (_genlock)
            {
                var timeslot = GetCurrentTimeslot();

                if (timeslot == _lastTimeslot && Options.SequenceGenerator.IsExhausted())
                {
                    if (Options.SequenceOverflowStrategy != SequenceOverflowStrategy.SpinWait)
                        throw new SequenceOverflowException();

                    SpinWait.SpinUntil(() => _lastTimeslot != Options.TimeSource.GetTicks());
                    timeslot = GetCurrentTimeslot();
                }
                
                if(timeslot != _lastTimeslot)
                {
                    Options.SequenceGenerator.Reset();
                    _lastTimeslot = timeslot;
                }

                unchecked
                {
                    // Build id by shifting all bits into their place
                    return (timeslot << SHIFT_TIME)
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
        
        private long GetCurrentTimeslot()
        {
            var timeslot = Options.TimeSource.GetTicks();
            if (timeslot >= Options.IdStructure.MaxIntervals)
                throw new TimestampOverflowException();

            if (timeslot < _lastTimeslot || timeslot < 0)
                throw new InvalidSystemClockException($"Clock moved backwards or wrapped around. Refusing to generate id for {_lastTimeslot - timeslot} ticks");
            
            return timeslot;
        }
    }
}