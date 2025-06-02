using System.Diagnostics.Contracts;
using System.Numerics;
using System.Reflection.PortableExecutable;
using BioSim.Datastructures.Datastructures;

namespace BioSim.Datastructures;

//TODO currently just copy pasted, still need to actually implement
//TODO write explanation here
public class Multithreaded2DFixedDataStructure : Chunk2DFixedDataStructure
{
    private int taskCount;
    private ExtendedChunk2D[][] chunkGroups;
    
    public Multithreaded2DFixedDataStructure(Vector2 minPosition, Vector2 maxPosition, float chunkSize, float largestOrganismSize) : base(minPosition, maxPosition, chunkSize, largestOrganismSize, true)
    {
        //TODO this only works if exactly set of 4, change later
        taskCount = ChunkCountX * ChunkCountY / 4;
        
        chunkGroups = new ExtendedChunk2D[4][];
        chunkGroups[0] = new ExtendedChunk2D[taskCount];
        
        (int, int)[] offset = [(0, 0), (0, 1), (1, 0), (1, 1)];
        for (int quadrant = 0; quadrant < 4; quadrant++)
        {
            chunkGroups[quadrant] = new ExtendedChunk2D[taskCount];
            (int offsetX, int offsetY) = offset[quadrant];
            
            int threadId = 0;
            //All workers are assigned a chunk where every chunk has no direct neighbour that is currently working, meaning we get a grid pattern
            //Note that x and y grow by 2 each loop
            for (int x = 0; x < ChunkCountX; x += 2)
            {
                for (int y = 0; y < ChunkCountY; y += 2)
                {
                    chunkGroups[quadrant][threadId] = Chunks[x + offsetX, y + offsetY];
                    threadId++;
                }
            }
        }
    }

    public async Task ChunkStepTask(int x, int y)
    {
        await Task.Run(Chunks[x,y].Step);
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
        (int, int)[] offset = [(0, 0), (0, 1), (1, 0), (1, 1)];
        for (int quadrant = 0; quadrant < 4; quadrant++)
        {
            (int, int)[] taskCoords = new (int, int)[taskCount];
            (int offsetX, int offsetY) = offset[quadrant];
            int threadId = 0;
            //All workers are assigned a chunk where every chunk has no direct neighbour that is currently working, meaning we get a grid pattern
            //Note that x and y grow by 2 each loop
            for (int x = 0; x < ChunkCountX; x+=2)
            {
                for (int y = 0; y < ChunkCountY; y+=2)
                {
                    taskCoords[threadId] = (x+offsetX, y+offsetY);
                    threadId++;
                }
            }
            
            List<Func<Task>> tasks = taskCoords
                .Select(coords => (Func<Task>)(() => ChunkStepTask(coords.Item1,coords.Item2)))
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