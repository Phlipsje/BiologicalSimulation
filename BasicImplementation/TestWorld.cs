using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;

namespace BasicImplementation;

/// <summary>
/// This is an example implementation of a simulation world
/// Very simple, only meant for testing
/// </summary>
public class TestWorld : World
{
    private Simulation simulation;
    private float worldHalfSize;
    public TestWorld(DataStructure dataStructure, Simulation simulation, float worldHalfSize, bool preciseMovement,
        bool randomisedExecutionOrder = false) : base(dataStructure, preciseMovement, randomisedExecutionOrder)
    {
        this.simulation = simulation;
        this.worldHalfSize = worldHalfSize;
    }
    public override void StartingDistribution()
    {
        float range = worldHalfSize * 0.9f;
        
        //This spawns 8 organisms of type A in random position in world
        for (int i = 0; i < 8; i++)
        {
            new TestOrganism(new Vector3(Randomiser.NextSingle()*range-range/2, Randomiser.NextSingle()*range-range/2, Randomiser.NextSingle()*range-range/2), 0.5f, this, DataStructure);
        }
        
        //This spawns 8 organisms of type B in Randomiser position in world
        for (int i = 0; i < 8; i++)
        {
            new TestOrganismB(new Vector3(Randomiser.NextSingle()*range-range/2, Randomiser.NextSingle()*range-range/2, Randomiser.NextSingle()*range-range/2), 0.5f, this, DataStructure);
        }

    }

    public override bool IsInBounds(Vector3 position)
    {
        //Check if within cube
        float cubeHalfSize = worldHalfSize;
        
        if (MathF.Abs(position.X) >= cubeHalfSize)
            return false;
        if (MathF.Abs(position.Y) >= cubeHalfSize)
            return false;
        if (MathF.Abs(position.Z) >= cubeHalfSize)
            return false;
        
        return true;
    }

    public override bool StopCondition()
    {
        //if (simulation.Tick > 3000)
        //    return true;
        
        //We only stop when program is forcefully halted
        return false;
    }
}