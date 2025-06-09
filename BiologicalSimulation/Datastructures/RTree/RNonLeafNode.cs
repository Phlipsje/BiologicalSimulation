using BioSim.Datastructures;

namespace BiologicalSimulation.Datastructures.RTree;

public class RNonLeafNode<T>(int minSize, int maxSize) : RNode<T>(minSize, maxSize) where T : IMinimumBoundable
{
    public List<RNode<T>> NodeEntries = new(maxSize);

    public override int Count => NodeEntries.Count;
    public override IEnumerable<IMinimumBoundable> Children => NodeEntries;

    public override void ForEach(Action<T> action)
    {
        for (int i = 0; i < Count; i++)
            NodeEntries[i].ForEach(action);
    }

    public override void Search(Mbb searchArea, ref List<T> results)
    {
        for (int i = 0; i < Count; i++)
        {
            if (NodeEntries[i].Mbb.Intersects(searchArea))
                NodeEntries[i].Search(searchArea, ref results);
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
                (RNode<T> L, RNode<T> LL) = SplitNode(node);
                AdjustTree(L, LL, ref root);
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
        (RNode<T> e1, RNode<T> e2) = SplitUtils.LinearPickSeeds(entries);
        entries.Remove(e1);
        entries.Remove(e2);
        RNonLeafNode<T> group1 = this;
        RNonLeafNode<T> group2 = new RNonLeafNode<T>(MinSize, MaxSize);
        group1.NodeEntries = new List<RNode<T>>(maxSize) { e1 };
        e1.Parent = group1;
        group1.Mbb = e1.GetMbb();
        group2.NodeEntries = new List<RNode<T>>(maxSize) { e2 };
        e2.Parent = group2;
        group2.Mbb = e2.GetMbb();
        /*Action<RNonLeafNode<T>, RNode<T>> addToGroup = (RNonLeafNode<T> group, RNode<T> node) =>
        {
            group.NodeEntries.Add(node);
            node.Parent = group;
            group.Mbb = group.Mbb.Enlarged(node.Mbb);
        };
        SplitUtils.DistributeEntries(entries, group1, group2, addToGroup, node => node.Count, minSize);*/
        for (int i = 0; i < entries.Count; i++)
        {
            RNode<T> currentEntry = entries[i];

            //if it is required to put all remaining entries into a group to ensure that group is filled to size m do so
            RNonLeafNode<T>? groupToFill = null;
            if (group1.Count + (entries.Count - i) <= group1.MinSize)
                groupToFill = group1;
            if (group2.Count + (entries.Count - i) <= group2.MinSize)
                groupToFill = group2;
            if (groupToFill != null)
            {
                for (int j = i; j < entries.Count; j++)
                {
                    currentEntry = entries[j];
                    groupToFill.NodeEntries.Add(currentEntry);
                    currentEntry.Parent = groupToFill;
                    groupToFill.Mbb = groupToFill.Mbb.Enlarged(currentEntry.GetMbb());
                }

                break;
            }

            Mbb group1Enlarged = group1.Mbb.Enlarged(currentEntry.GetMbb());
            Mbb group2Enlarged = group2.Mbb.Enlarged(currentEntry.GetMbb());
            if (group1Enlarged.Area < group2Enlarged.Area)
            {
                group1.NodeEntries.Add(currentEntry);
                currentEntry.Parent = group1;
                group1.Mbb = group1Enlarged;
                continue;
            }
            if (group1Enlarged.Area == group2Enlarged.Area)
            {
                if (group1.Count < group2.Count)
                {
                    group1.NodeEntries.Add(currentEntry);
                    currentEntry.Parent = group1;
                    group1.Mbb = group1Enlarged;
                }
                else
                {
                    group2.NodeEntries.Add(currentEntry);
                    currentEntry.Parent = group2;
                    group2.Mbb = group2Enlarged;
                }

                continue;
            }

            group2.NodeEntries.Add(currentEntry);
            currentEntry.Parent = group2;
            group2.Mbb = group2Enlarged;
        }

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