using System.Numerics;
using BioSim.Datastructures;

namespace BioSim;

public abstract class World
{
    public LinkedList<Organism> Organisms { get; }
    public int OrganismCount { get; private set; }
    
    public World()
    {
        Organisms = new LinkedList<Organism>();
    }

    public void Initialize()
    {
        OrganismCount = Organisms.Count;
    }

    public virtual void Step()
    {
        
    }

    public void AddOrganism(Organism organism)
    {
        Organisms.AddLast(organism);
        OrganismCount++;
    }

    public abstract void StartingDistribution(DataStructure dataStructure, Random random); //Where all organisms start
    public abstract bool IsInBounds(Vector3 position); //Use this to define the confines of space all organisms are in, if false, it can't move to that position
    public abstract bool StopCondition(); //If this is true, the program will halt
}