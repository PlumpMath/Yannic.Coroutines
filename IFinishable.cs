
namespace Yannic.Coroutines
{
	public delegate void FinishedHandler();

    public interface IFinishable
    {
        bool IsFinished { get; }
        event FinishedHandler Finished;
    }
}