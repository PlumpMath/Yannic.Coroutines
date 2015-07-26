using System;

namespace Yannic.Coroutines
{
    public delegate void StartedHandler();

    public interface IStartable
    {
        bool IsStarted { get; }
        event StartedHandler Started;

        void Start();
    }
    public interface IParametrizedStartable<TStartParam>
    {
        bool IsStarted { get; }
        event StartedHandler Started;

        void Start(TStartParam param);
    }
}
