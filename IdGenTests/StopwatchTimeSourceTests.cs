using System;
using System.Threading;
using IdGen;
using NUnit.Framework;

namespace IdGenTests
{
    [TestFixture]
    public class StopwatchTimeSourceTests
    {
        [Test]
        public void GetTicks_ReturnsNumber()
        {
            var timeSource = StopwatchTimeSource.GetInstance(DateTimeOffset.Now);
            Assert.That(timeSource.GetTicks(), Is.GreaterThanOrEqualTo(0));
        }
        
        [Test]
        public void GetTicks_InTheSameTickDuration_ReturnsSameNumber()
        {
            var timeSource = StopwatchTimeSource.GetInstance(DateTimeOffset.Now, TimeSpan.FromSeconds(2));
            Assert.That(timeSource.GetTicks(), Is.EqualTo(timeSource.GetTicks()));
        }
        
        [Test]
        public void GetTicks_AfterTickDurationElapses_ReturnsLargerNumber()
        {
            var timeSource = StopwatchTimeSource.GetInstance(DateTimeOffset.Now, TimeSpan.FromMilliseconds(1));
            
            var first = timeSource.GetTicks();
            Thread.Sleep(3);
            var second = timeSource.GetTicks();
            
            Assert.That(first, Is.LessThan(second));
        }

        [Test]
        public void MultipleTimeSourcesStayInOrder()
        {
            //this test specifies the problem in https://github.com/RobThree/IdGen/issues/20
            var epoch = DateTimeOffset.Now;
            var first = StopwatchTimeSource.GetInstance(epoch, TimeSpan.FromMilliseconds(2));
            Thread.Sleep(50);
            var second = StopwatchTimeSource.GetInstance(epoch, TimeSpan.FromMilliseconds(2));

            var a = second.GetTicks();
            Thread.Sleep(2);
            var b = first.GetTicks();

            Assert.That(a, Is.LessThan(b));
        }
        
        [Test]
        public void MultipleTimeSourcesReturnSameTicksInSameDuration()
        {
            var epoch = DateTimeOffset.Now;
            var first = StopwatchTimeSource.GetInstance(epoch, TimeSpan.FromMilliseconds(10));
            Thread.Sleep(5);
            var second = StopwatchTimeSource.GetInstance(epoch, TimeSpan.FromMilliseconds(10));

            var a = second.GetTicks();
            var b = first.GetTicks();

            Assert.That(a, Is.EqualTo(b));
        }
    }
}