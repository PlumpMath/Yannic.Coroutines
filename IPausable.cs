using System;

namespace Yannic.Coroutines
{
    public interface IPausable : IStartable
    {
        bool IsPaused { get; }
        event Action<bool> Paused;

        void Pause();
        void Unpause();
    }
}
