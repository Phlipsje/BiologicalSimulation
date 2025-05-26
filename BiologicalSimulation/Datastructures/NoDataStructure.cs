using System.Numerics;

namespace BioSim.Datastructures;

/// <summary>
/// This implements DataStructure (because that is required), but isn't a data structure
/// It simply stores all organisms in a list
/// </summary>
public class NoDataStructure(World world) : DataStructure(world)
{
    public override void Step()
    {
        //Nothing
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
        foreach (Organism otherOrganism in World.Organisms)
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

    public override bool CheckCollision(Organism organism, Vector3 position, List<LinkedList<Organism>> organismLists)
    {
        LinkedList<Organism> organisms = organismLists[0];
        
        //If out of bounds, then there is a collision
        if (!World.IsInBounds(position))
            return true;

        foreach (Organism otherOrganism in organisms)
        {
            //Cannot be a collision with itself
            if(otherOrganism == organism)
                continue;
            
            //Checks collision by checking distance between circles
            float x = position.X - otherOrganism.Position.X;
            float x2 = x * x;
            float y = position.Y - otherOrganism.Position.Y;
            float y2 = y * y;
            float z = position.Z - otherOrganism.Position.Z;
            float z2 = z * z;
            float sizes = organism.Size + otherOrganism.Size;
            if (x2 + y2 + z2 <= sizes * sizes)
                return true;
        }

        //If we reach this, then no collision
        return false;
    }

    public override void AddOrganism(Organism organism)
    {
        
    }
}