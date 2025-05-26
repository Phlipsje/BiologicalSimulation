using System.Collections;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace BioSim.Datastructures.Datastructures;

public class Chunk2DFixedDataStructure : DataStructure
{
    private Chunk2D[,] chunks;
    private Vector2 minPosition;
    private float chunkSize;
    private int chunkCountX;
    private int chunkCountY;
    
    public Chunk2DFixedDataStructure(World world, Vector2 minPosition, Vector2 maxPosition, float chunkSize, float largestOrganismSize) : base(world)
    {
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
                chunks[i, j] = new Chunk2D(chunkCenter, chunkSize, largestOrganismSize);
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

    public override void Step()
    {
        foreach (Chunk2D chunk2D in chunks)
        {
            chunk2D.Step();
        }
    }

    public override void AddOrganism(Organism organism)
    {
        (int x, int y) = GetChunk(organism.Position);
        chunks[x,y].CheckToBeAdded.Enqueue(organism);
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