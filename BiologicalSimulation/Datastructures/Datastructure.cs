using System.Collections;
using System.Numerics;

namespace BioSim.Datastructures;
/// <summary>
/// An abstract class used to define an object that can help in more efficiently running position based queries
/// </summary>
public abstract class DataStructure
{
    /// <summary>
    /// To connect with what is happening in the world.
    /// </summary>
    protected World World { get; private set; }

    public DataStructure()
    {
    }

    /// <summary>
    /// Sets the world to keep as a reference, already called for you by the simulation.
    /// </summary>
    /// <param name="world"></param>
    public void SetWorld(World world)
    {
        World = world;
    }
    
    /// <summary>
    /// Gets called every tick, after the updating of World.cs
    /// </summary>
    /// <returns></returns>
    public abstract Task Step();
    
    /// <summary>
    /// Adds a new organism to the simulation.
    /// </summary>
    /// <param name="organism"></param>
    public abstract void AddOrganism(Organism organism);
    
    /// <summary>
    /// Removes an organism from the simulation.
    /// </summary>
    /// <param name="organism"></param>
    /// <returns></returns>
    public abstract bool RemoveOrganism(Organism organism);
    
    /// <summary>
    /// Gets a list of all currently active organisms.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<Organism> GetOrganisms();
    
    /// <summary>
    /// Gets the total amount of organisms currently active.
    /// </summary>
    /// <returns></returns>
    public abstract int GetOrganismCount();
    
    /// <summary>
    /// Checks if an organisms would collide with the world bounds or another organisms by moving to the newly given position.
    /// </summary>
    /// <param name="organism"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public abstract bool CheckCollision(Organism organism, Vector3 position);
    
    /// <summary>
    /// Finds the Organism closest to the given organism.
    /// NOTE: Depending on the data structure that implements it, can return null if the range to a nearest organism is too great.
    /// </summary>
    /// <param name="organism"></param>
    /// <returns></returns>
    public abstract Organism? NearestNeighbour(Organism organism);
}