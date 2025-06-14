using System.Numerics;

namespace Continuum.Datastructures.SingleThreaded.RTree;

public class RLeafNode<T>(int minSize, int maxSize) : RNode<T>(minSize, maxSize)
    where T : IMinimumBoundable
{
    private const float Epsilon = 0.1f; //a small value to check whether an MBB lies on this mbb's edge
    public override int Count => LeafEntries.Count;
    protected override IEnumerable<IMinimumBoundable> Children => LeafEntries.Cast<IMinimumBoundable>();

    public List<T> LeafEntries = new(maxSize);

    public override void ForEach(Action<T> action)
    {
        for (int i = 0; i < LeafEntries.Count; i++)
        {
            action(LeafEntries[i]);
        }
    }
    public override void Search(Mbb searchArea, List<T> results)
    {
        for(int i = 0; i < LeafEntries.Count; i++)
        {
            T entry = LeafEntries[i];
            if (entry.GetMbb().Intersects(searchArea))
                results.Add(entry);
        }
    }

    public override void NearestNeighbour(T searchEntry, NearestNeighbour<T> nearest, Func<T,T,float> distance)
    {
        for (int i = 0; i < LeafEntries.Count; i++)
        {
            T entry = LeafEntries[i];
            if (entry.Equals(searchEntry))
                continue;
            float dist = distance(searchEntry, entry);
            if (dist < nearest.Distance)
            {
                nearest.Distance = dist;
                nearest.Entry = entry;
            }
        }
    }

    public override RLeafNode<T> ChooseLeaf(T entry)
    {
        return this;
    }
    public override void Insert(T entry, ref RNode<T> root)
    {
        if (Count < MaxSize)
        {
            LeafEntries.Add(entry);
            Mbb = Mbb.Enlarged(entry.GetMbb());
            AdjustTree(this, null, ref root);
            return;
        }

        (RNode<T> adjustedNode, RNode<T> newNode) = SplitNode(entry);
        AdjustTree(adjustedNode, newNode, ref root);
    }

    public override void ReInsert(RNode<T> node, int level, ref RNode<T> root)
    {
        //In this case reinserting at the right height is not possible so deconstruct the node and reinsert entries
        List<T> entries = [];
        node.GetAllLeafEntries(entries);
        for (int i = 0; i < entries.Count; i++)
        {
            root.Insert(entries[i], ref root);
        }
    }

    public override void GetAllLeafEntries(List<T> results)
    {
        results.AddRange(LeafEntries);
    }

    public override bool Delete(T entry, ref RNode<T> root)
    {
        if (LeafEntries.Remove(entry))
        {
            CondenseTree(ref root, []);
            return true;
        }

        return false;
    }

    public override bool UpdateMbb(T entry, Mbb newMbb, ref RNode<T> root)
    {
        //leaf was found by findLeaf so entry is guaranteed to be contained in this leaf
        Mbb oldMbb = entry.GetMbb();
        entry.SetMbb(newMbb);
        if (Mbb.Enlargement(newMbb) > 0)
        {
            LeafEntries.Remove(entry);
            CondenseTree(ref root, []);
            root.Insert(entry, ref root);
            return true;
        }

        //if entry's mbb was not near the edge moving it inwards can not lower the area of the nodes mbb since it is delimited by other entry's mbbs
        Vector3 smallVector = new Vector3(Epsilon);
        if (new Mbb(Mbb.Minimum + smallVector, Mbb.Maximum - smallVector).Contains(oldMbb))
        {
            return true;
        }

        //in this case the node's mbb should probably shrink so propagate changes
        CondenseTree(ref root, []);
        return true;
    }

    public override RLeafNode<T>? FindLeaf(T entry)
    {
        if (LeafEntries.Contains(entry))
            return this;
        return null;
    }

    public override void GetMbbsWithLevel(List<(Mbb, int)> list, RNode<T> root)
    {
        int level = GetLevel(root);
        list.Add((Mbb, level));
        foreach (T entry in LeafEntries)
        {
            list.Add((entry.GetMbb(), level + 1));
        }
    }

    public override (RNode<T>, RNode<T>) SplitNode(RNode<T> entry)
    {
        if (entry is RLeafNode<T> { LeafEntries.Count: 1 } leafNode)
            return SplitNode(leafNode.LeafEntries.Single());

        //this execution path should not be possible
        throw new Exception("Attempted to split a LeafNode by adding a node instead of a leafEntry");
    }

    private (RNode<T>, RNode<T>) SplitNode(T entry)
    {
        List<T> entries = LeafEntries;
        entries.Add(entry);
        RLeafNode<T> group1 = this;
        RLeafNode<T> group2 = new RLeafNode<T>(MinSize, MaxSize);
        void AddToGroup(RLeafNode<T> group, T item)
        {
            group.LeafEntries.Add(item);
            group.Mbb = group.Mbb.Enlarged(item.GetMbb());
        }
        SplitUtils.DistributeEntries(entries, group1, group2, AddToGroup, 
            (group, item) => { group.LeafEntries = new List<T>(MaxSize) { item }; },
            leaf => leaf.Count, MinSize);
        return (group1, group2);
    }
}