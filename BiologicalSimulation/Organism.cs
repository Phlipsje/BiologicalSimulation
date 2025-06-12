using System.Numerics;
using BioSim.Datastructures;

namespace BioSim;

/// <summary>
/// This is an abstract class which can be extended to create a type of organism
/// It must be given a Key, Size, Step function, ToString function and FromString function to describe it
/// </summary>
public abstract class Organism : IMinimumBoundable
{
    /// <summary>
    /// Used to identify type of organism in a file.
    /// </summary>
    public abstract string Key { get; }
    private Vector3 position;
    
    /// <summary>
    /// The position of the organism.
    /// </summary>
    public Vector3 Position
    {
        get => position;
        set
        {
            position = value;
            SetMbb(position);
        } 
    }
    
    /// <summary>
    /// Organism is a sphere, so this is the radius.
    /// </summary>
    public float Size { get; }
    
    /// <summary>
    /// Used to identify which organism it is in any visual representation, has no effect besides visual clarity.
    /// </summary>
    public virtual Vector3 Color { get; } = Vector3.Zero;
    
    /// <summary>
    /// Needs this to check if it is in bounds
    /// </summary>
    protected World World { get; }
    
    /// <summary>
    /// Needs this to understand where other organisms are
    /// </summary>
    protected DataStructure DataStructure { get; }
    
    /// <summary>
    /// Needs this to assist in random value generation.
    /// This is the same random class passed when Simulation was made and thus makes use of the same starting seed.
    /// This means that all actions in Organism are deterministic with seed (as long as multithreaded data structure is not being used).
    /// </summary>
    protected Random Random { get; }

    public Organism(Vector3 startingPosition, float size, World world, DataStructure dataStructure, Random random)
    {
        World = world;
        Position = startingPosition;
        Size = size;
        
        DataStructure = dataStructure;
        Random = random;
        DataStructure.AddOrganism(this);
    }

    /// <summary>
    /// Helper function that is used to copy over any unique, non-standard values to the contents of a new organism.
    /// </summary>
    /// <param name="startingPosition"></param>
    /// <returns></returns>
    public abstract Organism CreateNewOrganism(Vector3 startingPosition);
    
    
    /// <summary>
    /// Gets called every tick by the active data structure, used to express all the logic of an organism.
    /// </summary>
    public abstract void Step();
    
    /// <summary>
    /// Turns the contents of this organism into a string used for writing to file.
    /// </summary>
    /// <returns></returns>
    public new abstract string ToString();
    
    /// <summary>
    /// Gets a string and uses it to set the contents of this organism.
    /// </summary>
    /// <param name="s"></param>
    public abstract void FromString(string s);

    /// <summary>
    /// Moves the organism towards a given location, also accounts for collision checks.
    /// </summary>
    /// <param name="direction"></param>
    public virtual void Move(Vector3 direction)
    {
        //Simply add movement towards direction if there is no collision there
        Vector3 newPosition = Position + direction;

        if (!CheckCollision(newPosition))
        {
            //Otherwise no collision, so update position
            Position = newPosition;
        }
    }

    /// <summary>
    /// Looks for room to try to create a new organism of the same type.
    /// Note that if there is no room then no organism will be created and this returns null.
    /// </summary>
    /// <returns></returns>
    public virtual Organism Reproduce()
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

    /// <summary>
    /// Checks if this organism would collide with another organism if it would move to the given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private bool CheckCollision(Vector3 position)
    {
        return DataStructure.CheckCollision(this, position);
    }

    /// <summary>
    /// Get minimum bounding box
    /// </summary>
    /// <returns></returns>
    public Mbb GetMbb()
    {
        return _mbb;
    }
    
    /// <summary>
    /// Set minimum bounding box
    /// </summary>
    /// <param name="mbb"></param>
    public void SetMbb(Mbb mbb)
    {
        mbb = _mbb;
        Position = mbb.Minimum + new Vector3(Size);
    }
    
    /// <summary>
    /// Set minimum bounding box
    /// </summary>
    /// <param name="position"></param>
    private void SetMbb(Vector3 position)
    {
        Vector3 sizeVector = new Vector3(Size);
        _mbb = new Mbb(position - sizeVector, position + sizeVector);
    }
    private Mbb _mbb;
}