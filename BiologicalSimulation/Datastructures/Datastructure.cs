using System.Collections;
using System.Numerics;

namespace BioSim.Datastructures;
/// <summary>
/// An abstract class used to define an object that can help in more efficiently running position based queries
/// </summary>
public abstract class DataStructure
{
    protected World World { get; }

    public DataStructure(World world)
    {
        World = world;
    }

    //Gets called every frame (before updating world)
    public abstract void Step();
    public abstract void AddOrganism(Organism organism);
    public abstract bool CheckCollision(Organism organism, Vector3 position, List<LinkedList<Organism>> organismLists);
    public abstract Organism ClosestNeighbour(Organism organism);
}