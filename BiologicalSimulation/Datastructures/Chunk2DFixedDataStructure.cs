using System.Drawing;
using System.Numerics;

namespace BioSim.Datastructures;

/// <summary>
/// A simple data structure that divides the world in chunks, Organisms only check collision within the chunk, and it's direct neighbouring chunks
/// Organisms are re-inserted after every frame
/// This version of this data structure has a maximum size so that the chunks can be stored in a grid (which is a lot faster),
///  the program does not check if the position is supported before inserting.
/// For a version that supports an infinitely sized continuous space, use Chunk2DDataStructure.cs
/// </summary>
public class Chunk2DFixedDataStructure : DataStructure
{
    private Chunk2D[,] chunks;
    private Vector2 minPosition;
    private Vector2 chunkSize;
    private float largestOrganismSize;
    private IEnumerable<Organism> Organisms => World.Organisms.Concat(World.OrganismsToAdd);

    public Chunk2DFixedDataStructure(World world, Vector2 minPosition, Vector2 maxPosition, Vector2 chunkSize, float largestOrganismSize) : base(world)
    {
        int chunkCountX = (int)Math.Ceiling((maxPosition.X - minPosition.X) / chunkSize.X);
        int chunkCountY = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / chunkSize.Y);
        chunks = new Chunk2D[chunkCountX, chunkCountY];
        this.minPosition = minPosition;
        this.chunkSize = chunkSize;
        this.largestOrganismSize = largestOrganismSize;

        for (int i = 0; i < chunkCountX; i++)
        {
            for (int j = 0; j < chunkCountY; j++)
            {
                chunks[i, j] = new Chunk2D(i*chunkSize.X + minPosition.X, j*chunkSize.Y + minPosition.Y, chunkSize.X, chunkSize.Y);
            }
        }
    }

    private void FillChunks()
    {
        foreach (Chunk2D chunk in chunks)
        {
            chunk.Clear();
        }
        
        foreach (Organism organism in Organisms)
        {
            InsertIntoChunk(organism);
        }
    }

    private void InsertIntoChunk(Organism organism)
    {
        (int chunkX, int chunkY) = GetChunk(organism.Position);
        chunks[chunkX,chunkY].Insert(organism);
    }

    private (int, int) GetChunk(Vector3 position)
    {
        int chunkX = (int)Math.Ceiling((position.X - minPosition.X) / chunkSize.X);
        int chunkY = (int)Math.Ceiling((position.X - minPosition.X) / chunkSize.X);
        return (chunkX, chunkY);
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
        (int chunkX, int chunkY) = GetChunk(position);
        
        if(chunks[chunkX, chunkY].CheckCollision(organism, position, World))
            return true;
        
        //If no collision in the chunk the organism is in, then check surrounding possible chunks
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                
                if (chunks[chunkX+i, chunkY+j].CheckCollision(organism, position, World))
                    return true;
            }
        }

        return false;
    }

    protected override IEnumerator<IOrganism> ToEnumerator()
    {
        return Organisms.GetEnumerator();
    }
}