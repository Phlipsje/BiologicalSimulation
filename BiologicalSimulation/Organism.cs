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
    private DataStructure dataStructure; //Needs this to understand where other organisms are

    public Organism(Vector3 startingPosition, DataStructure dataStructure)
    {
        Position = startingPosition;
        this.dataStructure = dataStructure;
    }
    
    public abstract void Step();
    public new abstract string ToString();
    public abstract void FromString(string s);

    public void Move(Vector3 direction)
    {
        //TODO currently just updates position, change to also do collision detection
        Position = Position + direction;
    }
    
    public void Reproduce()
    {
        //Where do we add a copy of this organism relative to it?
        throw new NotImplementedException();
    }

    public Organism ClosestNeighbour()
    {
        return dataStructure.ClosestNeighbour(this);
    }

    private bool CheckCollision(Vector3 direction)
    {
        bool collision = false;
        
        Vector3 newPosition = Position + direction;
        
        foreach (Organism organism in dataStructure)
        {
            //TODO figure out how to do this,
            //We probably don't want to simply return a bool, but want to get as close to a different organism as possible
            //So maybe even use a different function?
        }

        return collision;
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