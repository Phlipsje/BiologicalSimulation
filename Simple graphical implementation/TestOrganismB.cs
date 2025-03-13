using System;
using System.IO;
using BioSim;
using BioSim.Datastructures;
using Microsoft.Xna.Framework;
using Vector3 = System.Numerics.Vector3;

namespace Simple_graphical_implementation;

public class TestOrganismB : VisualOrganism
{
    public override string Key => "TB";
    private Random random = new Random();
    private int growthTimeTicks;
    private int currentTicks;
    public override Color Color => Color.Yellow;
    public TestOrganismB(Vector3 startingPosition, float size, World world, DataStructure dataStructure) : base(startingPosition, size, world, dataStructure)
    {
        growthTimeTicks = random.Next(50, 200);
        currentTicks = 0;
    }

    public override TestOrganismB CreateNewOrganism(Vector3 startingPosition)
    {
        return new TestOrganismB(startingPosition, Size, World, DataStructure);
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
        return $"x{x/100}y{y/100}z{z/100}t{currentTicks}g{growthTimeTicks}";
    }

    public override void FromString(string s)
    {
        StringReader sr = new StringReader(s);
        char[] chars = ['x', 'y', 'z', 't', 'g'];
        float[] values = new float[5];
        for (int i = 0; i < 5; i++)
        {
            sr.Read();
            values[i] = BuildUpNumber(sr, chars[i]);
        }
        
        //Assignment
        Position = new Vector3(values[0], values[1], values[2]);
        currentTicks = (int)values[3];
        growthTimeTicks = (int)values[4];
    }

    //Made specifically to read this specific organism type FromString, no safety checks and a lot of assumptions
    private float BuildUpNumber(StringReader sr, char ch)
    {
        int resultingNumber = 0;
        while (sr.Peek() != ch)
        {
            if (sr.Peek() == '.')
                continue;

            resultingNumber *= 10;
            resultingNumber += int.Parse(sr.Read().ToString());
        }
        return resultingNumber / 100f;
    }
}