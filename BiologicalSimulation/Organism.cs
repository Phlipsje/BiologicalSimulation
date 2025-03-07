using System.Numerics;
using BioSim.Datastructures;

namespace BioSim;

/// <summary>
/// This is an abstract class which can be extended to create a type of organism
/// It must be given a Key, Size, Step function, ToString function and FromString function to describe it
/// </summary>
public abstract class Organism : IOrganism
{
    public string Key { get; } //Used to identify which organism it is in a file
    public Vector3 Position { get; private set; }
    public float Size { get; } //Organism is a sphere, so this is the radius
    protected World World { get; } //Needs this to check if it is in bounds
    protected DataStructure DataStructure { get; } //Needs this to understand where other organisms are

    public Organism(Vector3 startingPosition, World world, DataStructure dataStructure)
    {
        Position = startingPosition;
        World = world;
        DataStructure = dataStructure;
    }

    public abstract Organism CreateNewOrganism(Vector3 startingPosition);
    public abstract void Step();
    public new abstract string ToString();
    public abstract void FromString(string s);

    public void Move(Vector3 direction)
    {
        //Simply add movement towards direction if there is no collision there
        Vector3 newPosition = Position + direction;
        
        if(!CheckCollision(newPosition))
            Position = newPosition;
    }
    
    public Organism Reproduce()
    {
        float stepSize = Size * 2;
        //Try to add an organism around the current organism, fairly rigid test, can definitely be improved
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3 possibleReproductionPosition = Position + new Vector3(x, y, z) * stepSize;
                    if (!CheckCollision(possibleReproductionPosition))
                    {
                        //Creates a new organism of the same type at the new location
                        Organism newOrganism = CreateNewOrganism(possibleReproductionPosition);
                        World.AddOrganism(newOrganism);
                        
                        //Stops the entire loop after reproduction has taken place
                        return newOrganism;
                    }
                }
            }
        }

        return null;
    }

    public Organism ClosestNeighbour()
    {
        return DataStructure.ClosestNeighbour(this);
    }

    private bool CheckCollision(Vector3 position)
    {
        return DataStructure.CheckCollision(this, position);
    }
}

public interface IOrganism
{
    public string Key { get; } //Used to identify which organism it is in a file
    public Vector3 Position { get; }
    public float Size { get; } //Organism is a sphere, so this is the radius
    
    public void Step();
    public string ToString();
    public void FromString(string s);
}