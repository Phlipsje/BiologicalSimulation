using System.Numerics;
using BioSim.Datastructures;

namespace BioSim;

/// <summary>
/// This is an abstract class which can be extended to create a type of organism
/// It must be given a Key, Size, Step function, ToString function and FromString function to describe it
/// </summary>
public abstract class Organism : IOrganism, IEquatable<Organism>
{
    public ulong Id { get; } //Assigned at runtime, used to identify an organism without an expensive equals check
    public abstract string Key { get; } //Used to identify which organism it is in a file
    public Vector3 Position { get; set; }
    public float Size { get; } //Organism is a sphere, so this is the radius
    protected World World { get; } //Needs this to check if it is in bounds
    protected DataStructure DataStructure { get; } //Needs this to understand where other organisms are
    protected Random Random { get; }

    public Organism(Vector3 startingPosition, float size, World world, DataStructure dataStructure, Random random)
    {
        World = world;
        Position = startingPosition;
        Size = size;
        
        DataStructure = dataStructure;
        Random = random;
        DataStructure.AddOrganism(this);
        Id = GenerateNewOrganismID(0); //0 Because it does not have a parent
    }
    
    public Organism(Vector3 startingPosition, float size, Organism parent)
    {
        World = parent.World;
        Position = startingPosition;
        Size = size;
        
        DataStructure = parent.DataStructure;
        Random = parent.Random;
        DataStructure.AddOrganism(this);
        Id = GenerateNewOrganismID(parent.Id);
    }

    public abstract Organism CreateNewOrganism(Vector3 startingPosition);
    public abstract void Step(List<LinkedList<Organism>> organismLists);

    public new abstract string ToString();
    public abstract void FromString(string s);

    public void Move(Vector3 direction, List<LinkedList<Organism>> organismLists)
    {
        //Simply add movement towards direction if there is no collision there
        Vector3 newPosition = Position + direction;

        if (!CheckCollision(newPosition, organismLists))
        {
            //Otherwise no collision, so update position
            Position = newPosition;
        }
    }

    public Organism Reproduce(List<LinkedList<Organism>> organismLists)
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
            if (!CheckCollision(positiveNewPosition, organismLists) && !CheckCollision(negativeNewPosition, organismLists))
            {
                //Create new organism 
                Organism newOrganism = CreateNewOrganism(positiveNewPosition);
                World.AddOrganism(newOrganism);
            
                //Push the original organism away in the other direction
                Position = negativeNewPosition;
            
                return newOrganism;
            }
            else if (!CheckCollision(onlyPositiveNewPosition, organismLists))
            {
                //Create new organism 
                Organism newOrganism = CreateNewOrganism(onlyPositiveNewPosition);
                World.AddOrganism(newOrganism);
            
                return newOrganism;
            }
            else if (!CheckCollision(onlyNegativeNewPosition, organismLists))
            {
                //Create new organism 
                Organism newOrganism = CreateNewOrganism(onlyNegativeNewPosition);
                World.AddOrganism(newOrganism);
            
                return newOrganism;
            }
        }

        return null;
    }

    private bool CheckCollision(Vector3 position, List<LinkedList<Organism>> organismLists)
    {
        return DataStructure.CheckCollision(this, position, organismLists);
    }

    /// <summary>
    /// Pseudo-random algorithm to assign ID, algorithm should be fast, asynchronous, and have extremely unlikely chance to generate duplicate IDs
    /// </summary>
    /// <returns></returns>
    private ulong GenerateNewOrganismID(ulong parentOrganismId)
    {
        //Long has 8 bytes
        //We will fill it up with a few unrelated values and then perform some binary operations on it to give it a pseudo-random value
        
        ulong tick = (ulong)World.Tick; //1 byte used
        ulong x = (ulong)Position.X; //2 bytes used 
        ulong y = (ulong)Position.Y; //2 bytes used
        ulong z = (ulong)Position.Z; //2 bytes used
        ulong creatorId = parentOrganismId; //1 byte used
        
        ulong id = ShiftSectionLeft(tick, 56, 8) | //Becomes bits 64 to 56
                   ShiftSectionLeft(x, 40, 16) | //Becomes bits 55 to 40
                   ShiftSectionLeft(x, 24, 16) | //Becomes bits 39 to 24
                   ShiftSectionLeft(x, 8, 16)| //Becomes bits 24 to 9
                   (creatorId >> 56); //Becomes bits 8 to 1

        //XOR-shift algorithm (also known as a Linear Feedback Shift Register)
        //Used to change the value pseudo-randomly
        id ^= id >> 12;
        id ^= id << 31;
        id ^= id >> 23;

        return id;

        //Shifts left rightmost (lowest value) bits an amount to the left
        ulong ShiftSectionLeft(ulong value, int leftShiftDistance, int amountOfBits)
        {
            const int ulongBitCount = 64;
            ulong a = value << (ulongBitCount - amountOfBits);
            ulong b = value >> (ulongBitCount - amountOfBits - leftShiftDistance);
            return b;
        }
    }
    
    public bool Equals(Organism? other)
    {
        if(other is null) return false;
        
        return Id == other.Id;
    }
}

public interface IOrganism
{
    public string Key { get; } //Used to identify which organism it is in a file
    public Vector3 Position { get; }
    public float Size { get; } //Organism is a sphere, so this is the radius
    
    public void Step(List<LinkedList<Organism>> organismLists);
    public string ToString();
    public void FromString(string s);
}