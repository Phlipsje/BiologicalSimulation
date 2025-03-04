using System;
using System.Numerics;
using BioSim;
using BioSim.Datastructures;

namespace Simple_graphical_implementation;

public class TestOrganism : Organism
{
    private Random random;
    public TestOrganism(Vector3 startingPosition, DataStructure dataStructure) : base(startingPosition, dataStructure)
    {
    }

    public override void Step()
    {
        //Moves randomly by maximum of 0.1 in positive or negative direction for every axis
        Move(new Vector3((float)(random.NextDouble() * 0.2 - 0.1), (float)(random.NextDouble() * 0.2 - 0.1),(float)(random.NextDouble() * 0.2 - 0.1)));
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