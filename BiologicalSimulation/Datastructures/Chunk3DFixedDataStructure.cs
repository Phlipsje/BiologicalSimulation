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
    private Chunk3D[,,] chunks;
    private Vector3 minPosition;
    private float chunkSize;
    private int chunkCountX;
    private int chunkCountY;
    private int chunkCountZ;
    private int organismCount;
    
    public Chunk3DFixedDataStructure(Vector3 minPosition, Vector3 maxPosition, float chunkSize, float largestOrganismSize)
    {
        chunkCountX = (int)Math.Ceiling((maxPosition.X - minPosition.X) / chunkSize);
        chunkCountY = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / chunkSize);
        chunkCountZ = (int)Math.Ceiling((maxPosition.Z - minPosition.Z) / chunkSize);
        chunks = new Chunk3D[chunkCountX, chunkCountY, chunkCountZ];
        this.minPosition = minPosition;
        this.chunkSize = chunkSize;
        organismCount = 0;

        //Create all chunks
        for (int i = 0; i < chunkCountX; i++)
        {
            for (int j = 0; j < chunkCountY; j++)
            {
                for (int k = 0; k < chunkCountZ; k++)
                {
                    Vector3 chunkCenter = minPosition + new Vector3(i, j, k) * chunkSize + new Vector3(chunkSize*0.5f);
                    chunks[i, j, k] = new Chunk3D(chunkCenter, chunkSize, largestOrganismSize);
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
    }

    [Pure]
    private Chunk3D[] GetConnectedChunks(int chunkX, int chunkY, int chunkZ)
    {
        List<Chunk3D> connectedChunks = new List<Chunk3D>(26);
        
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

    public override void Step()
    {
        foreach (Chunk3D chunk3D in chunks)
        {
            chunk3D.Step();
        }
    }

    public override void AddOrganism(Organism organism)
    {
        (int x, int y, int z) = GetChunk(organism.Position);
        chunks[x,y,z].DirectlyInsertOrganism(organism);
        organismCount++;
    }

    public override IEnumerable<Organism> GetOrganisms()
    {
        Organism[] organisms = new Organism[organismCount];
        int i = 0;
        foreach (Chunk3D chunk in chunks)
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
    
    public override bool CheckCollision(Organism organism, Vector3 position, List<LinkedList<Organism>> organismLists)
    {
        LinkedList<Organism> collideableOrganisms = organismLists[0];
        LinkedList<Organism> collideableExtendedOrganisms = organismLists[1];
        
        if (!World.IsInBounds(position))
            return true;
        
        //Check for organisms within the chunk
        foreach (Organism otherOrganism in collideableOrganisms)
        {
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
        foreach (Organism otherOrganism in collideableExtendedOrganisms)
        {
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
}