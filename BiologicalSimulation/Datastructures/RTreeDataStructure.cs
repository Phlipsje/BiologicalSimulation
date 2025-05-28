using System.Numerics;

namespace BioSim.Datastructures;

//simple Octree implementation for comparison purposes 
public class RTreeDataStructure : DataStructure
{
    public RTreeDataStructure(World world) : base(world)
    {
    }

    public override void Step()
    {
        throw new NotImplementedException();
    }

    public override Organism ClosestNeighbour(Organism organism)
    {
        throw new NotImplementedException();
    }

    public override bool CheckCollision(Organism organism, Vector3 position)
    {
        throw new NotImplementedException();
    }

    protected override IEnumerator<IOrganism> ToEnumerator()
    {
        throw new NotImplementedException();
    }
}
