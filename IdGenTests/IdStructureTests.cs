using IdGen;
using IdGenTests.Mocks;
using System;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace IdGenTests
{
    [TestFixture]
    public class IdStructureTests
    {
        [Test]
        public void DefaultIdStructure_Matches_Expectations()
        {
            var s = IdStructure.Default;

            Assert.AreEqual(41, s.TimestampBits);
            Assert.AreEqual(10, s.GeneratorIdBits);
            Assert.AreEqual(12, s.SequenceBits);

            // We should be able to generate a total of 63 bits worth of Id's
            Assert.AreEqual(long.MaxValue, (s.MaxGenerators * s.MaxIntervals * s.MaxSequenceIds) - 1);
        }

        [Test]
        public void Constructor_Throws_OnIdStructureNotExactly63Bits()
        {
            Assert.Throws<InvalidOperationException>(()=>new IdStructure(41, 10, 11));
        }

        [Test]
        public void Constructor_Throws_OnGeneratorIdMoreThan31Bits()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()=>new IdStructure(21, 32, 10));
        }

        [Test]
        public void Constructor_Throws_OnSequenceMoreThan31Bits()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()=>new IdStructure(21, 10, 32));
        }

        [Test]
        public void IdStructure_CalculatesWraparoundInterval_Correctly()
        {
            var mc_ms = new MockTimeSource();

            // 40 bits of Timestamp should give us about 34 years worth of Id's
            Assert.AreEqual(34, (int)(new IdStructure(40, 11, 12).WraparoundInterval(mc_ms).TotalDays / 365.25));
            // 41 bits of Timestamp should give us about 69 years worth of Id's
            Assert.AreEqual(69, (int)(new IdStructure(41, 11, 11).WraparoundInterval(mc_ms).TotalDays / 365.25));
            // 42 bits of Timestamp should give us about 139 years worth of Id's
            Assert.AreEqual(139, (int)(new IdStructure(42, 11, 10).WraparoundInterval(mc_ms).TotalDays / 365.25));

            var mc_s = new MockTimeSource(TimeSpan.FromSeconds(0.1));

            // 40 bits of Timestamp should give us about 3484 years worth of Id's
            Assert.AreEqual(3484, (int)(new IdStructure(40, 11, 12).WraparoundInterval(mc_s).TotalDays / 365.25));
            // 41 bits of Timestamp should give us about 6968 years worth of Id's
            Assert.AreEqual(6968, (int)(new IdStructure(41, 11, 11).WraparoundInterval(mc_s).TotalDays / 365.25));
            // 42 bits of Timestamp should give us about 13936 years worth of Id's
            Assert.AreEqual(13936, (int)(new IdStructure(42, 11, 10).WraparoundInterval(mc_s).TotalDays / 365.25));

            var mc_d = new MockTimeSource(TimeSpan.FromDays(1));

            // 21 bits of Timestamp should give us about 5741 years worth of Id's
            Assert.AreEqual(5741, (int)(new IdStructure(21, 11, 31).WraparoundInterval(mc_d).TotalDays / 365.25));
            // 22 bits of Timestamp should give us about 11483 years worth of Id's
            Assert.AreEqual(11483, (int)(new IdStructure(22, 11, 30).WraparoundInterval(mc_d).TotalDays / 365.25));
            // 23 bits of Timestamp should give us about 22966 years worth of Id's
            Assert.AreEqual(22966, (int)(new IdStructure(23, 11, 29).WraparoundInterval(mc_d).TotalDays / 365.25));
        }

        [Test]
        public void IdStructure_Calculates_WraparoundDate_Correctly()
        {
            var s = IdStructure.Default;
            var mc = new MockTimeSource(TimeSpan.FromMilliseconds(1));
            var d = s.WraparoundDate(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), mc);
            Assert.AreEqual(new DateTime(643346200555520000, DateTimeKind.Utc), d.DateTime);
        }

        [Test]
        public void WraparoundDate_ThrowsOnNullTimeSource()
        {
            Assert.Throws<ArgumentNullException>(()=>IdStructure.Default.WraparoundDate(IdGeneratorOptions.DefaultEpoch, null));
        }

        [Test]
        public void WraparoundInterval_ThrowsOnNullTimeSource()
        {
            Assert.Throws<ArgumentNullException>(()=>IdStructure.Default.WraparoundInterval(null));
        }

        [Test]
        public void RoundTripEncoding_StartWithEncode()
        {
            var random = new Random();
            var ticks = random.Next(0, (2 ^ 31) - 1);
            var generator = random.Next(0, (2 ^ 20) - 1);
            var sequence = random.Next(0, (2 ^ 12) - 1);
            
            var structure = new IdStructure(31, 20, 12); // arguments must match powers above

            var encoded = structure.Encode(ticks, generator, sequence);
            structure.Decode(encoded, out var decodedTicks, out var decodedGenerator, out var decodedSequence);

            Assert.That(decodedTicks, Is.EqualTo(ticks));
            Assert.That(decodedGenerator, Is.EqualTo(generator));
            Assert.That(decodedSequence, Is.EqualTo(sequence));
        }
        
        [Test]
        public void RoundTripEncoding_StartWithDecode()
        {
            var random = new Random();
            var id = random.Next(0, (2 ^ 63) - 1);
            var structure = new IdStructure(31, 20, 12);

            
            structure.Decode(id, out var ticks, out var generator, out var sequence);
            var encoded = structure.Encode(ticks, generator, sequence);
            
            Assert.That(encoded, Is.EqualTo(id));
        }

        [Test]
        public void DecodeZero()
        {
            var structure = IdStructure.Default;
            structure.Decode(0, out var ticks, out var generator, out var sequence);

            Assert.That(ticks, Is.EqualTo(0));
            Assert.That(generator, Is.EqualTo(0));
            Assert.That(sequence, Is.EqualTo(0));
        }
        
        [Test]
        public void DecodeMax()
        {
            var structure = IdStructure.Default;
            structure.Decode(long.MaxValue, out var ticks, out var generator, out var sequence);

            Assert.That(ticks, Is.EqualTo(structure.MaxIntervals - 1));
            Assert.That(generator, Is.EqualTo(structure.MaxGenerators - 1));
            Assert.That(sequence, Is.EqualTo(structure.MaxSequenceIds - 1));
        }
        
        [Test]
        public void EncodeZero()
        {
            var structure = IdStructure.Default;
            var id = structure.Encode(0, 0, 0);

            Assert.That(id, Is.EqualTo(0));
        }
        
        [Test]
        public void EncodeMax()
        {
            var structure = IdStructure.Default;
            var id = structure.Encode(structure.MaxIntervals - 1, structure.MaxGenerators - 1, structure.MaxSequenceIds - 1);

            Assert.That(id, Is.EqualTo(long.MaxValue));
        }
    }
}
