using System;

namespace Yannic.Coroutines
{
    public interface IResetable
    {
        event Action Reseted;

        void Reset();
    }
}
