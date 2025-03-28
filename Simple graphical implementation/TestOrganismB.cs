using System;
using System.IO;
using BioSim;
using BioSim.Datastructures;
using Microsoft.Xna.Framework;
using Vector3 = System.Numerics.Vector3;

namespace Simple_graphical_implementation;

public class TestOrganismB : VisualOrganism
{
    public override string Key => "B";
    private int growthTimeTicks;
    private int currentTicks;
    private float growthRate;
    private float resources;
    private float BB1; //No idea what this is
    private float BB2; //No idea what this is
    private float biomass;
    public override Color Color => Color.Yellow;
    public TestOrganismB(Vector3 startingPosition, float size, World world, DataStructure dataStructure, Random random) : base(startingPosition, size, world, dataStructure, random)
    {
        VisualSimulation.OrganismBCount++;
        growthTimeTicks = 200; //random.Next(200, 200);
        currentTicks = 0;
        
        growthRate = random.NextSingle(); //Between 0 and 1
        resources = 0;
        BB2 = 0;
        biomass = 1;
    }

    public override TestOrganismB CreateNewOrganism(Vector3 startingPosition)
    {
        return new TestOrganismB(startingPosition, Size, World, DataStructure, Random);
    }
    
    public override void Step()
    {
        //Moves randomly by maximum of 0.1 in positive or negative direction for every axis
        //Also known as brownian motion
        Move(new Vector3((float)(Random.NextDouble() * 0.02 - 0.01), (float)(Random.NextDouble() * 0.02 - 0.01),(float)(Random.NextDouble() * 0.02 - 0.01)));

        GridValues values = GrowthGrid.GetValues(Position);
        float uptake = values.R * 0.01f * (1-resources/(resources+0.1f));
        GrowthGrid.SetRValue(Position, values.R - uptake);
        resources += uptake;

        float fractConverted = 0.5f * resources;
        BB2 += fractConverted*150;
        resources -= fractConverted;

        float leak = BB2 * 0.01f;
        GrowthGrid.SetBB2Valus(Position, BB2 + leak);
        BB2 -= leak;
        
        float upBB1 = values.BB1 / (BB1 + 1);
        BB1 += upBB1;
        GrowthGrid.SetBB1Valus(Position, BB1 - upBB1);
        
        growthRate += (BB1 * BB2 / (BB1 * BB2 + 1.0f)) * 0.99f;
        
        BB1 *= 1-growthRate;
        BB2 *= 1-growthRate;
        biomass += growthRate;

        if (biomass > 10)
        {
            Reproduce();
            biomass = 0;
        }
    }

    public override string ToString()
    {
        //Save position 2 decimal points precise
        int x = (int)(Position.X * 100);
        int y = (int)(Position.Y * 100);
        int z = (int)(Position.Z * 100);
        return $" {x/100f} {y/100f} {z/100f} {currentTicks} {growthTimeTicks}";
    }

    public override void FromString(string s)
    {
        string[] values = s.Split(' ');
        
        Position = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        currentTicks = int.Parse(values[3]);
        growthTimeTicks = int.Parse(values[4]);
    }
}