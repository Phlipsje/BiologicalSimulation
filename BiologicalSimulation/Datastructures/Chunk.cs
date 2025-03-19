using System.Numerics;

namespace BioSim.Datastructures;

/// <summary>
/// A chunk is the term for a bucket when grouping based off of position.
/// It's only function is holding references of organisms in World and looping through its (smaller) group of organisms as to exclude irrelevant organisms.
/// </summary>
public class Chunk {
    
    public LinkedList<Organism> Organisms { get; }

    public Chunk()
    {
        Organisms = new LinkedList<Organism>();
    }
    
    public void Clear()
    {
        Organisms.Clear();
    }

    public void Insert(Organism organism)
    {
        Organisms.AddLast(organism);
    }

    public bool CheckCollision(Organism organism, Vector3 position)
    {
        foreach (Organism otherOrganism in Organisms)
        {
            //Cannot be a collision with itself
            if(otherOrganism == organism)
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

        //If we reach this, then no collision
        return false;
    }
}