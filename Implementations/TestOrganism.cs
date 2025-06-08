using System;
using System.Collections.Generic;
using BioSim;
using BioSim.Datastructures;
using Microsoft.Xna.Framework;
using Vector3 = System.Numerics.Vector3;

namespace Implementations;

public class TestOrganism : VisualOrganism
{
    public override string Key => "A";
    public float GrowthRate;
    public float Resources;
    public float BB1; //No idea what this is
    public float BB2; //No idea what this is
    public float Biomass;
    public override Color Color => Color.Green;
    public TestOrganism(Vector3 startingPosition, float size, World world, DataStructure dataStructure, Random random) : base(startingPosition, size, world, dataStructure, random)
    {
        Main.OrganismACount++;
        GrowthRate = random.NextSingle(); //Between 0 and 1
        Resources = 0;
        BB1 = 0;
        BB2 = 0;
        Biomass = 1;
    }

    public override TestOrganism CreateNewOrganism(Vector3 startingPosition)
    {
        return new TestOrganism(startingPosition, Size, World, DataStructure, Random);
    }

    public override void Step()
    {
        //Moves randomly by maximum of 0.1 in positive or negative direction for every axis
        //Also known as brownian motion
        Vector3 direction = new Vector3((float)(Random.NextDouble() * 0.02 - 0.01),
            (float)(Random.NextDouble() * 0.02 - 0.01), (float)(Random.NextDouble() * 0.02 - 0.01));
        Move(direction);
        
        Reproduction();
    }
    
    private void Reproduction()
    {
        GrowthRate = 0.02f;
        GridValues values = GrowthGrid.GetValues(Position);
        float uptake = values.R * 0.01f * (1-Resources/(Resources+0.1f));
        GrowthGrid.SetRValue(Position, values.R - uptake);
        Resources += uptake;
        
        float fractConverted = 0.5f * Resources;
        BB1 += fractConverted*150;
        Resources -= fractConverted;

        float leak = BB1 * 0.01f;
        GrowthGrid.SetBB1Valus(Position, BB1 + leak);
        BB1 -= leak;

        float upBB2 = values.BB2 / (BB2 + 1);
        BB2 += upBB2;
        GrowthGrid.SetBB2Valus(Position, BB2 - upBB2);

        GrowthRate += (BB1 * BB2 / (BB1 * BB2 + 1.0f)) * 0.99f;
        
        BB1 *= 1-GrowthRate;
        BB2 *= 1-GrowthRate;
        Biomass += GrowthRate;

        if (Biomass > 6)
        {
            TestOrganism child = Reproduce() as TestOrganism;
            if (child is null)
                return;
            
            child.GrowthRate = GrowthRate;
            child.Resources = Resources;
            child.BB1 = BB1;
            child.BB2 = BB2;
            child.Biomass = Biomass / 2;
            Biomass = Biomass / 2;
        }
    }

    public override string ToString()
    {
        //Save position 2 decimal points precise
        int x = (int)(Position.X * 100);
        int y = (int)(Position.Y * 100);
        int z = (int)(Position.Z * 100);
        return $" {x/100f} {y/100f} {z/100f}";
    }

    public override void FromString(string s)
    {
        string[] values = s.Split(' ');
        
        Position = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
    }
}