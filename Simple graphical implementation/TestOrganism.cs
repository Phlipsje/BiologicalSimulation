using System;
using System.IO;
using BioSim;
using BioSim.Datastructures;
using Microsoft.Xna.Framework;
using Vector3 = System.Numerics.Vector3;

namespace Simple_graphical_implementation;

public class TestOrganism : VisualOrganism
{
    public override string Key => "A";
    private Random random = new Random();
    private int growthTimeTicks;
    private int currentTicks;
    public override Color Color => Color.Green;
    public TestOrganism(Vector3 startingPosition, float size, World world, DataStructure dataStructure) : base(startingPosition, size, world, dataStructure)
    {
        VisualSimulation.OrganismACount++;
        growthTimeTicks = random.Next(200, 200);
        currentTicks = 0;
    }

    public override TestOrganism CreateNewOrganism(Vector3 startingPosition)
    {
        return new TestOrganism(startingPosition, Size, World, DataStructure);
    }
    
    public override void Step()
    {
        //Moves randomly by maximum of 0.1 in positive or negative direction for every axis
        Move(new Vector3((float)(random.NextDouble() * 0.02 - 0.01), (float)(random.NextDouble() * 0.02 - 0.01),(float)(random.NextDouble() * 0.02 - 0.01)));

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
        //Save position 2 decimal points precise
        int x = (int)(Position.X * 100);
        int y = (int)(Position.Y * 100);
        int z = (int)(Position.Z * 100);
        return $" {x/100} {y/100} {z/100} {currentTicks} {growthTimeTicks}";
    }

    public override void FromString(string s)
    {
        string[] values = s.Split(' ');
        
        Position = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        currentTicks = int.Parse(values[3]);
        growthTimeTicks = int.Parse(values[4]);
    }
}