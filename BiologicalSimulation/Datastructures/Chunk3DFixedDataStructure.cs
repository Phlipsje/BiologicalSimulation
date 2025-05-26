using System.Diagnostics.Contracts;
using System.Numerics;

namespace BioSim.Datastructures;

public class Chunk3DFixedDataStructure : DataStructure
{
    private Chunk3D[,,] chunks;
    private Vector3 minPosition;
    private float chunkSize;
    private int chunkCountX;
    private int chunkCountY;
    private int chunkCountZ;
    
    public Chunk3DFixedDataStructure(World world, Vector3 minPosition, Vector3 maxPosition, float chunkSize, float largestOrganismSize) : base(world)
    {
        chunkCountX = (int)Math.Ceiling((maxPosition.X - minPosition.X) / chunkSize);
        chunkCountY = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / chunkSize);
        chunkCountZ = (int)Math.Ceiling((maxPosition.Z - minPosition.Z) / chunkSize);
        chunks = new Chunk3D[chunkCountX, chunkCountY, chunkCountZ];
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

    public override Organism ClosestNeighbour(Organism organism)
    {
        throw new NotImplementedException();
    }

    public override bool CheckCollision(Organism organism, Vector3 position)
    {
        throw new NotImplementedException();
    }

    public override void AddOrganism(Organism organism)
    {
        (int x, int y, int z) = GetChunk(organism.Position);
        chunks[x,y,z].CheckToBeAdded.Enqueue(organism);
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

    protected override IEnumerator<IOrganism> ToEnumerator()
    {
        throw new NotImplementedException();
    }
}