using System.Numerics;
using BioSim;
using BioSim.Datastructures;

namespace BiologicalSimulation.Datastructures.RTree;

public class RTreeDataStructure(float moveRange) : DataStructure
{
    public override bool IsMultithreaded { get; } = false;
    
    private RTree<Organism> rTree = new RTree<Organism>(2, 10);
    private int organismCount;
    private Dictionary<Organism, List<Organism>> collisionBuffer = [];
    private HashSet<Organism> removedOrganisms = [];
    
    public override Task Step()
    {
        removedOrganisms.Clear();
        List<Organism> organisms = rTree.ToList(); //can't apply step directly to data structure as it contents will change
        
        for (int i = 0; i < organisms.Count; i++)
        {
            if(removedOrganisms.Contains(organisms[i])) //Make sure not to apply step to already removed organisms
                continue;
            //extra 0.1f for floating point errors in bounding boxes
            Vector3 collisionRange = new Vector3(organisms[i].Size * 2 + moveRange + 0.1f);
            Mbb possibleCollisionArea = new Mbb(organisms[i].Position - collisionRange, organisms[i].Position + collisionRange);
            List<Organism> collidables = rTree.Search(possibleCollisionArea);
            collisionBuffer[organisms[i]] = collidables;
            Vector3 oldPos = organisms[i].Position;
            organisms[i].Step();
            Vector3 newPos = organisms[i].Position;
            if (newPos != oldPos)
            {
                //update tree structure
                Mbb newMbb = organisms[i].GetMbb();
                organisms[i].Position = oldPos; //the entry is contained in the rTree with the oldPos so reset it to ensure the entry is found
                rTree.UpdateMbb(organisms[i], newMbb);
            }
        }

        return Task.CompletedTask;
    }

    public override Task Clear()
    {
        rTree.Clear();
        return Task.CompletedTask;
    }

    public override void AddOrganism(Organism organism)
    {
        rTree.Insert(organism);
        organismCount++;
    }

    public override bool RemoveOrganism(Organism organism)
    {
        if (rTree.Delete(organism))
        {
            removedOrganisms.Add(organism);
            organismCount--;
            return true;
        }

        return false;
    }
    
    public override Task GetOrganisms(out IEnumerable<Organism> organisms) //warning: do not perform spatial operations on the organisms through the IEnumerable, the datastructure will become stale
    {
        organisms = rTree.ToList();
        return Task.CompletedTask;
    }
    
    public override Task GetOrganismCount(out int count)
    {
        count = organismCount;
        return Task.CompletedTask;
    }

    public override bool CheckCollision(Organism organism, Vector3 position)
    {
        if (!World.IsInBounds(position))
            return true;
        
        //Check for other organisms
        if(organism.CheckCollision(position, collisionBuffer[organism]))
            return true;
        
        return false;
    }
    
    public override bool FindFirstCollision(Organism organism, Vector3 normalizedDirection, float length, out float t)
    {
        if (!World.IsInBounds(organism.Position + normalizedDirection * length))
        {
            //Still block movement normally upon hitting world limit
            t = 0;
            return true;
        }
        
        return FindMinimumIntersection(organism, normalizedDirection, length, collisionBuffer[organism], out t);
    }

    public override Organism? NearestNeighbour(Organism organism)
    {
        return rTree.NearestNeighbour(organism, Distance);
    }
    private float Distance(Organism a, Organism b)
    {
        float dist = 0;
        for (int i = 0; i < 3; i++)
        {
            float axisDist = a.Position[i] - b.Position[i];
            dist += axisDist * axisDist; 
        }
        return dist;
    }
}
