namespace BioSim.Datastructures;
using System.Numerics;

public class RTree<T>(int m, int M)
    where T : IMinimumBoundable
{
    public RNode<T> Root => root != null ? root : new RLeafNode<T>(_m, _M); //public for testing
    
    private int _m = m, _M = M;
    private RNode<T>? root = null;
    public List<T> Search(Mbb searchArea)
    {
        if (root == null)
            throw new Exception("Tree is empty");
        List<T> results = [];
        root.Search(searchArea, ref results);
        return results;
    }
    public void Insert(T entry)
    {
        if (root == null)
        {
            RLeafNode<T> newRoot = new RLeafNode<T>(_m, M);
            newRoot.LeafEntries.Add(entry);
            newRoot.Mbb = entry.GetMbb();
            root = newRoot;
            return;
        }
        root.Insert(entry, ref root);
    }

    public void Delete(T entry)
    {
        root.Delete(entry, ref root);
        if (root.Count == 0)
            root = null;
    }

    public List<(Mbb,int)> GetMbbsWithLevel()
    {
        List<(Mbb, int)> list = [];
        if (root != null)
            root.GetMbbsWithLevel(list, root);
        return list;
    }

    public int DisconnectedParentCount()
    {
        if (root != null)
            return root.DisconnectedParentCount(root);
        return 0;
    }

    public int WrongParentCount()
    {
        if (root != null)
            return root.WrongParentCount();
        return 0;
    }
}

public abstract class RNode<T>(int m, int M) : IMinimumBoundable where T : IMinimumBoundable
{
    protected int _m = m;
    protected int _M = M;
    public RNode<T> Parent = null;
    public abstract int Count { get; }
    public Mbb Mbb;
    public Mbb GetMbb() { return Mbb; }

    public abstract int WrongParentCount();
    public abstract int DisconnectedParentCount(RNode<T> root);
    public abstract RLeafNode<T>? ThoroughContains(T entry);
    public abstract void Search(Mbb searchArea, ref List<T> results);
    public abstract void Insert(T entry, ref RNode<T> root);
    public abstract void ReInsert(RNode<T> node, int level, ref RNode<T> root);
    public abstract void Delete(T entry, ref RNode<T> root);
    public abstract void CondenseTree(ref RNode<T> root, List<(RNode<T>, int)> eliminatedNodes);
    public abstract RLeafNode<T>? FindLeaf(T entry);
    public abstract void GetMbbsWithLevel(List<(Mbb, int)> list, RNode<T> root);
    public abstract void RecalculateMbb();
    public abstract void GetAllLeafEntries(List<T> results);
    public int GetLevel(RNode<T> root)
    {
        if (this.Equals(root))
            return 0;
        return 1 + Parent.GetLevel(root);
    }
    protected void AdjustTree(RNode<T> L, RNode<T>? LL, ref RNode<T> root)
    {
        if (L.Equals(root)) //stop condition
        {
            if(LL == null)//Then root can stay the way it is
                return;
            //This means the root was split into L and LL
            RNonLeafNode<T> newRoot = new RNonLeafNode<T>(_m, _M);
            newRoot.Children = [L, LL];
            newRoot.Mbb = L.Mbb.Enlarged(LL.Mbb);
            L.Parent = newRoot;
            LL.Parent = newRoot;
            root = newRoot;
            return;
        }
        RNonLeafNode<T> P = (RNonLeafNode<T>)L.Parent; //a parent of a node should never be a leaf node.
        if (LL == null)
        {
            P.Mbb = P.Mbb.Enlarged(L.Mbb); 
            P.AdjustTree(P, null, ref root);
            return;
        }
        if (P.Count < _M)
        { 
            P.Mbb = P.Mbb.Enlarged(L.Mbb); 
            P.Children.Add(LL);
            LL.Parent = P;
            P.Mbb = P.Mbb.Enlarged(LL.Mbb);
            P.AdjustTree(P, null, ref root);
            return;
        }
        (RNode<T> N, RNode<T> NN) = P.SplitNode(LL);
        AdjustTree(N, NN, ref root);
    }
    
    public abstract RLeafNode<T> ChooseLeaf(T entry);
    public abstract (RNode<T>, RNode<T>) SplitNode(RNode<T> entry);
}

public class RLeafNode<T>(int m, int M) : RNode<T>(m,M)
    where T : IMinimumBoundable
{
    public override int Count => LeafEntries.Count;
    public List<T> LeafEntries = new (M);
    public override int WrongParentCount()
    {
        return 0;
    }

    public override int DisconnectedParentCount(RNode<T> root)
    {
        if (Parent == null && !this.Equals(root))
            return 1;
        else return 0;
    }

    public override RLeafNode<T>? ThoroughContains(T entry)
    {
        return LeafEntries.Contains(entry) ? this : null;
    }

    public override void Search(Mbb searchArea, ref List<T> results)
    {
        for (int i = 0; i < Count; i++)
        {
            if(LeafEntries[i].GetMbb().Intersects(searchArea))
                results.Add(LeafEntries[i]);
        }
    }
    public override RLeafNode<T> ChooseLeaf(T entry)
    {
        return this;
    }

    public override void Insert(T entry, ref RNode<T> root)
    {
        if (Count < M)
        {
            LeafEntries.Add(entry);
            Mbb = Mbb.Enlarged(entry.GetMbb());
            AdjustTree(this, null, ref root);
            return;
        }
        (RNode<T> L, RNode<T> LL) = SplitNode(entry);
        AdjustTree(L,LL, ref root);
    }

    public override void ReInsert(RNode<T> node, int level, ref RNode<T> root)
    {
        //In this case reinserting at the right height is not possible so deconstruct the node and reinsert entries
        List<T> entries = [];
        node.GetAllLeafEntries(entries);
        for(int i = 0; i < entries.Count; i++)
        {
            root.Insert(entries[i], ref root);
        }
        return;
        throw new Exception("Can't insert node in a leaf node. Reinsert should not be able to reach this far down");
    }

    public override void GetAllLeafEntries(List<T> results)
    {
        results.AddRange(LeafEntries);
    }
    public override void Delete(T entry, ref RNode<T> root)
    {
        LeafEntries.Remove(entry);
        CondenseTree(ref root, []);
    }

    public override void CondenseTree(ref RNode<T> root, List<(RNode<T>, int)> eliminatedNodes)
    {
        if (this.Equals(root)) //Stop and reinsert, when at root
        {
            RecalculateMbb();
            //Since the root is at leaf level it should not be possible for there to be eliminated nodes
            if (eliminatedNodes.Count > 0)
                throw new Exception(
                    "Unintended behaviour, eliminatedNodes should be empty when condensing a leafNode root");
            return;
        }
        RNonLeafNode<T> P = (RNonLeafNode<T>)Parent; //parent can't be a leaf
        if (Count < _m)
        {
            if (P.Children.Remove(this) == false)
                throw new Exception("Child was not present in its parent!");
            eliminatedNodes.Add((this, this.GetLevel(root)));
            this.Parent = null;
        }
        else
        {
            //adjust mbb of node
            RecalculateMbb();
        }
        P.CondenseTree(ref root, eliminatedNodes);
    }
    public override void RecalculateMbb()
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float maxZ = float.MinValue;
        for (int i = 0; i < Count; i++)
        {
            Vector3 min = LeafEntries[i].GetMbb().Minimum;
            Vector3 max = LeafEntries[i].GetMbb().Maximum;
            if (min.X < minX)
                minX = min.X;
            if (min.Y < minY)
                minY = min.Y;
            if (min.Z < minZ)
                minZ = min.Z;
            if (max.X > maxX)
                maxX = max.X;
            if (max.Y > maxY)
                maxY = max.Y;
            if (max.Z > maxZ)
                maxZ = max.Z;
            Mbb = new Mbb(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        }
    }
    public override RLeafNode<T>? FindLeaf(T entry)
    {
        if (LeafEntries.Contains(entry))
            return this;
        return null;
    }

    public override void GetMbbsWithLevel(List<(Mbb,int)> list, RNode<T> root)
    {
        int level = GetLevel(root);
        list.Add((Mbb, level));
        for (int i = 0; i < Count; i++)
        {
            list.Add((LeafEntries[i].GetMbb(), level + 1));
        }
    }

    public override (RNode<T>, RNode<T>) SplitNode(RNode<T> entry)
    {
        if (entry is RLeafNode<T> && ((RLeafNode<T>)entry).LeafEntries.Count == 1)
            return SplitNode(((RLeafNode<T>)entry).LeafEntries[0]);
        
        throw new NotImplementedException();
    }
    
    private (RNode<T>, RNode<T>) SplitNode(T entry)
    {
        List<T> entries = LeafEntries;
        entries.Add(entry);
        (T e1, T e2) = LinearPickSeeds(entries);
        entries.Remove(e1);
        entries.Remove(e2);
        RLeafNode<T> group1 = this;
        RLeafNode<T> group2 = new RLeafNode<T>(_m, _M);
        group1.LeafEntries = new List<T>(M) { e1 };
        group1.Mbb = e1.GetMbb();
        group2.LeafEntries = new List<T>(M) { e2 };
        group2.Mbb = e2.GetMbb();
        for (int i = 0; i < entries.Count; i++)
        {
            T currentEntry = entries[i];
            
            //if it is required to put all remaining entries into a group to ensure that group is filled to size m do so
            RLeafNode<T>? groupToFill = null;
            if (group1.Count + (entries.Count - i) <= group1._m)
                groupToFill = group1;
            if (group2.Count + (entries.Count - i) <= group2._m)
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
    private (T, T) LinearPickSeeds(List<T> entries)
    {
        //Find extreme rectangles along all dimensions and record total width of entries
        T highLowX = entries[0];
        T lowHighX = entries[0];
        T highLowY = entries[0];
        T lowHighY = entries[0];
        T highLowZ = entries[0];
        T lowHighZ = entries[0];
        float minX = entries[0].GetMbb().Minimum.X;
        float maxX = entries[0].GetMbb().Maximum.X;
        float minY = entries[0].GetMbb().Minimum.Y;
        float maxY = entries[0].GetMbb().Maximum.Y;
        float minZ = entries[0].GetMbb().Minimum.Z;
        float maxZ = entries[0].GetMbb().Maximum.Z;
        for (int i = 1; i < entries.Count; i++)
        {
            T entry = entries[i];
            Mbb entryMbb = entry.GetMbb();
            if (entryMbb.Minimum.X > highLowX.GetMbb().Minimum.X)
                highLowX = entry;
            if (entryMbb.Maximum.X < lowHighX.GetMbb().Maximum.X)
                lowHighX = entry;
            if (entryMbb.Minimum.Y > highLowY.GetMbb().Minimum.Y)
                highLowY = entry;
            if (entryMbb.Maximum.Y < lowHighY.GetMbb().Maximum.Y)
                lowHighY = entry;
            if (entryMbb.Minimum.Z > highLowZ.GetMbb().Minimum.Z)
                highLowZ = entry;
            if (entryMbb.Maximum.Z < lowHighZ.GetMbb().Maximum.Z)
                lowHighZ = entry;
            if (entryMbb.Minimum.X < minX)
                minX = entryMbb.Minimum.X;
            if (entryMbb.Minimum.Y < minY)
                minY = entryMbb.Minimum.Y;
            if (entryMbb.Minimum.Z < minZ)
                minZ = entryMbb.Minimum.Z;
            if (entryMbb.Maximum.X > maxX)
                maxX = entryMbb.Maximum.X;
            if (entryMbb.Maximum.Y > maxY)
                maxY = entryMbb.Maximum.Y;
            if (entryMbb.Maximum.Z > maxZ)
                maxZ = entryMbb.Maximum.Z;
        }
        //Adjust for shape of the rectangle cluster
        float widthX = maxX - minX;
        float widthY = maxY - minX;
        float widthZ = maxZ - minZ;
        float normSepX = (highLowX.GetMbb().Minimum.X - lowHighX.GetMbb().Maximum.X) / widthX;
        float normSepY = (highLowY.GetMbb().Minimum.Y - lowHighY.GetMbb().Maximum.Y) / widthY;
        float normSepZ = (highLowZ.GetMbb().Minimum.Z - lowHighZ.GetMbb().Maximum.Z) / widthZ;
        
        //return the pair with greatest seperation
        if (normSepX > normSepY && normSepX > normSepZ)
            return (highLowX, lowHighX);
        if (normSepY > normSepZ)
            return (highLowY, lowHighY);
        return (highLowZ, lowHighZ);
    }
}

public class RNonLeafNode<T>(int m, int M) : RNode<T>(m,M) where T : IMinimumBoundable
{
    public List<RNode<T>> Children = new (M);
    public override int Count => Children.Count;

    public override int WrongParentCount()
    {
        int n = 0;
        foreach (var node in Children)
        {
            if (node.Parent != this)
                n++;
            n += node.WrongParentCount();
        }
        return n;
    }

    public override int DisconnectedParentCount(RNode<T> root)
    {
        int n = Children.Select(x => x.DisconnectedParentCount(root)).Sum();
        if (Parent == null && !this.Equals(root))
            n++;
        return n;
    }

    public override RLeafNode<T>? ThoroughContains(T entry)
    {
        
        foreach (var node in Children)
        {
            RLeafNode<T>? leaf = node.ThoroughContains(entry);
            if (leaf != null)
                return leaf;
        }
        return null;
    }

    public override void Search(Mbb searchArea, ref List<T> results)
    {
        for (int i = 0; i < Count; i++)
        {
            if(Children[i].Mbb.Intersects(searchArea))
                Children[i].Search(searchArea, ref results);
        }
    }
    public override void GetMbbsWithLevel(List<(Mbb,int)> list, RNode<T> root)
    {
        int level = GetLevel(root);
        list.Add((Mbb, level));
        for (int i = 0; i < Count; i++)
        {
            Children[i].GetMbbsWithLevel(list, root);
        }
    }
    public override void Insert(T entry, ref RNode<T> root)
    {
        if (Count == 0 && this.Equals(root)) //special exception where root should become a leaf node
        {
            root = new RLeafNode<T>(_m, _M);
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
            if (Count < _M)
            {
                Children.Add(node);
                Mbb = Mbb.Enlarged(node.Mbb);
                node.Parent = this;  
                AdjustTree(this, null, ref root);
            }
            else
            {
                (RNode<T> L, RNode<T> LL) = SplitNode(node);
                AdjustTree(L,LL, ref root);
            }
            return;
        }
        if (Count == 0)
        {
            //In this case reinserting at the right height is not possible so deconstruct the node and reinsert entries
            List<T> entries = [];
            node.GetAllLeafEntries(entries);
            for(int i = 0; i < entries.Count; i++)
            {
                root.Insert(entries[i], ref root);
            }
            return;
        }
        RNode<T> best = Children[0];
        float leastOverlap = best.Mbb.Enlarged(node.Mbb).Area;
        for (int i = 1; i < Count; i++)
        {
            float overlap = Children[i].Mbb.Enlarged(node.Mbb).Area;
            if (overlap < leastOverlap)
            {
                best = Children[i];
                leastOverlap = overlap;
            }
        }
        best.ReInsert(node, level, ref root);
    }

    public override void GetAllLeafEntries(List<T> results)
    {
        for (int i = 0; i < Children.Count; i++)
        {
            Children[i].GetAllLeafEntries(results);
        }
    }
    public override void Delete(T entry, ref RNode<T> root)
    {
        RLeafNode<T>? leaf = FindLeaf(entry);
        if (leaf == null)
        {
            throw new Exception("Attempted deletion of non existent entry"); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //debug code
            leaf = root.ThoroughContains(entry);
            if (leaf != null)
            {
                throw new Exception("entry should have been found but wasn't");
            }
            return;
        }
        leaf.Delete(entry, ref root);
    }

    public override void CondenseTree(ref RNode<T> root, List<(RNode<T>, int)> eliminatedNodes)
    {
        //Stop and reinsert, when root is reached
        if (this.Equals(root))
        {
            RecalculateMbb();
            List<T> leafEntriesToBeAdded = [];
            foreach ((RNode<T> node, int level) in eliminatedNodes)
            {
                node.Parent = null; //ensure node can be removed from memory
                if (node is RLeafNode<T>)
                {
                    leafEntriesToBeAdded.AddRange(((RLeafNode<T>)node).LeafEntries);
                }
                else
                {
                    foreach (RNode<T> subNode in ((RNonLeafNode<T>)node).Children)
                    {
                        root.ReInsert(subNode, level + 1, ref root);
                    }
                }
            }
            for (int i = 0; i < leafEntriesToBeAdded.Count; i++)
            {
                    root.Insert(leafEntriesToBeAdded[i], ref root);
            }
            return;
        }
        RNonLeafNode<T> P = (RNonLeafNode<T>)Parent; //parent can't be a leaf
        if (Count < _m)
        {
            if(P.Children.Remove(this) == false)
                throw new Exception("node was not present in parent");
            eliminatedNodes.Add((this, this.GetLevel(root)));
        }
        else
        {
            //adjust mbb of node
            RecalculateMbb();
        }
        P.CondenseTree(ref root, eliminatedNodes);
    }
    public override void RecalculateMbb()
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float maxZ = float.MinValue;
        for (int i = 0; i < Count; i++)
        {
            Vector3 min = Children[i].Mbb.Minimum;
            Vector3 max = Children[i].Mbb.Maximum;
            if (min.X < minX)
                minX = min.X;
            if (min.Y < minY)
                minY = min.Y;   
            if (min.Z < minZ)
                minZ = min.Z;
            if (max.X > maxX)
                maxX = max.X;
            if (max.Y > maxY)
                maxY = max.Y;
            if (max.Z > maxZ)
                maxZ = max.Z;
        }
        Mbb = new Mbb(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
    }
    
    public override RLeafNode<T>? FindLeaf(T entry)
    {
        for (int i = 0; i < Count; i++)
        {
            if (Children[i].Mbb.Contains(entry.GetMbb()))
            {
                RLeafNode<T>? result = Children[i].FindLeaf(entry);
                if (result != null)
                    return result;
            }
        }
        return null;
    }

    public override RLeafNode<T> ChooseLeaf(T entry)
    {
        RNode<T> best = Children[0];
        float leastEnlargement = best.Mbb.Enlargement(entry.GetMbb());
        for (int i = 1; i < Count; i++)
        {
            float enlargement = Children[i].Mbb.Enlargement(entry.GetMbb());
            if (enlargement > leastEnlargement)
                continue;
            if (enlargement == leastEnlargement && best.Mbb.Area <= Children[i].Mbb.Area)
                continue;
            best = Children[i];
            leastEnlargement = enlargement;
        }
        return best.ChooseLeaf(entry);
    }

    public override (RNode<T>, RNode<T>) SplitNode(RNode<T> entry)
    {
        List<RNode<T>> entries = Children;
        entries.Add(entry);
        (RNode<T> e1, RNode<T> e2) = LinearPickSeeds(entries);
        entries.Remove(e1);
        entries.Remove(e2);
        RNonLeafNode<T> group1 = this;
        RNonLeafNode<T> group2 = new RNonLeafNode<T>(_m, _M);
        group1.Children = new List<RNode<T>>(M) { e1 };
        e1.Parent = group1;
        group1.Mbb = e1.GetMbb();
        group2.Children = new List<RNode<T>>(M) { e2 };
        e2.Parent = group2;
        group2.Mbb = e2.GetMbb();
        for (int i = 0; i < entries.Count; i++)
        {
            RNode<T> currentEntry = entries[i];
            
            //if it is required to put all remaining entries into a group to ensure that group is filled to size m do so
            RNonLeafNode<T> groupToFill = null;
            if (group1.Count + (entries.Count - i) <= group1._m)
                groupToFill = group1;
            if (group2.Count + (entries.Count - i) <= group2._m)
                groupToFill = group2;
            if (groupToFill != null)
            {
                for (int j = i; j < entries.Count; j++)
                {
                    currentEntry = entries[j];
                    groupToFill.Children.Add(currentEntry);
                    currentEntry.Parent = groupToFill;
                    groupToFill.Mbb = groupToFill.Mbb.Enlarged(currentEntry.GetMbb());
                }
                break;
            }

            Mbb group1Enlarged = group1.Mbb.Enlarged(currentEntry.GetMbb());
            Mbb group2Enlarged = group2.Mbb.Enlarged(currentEntry.GetMbb());
            if (group1Enlarged.Area < group2Enlarged.Area)
            {
                group1.Children.Add(currentEntry);
                currentEntry.Parent = group1;
                group1.Mbb = group1Enlarged;
                continue;
            }
            if (group1Enlarged.Area == group2Enlarged.Area)
            {
                if (group1.Count < group2.Count)
                {
                    group1.Children.Add(currentEntry);
                    currentEntry.Parent = group1;
                    group1.Mbb = group1Enlarged;
                }
                else
                {
                    group2.Children.Add(currentEntry);
                    currentEntry.Parent = group2;
                    group2.Mbb = group2Enlarged;
                }
                continue;
            }
            group2.Children.Add(currentEntry);
            currentEntry.Parent = group2;
            group2.Mbb = group2Enlarged;
        }
        return (group1, group2);
    }
    private (RNode<T>, RNode<T>) LinearPickSeeds(List<RNode<T>> entries)
    {
        //Find extreme rectangles along all dimensions and record total width of entries
        RNode<T> highLowX = entries[0];
        RNode<T> lowHighX = entries[0];
        RNode<T> highLowY = entries[0];
        RNode<T> lowHighY = entries[0];
        RNode<T> highLowZ = entries[0];
        RNode<T> lowHighZ = entries[0];
        float minX = entries[0].GetMbb().Minimum.X;
        float maxX = entries[0].GetMbb().Maximum.X;
        float minY = entries[0].GetMbb().Minimum.Y;
        float maxY = entries[0].GetMbb().Maximum.Y;
        float minZ = entries[0].GetMbb().Minimum.Z;
        float maxZ = entries[0].GetMbb().Maximum.Z;
        for (int i = 1; i < entries.Count; i++)
        {
            RNode<T> entry = entries[i];
            Mbb entryMbb = entry.GetMbb();
            if (entryMbb.Minimum.X > highLowX.GetMbb().Minimum.X)
                highLowX = entry;
            if (entryMbb.Maximum.X < lowHighX.GetMbb().Maximum.X)
                lowHighX = entry;
            if (entryMbb.Minimum.Y > highLowY.GetMbb().Minimum.Y)
                highLowY = entry;
            if (entryMbb.Maximum.Y < lowHighY.GetMbb().Maximum.Y)
                lowHighY = entry;
            if (entryMbb.Minimum.Z > highLowZ.GetMbb().Minimum.Z)
                highLowZ = entry;
            if (entryMbb.Maximum.Z < lowHighZ.GetMbb().Maximum.Z)
                lowHighZ = entry;
            if (entryMbb.Minimum.X < minX)
                minX = entryMbb.Minimum.X;
            if (entryMbb.Minimum.Y < minY)
                minY = entryMbb.Minimum.Y;
            if (entryMbb.Minimum.Z < minZ)
                minZ = entryMbb.Minimum.Z;
            if (entryMbb.Maximum.X > maxX)
                maxX = entryMbb.Maximum.X;
            if (entryMbb.Maximum.Y > maxY)
                maxY = entryMbb.Maximum.Y;
            if (entryMbb.Maximum.Z > maxZ)
                maxZ = entryMbb.Maximum.Z;
        }
        //Adjust for shape of the rectangle cluster
        float widthX = maxX - minX;
        float widthY = maxY - minX;
        float widthZ = maxZ - minZ;
        float normSepX = (highLowX.GetMbb().Minimum.X - lowHighX.GetMbb().Maximum.X) / widthX;
        float normSepY = (highLowY.GetMbb().Minimum.Y - lowHighY.GetMbb().Maximum.Y) / widthY;
        float normSepZ = (highLowZ.GetMbb().Minimum.Z - lowHighZ.GetMbb().Maximum.Z) / widthZ;

        //select the pair with greatest seperation
        (RNode<T>, RNode<T>) pair = normSepX > normSepY && normSepX > normSepZ ? 
            (highLowX, lowHighX) : normSepY > normSepZ ? (highLowY, lowHighY) : (highLowZ, lowHighZ);

        if (pair.Item1.Equals(pair.Item2)) //edge case where we should fall back on quadratic pick seeds
            pair = QuadraticPickSeeds(entries);
        
        return pair;
    }
    private (RNode<T>, RNode<T>) QuadraticPickSeeds(List<RNode<T>> entries)
    {
        (RNode<T>, RNode<T>) mostWasteful = (entries[0], entries[0]); //placeholder
        float largestD = float.MinValue;
        for (int i = 0; i < entries.Count; i++)
        {
            for (int j = i + 1; j < entries.Count; j++)
            {
                Mbb e1 = entries[i].GetMbb();
                Mbb e2 = entries[j].GetMbb();
                Mbb J = e1.Enlarged(e2);
                float d = J.Area - e1.Area - e2.Area;
                if(d > largestD)
                    mostWasteful = (entries[i], entries[j]);
            }
        }
        if (mostWasteful.Item1.Equals(mostWasteful.Item2))
            throw new Exception("NANI");
        return mostWasteful;
    }
}

public struct Mbb(Vector3 minimum, Vector3 maximum)
{
    private const float Epsilon = 0.01f; //extra offset to contains to account for floating point errors
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
        return Minimum.X <= other.Minimum.X + Epsilon && Minimum.Y <= other.Minimum.Y + Epsilon && Minimum.Z <= other.Minimum.Z + Epsilon &&
               Maximum.X >= other.Maximum.X - Epsilon && Maximum.Y >= other.Maximum.Y - Epsilon && Maximum.Z >= other.Maximum.Z - Epsilon;
    }
    
    //the enlargement to this minimum bounding box needed to fit the other mbb inside it. Also returns the new Mbb
    public float Enlargement(Mbb other)
    {
        float enlargement = Enlarged(other).Area - Area;
        return enlargement;
    }

    public Mbb Enlarged(Mbb other)
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