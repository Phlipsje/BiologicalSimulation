using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;

namespace Simple_graphical_implementation;

/// <summary>
/// This is an example implementation of a simulation world
/// Very simple, only meant for testing
/// </summary>
public class TestWorld : World
{
    private Simulation simulation;
    private float worldHalfSize;
    public TestWorld(DataStructure dataStructure, Simulation simulation, float worldHalfSize) : base(dataStructure)
    {
        this.simulation = simulation;
        this.worldHalfSize = worldHalfSize;
    }
    public override void StartingDistribution(Random random)
    {
        //One singular organism at the center
        
        //Singular setup 3D, 2D, 1D
        /*
        Organisms.AddFirst(new TestOrganism(new Vector3(-1, 0, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(1, 0, 0), 0.5f, this, dataStructure, random));
        */
        
        //Partial trapping setup 3D
        /*
        Organisms.AddFirst(new TestOrganism(new Vector3(-0.5f, -0.5f, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(0.5f, -0.5f, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(-0.5f, 0.5f, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(0.5f, 0.5f, 0), 0.5f, this, dataStructure, random));
        
        Organisms.AddFirst(new TestOrganismB(new Vector3(-1.5f, 0, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(1.5f, 0, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(0, -1.5f, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(0, 1.5f, 0), 0.5f, this, dataStructure, random));
        */
        
        //Partial trapping setup 2D
        /*
        Organisms.AddFirst(new TestOrganism(new Vector3(-0.5f, 0, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(0.5f, 0, 0), 0.5f, this, dataStructure, random));
        
        Organisms.AddFirst(new TestOrganismB(new Vector3(-1.5f, 0, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(1.5f, 0, 0), 0.5f, this, dataStructure, random));
        */
        
        //Full trapping setup 3D
        /*
        Organisms.AddFirst(new TestOrganism(new Vector3(-0.5f, -0.5f, -0.5f), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(0.5f, -0.5f, -0.5f), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(-0.5f, 0.5f, -0.5f), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(0.5f, 0.5f, -0.5f), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(-0.5f, -0.5f, 0.5f), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(0.5f, -0.5f, 0.5f), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(-0.5f, 0.5f, 0.5f), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(0.5f, 0.5f, 0.5f), 0.5f, this, dataStructure, random));

        float d = 1.4f;
        Organisms.AddFirst(new TestOrganismB(new Vector3(-d, -d, -d), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(-d, -d, d), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(-d, d, -d), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(-d, d, d), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(d, -d, -d), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(d, -d, d), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(d, d, -d), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(d, d, d), 0.5f, this, dataStructure, random));
        */
        
        //Full trapping setup 2D
        /*
        Organisms.AddFirst(new TestOrganism(new Vector3(-0.5f, -0.5f, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(0.5f, -0.5f, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(-0.5f, 0.5f, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(0.5f, 0.5f, 0), 0.5f, this, dataStructure, random));

        Organisms.AddFirst(new TestOrganismB(new Vector3(-1.4f, -1.4f, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(1.4f, -1.4f, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(-1.4f, 1.4f, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(1.4f, 1.4f, 0), 0.5f, this, dataStructure, random));
        */
        
        //Full trapping 1D
        /*
        Organisms.AddFirst(new TestOrganism(new Vector3(-0.5f, 0, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(0.5f, 0, 0), 0.5f, this, dataStructure, random));

        Organisms.AddFirst(new TestOrganismB(new Vector3(-1.5f, 0, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(1.5f, 0, 0), 0.5f, this, dataStructure, random));
        */
        
        //Random 3D
        
        DataStructure.AddOrganism(new TestOrganism(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganism(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganism(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganism(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganism(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganism(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganism(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganism(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        
        DataStructure.AddOrganism(new TestOrganismB(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganismB(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganismB(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganismB(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganismB(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganismB(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganismB(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        DataStructure.AddOrganism(new TestOrganismB(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, random.NextSingle()*20-10), 0.5f, this, DataStructure, random));
        
        
        //Random 2D
        /*
        Organisms.AddFirst(new TestOrganism(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, 0), 0.5f, this, dataStructure, random));
        
        Organisms.AddFirst(new TestOrganismB(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(random.NextSingle()*20-10, random.NextSingle()*20-10, 0), 0.5f, this, dataStructure, random));
        */
        
        //Random 1D
        /*
        Organisms.AddFirst(new TestOrganism(new Vector3(random.NextSingle()*20-10, 0, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganism(new Vector3(random.NextSingle()*20-10, 0, 0), 0.5f, this, dataStructure, random));
        
        Organisms.AddFirst(new TestOrganismB(new Vector3(random.NextSingle()*20-10, 0, 0), 0.5f, this, dataStructure, random));
        Organisms.AddFirst(new TestOrganismB(new Vector3(random.NextSingle()*20-10, 0, 0), 0.5f, this, dataStructure, random));
        */
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
        //For 2D
        //if (MathF.Abs(position.Z) > 0.6f)
        //    return false;
        
        //For 1D
        //if (MathF.Abs(position.Y) > 0.6f)
        //    return false;
        
        return true;
    }

    public override bool StopCondition()
    {
        if (simulation.Tick > 3000)
            return true;
        
        //We only stop when program is forcefully halted
        return false;
    }
}