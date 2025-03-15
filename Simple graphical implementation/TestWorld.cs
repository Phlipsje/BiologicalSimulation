using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BioSim;
using BioSim.Datastructures;

namespace Simple_graphical_implementation;

/// <summary>
/// This is an example implementation of a simulation world
/// Very simple, only meant for testing
/// </summary>
public class TestWorld : World
{
    public override void StartingDistribution(DataStructure dataStructure)
    {
        //One singular organism at the center
        Organisms.AddFirst(new TestOrganism(new Vector3(0, 0, 0), 0.5f, this, dataStructure));
        Organisms.AddFirst(new TestOrganismB(new Vector3(1, 0, 0), 0.5f, this, dataStructure));
        Organisms.AddFirst(new TestOrganismB(new Vector3(0, 1, 0), 0.5f, this, dataStructure));
        Organisms.AddFirst(new TestOrganismB(new Vector3(-1, 0, 0), 0.5f, this, dataStructure));
        Organisms.AddFirst(new TestOrganismB(new Vector3(0, -1, 0), 0.5f, this, dataStructure));
    }

    public override bool IsInBounds(Organism organism)
    {
        //Check if within cube
        int cubeHalfSize = 3;

        if (MathF.Abs(organism.Position.X) > cubeHalfSize)
            return false;
        if (MathF.Abs(organism.Position.Y) > cubeHalfSize)
            return false;
        if (MathF.Abs(organism.Position.Z) > cubeHalfSize)
            return false;

        return true;
    }

    public override bool StopCondition()
    {
        //We only stop when program is forcefully halted
        return false;
    }
}