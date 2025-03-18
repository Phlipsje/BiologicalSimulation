using System.Numerics;

namespace BioSim.Datastructures;

/// <summary>
/// A chunk that has infinite length in the 3rd dimension, thus it only selects based off of 2D location
/// This is a simpler type of chunk to make and is more optimal for flat growth patterns like Biofilms.
/// </summary>
public class Chunk2D {
    public float X { get; private set; }
    public float Y { get; private set; }
    public float Width { get; private set; }
    public float Height { get; private set; }
    
    public LinkedList<Organism> Organisms { get; }

    public Chunk2D(float x, float y, float width, float height)
    {
        Organisms = new LinkedList<Organism>();
        X = x;
        Y = y;
        Width = width;
        Height = height;
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