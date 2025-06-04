using System.Numerics;

namespace BioSim.Datastructures;

public class Multithreaded3DFixedDataStructure : Chunk3DFixedDataStructure
{
    private int taskCount;
    private (int, int, int)[][] chunkGroups;
    private int groupCount;
    private bool stepping = false;
    
    public Multithreaded3DFixedDataStructure(Vector3 minPosition, Vector3 maxPosition, float chunkSize, float largestOrganismSize) : base(minPosition, maxPosition, chunkSize, largestOrganismSize, true)
    {
        //TODO this only works if exactly set of 4, change later
        taskCount = ChunkCountX * ChunkCountY * ChunkCountZ / 4;
        
        (int, int, int)[] offset = [(0, 0, 0), (0, 1, 0), (1, 0, 0), (1, 1, 0), (0, 0, 1), (0, 1, 1), (1, 0, 1), (1, 1, 1)];
        groupCount = offset.Length;
        chunkGroups = new (int, int, int)[groupCount][];
        
        for (int group = 0; group < groupCount; group++)
        {
            chunkGroups[group] = new (int, int, int)[taskCount];
            (int offsetX, int offsetY, int offsetZ) = offset[group];
            
            int threadId = 0;
            //All workers are assigned a chunk where every chunk has no direct neighbour that is currently working, meaning we get a grid pattern
            //Note that x and y grow by 2 each loop
            for (int x = 0; x < ChunkCountX; x += 2)
            {
                for (int y = 0; y < ChunkCountY; y += 2)
                {
                    for (int z = 0; z < ChunkCountZ; z += 2)
                    {
                        chunkGroups[group][threadId] = (x + offsetX, y + offsetY, z + offsetZ);
                        threadId++;
                    }
                }
            }
        }
    }
    
    public async Task ChunkStepTask(int x, int y, int z)
    {
        await Task.Run(Chunks[x,y,z].Step);
    }
    
    public override async void Step()
    {
        //Apparently some frameworks are fucking funny (looking at you Monogame/XNA) and can break with multithreading,
        //so this check insures no update loop has been called twice in the same frame
        //Context for bug: Once every 5000 frames or so Step() would be called twice
        if (stepping)
            return;

        stepping = true;
        
        for (int group = 0; group < groupCount; group++)
        {
            List<Func<Task>> tasks = chunkGroups[group]
                .Select(coords => (Func<Task>)(() => ChunkStepTask(coords.Item1,coords.Item2, coords.Item3)))
                .ToList();

            await RunTasks(tasks);
        }

        stepping = false;
    }
    
    static async Task RunTasks(List<Func<Task>> taskFuncs)
    {
        var tasks = taskFuncs.Select(f => f()).ToArray();
        await Task.WhenAll(tasks);
    }
}