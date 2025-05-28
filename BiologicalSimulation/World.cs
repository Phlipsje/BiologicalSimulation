using System.Collections;
using System.Numerics;
using BioSim.Datastructures;

namespace BioSim;

public abstract class World
{
    protected Simulation.Simulation Simulation { get; }
    public DataStructure DataStructure { get; }
    public int Tick => Simulation.Tick;
    
    public World(Simulation.Simulation simulation, DataStructure dataStructure)
    {
        Simulation = simulation;
        DataStructure = dataStructure;
        dataStructure.SetWorld(this);
    }

    public virtual void Initialize()
    {
        
    }

    public virtual void Step()
    {
        
    }

    public void AddOrganism(Organism organism)
    {
        DataStructure.AddOrganism(organism);
    }

    public IEnumerable<Organism> GetOrganisms()
    {
        return DataStructure.GetOrganisms();
    }

    public int GetOrganismCount()
    {
        return DataStructure.GetOrganismCount();
    }

    public abstract void StartingDistribution(Random random); //Where all organisms start
    public abstract bool IsInBounds(Vector3 position); //Use this to define the confines of space all organisms are in, if false, it can't move to that position
    public abstract bool StopCondition(); //If this is true, the program will halt
}