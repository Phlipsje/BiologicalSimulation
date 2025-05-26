using System.Numerics;
using BioSim;
using BioSim.Datastructures;

namespace BiologicalSimulation;

public abstract class MultithreadedWorld
{
    private Thread[] threads { get; }
    public LinkedList<Organism> Organisms { get; } //TODO this might have to be done differently
    public int OrganismCount { get; private set; } //TODO this might have to be done differently
    
    public MultithreadedWorld(int threadCount)
    {
        threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(ThreadHoldingRoom);
        }
    }

    private void ThreadHoldingRoom()
    {
        //TODO remember that we dont want to create and destroy threads, we want to assign them normal and waiting tasks
    }
    
    public void Initialize()
    {
        
    }

    public void Step()
    {
        
    }

    public void AddOrganism()
    {
        
    }
    
    public abstract void StartingDistribution(MultithreadedDataStructure dataStructure, Random random); //Where all organisms start
    public abstract bool IsInBounds(Vector3 position); //Use this to define the confines of space all organisms are in, if false, it can't move to that position
    public abstract bool StopCondition(); //If this is true, the program will halt
}