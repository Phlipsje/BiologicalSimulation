using System.Numerics;

namespace Continuum.Datastructures.MultiThreaded;

public abstract class MultiThreadedDataStructure : DataStructure
{
    public override bool IsMultithreaded { get; } = true;
    
    /// <summary>
    /// Gets called every tick, after the updating of World.cs
    /// </summary>
    /// <returns></returns>
    public abstract Task Step();
    
    /// <summary>
    /// Removes all Organisms from the simulation.
    /// </summary>
    /// <returns></returns>
    public abstract Task Clear();
    
    /// <summary>
    /// Gets a list of all currently active organisms.
    /// </summary>
    /// <returns></returns>
    public abstract Task GetOrganisms(out IEnumerable<Organism> organisms);
    
    /// <summary>
    /// Gets the total amount of organisms currently active.
    /// </summary>
    /// <returns></returns>
    public abstract Task GetOrganismCount(out int count);
}