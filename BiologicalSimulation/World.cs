namespace BioSim;

public abstract class World
{
    public LinkedList<Organism> Organisms { get; }

    public World()
    {
        Organisms = new LinkedList<Organism>();
    }

    public void Step()
    {
        foreach (Organism organism in Organisms.Reverse())
        {
            organism.Step();
        }
    }

    public abstract void StartingDistribution(); //Where all organisms start
    public abstract bool IsInBounds(Organism organism); //Use this to define the confines of space all organisms are in, if false, it can't move to that position
    public abstract bool StopCondition(); //If this is true, the program will halt
}