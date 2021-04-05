using System;
using IdGen;
using IdGenTests.Mocks;
using NUnit.Framework;

namespace IdGenTests
{
    [TestFixture]
    public class IDTests
    {
        [Test]
        public void ID_DoesNotEqual_RandomObject()
        {
            var i = FromId(0);
            Assert.IsFalse(i.Equals(new object()));
            Assert.IsTrue(i.Equals((object)FromId(0)));
            Assert.IsTrue(i != FromId(1));
            Assert.IsTrue(i == FromId(0));
            Assert.AreEqual(i.GetHashCode(), FromId(0).GetHashCode());
        }

        [Test]
        public void ID_Equals_OtherId()
        {
            var i = FromId(1234567890);
            Assert.IsTrue(i.Equals(FromId(1234567890)));
            Assert.IsTrue(i.Equals((object)FromId(1234567890)));
            Assert.IsTrue(i != FromId(0));
            Assert.IsTrue(i == FromId(1234567890));
            Assert.AreEqual(i.GetHashCode(), FromId(1234567890).GetHashCode());
        }

        [Test]
        public void X()
        {
            var options = new IdGeneratorOptions();
            var i = Id.Parse(1, options.IdStructure, options.TimeSource);

            Assert.AreEqual(1, i.SequenceNumber);
            Assert.AreEqual(0, i.GeneratorId);
            Assert.AreEqual(options.TimeSource.Epoch, i.DateTimeOffset);
        }
        
        [Test]
        public void Parse_Returns_CorrectValue()
        {
            var epoch = new DateTimeOffset(2018, 7, 31, 14, 48, 2, TimeSpan.FromHours(2));  // Just some "random" epoch...
            var currentTicks = 5;
            var tickDurationSeconds = 7;
            var generatorId = 234;
            var expectedTimeAfterEpoch = TimeSpan.FromSeconds(currentTicks * tickDurationSeconds);
            
            var ts = new MockTimeSource(currentTicks, TimeSpan.FromSeconds(tickDurationSeconds), epoch);
            var s = new IdStructure(42, 8, 13); 

            var number = 34 + (generatorId << s.SequenceBits) + (currentTicks << (s.SequenceBits + s.GeneratorIdBits));
            var target = Id.Parse(number, s, ts);

            Assert.AreEqual(34, target.SequenceNumber);
            Assert.AreEqual(generatorId, target.GeneratorId);
            Assert.AreEqual(epoch.Add(expectedTimeAfterEpoch), target.DateTimeOffset);
        }

        private Id FromId(long value)
        {
            var options = new IdGeneratorOptions();
            return Id.Parse(value, options.IdStructure, options.TimeSource);
        }
    }
}
