using System.Diagnostics.Contracts;
using System.Numerics;

namespace BioSim.Datastructures;

public class MultithreadedChunk2DDataStructure : DataStructure
{
    public override bool IsMultithreaded { get; } = true;

    private Chunk2D[,] chunks;
    private Vector2 minPosition;
    private float chunkSize;
    private int chunkCountX;
    private int chunkCountY;
    //First array stores the differing groups needed to have sets of chunks that never directly touch eachother,
    // second array stores logical cores that can do tasks, third is the actual chunks that are run
    private Chunk2D[][][] chunkGroupBatches;
    private int groupCount;
    private bool stepping = false;
    
    public MultithreadedChunk2DDataStructure(Vector2 minPosition, Vector2 maxPosition, float chunkSize, float largestOrganismSize, int amountOfLogicalCoresToUse = 0)
    {
        //Chunk setup
        chunkCountX = (int)Math.Ceiling((maxPosition.X - minPosition.X) / chunkSize);
        chunkCountY = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / chunkSize);
        chunks = new Chunk2D[chunkCountX, chunkCountY];
        this.minPosition = minPosition;
        this.chunkSize = chunkSize;

        //Create all chunks
        for (int i = 0; i < chunkCountX; i++)
        {
            for (int j = 0; j < chunkCountY; j++)
            {
                Vector2 chunkCenter = minPosition + new Vector2(i, j) * chunkSize + new Vector2(chunkSize*0.5f);
                chunks[i, j] = new Chunk2D(chunkCenter, chunkSize);
            }
        }
        
        //Get all connected chunks
        for (int i = 0; i < chunkCountX; i++)
        {
            for (int j = 0; j < chunkCountY; j++)
            {
                chunks[i, j].Initialize(GetConnectedChunks(i, j));
            }
        }
        
        //Multithreading setup
        (int, int)[] offset = [(0, 0), (0, 1), (1, 0), (1, 1)];
        groupCount = offset.Length;
        
        //Round downwards for uneven task counts
        int lowestTaskCount = (int)Math.Floor(chunkCountX * chunkCountY / (float)groupCount);
        
        List<Chunk2D>[] chunkGroups = new List<Chunk2D>[groupCount+1];
        
        //First we find all the chunks in a group
        for (int group = 0; group < groupCount; group++)
        {
            chunkGroups[group] = new List<Chunk2D>(lowestTaskCount);
            (int offsetX, int offsetY) = offset[group];
            
            //All workers are assigned a chunk where every chunk has no direct neighbour that is currently working, meaning we get a grid pattern
            //Note that x and y grow by 2 each loop
            for (int x = 0; x < chunkCountX; x += 2)
            {
                for (int y = 0; y < chunkCountY; y += 2)
                {
                    chunkGroups[group].Add(chunks[x + offsetX, y + offsetY]);
                }
            }
        } 
        
        //Next we assign them to every logical core
        //Next we assign them to every logical core (and respect defined amount of cores if done)
        int allowedCores = amountOfLogicalCoresToUse == 0 ? Environment.ProcessorCount : amountOfLogicalCoresToUse;
        chunkGroupBatches = new Chunk2D[groupCount][][];
        for (int group = 0; group < groupCount; group++)
        {
            int index = 0;
            int chunkGroupCount = chunkGroups[group].Count;
            //Note: most physical cores have multiple logical cores (want rounded down task count, so that there is always at least 1 task per logical core)
            int logicalCores = Math.Min(allowedCores, chunkGroupCount);
            chunkGroupBatches[group] = new Chunk2D[logicalCores][];
            for (int core = 0; core < Math.Min(logicalCores, chunkGroupCount); core++)
            {
                //Divide all assigned chunks to a group to different logical cores
                int chunksPerCore = (int)Math.Floor(chunkGroups[group].Count / (float)logicalCores); //Rounded down
                int batchesWithExtraChunk = chunkGroups[group].Count % logicalCores;
                bool extraChunk = batchesWithExtraChunk > core;
                int batchSize = chunksPerCore + (extraChunk ? 1: 0);
                chunkGroupBatches[group][core] = new Chunk2D[batchSize];

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
    private Chunk2D[] GetConnectedChunks(int chunkX, int chunkY)
    {
        List<Chunk2D> connectedChunks = new List<Chunk2D>(8);
        
        for (int x = -1; x <= 1; x++)
        {
            //Check bounds
            if (chunkX + x < 0 || chunkX + x >= chunkCountX)
                continue;
            
            for (int y = -1; y <= 1; y++)
            {
                //Check bounds
                if (chunkY + y < 0 || chunkY + y >= chunkCountY)
                    continue;
                
                //Don't add self
                if (x == 0 && y == 0)
                    continue;
                    
                connectedChunks.Add(chunks[chunkX+x,chunkY+y]);
            }
        }
        
        return connectedChunks.ToArray();
    }
    
    public override async Task Step()
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
    
    private Task ChunkStepTask(Chunk2D[] coordBatch)
    {
        return Task.Run(() =>
        {
            foreach (Chunk2D chunk in coordBatch)
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

    public override Task Clear()
    {
        foreach (Chunk2D chunk in chunks)
        {
            chunk.Organisms.Clear();
        }
        return Task.CompletedTask;
    }

    public override void AddOrganism(Organism organism)
    {
        (int x, int y) = GetChunk(organism.Position);
        chunks[x,y].DirectlyInsertOrganism(organism);
    }
    
    public override bool RemoveOrganism(Organism organism)
    {
        (int x, int y) = GetChunk(organism.Position);
        return chunks[x, y].Organisms.Remove(organism);
    }

    public override Task GetOrganisms(out IEnumerable<Organism> organisms)
    {
        LinkedList<Organism> o = new LinkedList<Organism>();
        foreach (Chunk2D chunk in chunks)
        {
            for (LinkedListNode<Organism> node = chunk.Organisms.First!; node != null; node = node.Next!)
            {
                Organism organism = node.Value;
                o.AddLast(organism);
            }
        }

        organisms = o;
        return Task.CompletedTask;
    }

    public override Task GetOrganismCount(out int count)
    {
        int organismCount = 0;
        foreach (Chunk2D chunk2D in chunks)
        {
            organismCount += chunk2D.OrganismCount;
        }

        count = organismCount;
        return Task.CompletedTask;
    }

    private (int, int) GetChunk(Vector3 position)
    {
        int chunkX = (int)Math.Floor((position.X - minPosition.X) / chunkSize);
        int chunkY = (int)Math.Floor((position.Y - minPosition.Y) / chunkSize);
        //Math.Min because otherwise can throw error if X,Y, or Z is exactly maxValue
        chunkX = Math.Min(chunkX, chunkCountX - 1);
        chunkY = Math.Min(chunkY, chunkCountY - 1);
        return (chunkX, chunkY);
    }

    public override bool CheckCollision(Organism organism, Vector3 position)
    {
        (int cX, int cY) = GetChunk(organism.Position);
        Chunk2D chunk = chunks[cX, cY];
        
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
        foreach (Chunk2D neighbouringChunk in chunk.ConnectedChunks)
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
    
    public override bool FindFirstCollision(Organism organism, Vector3 normalizedDirection, float length, out float t)
    {
        t = float.MaxValue;
        
        if (!World.IsInBounds(organism.Position + normalizedDirection * length))
        {
            //Still block movement normally upon hitting world limit
            t = 0;
            return true;
        }
        
        (int cX, int cY) = GetChunk(organism.Position);
        Chunk2D chunk = chunks[cX, cY];

        //Check within own chunk
        for (LinkedListNode<Organism> node = chunk.Organisms.First!; node != null; node = node.Next!)
        {
            Organism otherOrganism = node.Value;

            if (RayIntersects(organism.Position, normalizedDirection, length, otherOrganism.Position,
                    organism.Size + otherOrganism.Size, out float tHit))
            {
                if (tHit < t)
                    t = tHit;
            }
        }
        
        //Check edges of other chunks
        foreach (Chunk2D neighbouringChunk in chunk.ConnectedChunks)
        {
            for (LinkedListNode<Organism> node = neighbouringChunk.Organisms.First!; node != null; node = node.Next!)
            {
                Organism otherOrganism = node.Value;

                if (RayIntersects(organism.Position, normalizedDirection, length, otherOrganism.Position,
                        organism.Size + otherOrganism.Size, out float tHit))
                {
                    if (tHit < t)
                        t = tHit;
                }
            } 
        }
        

        //Return if there even was a collision
        return Math.Abs(t - float.MaxValue) > 1f;
    }

    /// <summary>
    /// Returns the closest neighbour within reason: will return nothing if there is no Organism within this and all neighbouring chunks
    /// </summary>
    /// <param name="organism"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override Organism? NearestNeighbour(Organism organism)
    {
        (int cX, int cY) = GetChunk(organism.Position);
        Chunk2D chunk = chunks[cX, cY];
        
        float closestSquareDistance = 9999999999999f;
        Organism? knownNearest = null;
        
        //Check for organisms within the chunk
        for (LinkedListNode<Organism> node = chunk.Organisms.First!; node != null; node = node.Next!)
        {
            Organism otherOrganism = node.Value;
            
            if (organism == otherOrganism)
                continue;
            
            float distanceSquared = Vector3.DistanceSquared(organism.Position, otherOrganism.Position);
            if (distanceSquared < closestSquareDistance)
            {
                closestSquareDistance = distanceSquared;
                knownNearest = otherOrganism;
            }
        }

        //Check all organisms in neighbouring chunks
        foreach (Chunk2D neighbouringChunk in chunk.ConnectedChunks)
        {
            for (LinkedListNode<Organism> node = neighbouringChunk.Organisms.First!; node != null; node = node.Next!)
            {
                Organism otherOrganism = node.Value;
            
                if (organism == otherOrganism)
                    continue;
            
                float distanceSquared = Vector3.DistanceSquared(organism.Position, otherOrganism.Position);
                if (distanceSquared < closestSquareDistance)
                {
                    closestSquareDistance = distanceSquared;
                    knownNearest = otherOrganism;
                }
            }
        }
        
        return knownNearest;
    }

    public override IEnumerable<Organism> OrganismsWithinRange(Organism organism, float range)
    {
        throw new NotSupportedException();
    }
    
    #region Warnings and errors

    private void CheckWarnings(float largestOrganismSize, int amountOfCoresBeingUsed)
    {
        if (chunkSize > largestOrganismSize * 10)
            Console.WriteLine("Warning: Chunk size is rather large, smaller chunk size would improve performance");
        
        if(amountOfCoresBeingUsed == 1)
            Console.WriteLine("Warning: Only 1 logical core in use while using multithreading! Switch to single-threaded datastructure for better performance");
        
        if(Environment.ProcessorCount < amountOfCoresBeingUsed)
            Console.WriteLine($"Warning: More logical cores assigned then available: Requested: {amountOfCoresBeingUsed}, Available: {Environment.ProcessorCount}");
    }

    private void CheckErrors(float largestOrganismSize)
    {
        if (chunkSize / 2f < largestOrganismSize)
            throw new ArgumentException("Chunk size must be at least twice largest organism size");
    }
    #endregion
}