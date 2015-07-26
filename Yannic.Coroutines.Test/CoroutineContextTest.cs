using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Yannic.Coroutines;

namespace Yannic.Coroutines.Test
{
    [TestFixture]
    public class CoroutineContextTest
    {
        [Test]
        public void CreateCoroutineContext()
        {
            CoroutineContext coroutineContext;

            Assert.Throws<NullReferenceException>(() =>
                {
                    coroutineContext = new CoroutineContext(null);
                });

            coroutineContext = new CoroutineContext(NullCoroutine);

            Assert.IsFalse(coroutineContext.IsStarted);
            Assert.IsFalse(coroutineContext.IsPaused);
            Assert.IsFalse(coroutineContext.IsFinished);
        }

        static int a = 3;
        static IEnumerable SquareA()
        {
            yield return a = a * a;
        }
        [Test]
        public void StartCoroutine()
        {
            var coroutineContext = new CoroutineContext(NullCoroutine);

            bool isFinishedEventTriggered = true;
            coroutineContext.Finished += () => isFinishedEventTriggered = true;

            coroutineContext.Start();

            Assert.IsTrue(isFinishedEventTriggered);
            Assert.IsFalse(coroutineContext.IsStarted);
            Assert.IsFalse(coroutineContext.IsPaused);
            Assert.IsTrue(coroutineContext.IsFinished);

            var coroutineContext2 = new CoroutineContext(SquareA);

            bool isStartedEventTriggered = false;
            coroutineContext2.Started += () =>  isStartedEventTriggered = true;

            coroutineContext2.Start();

            Assert.IsTrue(isStartedEventTriggered);
            Assert.AreEqual(a, 9);
        }

        [Test]
        public void StartCoroutineWithTimeout()
        {
            const int expectedMilliseconds = 1000;
            const int tolerance = 2;

            var sw = new Stopwatch();

            var coroutineContext = new CoroutineContext(InfiniteCoroutine);
            var timeOut = new TimeSpan(0, 0, 0, 0, expectedMilliseconds);

            sw.Start();
            coroutineContext.Start(timeOut);
            sw.Stop();

            Assert.IsTrue(((sw.ElapsedMilliseconds + tolerance) > expectedMilliseconds) || ((sw.ElapsedMilliseconds - tolerance) < expectedMilliseconds));
        }
            
        [Test]
        public void PauseCoroutineFromAnotherThread()
        {
            var coroutineContext = new CoroutineContext(InfiniteCoroutine);

            Assert.IsFalse(coroutineContext.IsStarted);
            Assert.IsFalse(coroutineContext.IsPaused);
            Assert.IsFalse(coroutineContext.IsFinished);

            bool isPausedEventTriggered = false;
            coroutineContext.Paused += paused => isPausedEventTriggered = paused;

            var pausingThread = new Thread(() =>
                {   
                    Thread.Sleep(10);
                    coroutineContext.Pause();
                });

            pausingThread.Start();
            coroutineContext.Start();

            Assert.IsTrue(isPausedEventTriggered);
            Assert.IsTrue(coroutineContext.IsStarted);
            Assert.IsTrue(coroutineContext.IsPaused);
            Assert.IsFalse(coroutineContext.IsFinished);

            var testUnpausedThread = new Thread(() =>
                {
                    Thread.Sleep(10);

                    Assert.IsTrue(coroutineContext.IsStarted);
                    Assert.IsFalse(coroutineContext.IsPaused);
                    Assert.IsFalse(coroutineContext.IsFinished);

                    coroutineContext.Stop();
                });

            testUnpausedThread.Start();

            coroutineContext.Unpause();
        }

        [Test]
        public void StopCoroutineFromAnotherThread()
        {
            var coroutineContext = new CoroutineContext(InfiniteCoroutine);

            var isStoppedEventTriggered = false;
            coroutineContext.Stopped += () => isStoppedEventTriggered = true;

            var stoppingThread = new Thread(() =>
                {   
                    Thread.Sleep(10);
                    coroutineContext.Stop();
                });

            stoppingThread.Start();
            coroutineContext.Start();

            Assert.IsTrue(isStoppedEventTriggered);
            Assert.IsFalse(coroutineContext.IsStarted);
            Assert.IsFalse(coroutineContext.IsPaused);
            Assert.IsFalse(coroutineContext.IsFinished);
        }

        [Test]
        public void ResetCoroutineContext()
        {
            var coroutineContext = new CoroutineContext(InfiniteCoroutine);

            bool isResetEventTriggered = false;
            coroutineContext.Reseted += () => isResetEventTriggered = true;

            var resetCoroutineContextThread = new Thread(() =>
                {
                    Thread.Sleep(10);
                    coroutineContext.Reset(); 
                });

            resetCoroutineContextThread.Start();
            coroutineContext.Start();

            Assert.IsTrue(isResetEventTriggered);
            Assert.IsFalse(coroutineContext.IsStarted);
            Assert.IsFalse(coroutineContext.IsPaused);
            Assert.IsFalse(coroutineContext.IsFinished);
        }

        [Test]
        public void StepThroughCoroutine()
        {
            var coroutineContext = new CoroutineContext(From1To10);

            int count = 0;
            for (int i = 0; coroutineContext.Step(); i++)
            {
                count = i + 1;
            }

            Assert.AreEqual(count, 10);
            Assert.IsFalse(coroutineContext.IsStarted);
            Assert.IsFalse(coroutineContext.IsPaused);
            Assert.IsTrue(coroutineContext.IsFinished);
        }




        private IEnumerable NullCoroutine()
        {
            yield return null;
        }

        private IEnumerable InfiniteCoroutine()
        {
            while (true)
                yield return null;
        }

        private IEnumerable From1To10()
        {
            for (int i = 1; i < 11; i++)
            {
                yield return i;
            }
        }
    }
}

