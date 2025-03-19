using System.Numerics;

namespace BioSim.Datastructures;

/// <summary>
/// A chunk is the term for a bucket when grouping based off of position.
/// It's only function is holding references of organisms in World and looping through its (smaller) group of organisms as to exclude irrelevant organisms.
/// </summary>
public class Chunk {
    
    //An array that gets reused is faster than a linked list getting clear every frame, as resetting and overwriting never calls the garbage collector
    //When using a linked list, it is accompanied by a LinkedListNode<T> which does get garbage collected after an element is removed
    public Organism[] Organisms { get; }
    private int count = 0;

    public Chunk()
    {
        //Currently hardcoded to 40 as that should be an upper bound for a normally sized chunk,
        // but should look into finding a dynamic upper bound based off of size
        Organisms = new Organism[40];
    }
    
    public void Clear()
    {
        count = 0;
    }

    public void Insert(Organism organism)
    {
        //Organisms.AddLast(organism);
        Organisms[count] = organism;
        count++;
    }

    public bool CheckCollision(Organism organism, Vector3 position)
    {
        for (int i = 0; i < count; i++)
        {
            Organism otherOrganism = Organisms[i];
            
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