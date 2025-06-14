using System.Numerics;
using Continuum;

namespace Continuum.Datastructures.SingleThreaded.RTree;

public class RTreeDataStructure(float orthogonalMoveRange, int minimumBranchingFactor = 2, int maximumBranchingFactor = 10) : SingleThreadedDataStructure
{
    public override bool IsMultithreaded { get; } = false;
    
    private RTree<Organism> rTree = new RTree<Organism>(minimumBranchingFactor, maximumBranchingFactor);
    private int organismCount;
    private Dictionary<Organism, List<Organism>> collisionBuffer = [];
    private HashSet<Organism> removedOrganisms = [];
    private List<Organism> organismsInRangeResult = [];
    
    public override void Step()
    {
        removedOrganisms.Clear();
        List<Organism> organisms = rTree.ToList(); //can't apply step directly to data structure as it contents will change
        if (World.RandomisedExecutionOrder)
            HelperFunctions.KnuthShuffle(organisms);
        for (int i = 0; i < organisms.Count; i++)
        {
            Organism organism = organisms[i];
            if(removedOrganisms.Contains(organism)) //Make sure not to apply step to already removed organisms
                continue;
            Vector3 collisionRange = new Vector3(organism.Size * 3 + orthogonalMoveRange);
            Mbb possibleCollisionArea = new Mbb(organism.Position - collisionRange, organism.Position + collisionRange);
            List<Organism> collidables = rTree.Search(possibleCollisionArea);
            collisionBuffer[organism] = collidables;
            //Vector3 oldPos = currentOrganism.Position;
            organism.Step();
            //Vector3 newPos = currentOrganism.Position;
            /*if (newPos != oldPos)
            {
                //update tree structure
                Mbb newMbb = currentOrganism.GetMbb();
                currentOrganism.Position = oldPos; //the entry is contained in the rTree with the oldPos so reset it to ensure the entry is found
                if (!rTree.UpdateMbb(currentOrganism, newMbb))
                {
                    //entry was not found but could be caused by an update through splitnode in insert
                    currentOrganism.Position = possibleNewPositionInTree; //try to find with possible new position
                    if(!rTree.UpdateMbb(currentOrganism, newMbb))
                        throw new Exception();
                }
            }*/
        }
    }
    
    public void OnReposition(Organism organism, Mbb newPos)
    {
        if (!rTree.UpdateMbb(organism, newPos))
            throw new Exception();
    }
    
    public override void Clear()
    {
        rTree.Clear();
    }

    public override void AddOrganism(Organism organism)
    {
        rTree.Insert(organism);
        organism.OnReposition += OnReposition;
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
    
    public override IEnumerable<Organism> GetOrganisms() //warning: do not perform spatial operations on the organisms through the IEnumerable, the datastructure will become stale
    {
        return rTree.ToList();
    }
    
    public override int GetOrganismCount()
    {
        return organismCount;
    }

    public override bool CheckCollision(Organism organism, Vector3 position)
    {
        if (!World.IsInBounds(position))
            return true;
        
        //Check for other organisms
        return organism.CheckCollision(position, collisionBuffer[organism],
            otherOrganism => organism == otherOrganism || removedOrganisms.Contains(otherOrganism));
    }

    public override bool FindFirstCollision(Organism organism, Vector3 normalizedDirection, float length, out float t)
    {
        if (!World.IsInBounds(organism.Position + normalizedDirection * length))
        {
            //Still block movement normally upon hitting world limit
            t = 0;
            return true;
        }

        return FindMinimumIntersection(organism, normalizedDirection, length, collisionBuffer[organism], 
            otherOragism => organism == otherOragism || removedOrganisms.Contains(otherOragism) , out t);
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
    
    
    public override IEnumerable<Organism> OrganismsWithinRange(Organism organism, float range)
    {
        Vector3 rangeVector = new Vector3(range);
        Mbb searchArea = new Mbb(organism.Position - rangeVector, organism.Position + rangeVector);
        List<Organism> possibleInRange = rTree.Search(searchArea);
        organismsInRangeResult.Clear();
        float rangeSquared = range * range;
        foreach (Organism otherOrganism in possibleInRange)
        {
            if(organism == otherOrganism || (otherOrganism.Position - organism.Position).LengthSquared() > rangeSquared)
                continue;
            organismsInRangeResult.Add(otherOrganism);
        }
        return organismsInRangeResult;
    }
}
