namespace BioSim.Datastructures;

/// <summary>
/// This is used to interact with a thread
/// </summary>
public class ThreadWorker
{
    public bool Active { get; private set; }
    public Thread Thread { get; }
    public int AssignedChunkX { get; private set; }
    public int AssignedChunkY { get; private set; }
    public int AssignedChunkZ { get; private set; }
    
    public ThreadWorker(ThreadStart threadStart)
    {
        Thread = new Thread(threadStart);
        Active = false;
        AssignedChunkX = 0;
        AssignedChunkY = 0;
        AssignedChunkZ = 0;
    }

    /// <summary>
    /// Will immediately start running Step() within the given chunk
    /// 3D chunked version
    /// </summary>
    /// <param name="chunkX"></param>
    /// <param name="chunkY"></param>
    /// <param name="chunkZ"></param>
    public void AssignTask(int chunkX, int chunkY, int chunkZ)
    {
        Active = true;
        AssignedChunkX = chunkX;
        AssignedChunkY = chunkY;
        AssignedChunkZ = chunkZ;
    }
    
    /// <summary>
    /// Will immediately start running Step() within the given chunk
    /// 2D chunked version
    /// </summary>
    /// <param name="chunkX"></param>
    /// <param name="chunkY"></param>
    public void AssignTask(int chunkX, int chunkY)
    {
        Active = true;
        AssignedChunkX = chunkX;
        AssignedChunkY = chunkY;
        Thread.Start();
    }
}