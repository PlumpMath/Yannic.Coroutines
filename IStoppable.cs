
namespace Yannic.Coroutines
{
	public delegate void StoppedHandler();

    public interface IStoppable
    {
        event StoppedHandler Stopped;

        void Stop();
    }

}