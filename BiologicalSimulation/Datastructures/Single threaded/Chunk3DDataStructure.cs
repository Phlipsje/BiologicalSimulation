using System.Diagnostics.Contracts;
using System.Numerics;

namespace BioSim.Datastructures;

/// <summary>
/// Uses chunks to separate distant Organisms from each other.
/// Simple and fast, Organisms are stored not globally, but within the chunks themselves.
/// Chunks stored in arrays to make access very quick.
/// 2D version of this exists for groups of organisms that grow mostly in 2 dimensions, such as biofilms
/// </summary>
public class Chunk3DDataStructure : DataStructure
{
    private ExtendedChunk3D[,,] chunks;
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

    public override Task Step()
    {
        foreach (ExtendedChunk3D chunk3D in chunks)
        {
            chunk3D.Step();
        }
        
        return Task.CompletedTask;
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
        Organism[] organisms = new Organism[GetOrganismCount()];
        int i = 0;
        foreach (ExtendedChunk3D chunk in chunks)
        {
            foreach (Organism organism in chunk.Organisms)
            {
                organisms[i] = organism;
                i++;
            }
        }
        
        return organisms;
    }

    public override int GetOrganismCount()
    {
        int organismCount = 0;
        foreach (ExtendedChunk3D chunk3D in chunks)
        {
            organismCount += chunk3D.OrganismCount;
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
        for (LinkedListNode<Organism> node = chunk.Organisms.First!; node != null; node = node.Next!)
        {
            Organism otherOrganism = node.Value;

            if (organism == otherOrganism)
                continue;
            
            //Checks collision by checking distance between spheres
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
        
        //Check for any organisms within neighbouring chunks that are within distance of possibly touching with this
        for (LinkedListNode<Organism> node = chunk.ExtendedCheck.First!; node != null; node = node.Next!)
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

        return false;
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
        foreach (ExtendedChunk3D neighbouringChunk in chunk.ConnectedChunks)
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