using System.Drawing;
using System.Numerics;

namespace BioSim.Datastructures;

/// <summary>
/// A simple data structure that divides the world in chunks, Organisms only check collision within the chunk, and it's direct neighbouring chunks
/// Organisms are re-inserted after every frame
/// This version of this data structure has a maximum size so that the chunks can be stored in a grid (which is a lot faster),
///  the program does not check if the position is supported before inserting.
/// For a version that supports an infinitely sized continuous space, use Chunk3DDataStructure.cs
/// </summary>
public class Chunk3DFixedDataStructure : DataStructure
{
    private Chunk[,,] chunks;
    private Vector3 minPosition;
    private Vector3 chunkSize;
    private float largestOrganismSize;

    public Chunk3DFixedDataStructure(World world, Vector3 minPosition, Vector3 maxPosition, Vector3 chunkSize, float largestOrganismSize) : base(world)
    {
        minPosition = minPosition - chunkSize*2;
        maxPosition = maxPosition + chunkSize*2;
        int chunkCountX = (int)Math.Ceiling((maxPosition.X - minPosition.X) / chunkSize.X);
        int chunkCountY = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / chunkSize.Y);
        int chunkCountZ = (int)Math.Ceiling((maxPosition.Z - minPosition.Z) / chunkSize.Z);
        chunks = new Chunk[chunkCountX, chunkCountY, chunkCountZ];
        this.minPosition = minPosition;
        this.chunkSize = chunkSize;
        this.largestOrganismSize = largestOrganismSize;

        for (int i = 0; i < chunkCountX; i++)
        {
            for (int j = 0; j < chunkCountY; j++)
            {
                for (int k = 0; k < chunkCountZ; k++)
                {
                    chunks[i, j, k] = new Chunk();
                }
            }
        }
    }

    private void FillChunks()
    {
        foreach (Chunk chunk in chunks)
        {
            chunk.Clear();
        }
        
        foreach (Organism organism in World.Organisms)
        {
            InsertIntoChunk(organism);
        }
    }

    private void InsertIntoChunk(Organism organism)
    {
        (int chunkX, int chunkY, int chunkZ) = GetChunk(organism.Position);
        chunks[chunkX,chunkY, chunkZ].Insert(organism);
    }

    private (int, int, int) GetChunk(Vector3 position)
    {
        int chunkX = (int)Math.Ceiling((position.X - minPosition.X) / chunkSize.X);
        int chunkY = (int)Math.Ceiling((position.Y - minPosition.Y) / chunkSize.Y);
        int chunkZ = (int)Math.Ceiling((position.Z - minPosition.Z) / chunkSize.Z);
        return (chunkX, chunkY, chunkZ);
    }

    public override void Step()
    {
        FillChunks();
    }

    public override Organism ClosestNeighbour(Organism organism)
    {
        throw new NotImplementedException();
    }

    public override bool CheckCollision(Organism organism, Vector3 position)
    {
        if (!World.IsInBounds(position))
            return true;
        
        (int chunkX, int chunkY, int chunkZ) = GetChunk(position);
        
        if(chunks[chunkX, chunkY, chunkZ].CheckCollision(organism, position))
            return true;
        
        //If no collision in the chunk the organism is in, then check surrounding possible chunks
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    if (i == 0 && j == 0 && k == 0)
                        continue;
                
                    if (chunks[chunkX+i, chunkY+j, chunkZ+k].CheckCollision(organism, position))
                        return true;
                }
            }
        }

        return false;
    }

    protected override IEnumerator<IOrganism> ToEnumerator()
    {
        return World.Organisms.GetEnumerator();
    }
}