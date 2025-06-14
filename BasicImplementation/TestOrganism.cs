namespace BasicImplementation;

using System;
using System.Collections.Generic;
using BioSim;
using BioSim.Datastructures;
using Microsoft.Xna.Framework;
using Vector3 = System.Numerics.Vector3;

public class TestOrganism : Organism
{
    public override string Key => "A";
    private int reproductionCounter = 0;
    private int ticksForReproduction = 0;
    public override Vector3 Color => color;
    private static readonly Vector3 color = new Vector3(0.15f, 0.5f, 0.15f);
    public TestOrganism(Vector3 startingPosition, float size, World world, DataStructure dataStructure) : base(startingPosition, size, world, dataStructure)
    {
        Program.OrganismACount++;
        ticksForReproduction = 20; //random.Next(210, 250);
    }

    public override TestOrganism CreateNewOrganism(Vector3 startingPosition)
    {
        return new TestOrganism(startingPosition, Size, World, DataStructure);
    }

    public override void Step()
    {
        //Moves randomly by maximum of 0.1 in positive or negative direction for every axis
        //Also known as brownian motion
        Vector3 direction = new Vector3((Randomiser.NextSingle() * 0.02f - 0.01f),
            (float)(Randomiser.NextDouble() * 0.02 - 0.01), (Randomiser.NextSingle() * 0.02f - 0.01f));
        Move(direction);
        
        Reproduction();
        reproductionCounter++;
    }
    
    private void Reproduction()
    {
        if (reproductionCounter > ticksForReproduction)
        {
            Reproduce();
            reproductionCounter = 0;
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