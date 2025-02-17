﻿using System;
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
        private readonly int _generatorid;
        
        private long _lastTimeslot = -1;
        
        private IdGenerator(int generatorId, IdGeneratorOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            if (generatorId >= Options.IdStructure.MaxGenerators || generatorId < 0)
                throw new ArgumentOutOfRangeException(nameof(generatorId), $"GeneratorId must be between 0 and {Options.IdStructure.MaxGenerators - 1}.");
            
            _generatorid = generatorId;
        }
        
        /// <summary>
        /// Creates a new <see cref="IIdGenerator"/> with the default <see cref="IdGeneratorOptions"/>.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        public static IIdGenerator CreateInstanceWithDefaultOptions(int generatorId)
        {
            return CreateInstance(generatorId, IdGeneratorOptions.Default);
        }

        /// <summary>
        /// Creates a new <see cref="IIdGenerator"/> class with the specified <see cref="IdGeneratorOptions"/>.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="options">The <see cref="IdGeneratorOptions"/> for the <see cref="IdGenerator"/></param>.
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static IIdGenerator CreateInstance(int generatorId, IdGeneratorOptions options)
        {
            return new IdGenerator(generatorId, options);
        }

        public int Id => _generatorid;

        public IdGeneratorOptions Options { get; }
        
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

                return Options.IdStructure.Encode(timeslot, _generatorid, Options.SequenceGenerator.GetNextValue());
            }
        }
        
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