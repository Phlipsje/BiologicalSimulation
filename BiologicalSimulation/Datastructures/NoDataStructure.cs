using System.Numerics;

namespace BioSim.Datastructures;

/// <summary>
/// This implements DataStructure (because that is required), but isn't a data structure
/// It simply stores all organisms in a list
/// </summary>
public class NoDataStructure : DataStructure
{
    private World world;
    private LinkedList<Organism> organisms => world.Organisms;
    
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
        foreach (Organism otherOrganism in organisms)
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

    protected override IEnumerator<IOrganism> ToEnumerator()
    {
        return organisms.GetEnumerator();
    }
}