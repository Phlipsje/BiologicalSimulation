using System.Numerics;
using BioSim.Datastructures;

namespace BioSim;

/// <summary>
/// This is an abstract class which can be extended to create a type of organism
/// It must be given a Key, Size, Step function, ToString function and FromString function to describe it
/// </summary>
public abstract class Organism : IMinimumBoundable
{
    public abstract string Key { get; } //Used to identify which organism it is in a file
    private Vector3 position;
    public Vector3 Position
    {
        get => position;
        set
        {
            position = value;
            SetMbb(position);
        } 
    }
    public float Size { get; } //Organism is a sphere, so this is the radius
    public virtual Vector3 Color { get; } = Vector3.Zero; //Used to identify which organism it is in any visual representation, has no effect besides visual clarity
    protected World World { get; } //Needs this to check if it is in bounds
    protected DataStructure DataStructure { get; } //Needs this to understand where other organisms are
    protected Random Random;

    public Organism(Vector3 startingPosition, float size, World world, DataStructure dataStructure, Random random)
    {
        World = world;
        Position = startingPosition;
        Size = size;
        
        DataStructure = dataStructure;
        Random = random;
        DataStructure.AddOrganism(this);
    }

    public abstract Organism CreateNewOrganism(Vector3 startingPosition);
    public abstract void Step();
    public new abstract string ToString();
    public abstract void FromString(string s);

    public void Move(Vector3 direction)
    {
        //Simply add movement towards direction if there is no collision there
        Vector3 newPosition = Position + direction;

        if (!CheckCollision(newPosition))
        {
            //Otherwise no collision, so update position
            Position = newPosition;
        }
    }

    public Organism Reproduce()
    {
        //Will do a maximum of 5 attempts
        for (int i = 0; i < 5; i++)
        {
            //Get a direction in a 3D circular radius, length is exactly 1
            float phi = (float)(MathF.Acos(2 * (float)Random.NextDouble() - 1) - Math.PI / 2);
            float lambda = (float)(2 * Math.PI * Random.NextDouble());
            float x = MathF.Cos(phi) * MathF.Cos(lambda);
            float y = MathF.Cos(phi) * MathF.Sin(lambda);
            float z = MathF.Sin(phi);
            
            Vector3 direction = new Vector3(x, y, z);
            
            Vector3 positiveNewPosition = Position + direction * Size;
            Vector3 negativeNewPosition = Position - direction * Size;
            Vector3 onlyPositiveNewPosition = Position + direction * 2 * Size;
            Vector3 onlyNegativeNewPosition = Position - direction * 2 * Size;

            //Check if both positions are not within another organism
            if (!CheckCollision(positiveNewPosition) && !CheckCollision(negativeNewPosition))
            {
                //Create new organism 
                Organism newOrganism = CreateNewOrganism(positiveNewPosition);
            
                //Push the original organism away in the other direction
                Position = negativeNewPosition;
            
                return newOrganism;
            }
            else if (!CheckCollision(onlyPositiveNewPosition))
            {
                //Create new organism 
                Organism newOrganism = CreateNewOrganism(onlyPositiveNewPosition);
            
                return newOrganism;
            }
            else if (!CheckCollision(onlyNegativeNewPosition))
            {
                //Create new organism 
                Organism newOrganism = CreateNewOrganism(onlyNegativeNewPosition);
            
                return newOrganism;
            }
        }

        return null;
    }

    private bool CheckCollision(Vector3 position)
    {
        return DataStructure.CheckCollision(this, position);
    }

    public Mbb GetMbb()
    {
        return _mbb;
    }
    public void SetMbb(Mbb mbb)
    {
        mbb = _mbb;
        Position = mbb.Minimum + new Vector3(Size);
    }
    private void SetMbb(Vector3 position)
    {
        Vector3 sizeVector = new Vector3(Size);
        _mbb = new Mbb(Position - sizeVector, Position + sizeVector);
    }
    private Mbb _mbb;
}