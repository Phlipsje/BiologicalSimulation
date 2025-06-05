namespace BioSim.Datastructures;
using System.Numerics;

public class RTree<T> where T : IMinimumBoundable
{
    private RNode<T> root;
    public List<T> Search(Mbb searchArea)
    {
        List<T> results = [];
        root.Search(searchArea, ref results);
        return results;
    }
    public void Insert(T entry)
    {
        root.Insert(entry, ref root);
    }

    public void Delete(T entry)
    {
        root.Delete(entry, ref root);
    }
}

public abstract class RNode<T>(int m, int M) : IMinimumBoundable where T : IMinimumBoundable
{
    protected int _m = m;
    protected int _M = M;
    public RNode<T> Parent = null;
    public Mbb Mbb;
    public Mbb GetMbb() { return Mbb; }
    public abstract void Search(Mbb searchArea, ref List<T> results);
    public abstract void Insert(T entry, ref RNode<T> root);
    public abstract void ReInsert(RNonLeafNode<T> node, ref RNode<T> root);
    public abstract void Delete(T entry, ref RNode<T> root);
    public abstract void CondenseTree(ref RNode<T> root, List<RNode<T>> eliminatedNodes);
    public abstract RLeafNode<T>? FindLeaf(T entry);
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
        P.Mbb = P.Mbb.Enlarged(L.Mbb);
        if (LL == null)
        {
            P.AdjustTree(P, null, ref root);
            return;
        }
        if (P.Count < _M)
        {
            P.Children.Add(LL);
            P.Mbb = P.Mbb.Enlarged(LL.Mbb);
            LL.Parent = P;
            P.AdjustTree(P, null, ref root);
            return;
        }
        (_, RNode<T> PP) = P.SplitNode(LL);
        AdjustTree(P, PP, ref root);
    }
    
    public abstract RLeafNode<T> ChooseLeaf(T entry);
    public abstract (RNode<T>, RNode<T>) SplitNode(RNode<T> entry);
}

public class RLeafNode<T>(int m, int M) : RNode<T>(m,M)
    where T : IMinimumBoundable
{
    public int Count => LeafEntries.Count;
    public List<T> LeafEntries = new (M);

    public override void DebugDraw(int axis0, int axis1)
    {
        //draw self and leaf entries
        
        
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
            return;
        }
        (RNode<T> L, RNode<T> LL) = SplitNode(entry);
        AdjustTree(L,LL, ref root);
    }

    public override void ReInsert(RNonLeafNode<T> node, ref RNode<T> root)
    {
        throw new Exception("Can't insert node in a leaf node. Reinsert should not be able to reach this far down");
    }

    public override void Delete(T entry, ref RNode<T> root)
    {
        LeafEntries.Remove(entry);
        CondenseTree(ref root, []);
    }

    public override void CondenseTree(ref RNode<T> root, List<RNode<T>> eliminatedNodes)
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
            P.Children.Remove(this);
            eliminatedNodes.Add(this);
        }
        else
        {
            //adjust mbb of node
            RecalculateMbb();
        }
        P.CondenseTree(ref root, eliminatedNodes);
    }

    private void RecalculateMbb()
    {
        float minX = Mbb.Minimum.X;
        float minY = Mbb.Minimum.Y;
        float minZ = Mbb.Minimum.Z;
        float maxX = Mbb.Minimum.X;
        float maxY = Mbb.Maximum.Y;
        float maxZ = Mbb.Maximum.Z;
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
            RLeafNode<T> groupToFill = null;
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
            Mbb group2Enlarged = group1.Mbb.Enlarged(currentEntry.GetMbb());
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
    private (T, T) QuadraticPickSeeds(List<T> entries)
    {
        (T, T) mostWasteful = (entries[0], entries[0]); //placeholder
        float largestD = 0;
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
        return mostWasteful;
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
    public int Count => Children.Count;

    public override void Search(Mbb searchArea, ref List<T> results)
    {
        for (int i = 0; i < Count; i++)
        {
            if(Children[i].Mbb.Intersects(searchArea))
                Children[i].Search(searchArea, ref results);
        }
    }

    public override void Insert(T entry, ref RNode<T> root)
    {
        RLeafNode<T> leaf = ChooseLeaf(entry);
        leaf.Insert(entry, ref root);
    }

    public override void ReInsert(RNonLeafNode<T> node, ref RNode<T> root)
    {
        if (this.GetLevel(root) == node.GetLevel(root) - 1)
        {
            if (Count < _M)
            {
                Children.Add(node);
                Mbb = Mbb.Enlarged(node.Mbb);
                node.Parent = this;   
            }
            else
            {
                (RNode<T> L, RNode<T> LL) = SplitNode(node);
                AdjustTree(L,LL, ref root);
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
        best.ReInsert(node, ref root);
    }

    public override void Delete(T entry, ref RNode<T> root)
    {
        RLeafNode<T>? leaf = FindLeaf(entry);
        if (leaf == null)
            throw new Exception("Attempted deletion of non existent entry");
        leaf.Delete(entry, ref root);
    }

    public override void CondenseTree(ref RNode<T> root, List<RNode<T>> eliminatedNodes)
    {
        //Stop and reinsert, when root is reached
        if (this.Equals(root))
        {
            foreach (RNode<T> node in eliminatedNodes)
            {
                if (node is RLeafNode<T>)
                {
                    foreach (T entry in ((RLeafNode<T>)node).LeafEntries)
                    {
                        Insert(entry, ref root);
                    }
                }
                else
                {
                    ReInsert((RNonLeafNode<T>)node, ref root);
                }
            }
            return;
        }
        RNonLeafNode<T> P = (RNonLeafNode<T>)Parent; //parent can't be a leaf
        if (Count < _m)
        {
            P.Children.Remove(this);
            eliminatedNodes.Add(this);
        }
        else
        {
            //adjust mbb of node
            RecalculateMbb();
        }
        P.CondenseTree(ref root, eliminatedNodes);
    }
    private void RecalculateMbb()
    {
        float minX = Mbb.Minimum.X;
        float minY = Mbb.Minimum.Y;
        float minZ = Mbb.Minimum.Z;
        float maxX = Mbb.Minimum.X;
        float maxY = Mbb.Maximum.Y;
        float maxZ = Mbb.Maximum.Z;
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
            Mbb = new Mbb(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        }
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
        group1.Mbb = e1.GetMbb();
        group2.Children = new List<RNode<T>>(M) { e2 };
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
                    groupToFill.Mbb = groupToFill.Mbb.Enlarged(currentEntry.GetMbb());
                }
                break;
            }

            Mbb group1Enlarged = group1.Mbb.Enlarged(currentEntry.GetMbb());
            Mbb group2Enlarged = group1.Mbb.Enlarged(currentEntry.GetMbb());
            if (group1Enlarged.Area < group2Enlarged.Area)
            {
                group1.Children.Add(currentEntry);
                group1.Mbb = group1Enlarged;
                continue;
            }
            if (group1Enlarged.Area == group2Enlarged.Area)
            {
                if (group1.Count < group2.Count)
                {
                    group1.Children.Add(currentEntry);
                    group1.Mbb = group1Enlarged;
                }
                else
                {
                    group2.Children.Add(currentEntry);
                    group2.Mbb = group2Enlarged;
                }
                continue;
            }
            group2.Children.Add(currentEntry);
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
        
        //return the pair with greatest seperation
        if (normSepX > normSepY && normSepX > normSepZ)
            return (highLowX, lowHighX);
        if (normSepY > normSepZ)
            return (highLowY, lowHighY);
        return (highLowZ, lowHighZ);
    }
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