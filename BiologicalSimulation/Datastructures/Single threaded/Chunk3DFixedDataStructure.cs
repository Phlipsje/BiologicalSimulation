using System.Diagnostics.Contracts;
using System.Numerics;

namespace BioSim.Datastructures;

/// <summary>
/// Uses chunks to separate distant Organisms from each other.
/// Simple and fast, Organisms are stored not globally, but within the chunks themselves.
/// Chunks stored in arrays to make access very quick.
/// 2D version of this exists for groups of organisms that grow mostly in 2 dimensions, such as biofilms
/// </summary>
public class Chunk3DFixedDataStructure : DataStructure
{
    protected ExtendedChunk3D[,,] Chunks;
    protected Vector3 MinPosition;
    protected float ChunkSize;
    protected int ChunkCountX;
    protected int ChunkCountY;
    protected int ChunkCountZ;
    
    public Chunk3DFixedDataStructure(Vector3 minPosition, Vector3 maxPosition, float chunkSize, float largestOrganismSize, bool multithreaded = false)
    {
        ChunkCountX = (int)Math.Ceiling((maxPosition.X - minPosition.X) / chunkSize);
        ChunkCountY = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / chunkSize);
        ChunkCountZ = (int)Math.Ceiling((maxPosition.Z - minPosition.Z) / chunkSize);
        Chunks = new ExtendedChunk3D[ChunkCountX, ChunkCountY, ChunkCountZ];
        MinPosition = minPosition;
        ChunkSize = chunkSize;

        //Create all chunks
        for (int i = 0; i < ChunkCountX; i++)
        {
            for (int j = 0; j < ChunkCountY; j++)
            {
                for (int k = 0; k < ChunkCountZ; k++)
                {
                    Vector3 chunkCenter = minPosition + new Vector3(i, j, k) * chunkSize + new Vector3(chunkSize*0.5f);
                    Chunks[i, j, k] = new ExtendedChunk3D(multithreaded, chunkCenter, chunkSize, largestOrganismSize);
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
                    
                    connectedChunks.Add(Chunks[chunkX+x,chunkY+y,chunkZ+z]);
                }
            }
        }
        
        return connectedChunks.ToArray();
    }

    public override void Step()
    {
        foreach (ExtendedChunk3D chunk3D in Chunks)
        {
            chunk3D.Step();
        }
    }

    public override void AddOrganism(Organism organism)
    {
        (int x, int y, int z) = GetChunk(organism.Position);
        Chunks[x,y,z].DirectlyInsertOrganism(organism);
    }

    public override IEnumerable<Organism> GetOrganisms()
    {
        Organism[] organisms = new Organism[GetOrganismCount()];
        int i = 0;
        foreach (ExtendedChunk3D chunk in Chunks)
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
        foreach (ExtendedChunk3D chunk3D in Chunks)
        {
            organismCount += chunk3D.OrganismCount;
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
        (int cX, int cY, int cZ) = GetChunk(position);
        ExtendedChunk3D chunk = Chunks[cX, cY, cZ];
        
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

    public override Organism ClosestNeighbour(Organism organism)
    {
        throw new NotImplementedException();
    }
    
    #region Warnings and errors

    private void CheckWarnings(float largestOrganismSize)
    {
        if (ChunkSize > largestOrganismSize * 10)
            Console.WriteLine("Warning: Chunk size is rather large, smaller chunk size would improve performance");
    }

    private void CheckErrors(float largestOrganismSize)
    {
        if (ChunkSize / 2f < largestOrganismSize)
            throw new ArgumentException("Chunk size must be at least twice largest organism size");
    }
    #endregion
}