using System.Collections;
using System.Numerics;

namespace Continuum.Datastructures;
/// <summary>
/// An abstract class used to define an object that can help in more efficiently running position based queries
/// </summary>
public abstract class SingleThreadedDataStructure : DataStructure
{
    public override bool IsMultithreaded { get; } = false;
    
    /// <summary>
    /// Gets called every tick, after the updating of World.cs
    /// </summary>
    /// <returns></returns>
    public abstract void Step();
    
    /// <summary>
    /// Removes all Organisms from the simulation.
    /// </summary>
    /// <returns></returns>
    public abstract void Clear();
    
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
}