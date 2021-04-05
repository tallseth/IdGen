using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IdGen;
using IdGenTests.Mocks;
using NUnit.Framework;

namespace IdGenTests
{
    [TestFixture]
    public class IdGeneratorMultiThreadTests
    {
        [Test]
        public void SlidingWindowBasicTest()
        {
            var generator = IdGenerator.CreateInstanceWithDefaultOptions(0);
            
            var ids = new List<long>();

            Action baseAction = () =>
            {
                var id = generator.CreateId();
                lock (ids)
                {
                    ids.Add(id);
                }
            };
            
            var exceptions = RunSlidingWindowThreadTest(baseAction, 300);

            Assert.That(exceptions, Is.Empty);
            Assert.That(ids.Count, Is.EqualTo(300), "not enough ids");
            Assert.That(ids.Distinct().Count(), Is.EqualTo(ids.Count), "not enough distinct ids");
        }
        
        [Test]
        public void SlidingWindowSpinWaitTest()
        {
            var idGeneratorOptions = new IdGeneratorOptions(
                timeSource:new MockAutoIncrementingIntervalTimeSource(10), 
                idStructure: new IdStructure(61, 0, 2), 
                sequenceOverflowStrategy:SequenceOverflowStrategy.SpinWait);
            var generator = IdGenerator.CreateInstance(0, idGeneratorOptions);
            
            var ids = new List<long>();

            Action baseAction = () =>
            {
                var id = generator.CreateId();
                lock (ids)
                {
                    ids.Add(id);
                }
            };
            
            var exceptions = RunSlidingWindowThreadTest(baseAction, 300);

            Assert.That(exceptions, Is.Empty);
            Assert.That(ids.Count, Is.EqualTo(300), "not enough ids");
            Assert.That(ids.Distinct().Count(), Is.EqualTo(ids.Count), "not enough distinct ids");
        }
        
        private static List<Exception> RunSlidingWindowThreadTest(Action baseAction, int threadCount)
        {
            var mre = new ManualResetEvent(false);
            var exceptions = new List<Exception>();

            ThreadStart GetThreadProc(int millisecondsOfDelay)
            {
                return () =>
                {
                    mre.WaitOne();
                    Thread.Sleep(millisecondsOfDelay);
                    try
                    {
                        baseAction();
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                };
            }

            var threads = new List<Thread>();
            threads.Add(new Thread(GetThreadProc(0)));
            threads.Add(new Thread(GetThreadProc(0)));
            threads.Add(new Thread(GetThreadProc(0)));
            threads.Add(new Thread(GetThreadProc(0)));
            threads.Add(new Thread(GetThreadProc(0)));
            for (int i = 1; i <= threadCount - 5; i++)
            {
                threads.Add(new Thread(GetThreadProc(i)));
            }

            threads.ForEach(t => t.Start());
            Thread.Sleep(100);
            mre.Set();
            threads.ForEach(t => t.Join());
            return exceptions;
        }
    }
}