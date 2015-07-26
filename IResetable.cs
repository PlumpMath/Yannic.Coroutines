
namespace Yannic.Coroutines
{
	public delegate void ResetedHandler();

    public interface IResetable
    {
        event ResetedHandler Reseted;

        void Reset();
    }
}
