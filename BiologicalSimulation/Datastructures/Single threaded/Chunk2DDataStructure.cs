using System.Collections;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace BioSim.Datastructures.Datastructures;

/// <summary>
/// Uses chunks to separate distant Organisms from each other.
/// Simple and fast, Organisms are stored not globally, but within the chunks themselves.
/// Chunks stored in arrays to make access very quick.
/// 3D version of this exists for more general usecase
/// </summary>
public class Chunk2DDataStructure : DataStructure
{
    public override bool IsMultithreaded { get; } = false;
    
    private ExtendedChunk2D[,] chunks;
    private Vector2 minPosition;
    private float chunkSize;
    private int chunkCountX;
    private int chunkCountY;
    
    public Chunk2DDataStructure(Vector2 minPosition, Vector2 maxPosition, float chunkSize, float largestOrganismSize)
    {
        chunkCountX = (int)Math.Ceiling((maxPosition.X - minPosition.X) / chunkSize);
        chunkCountY = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / chunkSize);
        chunks = new ExtendedChunk2D[chunkCountX, chunkCountY];
        this.minPosition = minPosition;
        this.chunkSize = chunkSize;

        //Create all chunks
        for (int i = 0; i < chunkCountX; i++)
        {
            for (int j = 0; j < chunkCountY; j++)
            {
                Vector2 chunkCenter = minPosition + new Vector2(i, j) * chunkSize + new Vector2(chunkSize*0.5f);
                chunks[i, j] = new ExtendedChunk2D(chunkCenter, chunkSize, largestOrganismSize);
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
        
        CheckWarnings(largestOrganismSize);
        CheckErrors(largestOrganismSize);
    }

    [Pure]
    private ExtendedChunk2D[] GetConnectedChunks(int chunkX, int chunkY)
    {
        List<ExtendedChunk2D> connectedChunks = new List<ExtendedChunk2D>(8);
        
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
    
    public override void Initialize()
    {
        foreach (ExtendedChunk2D chunk in chunks)
        {
            chunk.World = World;
        }
    }

    public override Task Step()
    {
        foreach (ExtendedChunk2D chunk2D in chunks)
        {
            chunk2D.Step();
        }

        return Task.CompletedTask;
    }
    
    public override Task Clear()
    {
        foreach (ExtendedChunk2D chunk in chunks)
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
        foreach (ExtendedChunk2D chunk in chunks)
        {
            foreach (Organism organism in chunk.Organisms)
            {
                o.AddLast(organism);
            }
        }

        organisms = o;
        return Task.CompletedTask;
    }

    public override Task GetOrganismCount(out int count)
    {
        int organismCount = 0;
        foreach (ExtendedChunk2D chunk2D in chunks)
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
        ExtendedChunk2D chunk = chunks[cX, cY];
        
        if (!World.IsInBounds(position))
            return true;
        
        //Check for organisms within the chunk
        if(organism.CheckCollision(position, chunk.Organisms))
            return true;
        
        //Check for any organisms within neighbouring chunks that are within distance of possibly touching with this
        return organism.CheckCollision(position, chunk.ExtendedCheck);
    }
    
    public override bool FindFirstCollision(Organism organism, Vector3 normalizedDirection, float length, out float t)
    {
        if (!World.IsInBounds(organism.Position + normalizedDirection * length))
        {
            //Still block movement normally upon hitting world limit
            t = 0;
            return true;
        }
        
        t = float.MaxValue;
        bool hit = false;
        
        (int cX, int cY) = GetChunk(organism.Position);
        ExtendedChunk2D chunk = chunks[cX, cY];

        //Check within own chunk
        if (FindMinimumIntersection(organism, normalizedDirection, length, chunk.Organisms, out float hitT1))
        {
            if (hitT1 < t)
            {
                t = hitT1;
                hit = true;
            }
        }
        
        //Check edges of other chunks
        if (FindMinimumIntersection(organism, normalizedDirection, length, chunk.ExtendedCheck, out float hitT2))
        {
            if(hitT2 < t)
                t = hitT2;
        }
        
        float epsilon = 0.01f;
        t -= epsilon;
        
        //Return if there even was a collision
        return hit;
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
        ExtendedChunk2D chunk = chunks[cX, cY];
        
        float closestSquareDistance = 9999999999999f;
        Organism? knownNearest = null;
        
        //Check for organisms within the chunk
        foreach (Organism otherOrganism in chunk.Organisms)
        {
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
        foreach (ExtendedChunk2D neighbouringChunk in chunk.ConnectedChunks)
        {
            foreach (Organism otherOrganism in neighbouringChunk.Organisms)
            {
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
        List<Organism> organismsWithinRange = new List<Organism>(50);
        foreach (ExtendedChunk2D chunk2D in chunks)
        {
            if (Vector2.DistanceSquared(new Vector2(organism.Position.X, organism.Position.Y), chunk2D.Center) <=
                (range + chunk2D.HalfDimension) * (range + chunk2D.HalfDimension))
            {
                foreach (Organism otherOrganism in chunk2D.Organisms)
                {
                    if (Vector3.DistanceSquared(organism.Position, otherOrganism.Position) <= range * range)
                    {
                        organismsWithinRange.Add(otherOrganism);
                    }
                }
            }
        }

        return organismsWithinRange;
    }
    
    #region Warnings and errors

    private void CheckWarnings(float largestOrganismSize)
    {
        if (chunkSize > largestOrganismSize * 10)
            Console.WriteLine("Warning: Chunk size is rather large, smaller chunk size would improve performance");
    }

    private void CheckErrors(float largestOrganismSize)
    {
        if (chunkSize / 2f < largestOrganismSize)
            throw new ArgumentException("Chunk size must be at least twice largest organism size");
    }
    #endregion
}