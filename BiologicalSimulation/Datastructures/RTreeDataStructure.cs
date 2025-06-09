using System.Numerics;

namespace BioSim.Datastructures;

//simple Octree implementation for comparison purposes 
public class RTreeDataStructure(float moveRange) : DataStructure
{
    private RTree<Organism> rTree = new RTree<Organism>(2, 10);
    private int organismCount = 0;
    private Dictionary<Organism, List<Organism>> collisionBuffer = [];
    
    public override void Step()
    {
        List<Organism> organisms = rTree.ToList(); //can't apply step directly to data structure as it contents will change
        for (int i = 0; i < organisms.Count; i++)
        {
            Organism organism = organisms[i];
            Vector3 collisionRange = new Vector3(organism.Size * 2 + moveRange);
            Mbb possibleCollisionArea = new Mbb(organism.Position - collisionRange, organism.Position + collisionRange);
            List<Organism> collidables = rTree.Search(possibleCollisionArea);
            collisionBuffer[organism] = collidables;
            Vector3 oldPos = organisms[i].Position;
            organism.Step([]);
            if (organisms[i].Position != oldPos)
            {
                //update data structure
                Mbb newMbb = organism.GetMbb();
                organism.Position = oldPos; //the entry is contained in the rTree with the oldPos so reset it to ensure the entry is found
                rTree.UpdateMbb(organism, newMbb);
            }
        }
    }

    public override void AddOrganism(Organism organism)
    {
        rTree.Insert(organism);
        organismCount++;
    }

    public void RemoveOrganism(Organism organism)
    {
        if (rTree.Delete(organism))
            organismCount--;
    }
    
    public override IEnumerable<Organism> GetOrganisms() //warning: do not perform spatial operations on the organisms through the IEnumerable, the datastructure will become stale
    {
        return rTree.ToList();
    }
    
    public override int GetOrganismCount()
    {
        return organismCount;
    }

    public override bool CheckCollision(Organism organism, Vector3 position, List<LinkedList<Organism>> organismLists)
    {
        if (!World.IsInBounds(position))
            return true;
        
        foreach (Organism otherOrganism in collisionBuffer[organism])
        {
            //Cannot be a collision with itself
            if(otherOrganism == organism)
                continue;
            
            //Checks collision by checking distance between spheres
            float x = position.X - otherOrganism.Position.X;
            float xSquared = x * x;
            float y = position.Y - otherOrganism.Position.Y;
            float ySquared = y * y;
            float z = position.Z - otherOrganism.Position.Z;
            float zSquared = z * z;
            float sizes = organism.Size + otherOrganism.Size;
            if (xSquared + ySquared + zSquared <= sizes * sizes)
                return true;
        }
        return false;
    }

    public override Organism ClosestNeighbour(Organism organism)
    {
        throw new NotImplementedException();
    }
}
