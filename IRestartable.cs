
namespace Yannic.Coroutines
{
    public interface IRestartable : IStartable, IResetable
    {
        void Restart();
    }

    public interface IRestartable<TStartParam> : IParametrizedStartable<TStartParam>, IResetable
    {
        void Restart();
    }
}
