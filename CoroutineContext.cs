using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Yannic.Coroutines
{
    public class CoroutineContext : IControllable
    {
        public bool IsPaused { get; set;}
        public bool IsStarted { get; set;}
        public bool IsFinished {get; set;}

        public event StartedHandler Started = delegate { };
        public event PausedHandler Paused = delegate { };
        public event StoppedHandler Stopped = delegate { };
        public event ResetedHandler Reseted = delegate { };
        public event FinishedHandler Finished = delegate { };


        private IEnumerator coroutineEnumerator;

        private IEnumerable coroutineEnumerable = new List<object>();
        protected IEnumerable CoroutineEnumerable
        {
            get { return this.coroutineEnumerable; }
            set 
            { 
                lock (this.startLock) 
                { 
                    this.coroutineEnumerable = value ?? this.coroutineEnumerable;
                    this.coroutineEnumerator = this.coroutineEnumerable.GetEnumerator();
                } 
            }
        }
            
        public Func<IEnumerable> Coroutine
        {
            get { return () => this.CoroutineEnumerable; }
            set { this.CoroutineEnumerable = value(); }
        }

        protected readonly object startLock = new object();
        protected readonly object pauseLock = new object();
        protected readonly object unpauseLock = new object();
        protected readonly object stopLock = new object();
        protected readonly object resetLock = new object();



        protected CoroutineContext()
        {
        }

        public CoroutineContext(Func<IEnumerable> coroutine)
        {
            this.Coroutine = coroutine;
        }
            

        public void Start()
        {
            this.Start(TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        public void Start(TimeSpan timeOut)
        {
            Func<TimeSpan, bool>  noTermination = elapsed => false;
            Func<TimeSpan, bool>  terminationByTimeOut = elapsed => (timeOut >= elapsed);

            Func<TimeSpan, bool> terminationCondition = 
                (timeOut.Milliseconds == Timeout.Infinite) ? 
                noTermination : terminationByTimeOut;

            this.Start(terminationCondition);
        }

        public void Start(Func<bool> terminationCondition)
        {
            this.Start(e => terminationCondition());
        }

        public void Start(Func<TimeSpan, bool> timeDependendTerminationCondition)
        {
            if (this.IsStarted && !this.IsPaused)
                return;
            
            lock (this.startLock)
            {
                lock (this.pauseLock)
                {
                    this.IsPaused = false;
                    this.IsStarted = true;
                }
                this.Started();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                while (!this.IsPaused && !timeDependendTerminationCondition(stopwatch.Elapsed) && this.Step())
                {
                }
                this.IsStarted = !this.IsFinished;

                // wait for variables to be set and events to be called by other functions
                // before giving back control to the thread
                lock (this.pauseLock) {}
                lock (this.stopLock) {}
                lock (this.resetLock) {}
            }
        }

        public bool Step()
        {
            lock (this.coroutineEnumerator)
            {
                try
                {
                    if (this.IsFinished = !this.coroutineEnumerator.MoveNext())
                        this.Finished();
                    return !this.IsFinished;
                }
                catch
                {
                    this.IsFinished = true;
                    throw;
                }
            }
        }

        public void Pause()
        {
            if (this.IsPaused)
                return;
            
            lock (this.pauseLock)
            {
                this.IsPaused = true;
                this.Paused(paused: true);
            }
        }

        public void Unpause()
        {
            if (!this.IsPaused)
                return;
            
            lock (this.unpauseLock)
            {
                lock (this.pauseLock)
                {
                    this.IsPaused = false;
                    this.Paused(paused: false);
                }
                this.Start();
            }
        }

        public void Stop()
        {
            if (!this.IsStarted)
                return;
            
            lock (this.stopLock)
            {
                this.Reset();
                this.Stopped();
            }
        }

        public void Reset()
        {
            lock (this.resetLock)
            {
                this.Pause();

                lock (this.coroutineEnumerator)
                {
                    this.coroutineEnumerator = this.CoroutineEnumerable.GetEnumerator();
                    this.IsPaused = this.IsStarted = this.IsFinished = false;
                }

                this.Reseted();
            }
        }

        public void Restart()
        {
            this.Reset();
            this.Start();
        }

    }

    public class CoroutineContext<TStartParam> : CoroutineContext, IControllable<TStartParam>
    {
        public Func<TStartParam, IEnumerable> ParametrizedCoroutine { get; protected set; }

        public CoroutineContext(Func<TStartParam, IEnumerable> parametrizedCoroutine)
        {
            this.ParametrizedCoroutine = parametrizedCoroutine;  
        }

        protected CoroutineContext()
        {
        }

        public void Start(TStartParam arg)
        {
            this.Coroutine = () => this.ParametrizedCoroutine(arg);
            base.Start();
        }
    }
}
