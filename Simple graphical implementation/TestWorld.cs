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
    private float worldHalfSize;
    public TestWorld(float worldHalfSize)
    {
        this.worldHalfSize = worldHalfSize;
    }
    public override void StartingDistribution(DataStructure dataStructure, Random random)
    {
        //One singular organism at the center
        Organisms.AddFirst(new TestOrganism(new Vector3(0, 0, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(1, 0, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(0, 1, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(-1, 0, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(0, -1, 0), 0.5f, this, dataStructure, random));
    }

    public override bool IsInBounds(Vector3 position)
    {
        //Check if within cube
        float cubeHalfSize = worldHalfSize;

        if (MathF.Abs(position.X) > cubeHalfSize)
            return false;
        if (MathF.Abs(position.Y) > cubeHalfSize)
            return false;
        if (MathF.Abs(position.Z) > cubeHalfSize)
            return false;

        return true;
    }

    public override bool StopCondition()
    {
        //We only stop when program is forcefully halted
        return false;
    }
}