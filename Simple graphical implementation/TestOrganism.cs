using System;
using System.Numerics;
using BioSim;
using BioSim.Datastructures;

namespace Simple_graphical_implementation;

public class TestOrganism : Organism
{
    private Random random = new Random();
    public TestOrganism(Vector3 startingPosition, World world, DataStructure dataStructure) : base(startingPosition, world, dataStructure)
    {
    }

    public override TestOrganism CreateNewOrganism(Vector3 startingPosition)
    {
        return new TestOrganism(startingPosition, World, DataStructure);
    }
    
    public override void Step()
    {
        //Moves randomly by maximum of 0.1 in positive or negative direction for every axis
        Move(new Vector3(0.01f, (float)(random.NextDouble() * 0.02 - 0.01),(float)(random.NextDouble() * 0.02 - 0.01)));
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