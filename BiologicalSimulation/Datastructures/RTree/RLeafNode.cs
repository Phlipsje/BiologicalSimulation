using System.Numerics;
using BiologicalSimulation.Datastructures.RTree;

namespace BioSim.Datastructures;

public class RLeafNode<T>(int minSize, int maxSize) : RNode<T>(minSize, maxSize)
    where T : IMinimumBoundable
{
    private const float Epsilon = 0.01f; //a small value to check whether an MBB lies on this mbb's edge
    public override int Count => LeafEntries.Count;
    public override IEnumerable<IMinimumBoundable> Children => LeafEntries.Cast<IMinimumBoundable>();

    public List<T> LeafEntries = new(maxSize);

    public override void ForEach(Action<T> action)
    {
        foreach (T entry in LeafEntries)
        {
            action(entry);
        }
    }
    public override void Search(Mbb searchArea, ref List<T> results)
    {
        foreach (T entry in LeafEntries)
        {
            if (entry.GetMbb().Intersects(searchArea))
                results.Add(entry);
        }
    }
    public override RLeafNode<T> ChooseLeaf(T entry)
    {
        return this;
    }
    public override void Insert(T entry, ref RNode<T> root)
    {
        if (Count < maxSize)
        {
            LeafEntries.Add(entry);
            Mbb = Mbb.Enlarged(entry.GetMbb());
            AdjustTree(this, null, ref root);
            return;
        }

        (RNode<T> L, RNode<T> LL) = SplitNode(entry);
        AdjustTree(L, LL, ref root);
    }

    public override void ReInsert(RNode<T> node, int level, ref RNode<T> root)
    {
        //In this case reinserting at the right height is not possible so deconstruct the node and reinsert entries
        List<T> entries = [];
        node.GetAllLeafEntries(entries);
        foreach (var entry in entries)
        {
            root.Insert(entry, ref root);
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
        entry.SetMbb(newMbb);
        if (Mbb.Enlarged(newMbb).Area > Mbb.Area)
        {
            LeafEntries.Remove(entry);
            CondenseTree(ref root, []);
            root.Insert(entry, ref root);
            return true;
        }

        //if entry's mbb is not near the edge moving it inwards can not lower the area of the nodes mbb since it is delimited by other entry's mbbs
        Vector3 smallVector = new Vector3(Epsilon);
        if (new Mbb(Mbb.Minimum + smallVector, Mbb.Maximum - smallVector).Contains(Mbb))
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
        (T e1, T e2) = SplitUtils.LinearPickSeeds(entries);
        entries.Remove(e1);
        entries.Remove(e2);
        RLeafNode<T> group1 = this;
        RLeafNode<T> group2 = new RLeafNode<T>(MinSize, MaxSize);
        group1.LeafEntries = new List<T>(maxSize) { e1 };
        group1.Mbb = e1.GetMbb();
        group2.LeafEntries = new List<T>(maxSize) { e2 };
        group2.Mbb = e2.GetMbb();
        for (int i = 0; i < entries.Count; i++)
        {
            T currentEntry = entries[i];

            //if it is required to put all remaining entries into a group to ensure that group is filled to size m do so
            RLeafNode<T>? groupToFill = null;
            if (group1.Count + (entries.Count - i) <= group1.MinSize)
                groupToFill = group1;
            if (group2.Count + (entries.Count - i) <= group2.MinSize)
                groupToFill = group2;
            if (groupToFill != null)
            {
                for (int j = i; j < entries.Count; j++)
                {
                    currentEntry = entries[j];
                    groupToFill.LeafEntries.Add(currentEntry);
                    groupToFill.Mbb = groupToFill.Mbb.Enlarged(currentEntry.GetMbb());
                }

                break;
            }

            Mbb group1Enlarged = group1.Mbb.Enlarged(currentEntry.GetMbb());
            Mbb group2Enlarged = group2.Mbb.Enlarged(currentEntry.GetMbb());
            if (group1Enlarged.Area < group2Enlarged.Area)
            {
                group1.LeafEntries.Add(currentEntry);
                group1.Mbb = group1Enlarged;
                continue;
            }

            if (group1Enlarged.Area == group2Enlarged.Area)
            {
                if (group1.Count < group2.Count)
                {
                    group1.LeafEntries.Add(currentEntry);
                    group1.Mbb = group1Enlarged;
                }
                else
                {
                    group2.LeafEntries.Add(currentEntry);
                    group2.Mbb = group2Enlarged;
                }

                continue;
            }

            group2.LeafEntries.Add(currentEntry);
            group2.Mbb = group2Enlarged;
        }

        return (group1, group2);
    }
}