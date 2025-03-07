using System.Numerics;

namespace BioSim.Datastructures;

/// <summary>
/// This implements DataStructure (because that is required), but isn't a data structure
/// It simply stores all organisms in a list
/// </summary>
public class NoDataStructure : DataStructure
{
    private World world;
    private IEnumerable<Organism> Organisms => world.Organisms.Concat(World.OrganismsToAdd);
    
    public NoDataStructure(World world) : base(world)
    {
        this.world = world;
    }
    
    /// <summary>
    /// Gets the organism closest to this organism
    /// </summary>
    /// <param name="organism"></param>
    /// <returns>NOTE: This returns the original organism if no other organisms exist</returns>
    public override Organism ClosestNeighbour(Organism organism)
    {
        //Tracking distance without the square root, because it is not needed to find the closest organism and would only take more compute
        float currentDistanceSquared = float.MaxValue;
        Organism closestOrganism = organism;
        foreach (Organism otherOrganism in Organisms)
        {
            //If the organism is itself, we need to exclude it (because it's distance to itself is not what we want)
            if (otherOrganism == organism)
            {
                continue;
            }

            //Compare distance to our currently best found distance
            float distanceSquared = Vector3.DistanceSquared(otherOrganism.Position, organism.Position);
            if (distanceSquared < currentDistanceSquared)
            {
                currentDistanceSquared = distanceSquared;
                closestOrganism = otherOrganism;
            }
        }
        
        return closestOrganism;
    }

    public override bool CheckCollision(Organism organism, Vector3 position)
    {
        //If out of bounds, then there is a collision
        if (!World.IsInBounds(organism))
            return false;
        
        foreach (Organism otherOrganism in this)
        {
            //Cannot be a collision with itself
            if(otherOrganism == organism)
                continue;
            
            //Check for each dimension if the organism is within distance of the new position
            float x = position.X - otherOrganism.Position.X;
            if (MathF.Abs(x) < organism.Size + otherOrganism.Size)
                return true;
            
            float y = position.Y - otherOrganism.Position.Y;
            if (MathF.Abs(y) < organism.Size + otherOrganism.Size)
                return true;
            
            float z = position.Z - otherOrganism.Position.Z;
            if (MathF.Abs(z) < organism.Size + otherOrganism.Size)
                return true;
        }

        //If we reach this, then no collision
        return false;
    }

    protected override IEnumerator<IOrganism> ToEnumerator()
    {
        return Organisms.GetEnumerator();
    }
}