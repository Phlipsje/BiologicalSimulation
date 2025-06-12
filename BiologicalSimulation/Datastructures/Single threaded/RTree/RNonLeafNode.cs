using BioSim.Datastructures;

namespace BiologicalSimulation.Datastructures.RTree;

public class RNonLeafNode<T>(int minSize, int maxSize) : RNode<T>(minSize, maxSize) where T : IMinimumBoundable
{
    public List<RNode<T>> NodeEntries = new(maxSize);

    public override int Count => NodeEntries.Count;
    protected override IEnumerable<IMinimumBoundable> Children => NodeEntries;

    public override void ForEach(Action<T> action)
    {
        for (int i = 0; i < Count; i++)
            NodeEntries[i].ForEach(action);
    }

    public override void Search(Mbb searchArea, List<T> results)
    {
        for (int i = 0; i < Count; i++)
        {
            if (NodeEntries[i].Mbb.Intersects(searchArea))
                NodeEntries[i].Search(searchArea, results);
        }
    }

    public override void NearestNeighbour(T searchEntry, NearestNeighbour<T> nearest, Func<T,T,float> distance)
    {
        PriorityQueue<(RNode<T>, float), float> pq = new();
        foreach (var node in NodeEntries)
        {
            float minDist = node.Mbb.MinDist(searchEntry.GetMbb().Position);
            pq.Enqueue((node, minDist), minDist);
        }

        while (pq.Count != 0)
        {
            (RNode<T> node, float dist) = pq.Dequeue();
            if (dist > nearest.Distance)
                break; // Prune upward — no better result possible
            node.NearestNeighbour(searchEntry, nearest, distance);
        }
    }

    public override void GetMbbsWithLevel(List<(Mbb, int)> list, RNode<T> root)
    {
        int level = GetLevel(root);
        list.Add((Mbb, level));
        for (int i = 0; i < Count; i++)
        {
            NodeEntries[i].GetMbbsWithLevel(list, root);
        }
    }

    public override void Insert(T entry, ref RNode<T> root)
    {
        if (Count == 0 && this.Equals(root)) //special exception where root should become a leaf node
        {
            root = new RLeafNode<T>(MinSize, MaxSize);
            ((RLeafNode<T>)root).LeafEntries.Add(entry);
            root.Mbb = entry.GetMbb();
            return;
        }

        RLeafNode<T> leaf = ChooseLeaf(entry);
        leaf.Insert(entry, ref root);
    }

    public override void ReInsert(RNode<T> node, int level, ref RNode<T> root)
    {
        if (this.GetLevel(root) == level - 1)
        {
            if (Count < MaxSize)
            {
                NodeEntries.Add(node);
                Mbb = Mbb.Enlarged(node.Mbb);
                node.Parent = this;
                AdjustTree(this, null, ref root);
            }
            else
            {
                (RNode<T> adjustedNode, RNode<T> newNode) = SplitNode(node);
                AdjustTree(adjustedNode, newNode, ref root);
            }

            return;
        }
        if (Count == 0)
        {
            //In this case reinserting at the right height is not possible so deconstruct the node and reinsert entries
            List<T> entries = [];
            node.GetAllLeafEntries(entries);
            foreach (T entry in entries)
            {
                root.Insert(entry, ref root);
            }

            return;
        }
        RNode<T> best = LeastEnlargedChild(node.Mbb);
        best.ReInsert(node, level, ref root);
    }

    public override void GetAllLeafEntries(List<T> results)
    {
        foreach (RNode<T> node in NodeEntries)
        {
            node.GetAllLeafEntries(results);
        }
    }

    public override bool Delete(T entry, ref RNode<T> root)
    {
        RLeafNode<T>? leaf = FindLeaf(entry);
        if (leaf == null)
            return false;
        return leaf.Delete(entry, ref root);
    }

    public override bool UpdateMbb(T entry, Mbb newMbb, ref RNode<T> root)
    {
        RLeafNode<T>? leaf = FindLeaf(entry);
        if (leaf == null)
            return false;
        return leaf.UpdateMbb(entry, newMbb, ref root);
    }
    
    public override RLeafNode<T>? FindLeaf(T entry)
    {
        for (int i = 0; i < Count; i++)
        {
            if (NodeEntries[i].Mbb.Contains(entry.GetMbb()))
            {
                RLeafNode<T>? result = NodeEntries[i].FindLeaf(entry);
                if (result != null)
                    return result;
            }
        }

        return null;
    }

    public override RLeafNode<T> ChooseLeaf(T entry)
    {
        RNode<T> node = LeastEnlargedChild(entry.GetMbb());
        return node.ChooseLeaf(entry);
    }
    public override (RNode<T>, RNode<T>) SplitNode(RNode<T> entry)
    {
        List<RNode<T>> entries = NodeEntries;
        entries.Add(entry);
        RNonLeafNode<T> group1 = this;
        RNonLeafNode<T> group2 = new RNonLeafNode<T>(MinSize, MaxSize);
        void InsertInitial(RNonLeafNode<T> group, RNode<T> node)
        {
            group.NodeEntries = new List<RNode<T>>(MaxSize) { node };
            node.Parent = group;
        }
        void AddToGroup(RNonLeafNode<T> group, RNode<T> node)
        {
            group.NodeEntries.Add(node);
            node.Parent = group;
            group.Mbb = group.Mbb.Enlarged(node.Mbb);
        }
        SplitUtils.DistributeEntries(entries, group1, group2, AddToGroup, InsertInitial, node => node.Count, MinSize);
        return (group1, group2);
    }

    private RNode<T> LeastEnlargedChild(Mbb addedMbb)
    {
        RNode<T> best = NodeEntries[0];
        float leastEnlargement = best.Mbb.Enlargement(addedMbb);
        for (int i = 1; i < Count; i++)
        {
            float enlargement = NodeEntries[i].Mbb.Enlargement(addedMbb);
            
            if (enlargement > leastEnlargement)
                continue;
            if (enlargement == leastEnlargement && best.Mbb.Area <= NodeEntries[i].Mbb.Area)
                continue;
            best = NodeEntries[i];
            leastEnlargement = enlargement;
        }
        return best;
    }
}