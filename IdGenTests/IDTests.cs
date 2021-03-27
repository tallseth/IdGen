using IdGen;
using NUnit.Framework;

namespace IdGenTests
{
    [TestFixture]
    public class IDTests
    {
        [Test]
        public void ID_DoesNotEqual_RandomObject()
        {
            var g = new IdGenerator(0);
            var i = g.FromId(0);
            Assert.IsFalse(i.Equals(new object()));
            Assert.IsTrue(i.Equals((object)g.FromId(0)));
            Assert.IsTrue(i != g.FromId(1));
            Assert.IsTrue(i == g.FromId(0));
            Assert.AreEqual(i.GetHashCode(), g.FromId(0).GetHashCode());
        }

        [Test]
        public void ID_Equals_OtherId()
        {
            var g = new IdGenerator(0);
            var i = g.FromId(1234567890);
            Assert.IsTrue(i.Equals(g.FromId(1234567890)));
            Assert.IsTrue(i.Equals((object)g.FromId(1234567890)));
            Assert.IsTrue(i != g.FromId(0));
            Assert.IsTrue(i == g.FromId(1234567890));
            Assert.AreEqual(i.GetHashCode(), g.FromId(1234567890).GetHashCode());
        }

        [Test]
        public void X()
        {
            var g = new IdGenerator(0);
            var i = g.FromId(1);

            Assert.AreEqual(1, i.SequenceNumber);
            Assert.AreEqual(0, i.GeneratorId);
            Assert.AreEqual(g.Options.TimeSource.Epoch, i.DateTimeOffset);
        }

    }
}
