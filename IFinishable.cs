using System;

namespace Yannic.Coroutines
{
    public interface IFinishable
    {
        bool IsFinished { get; }
        event Action Finished;
    }
}