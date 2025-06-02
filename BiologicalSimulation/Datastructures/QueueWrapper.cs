using System.Collections.Concurrent;

namespace BiologicalSimulation.Datastructures;

/// <summary>
/// This thing is a Queue or ConcurrentQueue as decided at runtime.
/// This is needed because multithreading needs a ConcurrentQueue, while single-threaded prefers a normal Queue as it is a lot faster.
/// Making this class allows us to keep classes like Chunk2D and Chunk3D to a single version and not a separate single- and multithreaded versions
/// </summary>
public class QueueWrapper<T>
{
    private bool multithreaded { get; }
    private ConcurrentQueue<T> ConcurrentQueue { get; }
    private Queue<T> SingleQueue { get; }
    
    public QueueWrapper(bool multithreaded)
    {
        this.multithreaded = multithreaded;
        if(multithreaded)
            ConcurrentQueue = new ConcurrentQueue<T>();
        else
            SingleQueue = new Queue<T>();
    }

    public void Enqueue(T item)
    {
        if(multithreaded)
            ConcurrentQueue.Enqueue(item);
        else
            SingleQueue.Enqueue(item);
    }

    public bool Dequeue(out T item)
    {
        if (multithreaded)
        {
            ConcurrentQueue.TryDequeue(out T possibleItem);
            item = possibleItem;
            
            return (possibleItem != null);
        }
        else
        {
            item = SingleQueue.Dequeue();
            return true;
        }
    }
    
    public int Count => multithreaded ? ConcurrentQueue.Count : SingleQueue.Count;
}