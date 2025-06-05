using System.Diagnostics.Contracts;
using System.Numerics;

namespace BioSim.Datastructures;

public class MultithreadedChunk3DFixedDataStructure : DataStructure
{
    protected Chunk3D[,,] Chunks;
    protected Vector3 MinPosition;
    protected float ChunkSize;
    protected int ChunkCountX;
    protected int ChunkCountY;
    protected int ChunkCountZ;
    //First array stores the differing groups needed to have sets of chunks that never directly touch eachother,
    // second array stores logical cores that can do tasks, third is the actual chunks that are run
    private Chunk3D[][][] chunkGroupBatches;
    private int groupCount;
    private bool stepping = false;
    
    public MultithreadedChunk3DFixedDataStructure(Vector3 minPosition, Vector3 maxPosition, float chunkSize, float largestOrganismSize, int amountOfLogicalCoresToUse = 0)
    {
        //Chunk setup
        ChunkCountX = (int)Math.Ceiling((maxPosition.X - minPosition.X) / chunkSize);
        ChunkCountY = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / chunkSize);
        ChunkCountZ = (int)Math.Ceiling((maxPosition.Z - minPosition.Z) / chunkSize);
        Chunks = new Chunk3D[ChunkCountX, ChunkCountY, ChunkCountZ];
        MinPosition = minPosition;
        ChunkSize = chunkSize;

        //Create all chunks
        for (int i = 0; i < ChunkCountX; i++)
        {
            for (int j = 0; j < ChunkCountY; j++)
            {
                for (int k = 0; k < ChunkCountZ; k++)
                {
                    Vector3 chunkCenter = minPosition + new Vector3(i, j, k) * chunkSize + new Vector3(chunkSize * 0.5f);
                    Chunks[i, j, k] = new Chunk3D(chunkCenter, chunkSize);
                }
            }
        }
        
        //Get all connected chunks
        for (int i = 0; i < ChunkCountX; i++)
        {
            for (int j = 0; j < ChunkCountY; j++)
            {
                for (int k = 0; k < ChunkCountZ; k++)
                {
                    Chunks[i, j, k].Initialize(GetConnectedChunks(i, j, k));
                }
            }
        }
        
        //Multithreading setup
        (int, int, int)[] offset = [(0, 0, 0), (0, 1, 0), (1, 0, 0), (1, 1, 0), (0, 0, 1), (0, 1, 1), (1, 0, 1), (1, 1, 1)];
        groupCount = offset.Length;
        
        //Round downwards for uneven task counts
        int lowestTaskCount = (int)Math.Floor(ChunkCountX * ChunkCountY * ChunkCountZ / (float)groupCount);
        
        List<Chunk3D>[] chunkGroups = new List<Chunk3D>[groupCount+1];
        
        //First we find all the chunks in a group
        for (int group = 0; group < groupCount; group++)
        {
            chunkGroups[group] = new List<Chunk3D>(lowestTaskCount);
            (int offsetX, int offsetY, int offsetZ) = offset[group];
            
            //All workers are assigned a chunk where every chunk has no direct neighbour that is currently working, meaning we get a grid pattern
            //Note that x and y grow by 2 each loop
            for (int x = 0; x < ChunkCountX; x += 2)
            {
                for (int y = 0; y < ChunkCountY; y += 2)
                {
                    for (int z = 0; z < ChunkCountZ; z += 2)
                    {
                        chunkGroups[group].Add(Chunks[x + offsetX, y + offsetY, z + offsetZ]);
                    }
                }
            }
        } 
        
        //Next we assign them to every logical core (and respect defined amount of cores if done)
        int allowedCores = amountOfLogicalCoresToUse == 0 ? Environment.ProcessorCount : amountOfLogicalCoresToUse;
        chunkGroupBatches = new Chunk3D[groupCount][][];
        for (int group = 0; group < groupCount; group++)
        {
            int index = 0;
            int chunkGroupCount = chunkGroups[group].Count;
            //Note: most physical cores have multiple logical cores (want rounded down task count, so that there is always at least 1 task per logical core)
            int logicalCores = Math.Min(allowedCores, chunkGroupCount);
            chunkGroupBatches[group] = new Chunk3D[logicalCores][];
            for (int core = 0; core < Math.Min(logicalCores, chunkGroupCount); core++)
            {
                //Divide all assigned chunks to a group to different logical cores
                int chunksPerCore = (int)Math.Floor(chunkGroups[group].Count / (float)logicalCores); //Rounded down
                int batchesWithExtraChunk = chunkGroups[group].Count % logicalCores;
                bool extraChunk = batchesWithExtraChunk > core;
                int batchSize = chunksPerCore + (extraChunk ? 1: 0);
                chunkGroupBatches[group][core] = new Chunk3D[batchSize];

                for (int i = 0; i < batchSize; i++)
                {
                    chunkGroupBatches[group][core][i] = chunkGroups[group][index];
                    index++;
                }
            }
        }

        CheckWarnings(largestOrganismSize, allowedCores);
        CheckErrors(largestOrganismSize);
    }
    
    [Pure]
    protected Chunk3D[] GetConnectedChunks(int chunkX, int chunkY, int chunkZ)
    {
        List<Chunk3D> connectedChunks = new List<Chunk3D>(26);
        
        for (int x = -1; x <= 1; x++)
        {
            //Check bounds
            if (chunkX + x < 0 || chunkX + x >= ChunkCountX)
                continue;
            
            for (int y = -1; y <= 1; y++)
            {
                //Check bounds
                if (chunkY + y < 0 || chunkY + y >= ChunkCountY)
                    continue;
                for (int z = -1; z <= 1; z++)
                {
                    //Check bounds
                    if (chunkZ + z < 0 || chunkZ + z >= ChunkCountZ)
                        continue;
                    
                    //Don't add self
                    if (x == 0 && y == 0 && z == 0)
                        continue;
                    
                    connectedChunks.Add(Chunks[chunkX+x,chunkY+y, chunkZ+z]);
                }
            }
        }
        
        return connectedChunks.ToArray();
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
            List<Func<Task>> tasks = chunkGroupBatches[group]
                .Select(chunkBatch => (Func<Task>)(() => ChunkStepTask(chunkBatch)))
                .ToList();

            await RunTasks(tasks);
        }

        stepping = false;
    }
    
    private Task ChunkStepTask(Chunk3D[] coordBatch)
    {
        return Task.Run(() =>
        {
            foreach (Chunk3D chunk in coordBatch)
            {
                chunk.Step();
            }
        });
    }
    
    private async Task RunTasks(List<Func<Task>> taskFuncs)
    {
        var tasks = taskFuncs.Select(f => f()).ToArray();
        await Task.WhenAll(tasks);
    }

    public override void AddOrganism(Organism organism)
    {
        (int x, int y, int z) = GetChunk(organism.Position);
        Chunks[x,y,z].DirectlyInsertOrganism(organism);
    }

    public override IEnumerable<Organism> GetOrganisms()
    {
        LinkedList<Organism> organisms = new LinkedList<Organism>();
        foreach (Chunk3D chunk in Chunks)
        {
            for (LinkedListNode<Organism> node = chunk.Organisms.First!; node != null; node = node.Next!)
            {
                Organism organism = node.Value;
                organisms.AddLast(organism);
            }
        }
        
        return organisms;
    }

    public override int GetOrganismCount()
    {
        int organismCount = 0;
        foreach (Chunk3D chunk in Chunks)
        {
            organismCount += chunk.OrganismCount;
        }

        return organismCount;
    }

    private (int, int, int) GetChunk(Vector3 position)
    {
        int chunkX = (int)Math.Floor((position.X - MinPosition.X) / ChunkSize);
        int chunkY = (int)Math.Floor((position.Y - MinPosition.Y) / ChunkSize);
        int chunkZ = (int)Math.Floor((position.Z - MinPosition.Z) / ChunkSize);
        //Math.Min because otherwise can throw error if X,Y, or Z is exactly maxValue
        chunkX = Math.Min(chunkX, ChunkCountX - 1);
        chunkY = Math.Min(chunkY, ChunkCountY - 1);
        chunkZ = Math.Min(chunkZ, ChunkCountZ - 1);
        return (chunkX, chunkY, chunkZ);
    }

    public override bool CheckCollision(Organism organism, Vector3 position)
    {
        (int cX, int cY, int cZ) = GetChunk(organism.Position);
        Chunk3D chunk = Chunks[cX, cY, cZ];
        
        if (!World.IsInBounds(position))
            return true;
        
        //Check for organisms within the chunk
        for (LinkedListNode<Organism> node = chunk.Organisms.First!; node != null; node = node.Next!)
        {
            Organism otherOrganism = node.Value;
            
            if (organism == otherOrganism)
                continue;
            
            //Checks collision by checking distance between circles
            float x = position.X - otherOrganism.Position.X;
            float x2 = x * x;
            float y = position.Y - otherOrganism.Position.Y;
            float y2 = y * y;
            float z = position.Z - otherOrganism.Position.Z;
            float z2 = z * z;
            float sizes = organism.Size + otherOrganism.Size;
            if (x2 + y2 + z2 <= sizes * sizes)
                return true;
        }

        //Check all organisms in neighbouring chunks
        foreach (Chunk3D neighbouringChunk in chunk.ConnectedChunks)
        {
            for (LinkedListNode<Organism> node = neighbouringChunk.Organisms.First!; node != null; node = node.Next!)
            {
                Organism otherOrganism = node.Value;
            
                if (organism == otherOrganism)
                    continue;
            
                //Checks collision by checking distance between circles
                float x = position.X - otherOrganism.Position.X;
                float x2 = x * x;
                float y = position.Y - otherOrganism.Position.Y;
                float y2 = y * y;
                float z = position.Z - otherOrganism.Position.Z;
                float z2 = z * z;
                float sizes = organism.Size + otherOrganism.Size;
                if (x2 + y2 + z2 <= sizes * sizes)
                    return true;
            }
        }

        return false;
    }

    public override Organism ClosestNeighbour(Organism organism)
    {
        throw new NotImplementedException();
    }
    
    #region Warnings and errors

    private void CheckWarnings(float largestOrganismSize, int amountOfCoresBeingUsed)
    {
        if (ChunkSize > largestOrganismSize * 10)
            Console.WriteLine("Warning: Chunk size is rather large, smaller chunk size would improve performance");
        
        if(amountOfCoresBeingUsed == 1)
            Console.WriteLine("Warning: Only 1 logical core in use while using multithreading! Switch to single-threaded datastructure for better performance");
        
        if(Environment.ProcessorCount < amountOfCoresBeingUsed)
            Console.WriteLine($"Warning: More logical cores assigned then available: Requested: {amountOfCoresBeingUsed}, Available: {Environment.ProcessorCount}");
    }

    private void CheckErrors(float largestOrganismSize)
    {
        if (ChunkSize / 2f < largestOrganismSize)
            throw new ArgumentException("Chunk size must be at least twice largest organism size");
    }
    #endregion
}