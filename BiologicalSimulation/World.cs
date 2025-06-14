using System.Collections;
using System.Numerics;
using BioSim.Datastructures;

namespace BioSim;

public abstract class World
{
    /// <summary>
    /// The data structure being used in the simulation.
    /// </summary>
    public DataStructure DataStructure { get; }
    
    internal bool PreciseMovement { get; }
    internal bool RandomisedExecutionOrder { get; }
    
    public World(DataStructure dataStructure, bool preciseMovement, bool randomisedExecutionOrder)
    {
        DataStructure = dataStructure;
        dataStructure.SetWorld(this);
        PreciseMovement = preciseMovement;
        RandomisedExecutionOrder = randomisedExecutionOrder;
    }

    /// <summary>
    /// An optional extra to run logic at the very first tick of the simulation.
    /// This is called just before StartingDistribution().
    /// </summary>
    public virtual void Initialize()
    {
        
    }

    /// <summary>
    /// An optional extra to run logic BEFORE the data structure including all organisms are updated.
    /// Make sure NOT to include Datastructure.Step() in here, as it is already called by the simulation itself.
    /// </summary>
    public virtual void Step()
    {
        
    }

    /// <summary>
    /// Removes all Organisms from the simulation.
    /// </summary>
    public void Clear()
    {
        DataStructure.Clear().Wait();
    }

    /// <summary>
    /// Adds an organism to the simulation.
    /// This method is specifically meant for adding from file, not for normal adding.
    /// </summary>
    /// <param name="organism"></param>
    public void AddOrganism(Organism organism)
    {
        DataStructure.AddOrganism(organism);
    }

    /// <summary>
    /// Removes an organism from the simulation.
    /// </summary>
    /// <param name="organism"></param>
    public void RemoveOrganism(Organism organism)
    {
        DataStructure.RemoveOrganism(organism);
    }

    /// <summary>
    /// Get a list of all organisms currently in the simulation.
    /// </summary>
    /// <returns></returns>
    public Task GetOrganisms(out IEnumerable<Organism> organisms)
    {
        IEnumerable<Organism> o;
        if (DataStructure.IsMultithreaded)
            DataStructure.GetOrganisms(out o).Wait();
        else
            DataStructure.GetOrganisms(out o);
        organisms = o;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the current amount of active organisms in the simulation.
    /// </summary>
    /// <returns></returns>
    public Task GetOrganismCount(out int count)
    {
        int c;
        if (DataStructure.IsMultithreaded)
            DataStructure.GetOrganismCount(out c).Wait();
        else
            DataStructure.GetOrganismCount(out c);
        count = c;
        return Task.CompletedTask;
    }

    /// <summary>
    /// A method used to indicate the starting conditions of your simulation.
    /// </summary>
    /// <param name="random"></param>
    public abstract void StartingDistribution(Random random);
    
    /// <summary>
    /// Use this to define the confines of the space all organisms are in, if false, it can't move to that position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public abstract bool IsInBounds(Vector3 position); 
    
    /// <summary>
    /// If this is true, the simulation will be aborted.
    /// </summary>
    /// <returns></returns>
    public abstract bool StopCondition();
}