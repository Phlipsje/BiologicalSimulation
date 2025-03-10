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
    private Random random;

    public Organism(Vector3 startingPosition, float size, World world, DataStructure dataStructure)
    {
        Position = startingPosition;
        Size = size;
        World = world;
        DataStructure = dataStructure;
        random = new Random();
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
        //Will do a maximum of 5 attempts
        for (int i = 0; i < 5; i++)
        {
            //Get a direction in a 3D circular radius
            float randValueXY = (float)(random.NextDouble() * Math.PI);
            float randValueZ = (float)(random.NextDouble() * Math.PI);
        
            Vector3 direction = new Vector3(MathF.Sin(randValueXY), MathF.Cos(randValueXY), MathF.Sin(randValueZ));
        
            Vector3 positiveNewPosition = Position + direction * Size * 1.05f;
            Vector3 negativeNewPosition = Position - direction * Size * 1.05f;
            Vector3 onlyPositiveNewPosition = Position + direction * 2 *Size * 1.05f;
            Vector3 onlyNegativeNewPosition = Position - direction * 2 * Size * 1.05f;

            //Check if both positions are not within another organism
            if (!CheckCollision(positiveNewPosition) && !CheckCollision(negativeNewPosition))
            {
                //Create new organism 
                Organism newOrganism = CreateNewOrganism(positiveNewPosition);
                World.AddOrganism(newOrganism);
            
                //Push the original organism away in the other direction
                Position = negativeNewPosition;
            
                return newOrganism;
            }
            else if (!CheckCollision(onlyPositiveNewPosition))
            {
                //Create new organism 
                Organism newOrganism = CreateNewOrganism(onlyPositiveNewPosition);
                World.AddOrganism(newOrganism);
            
                return newOrganism;
            }
            else if (!CheckCollision(onlyNegativeNewPosition))
            {
                //Create new organism 
                Organism newOrganism = CreateNewOrganism(onlyNegativeNewPosition);
                World.AddOrganism(newOrganism);
            
                return newOrganism;
            }
        }

        return null;
    }

    public Organism ClosestNeighbour()
    {
        return DataStructure.ClosestNeighbour(this);
    }

    private bool CheckCollision(Organism organism, Vector3 position)
    {
        return DataStructure.CheckCollision(organism, position);
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