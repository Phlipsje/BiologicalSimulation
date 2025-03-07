using System;
using System.Numerics;
using BioSim;
using BioSim.Datastructures;

namespace Simple_graphical_implementation;

public class TestOrganism : Organism
{
    private Random random = new Random();
    private int growthTimeTicks;
    private int currentTicks;
    public TestOrganism(Vector3 startingPosition, float size, World world, DataStructure dataStructure) : base(startingPosition, size, world, dataStructure)
    {
        growthTimeTicks = random.Next(50, 200);
        currentTicks = 0;
    }

    public override TestOrganism CreateNewOrganism(Vector3 startingPosition)
    {
        return new TestOrganism(startingPosition, Size, World, DataStructure);
    }
    
    public override void Step()
    {
        //Moves randomly by maximum of 0.1 in positive or negative direction for every axis
        Move(new Vector3(0.01f, (float)(random.NextDouble() * 0.02 - 0.01),(float)(random.NextDouble() * 0.02 - 0.01)));

        currentTicks++;

        //Reproduce every so often
        if (currentTicks >= growthTimeTicks)
        {
            currentTicks = 0;
            Reproduce();
        }
    }

    public override string ToString()
    {
        throw new System.NotImplementedException();
    }

    public override void FromString(string s)
    {
        throw new System.NotImplementedException();
    }
}