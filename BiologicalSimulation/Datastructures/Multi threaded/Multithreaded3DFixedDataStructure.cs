using System.Numerics;

namespace BioSim.Datastructures;

public class Multithreaded3DFixedDataStructure : Chunk3DFixedDataStructure
{
    private int threadCount;
    private Chunk3D[][] chunkGroups;
    
    public Multithreaded3DFixedDataStructure(Vector3 minPosition, Vector3 maxPosition, float chunkSize, float largestOrganismSize) : base(minPosition, maxPosition, chunkSize, largestOrganismSize, true)
    {
        //TODO this only works if exactly set of 4, change later
        threadCount = ChunkCountX * ChunkCountY * ChunkCountZ / 4;
        
        chunkGroups = new Chunk3D[4][];
        chunkGroups[0] = new Chunk3D[threadCount];
        
        (int, int, int)[] offset = [(0, 0, 0), (0, 1, 0), (1, 0, 0), (1, 1, 0), (0, 0, 1), (0, 1, 1), (1, 0, 1), (1, 1, 1)];
        for (int quadrant = 0; quadrant < 4; quadrant++)
        {
            chunkGroups[quadrant] = new Chunk3D[threadCount];
            (int offsetX, int offsetY, int offsetZ) = offset[quadrant];
            
            int threadId = 0;
            //All workers are assigned a chunk where every chunk has no direct neighbour that is currently working, meaning we get a grid pattern
            //Note that x and y grow by 2 each loop
            for (int x = 0; x < ChunkCountX; x += 2)
            {
                for (int y = 0; y < ChunkCountY; y += 2)
                {
                    for (int z = 0; z < ChunkCountZ; z += 2)
                    {
                        chunkGroups[quadrant][threadId] = Chunks[x + offsetX, y + offsetY, z + offsetZ];
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
        //Slower version
        // for (int i = 0; i < 4; i++)
        // {
        //     Parallel.ForEach(chunkGroups[i], chunk =>
        //     {
        //         chunk.Step();
        //     });
        // }
        //
        //
        // return;
        
        //Quicker version, but sometimes seems to not work
        (int, int, int)[] offset = [(0, 0, 0), (0, 1, 0), (1, 0, 0), (1, 1, 0), (0, 0, 1), (0, 1, 1), (1, 0, 1), (1, 1, 1)];
        for (int quadrant = 0; quadrant < 4; quadrant++)
        {
            (int, int, int)[] taskCoords = new (int, int, int)[threadCount];
            (int offsetX, int offsetY, int offsetZ) = offset[quadrant];
            int threadId = 0;
            //All workers are assigned a chunk where every chunk has no direct neighbour that is currently working, meaning we get a grid pattern
            //Note that x and y grow by 2 each loop
            for (int x = 0; x < ChunkCountX; x+=2)
            {
                for (int y = 0; y < ChunkCountY; y+=2)
                {
                    for (int z = 0; z < ChunkCountZ; z += 2)
                    {
                        taskCoords[threadId] = (x+offsetX, y+offsetY, z+offsetZ);
                        threadId++;
                    }
                }
            }
            
            List<Func<Task>> tasks = taskCoords
                .Select(coords => (Func<Task>)(() => ChunkStepTask(coords.Item1,coords.Item2, coords.Item3)))
                .ToList();

            await RunTasks(tasks);
        }
    }
    
    static async Task RunTasks(List<Func<Task>> taskFuncs)
    {
        var tasks = taskFuncs.Select(f => f()).ToArray();
        await Task.WhenAll(tasks);
    }
}