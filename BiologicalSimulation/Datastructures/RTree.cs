namespace BioSim.Datastructures;
using System.Numerics;

public class RTree<T> where T : IMinimumBoundable
{
    private RNode<T> root;
    public List<T> Search(Mbb searchArea)
    {
        throw new NotImplementedException();
    }
}

public abstract class RNode<T> where T : IMinimumBoundable
{
    public abstract void Search(Mbb searchArea, ref List<T> results);
    public abstract void Insert(LeafEntry<T> entry);
    public abstract RLeafNode<T> ChooseLeaf(LeafEntry<T> entry);
}

public class RLeafNode<T>(int m, int M) : RNode<T>
    where T : IMinimumBoundable
{
    public int Count = 0;
    public LeafEntry<T>?[] LeafEntries = new LeafEntry<T>?[M];
    private int _m = m;

    public override void Search(Mbb searchArea, ref List<T> results)
    {
        for (int i = 0; i < M; i++)
        {
            if(LeafEntries[i].HasValue && LeafEntries[i].Value.Mbb.Intersects(searchArea))
                results.Add(LeafEntries[i].Value.Item);
        }
    }

    public override void Insert(LeafEntry<T> entry)
    {
        throw new NotImplementedException();
    }

    public override RLeafNode<T> ChooseLeaf(LeafEntry<T> entry)
    {
        return this;
    }
}

public class RNonLeafNode<T>(int m, int M) : RNode<T> where T : IMinimumBoundable
{
    public NodeEntry<T>?[] NodeEntries = new NodeEntry<T>?[M];
    public int Count = 0;
    private int _m = m, _M = M;
    
    //should find a better way for this annoying null system to avoid lists
    private int firstNonEmptyIndex
    {
        get
        {
            for(int i = 0; i < M; i++)
                if (NodeEntries[i].HasValue)
                    return i;
            throw new Exception("node contained no entries? should be more than _m");
        }
    }

    public override void Search(Mbb searchArea, ref List<T> results)
    {
        for (int i = 0; i < M; i++)
        {
            if(NodeEntries[i].HasValue && NodeEntries[i].Value.Mbb.Intersects(searchArea))
                NodeEntries[i].Value.Node.Search(searchArea, ref results);
        }
    }

    public override void Insert(LeafEntry<T> entry)
    {
        throw new NotImplementedException();
    }

    public override RLeafNode<T> ChooseLeaf(LeafEntry<T> entry)
    {
        NodeEntry<T> best = NodeEntries[firstNonEmptyIndex].Value;
        float leastEnlargement = NodeEntries[firstNonEmptyIndex].Value.Mbb.Enlargement(entry.Mbb);
        for (int i = firstNonEmptyIndex + 1; i < M; i++)
        {
            if(!NodeEntries[i].HasValue)
                continue;
            float enlargement = NodeEntries[i].Value.Mbb.Enlargement(entry.Mbb);
            if (enlargement > leastEnlargement)
                continue;
            if (enlargement == leastEnlargement && best.Mbb.Area <= NodeEntries[i].Value.Mbb.Area)
                continue;
            best = NodeEntries[i].Value;
        }
        return best.Node.ChooseLeaf(entry);
    }
}

public struct LeafEntry<T>(Mbb mbb, T item)
{
    public T Item = item;
    public Mbb Mbb = mbb;
}

public struct NodeEntry<T>(Mbb mbb, RNode<T> node) where T : IMinimumBoundable
{
    public RNode<T> Node = node;
    public Mbb Mbb = mbb;
}

public struct Mbb(Vector3 minimum, Vector3 maximum)
{
    public Vector3 Minimum = minimum;
    public Vector3 Maximum = maximum;
    public float Area => (Maximum.X - Minimum.X) * (Maximum.Y - Minimum.Y) * (Maximum.Z - Minimum.Z);

    public bool Intersects(Mbb other)
    {
        return Minimum.X < other.Maximum.X && other.Minimum.X < Maximum.X &&
               Minimum.Y < other.Maximum.Y && other.Minimum.Y < Maximum.Y &&
               Minimum.Z < other.Maximum.Z && other.Minimum.Z < Maximum.Z;

    }

    public bool Contains(Mbb other)
    {
        return Minimum.X < other.Minimum.X && Minimum.Y < other.Minimum.Y && Minimum.Z < other.Minimum.Z &&
               Maximum.X > other.Maximum.X && Maximum.Y > other.Maximum.Y && Maximum.Z > other.Maximum.Z;
    }
    
    //the enlargement to this minimum bounding box needed to fit the other mbb inside it. Also returns the new Mbb
    public float Enlargement(Mbb other)
    {
        if (Contains(other))
            return 0;
        float enlargement = Enlarge(other).Area - Area;
        return enlargement;
    }

    public Mbb Enlarge(Mbb other)
    {
        Vector3 newMinimum = new Vector3(MathF.Min(Minimum.X, other.Minimum.X), MathF.Min(Minimum.Y, other.Minimum.Y),
            MathF.Min(Minimum.Z, other.Minimum.Z));
        Vector3 newMaximum = new Vector3(MathF.Max(Maximum.X, other.Maximum.X), MathF.Max(Maximum.Y, other.Maximum.Y),
            MathF.Max(Maximum.Z, other.Maximum.Z));
        return new Mbb(newMinimum, newMaximum);
    }
}

public interface IMinimumBoundable
{
    public Mbb GetMbb();
}