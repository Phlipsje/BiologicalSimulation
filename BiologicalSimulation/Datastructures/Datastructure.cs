using System.Collections;
using System.Numerics;

namespace BioSim.Datastructures;
/// <summary>
/// An abstract class used to define an object that can help in more efficiently running position based queries
/// </summary>
public abstract class DataStructure
{
    protected World World { get; private set; }

    public DataStructure()
    {
    }

    public void SetWorld(World world)
    {
        World = world;
    }

    //Gets called every frame (before updating world)
    public abstract Task Step();
    public abstract void AddOrganism(Organism organism);
    public abstract bool RemoveOrganism(Organism organism);
    public abstract IEnumerable<Organism> GetOrganisms();
    public abstract int GetOrganismCount();
    public abstract bool CheckCollision(Organism organism, Vector3 position);
    public abstract Organism? NearestNeighbour(Organism organism);
}