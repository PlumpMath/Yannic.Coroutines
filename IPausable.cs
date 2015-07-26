
namespace Yannic.Coroutines
{
	public delegate void PausedHandler(bool paused);

    public interface IPausable : IStartable
    {
        bool IsPaused { get; }
        event PausedHandler Paused;

        void Pause();
        void Unpause();
    }
}
