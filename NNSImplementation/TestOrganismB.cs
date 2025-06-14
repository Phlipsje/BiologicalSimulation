using Continuum;
using Continuum.Datastructures;

namespace NNSImplementation;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vector3 = System.Numerics.Vector3;

public class TestOrganismB : Organism
{
    private const float Speed = 0.018f;
    private const float KillRange = 0.2f;
    public override string Key => "B";
    private int reproductionCounter = 0;
    private int ticksForReproduction = 0;
    public override Vector3 Color => color;
    private static readonly Vector3 color = new Vector3(0.5f, 0.15f, 0.15f);
    public TestOrganismB(Vector3 startingPosition, float size, World world, DataStructure dataStructure) : base(startingPosition, size, world, dataStructure)
    {
        ticksForReproduction = Randomiser.Next(210, 250);
    }

    public override TestOrganismB CreateNewOrganism(Vector3 startingPosition)
    {
        return new TestOrganismB(startingPosition, Size, World, DataStructure);
    }

    public override void Step()
    {
        //Moves randomly by maximum of 0.1 in positive or negative direction for every axis
        //Also known as brownian motion
        Organism? other = DataStructure.NearestNeighbour(this);
        Vector3 direction;
        float magnitude = Speed;
        if (other == null)
        {
            direction = new Vector3((float)(Randomiser.NextDouble() * 0.02 - 0.01), 
                (float)(Randomiser.NextDouble() * 0.02 - 0.01), (float)(Randomiser.NextDouble() * 0.02 - 0.01));
            Reproduction();
            reproductionCounter++;
            return;
        }
        if (other is TestOrganismB)
        {
            direction = (this.Position - other.Position);
            direction /= direction.Length(); //normalise
            direction *= magnitude;
        }
        else
        {
            direction = (other.Position - this.Position);
            direction /= direction.Length(); //normalise
            direction *= magnitude;
        }
        Move(direction);
        float killRadius = this.Size + other.Size + KillRange;
        if (other is TestOrganism && (other.Position - Position).LengthSquared() < killRadius * killRadius)
        {
            DataStructure.RemoveOrganism(other);
        }
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