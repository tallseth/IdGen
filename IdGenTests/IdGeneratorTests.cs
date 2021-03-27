﻿using IdGen;
using IdGenTests.Mocks;
using System;
using System.Collections;
using System.Linq;
using IdGen.Exceptions;
using NUnit.Framework;

namespace IdGenTests
{
    [TestFixture]
    public class IdGeneratorTests
    {
        [Test]
        public void Sequence_ShouldIncrease_EveryInvocation()
        {
            // We setup our generator so that the time is 0, generator id 0 and we're only left with the sequence
            // increasing each invocation of CreateId();
            var ts = new MockTimeSource(0);
            var g = new IdGenerator(0, new IdGeneratorOptions(timeSource: ts));

            Assert.AreEqual(0, g.CreateId());
            Assert.AreEqual(1, g.CreateId());
            Assert.AreEqual(2, g.CreateId());
        }

        [Test]
        public void Sequence_ShouldReset_EveryNewTick()
        {
            // We setup our generator so that the time is 0, generator id 0 and we're only left with the sequence
            // increasing each invocation of CreateId();
            var ts = new MockTimeSource(0);
            var g = new IdGenerator(0, new IdGeneratorOptions(timeSource: ts));

            Assert.AreEqual(0, g.CreateId());
            Assert.AreEqual(1, g.CreateId());
            ts.NextTick();
            // Since the timestamp has increased, we should now have a much higher value (since the timestamp is
            // shifted left a number of bits (specifically GeneratorIdBits + SequenceBits)
            Assert.AreEqual((1 << (g.Options.IdStructure.GeneratorIdBits + g.Options.IdStructure.SequenceBits)) + 0, g.CreateId());
            Assert.AreEqual((1 << (g.Options.IdStructure.GeneratorIdBits + g.Options.IdStructure.SequenceBits)) + 1, g.CreateId());
        }

        [Test]
        public void GeneratorId_ShouldBePresent_InID1()
        {
            // We setup our generator so that the time is 0 and generator id equals 1023 so that all 10 bits are set
            // for the generator.
            var ts = new MockTimeSource(0);
            var g = new IdGenerator(1023, new IdGeneratorOptions(timeSource: ts));

            // Make sure all expected bits are set
            Assert.AreEqual((1 << g.Options.IdStructure.GeneratorIdBits) - 1 << g.Options.IdStructure.SequenceBits, g.CreateId());
        }

        [Test]
        public void GeneratorId_ShouldBePresent_InID2()
        {
            // We setup our generator so that the time is 0 and generator id equals 4095 so that all 12 bits are set
            // for the generator.
            var ts = new MockTimeSource();
            var s = new IdStructure(40, 12, 11); // We use a custom IdStructure with 12 bits for the generator this time
            var g = new IdGenerator(4095, new IdGeneratorOptions(s, ts));

            // Make sure all expected bits are set
            Assert.AreEqual(-1 & ((1 << 12) - 1), g.Id);
            Assert.AreEqual((1 << 12) - 1 << 11, g.CreateId());
        }

        [Test]
        public void GeneratorId_ShouldBeMasked_WhenReadFromProperty()
        {
            // We setup our generator so that the time is 0 and generator id equals 1023 so that all 10 bits are set
            // for the generator.
            var ts = new MockTimeSource();
            var g = new IdGenerator(1023, new IdGeneratorOptions(timeSource: ts));

            // Make sure all expected bits are set
            Assert.AreEqual((1 << g.Options.IdStructure.GeneratorIdBits) - 1, g.Id);
        }

        [Test]
        public void Constructor_Throws_OnNull_Options()
        {
            Assert.Throws<ArgumentNullException>(()=>new IdGenerator(1024, null));
        }


        [Test]
        public void Constructor_Throws_OnInvalidGeneratorId_Positive()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()=>new IdGenerator(1024, new IdGeneratorOptions(new IdStructure(41, 10, 12))));
        }

        [Test]
        public void Constructor_Throws_OnInvalidGeneratorId_Negative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()=>new IdGenerator(-1));
        }

        [Test]
        public void Constructor_UsesCorrectId()
        {
            Assert.AreEqual(42, new IdGenerator(42).Id);
        }

        [Test]
        public void CreateId_Throws_OnSequenceOverflow()
        {
            var ts = new MockTimeSource();
            var s = new IdStructure(41, 20, 2);
            var g = new IdGenerator(0, new IdGeneratorOptions(idStructure: s, timeSource: ts));

            // We have a 2-bit sequence; generating 4 id's shouldn't be a problem
            for (var i = 0; i < 4; i++)
                Assert.AreEqual(i, g.CreateId());

            // However, if we invoke once more we should get an SequenceOverflowException
            Assert.Throws<SequenceOverflowException>(()=>g.CreateId());
        }

        [Test]
        public void TryCreateId_Returns_False_OnSequenceOverflow()
        {
            var ts = new MockTimeSource();
            var s = new IdStructure(41, 20, 2);
            var g = new IdGenerator(0, new IdGeneratorOptions(idStructure: s, timeSource: ts));

            // We have a 2-bit sequence; generating 4 id's shouldn't be a problem
            for (var i = 0; i < 4; i++)
                Assert.IsTrue(g.TryCreateId(out var _));

            // However, if we invoke once more we should get an SequenceOverflowException
            // which should be indicated by the false return value
            Assert.IsFalse(g.TryCreateId(out var _));
        }

        [Test]
        public void Enumerable_ShoudReturn_Ids()
        {
            var g = new IdGenerator(0, IdGeneratorOptions.Default);
            var ids = g.Take(1000).ToArray();

            Assert.AreEqual(1000, ids.Distinct().Count());
        }

        [Test]
        public void Enumerable_ShoudReturn_Ids_InterfaceExplicit()
        {
            var g = (IEnumerable)new IdGenerator(0, IdGeneratorOptions.Default);
            var ids = g.OfType<long>().Take(1000).ToArray();
            Assert.AreEqual(1000, ids.Distinct().Count());
        }

        [Test]
        public void CreateId_Throws_OnClockBackwards()
        {
            var ts = new MockTimeSource(100);
            var g = new IdGenerator(0, new IdGeneratorOptions(timeSource: ts));

            g.CreateId();
            ts.PreviousTick(); // Set clock back 1 'tick', this results in the time going from "100" to "99"
            Assert.Throws<InvalidSystemClockException>(()=>g.CreateId());
        }
        
        [Test]
        public void CreateId_Throws_OnTimeOverflow()
        {
            var ts = new MockTimeSource(11); //start at a time that doesn't fit
            var g = new IdGenerator(0, new IdGeneratorOptions(timeSource: ts, idStructure: new IdStructure(3, 30, 30)));

            Assert.Throws<TimestampOverflowException>(()=>g.CreateId()); // blow up on the first id tried
        }

        [Test]
        public void TryCreateId_Returns_False_OnClockBackwards()
        {
            var ts = new MockTimeSource(100);
            var g = new IdGenerator(0, new IdGeneratorOptions(timeSource: ts));

            Assert.IsTrue(g.TryCreateId(out var _));
            ts.PreviousTick(); // Set clock back 1 'tick', this results in the time going from "100" to "99"
            Assert.IsFalse(g.TryCreateId(out var _));
        }

        [Test]
        public void CreateId_Throws_OnTimestampWraparound()
        {
            var ts = new MockTimeSource(IdStructure.Default.MaxIntervals - 1);  // Set clock to 1 'tick' before wraparound
            var g = new IdGenerator(0, new IdGeneratorOptions(timeSource: ts));

            Assert.IsTrue(g.CreateId() > 0);                                // Should succeed;
            ts.NextTick();
            Assert.Throws<TimestampOverflowException>(()=>g.CreateId());   // Should fail
        }

        [Test]
        public void TryCreateId_Returns_False_OnTimestampWraparound()
        {
            var ts = new MockTimeSource(IdStructure.Default.MaxIntervals - 1);  // Set clock to 1 'tick' before wraparound
            var g = new IdGenerator(0, new IdGeneratorOptions(timeSource: ts));

            Assert.IsTrue(g.TryCreateId(out var _));    // Should succeed;
            ts.NextTick();
            Assert.IsFalse(g.TryCreateId(out var _));   // Should fail
        }

        [Test]
        public void FromId_Returns_CorrectValue()
        {
            var s = new IdStructure(42, 8, 13);
            var epoch = new DateTimeOffset(2018, 7, 31, 14, 48, 2, TimeSpan.FromHours(2));  // Just some "random" epoch...
            var ts = new MockTimeSource(5, TimeSpan.FromSeconds(7), epoch);                 // Set clock at 5 ticks; each tick being 7 seconds...
                                                                                            // Set generator ID to 234
            var g = new IdGenerator(234, new IdGeneratorOptions(s, ts));

            // Generate a bunch of id's
            long id = 0;
            for (var i = 0; i < 35; i++)
                id = g.CreateId();

            var target = g.FromId(id);


            Assert.AreEqual(34, target.SequenceNumber);                                     // We generated 35 id's in the same tick, so sequence should be at 34.
            Assert.AreEqual(234, target.GeneratorId);                                       // With generator id 234
            Assert.AreEqual(epoch.Add(TimeSpan.FromSeconds(5 * 7)), target.DateTimeOffset); // And the clock was at 5 ticks, with each tick being
                                                                                            // 7 seconds (so 35 seconds from epoch)
                                                                                            // And epoch was 2018-7-31 14:48:02 +02:00...
        }

        [Test]
        public void CreateId_Waits_OnSequenceOverflow()
        {
            // Use timesource that generates a new tick every 10 calls to GetTicks()
            var ts = new MockAutoIncrementingIntervalTimeSource(10);
            var s = new IdStructure(61, 0, 2);
            var g = new IdGenerator(0, new IdGeneratorOptions(idStructure: s, timeSource: ts, sequenceOverflowStrategy: SequenceOverflowStrategy.SpinWait));

            // We have a 2-bit sequence; generating 4 id's in a single time slot - wait for other then
            Assert.AreEqual(0, g.CreateId());
            Assert.AreEqual(1, g.CreateId());
            Assert.AreEqual(2, g.CreateId());
            Assert.AreEqual(3, g.CreateId());
            Assert.AreEqual(4, g.CreateId());   // This should trigger a spinwait and return the next ID
            Assert.AreEqual(5, g.CreateId());
        }
    }
}