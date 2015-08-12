using System;

namespace Yannic.Coroutines
{
    public interface IStoppable
    {
        event Action Stopped;

        void Stop();
    }

}