using System.Numerics;

namespace BioSim.Datastructures;

public class Variant2DMultithreaded : DataStructure
{
    protected Chunk2D[,] Chunks;
    protected Vector2 MinPosition;
    protected float ChunkSize;
    protected int ChunkCountX;
    protected int ChunkCountY;
    private int taskCount;
    private ExtendedChunk2D[][] chunkGroups;
    
    public Variant2DMultithreaded(Vector2 minPosition, Vector2 maxPosition, float chunkSize, float largestOrganismSize, bool multithreaded = false)
    {
        ChunkCountX = (int)Math.Ceiling((maxPosition.X - minPosition.X) / chunkSize);
        ChunkCountY = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / chunkSize);
        Chunks = new Chunk2D[ChunkCountX, ChunkCountY];
        MinPosition = minPosition;
        ChunkSize = chunkSize;

        //Create all chunks
        for (int i = 0; i < ChunkCountX; i++)
        {
            for (int j = 0; j < ChunkCountY; j++)
            {
                Vector2 chunkCenter = minPosition + new Vector2(i, j) * chunkSize + new Vector2(chunkSize*0.5f);
                Chunks[i, j] = new Chunk2D(chunkCenter, chunkSize, largestOrganismSize);
            }
        }
        
        //Get all connected chunks
        for (int i = 0; i < ChunkCountX; i++)
        {
            for (int j = 0; j < ChunkCountY; j++)
            {
                Chunks[i, j].Initialize(GetConnectedChunks(i, j));
            }
        }
        
        CheckWarnings(largestOrganismSize);
        CheckErrors(largestOrganismSize);
    }
    
    public override void Step()
    {
        throw new NotImplementedException();
    }

    public override void AddOrganism(Organism organism)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Organism> GetOrganisms()
    {
        throw new NotImplementedException();
    }

    public override int GetOrganismCount()
    {
        throw new NotImplementedException();
    }

    public override bool CheckCollision(Organism organism, Vector3 position, List<LinkedList<Organism>> organismLists)
    {
        throw new NotImplementedException();
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