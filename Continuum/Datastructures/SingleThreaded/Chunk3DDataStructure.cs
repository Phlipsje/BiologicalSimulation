using System.Diagnostics.Contracts;
using System.Numerics;

namespace Continuum.Datastructures.SingleThreaded;

/// <summary>
/// Uses chunks to separate distant Organisms from each other.
/// Simple and fast, Organisms are stored not globally, but within the chunks themselves.
/// Chunks stored in arrays to make access very quick.
/// 2D version of this exists for groups of organisms that grow mostly in 2 dimensions, such as biofilms
/// </summary>
public class Chunk3DDataStructure : SingleThreadedDataStructure
{
    public override bool IsMultithreaded { get; } = false;
    
    private ExtendedChunk3D[,,] chunks;
    private ExtendedChunk3D[] chunkExecutionOrder;
    private Vector3 minPosition;
    private float chunkSize;
    private int chunkCountX;
    private int chunkCountY;
    private int chunkCountZ;
    
    public Chunk3DDataStructure(Vector3 minPosition, Vector3 maxPosition, float chunkSize, float largestOrganismSize)
    {
        chunkCountX = (int)Math.Ceiling((maxPosition.X - minPosition.X) / chunkSize);
        chunkCountY = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / chunkSize);
        chunkCountZ = (int)Math.Ceiling((maxPosition.Z - minPosition.Z) / chunkSize);
        chunks = new ExtendedChunk3D[chunkCountX, chunkCountY, chunkCountZ];
        chunkExecutionOrder = new ExtendedChunk3D[chunkCountX * chunkCountY * chunkCountZ];
        this.minPosition = minPosition;
        this.chunkSize = chunkSize;

        //Create all chunks
        for (int i = 0; i < chunkCountX; i++)
        {
            for (int j = 0; j < chunkCountY; j++)
            {
                for (int k = 0; k < chunkCountZ; k++)
                {
                    Vector3 chunkCenter = minPosition + new Vector3(i, j, k) * chunkSize + new Vector3(chunkSize*0.5f);
                    chunks[i, j, k] = new ExtendedChunk3D(chunkCenter, chunkSize, largestOrganismSize);
                }
            }
        }
        
        //Get all connected chunks
        for (int i = 0; i < chunkCountX; i++)
        {
            for (int j = 0; j < chunkCountY; j++)
            {
                for (int k = 0; k < chunkCountZ; k++)
                {
                    chunks[i, j, k].Initialize(GetConnectedChunks(i, j, k));
                    chunkExecutionOrder[k*chunkCountX*chunkCountY + j*chunkCountX + i] = chunks[i, j, k];
                }
            }
        }
        
        CheckWarnings(largestOrganismSize);
        CheckErrors(largestOrganismSize);
    }

    [Pure]
    private ExtendedChunk3D[] GetConnectedChunks(int chunkX, int chunkY, int chunkZ)
    {
        List<ExtendedChunk3D> connectedChunks = new List<ExtendedChunk3D>(26);
        
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
                
                for (int z = -1; z <= 1; z++)
                {
                    //Check bounds
                    if (chunkZ + z < 0 || chunkZ + z >= chunkCountZ)
                        continue;

                    //Don't add self
                    if (x == 0 && y == 0 && z == 0)
                        continue;
                    
                    connectedChunks.Add(chunks[chunkX+x,chunkY+y,chunkZ+z]);
                }
            }
        }
        
        return connectedChunks.ToArray();
    }

    public override void Initialize()
    {
        foreach (ExtendedChunk3D chunk in chunks)
        {
            chunk.World = World;
        }
    }

    public override void Step()
    {
        if(World.RandomisedExecutionOrder)
            HelperFunctions.KnuthShuffle(chunkExecutionOrder);
        
        foreach (ExtendedChunk3D chunk3D in chunks)
        {
            chunk3D.Step();
        }
    }
    
    public override void Clear()
    {
        foreach (ExtendedChunk3D chunk in chunks)
        {
            chunk.Organisms.Clear();
        }
    }


    public override void AddOrganism(Organism organism)
    {
        (int x, int y, int z) = GetChunk(organism.Position);
        chunks[x,y,z].DirectlyInsertOrganism(organism);
    }
    
    public override bool RemoveOrganism(Organism organism)
    {
        (int x, int y, int z) = GetChunk(organism.Position);
        return chunks[x, y, z].Organisms.Remove(organism);
    }

    public override IEnumerable<Organism> GetOrganisms()
    {
        LinkedList<Organism> organisms = new LinkedList<Organism>();
        foreach (ExtendedChunk3D chunk in chunks)
        {
            foreach (Organism organism in chunk.Organisms)
            {
                organisms.AddLast(organism);
            }
        }

        return organisms;
    }

    public override int GetOrganismCount()
    {
        int organismCount = 0;
        foreach (ExtendedChunk3D chunk2D in chunks)
        {
            organismCount += chunk2D.Organisms.Count;
        }

        return organismCount;
    }

    private (int, int, int) GetChunk(Vector3 position)
    {
        int chunkX = (int)Math.Floor((position.X - minPosition.X) / chunkSize);
        int chunkY = (int)Math.Floor((position.Y - minPosition.Y) / chunkSize);
        int chunkZ = (int)Math.Floor((position.Z - minPosition.Z) / chunkSize);
        //Math.Min because otherwise can throw error if X,Y, or Z is exactly maxValue
        chunkX = Math.Min(chunkX, chunkCountX - 1);
        chunkY = Math.Min(chunkY, chunkCountY - 1);
        chunkZ = Math.Min(chunkZ, chunkCountZ - 1);
        return (chunkX, chunkY, chunkZ);
    }
    
    public override bool CheckCollision(Organism organism, Vector3 position)
    {
        (int cX, int cY, int cZ) = GetChunk(organism.Position);
        ExtendedChunk3D chunk = chunks[cX, cY, cZ];
        
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
        t = float.MaxValue;
        bool hit = false;
        
        if (!World.IsInBounds(organism.Position + normalizedDirection * length))
        {
            //Still block movement normally upon hitting world limit
            t = 0;
            return true;
        }
        
        (int cX, int cY, int cZ) = GetChunk(organism.Position);
        ExtendedChunk3D chunk = chunks[cX, cY, cZ];

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
        (int cX, int cY, int cZ) = GetChunk(organism.Position);
        ExtendedChunk3D chunk = chunks[cX, cY, cZ];
        
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
        foreach (ExtendedChunk3D neighbouringChunk in chunk.ConnectedChunks)
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
        foreach (ExtendedChunk3D chunk3D in chunks)
        {
            if (Vector3.DistanceSquared(organism.Position, chunk3D.Center) <=
                (range + chunk3D.HalfDimension) * (range + chunk3D.HalfDimension))
            {
                foreach (Organism otherOrganism in chunk3D.Organisms)
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