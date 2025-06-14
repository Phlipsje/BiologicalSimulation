using System.Numerics;

namespace Continuum.Datastructures.SingleThreaded;

/// <summary>
/// This implements DataStructure (because that is required), but isn't a data structure
/// It simply stores all organisms in a list, used as a fallback if no data structure is given
/// </summary>
public class NoDataStructure : SingleThreadedDataStructure
{
    public override bool IsMultithreaded { get; } = false;
    
    public LinkedList<Organism> Organisms { get; }

    public NoDataStructure()
    {
        Organisms = new LinkedList<Organism>();
    }
    
    public override void Step()
    {
        for (LinkedListNode<Organism> organismNode = Organisms.First; organismNode != null; organismNode = organismNode.Next)
        {
            Organism organism = organismNode.Value;
            
            //Move and run step for organism (organism does collision check with knowledge of exclusively what this chunk knows (which is enough)
            organism.Step();
        }
    }

    /// <summary>
    /// Gets the organism closest to this organism
    /// </summary>
    /// <param name="organism"></param>
    /// <returns>NOTE: This returns the original organism if no other organisms exist</returns>
    public override Organism NearestNeighbour(Organism organism)
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
        if (!World.IsInBounds(position))
            return true;

        //Check other organisms for collision
        return organism.CheckCollision(position, Organisms);
    }

    public override bool FindFirstCollision(Organism organism, Vector3 normalizedDirection, float length, out float t)
    {
        if (!World.IsInBounds(organism.Position + normalizedDirection * length))
        {
            //Still block movement normally upon hitting world limit
            t = 0;
            return true;
        }
        
        bool hit = FindMinimumIntersection(organism, normalizedDirection, length, Organisms, out t);
            
        float epsilon = 0.01f;
        t -= epsilon;
        return hit;
    }
    
    public override void Clear()
    {
        Organisms.Clear();
    }


    public override void AddOrganism(Organism organism)
    {
        Organisms.AddFirst(organism);
    }

    public override bool RemoveOrganism(Organism organism)
    {
        return Organisms.Remove(organism);
    }

    public override IEnumerable<Organism> GetOrganisms()
    {
        return Organisms;
    }
    
    public override int GetOrganismCount()
    {
        return Organisms.Count;
    }
    
    public override IEnumerable<Organism> OrganismsWithinRange(Organism organism, float range)
    {
        List<Organism> organismsWithinRange = new List<Organism>(50);
        foreach (Organism otherOrganism in Organisms)
        {
            if (Vector3.DistanceSquared(organism.Position, otherOrganism.Position) <= range * range)
            {
                organismsWithinRange.Add(otherOrganism);
            }
        }
        
        return organismsWithinRange;
    }
}