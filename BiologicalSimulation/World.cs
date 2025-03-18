using System.Numerics;
using BioSim.Datastructures;

namespace BioSim;

public abstract class World
{
    public LinkedList<Organism> Organisms { get; }
    //This exists, because a collection cannot be altered while it is being looped through
    // so we add new organisms to this list and at the end of a loop, we add all these to Organisms and then clear this.
    public LinkedList<Organism> OrganismsToAdd { get; } 

    public World()
    {
        Organisms = new LinkedList<Organism>();
        OrganismsToAdd = new LinkedList<Organism>();
    }

    public void Step()
    {
        foreach (Organism organism in Organisms)
        {
            organism.Step();
        }

        //When looping is done, add all new organisms to the main list
        foreach (Organism organism in OrganismsToAdd)
        {
            Organisms.AddFirst(organism);
        }
        
        OrganismsToAdd.Clear();
    }

    public void AddOrganism(Organism organism)
    {
        OrganismsToAdd.AddFirst(organism);
    }

    public abstract void StartingDistribution(DataStructure dataStructure, Random random); //Where all organisms start
    public abstract bool IsInBounds(Vector3 position); //Use this to define the confines of space all organisms are in, if false, it can't move to that position
    public abstract bool StopCondition(); //If this is true, the program will halt
}