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

    public override Task Step()
    {
        foreach (ExtendedChunk2D chunk2D in chunks)
        {
            chunk2D.Step();
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
            for (LinkedListNode<Organism> node = chunk.Organisms.First!; node != null; node = node.Next!)
            {
                Organism organism = node.Value;
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
        foreach (ExtendedChunk2D neighbouringChunk in chunk.ConnectedChunks)
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