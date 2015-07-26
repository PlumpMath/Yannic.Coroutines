
namespace Yannic.Coroutines
{
    public interface IControllable : IRestartable, IPausable, IStoppable, IFinishable
    {
    }

    public interface IControllable<TStartParam> : IRestartable<TStartParam>, IPausable, IStoppable, IFinishable
    {
    }
}

