using System;

namespace Yannic.Coroutines
{
    public interface IStartable
    {
        bool IsStarted { get; }
        event Action Started;

        void Start();
    }
    public interface IParametrizedStartable<TStartParam>
    {
        bool IsStarted { get; }
        event Action Started;

        void Start(TStartParam param);
    }
}
