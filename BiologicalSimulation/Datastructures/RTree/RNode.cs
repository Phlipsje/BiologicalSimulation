using BioSim.Datastructures;

namespace BiologicalSimulation.Datastructures.RTree;

public abstract class RNode<T>(int minSize, int maxSize) : IMinimumBoundable where T : IMinimumBoundable
{
    protected readonly int MinSize = minSize;
    protected readonly int MaxSize = maxSize;
    public RNode<T> Parent;
    public abstract int Count { get; }
    protected abstract IEnumerable<IMinimumBoundable> Children { get; }
    public Mbb Mbb;
    public Mbb GetMbb() { return Mbb; }
    public void SetMbb(Mbb newMbb) { Mbb = newMbb; }
    
    public abstract void ForEach(Action<T> action);
    public abstract void Search(Mbb searchArea, List<T> results);
    public abstract void NearestNeighbour(T searchEntry, NearestNeighbour<T> nearest, Func<T,T,float> distance);
    public abstract void Insert(T entry, ref RNode<T> root);
    public abstract void ReInsert(RNode<T> node, int level, ref RNode<T> root);
    public abstract bool Delete(T entry, ref RNode<T> root);
    public abstract RLeafNode<T> ChooseLeaf(T entry);
    public abstract (RNode<T>, RNode<T>) SplitNode(RNode<T> entry);
    public abstract bool UpdateMbb(T entry, Mbb newMbb, ref RNode<T> root);
    public abstract RLeafNode<T>? FindLeaf(T entry);
    public abstract void GetAllLeafEntries(List<T> results);
    public abstract void GetMbbsWithLevel(List<(Mbb, int)> list, RNode<T> root); //for debugging
    private void RecalculateMbb()
    {
        Mbb = Mbb.ComputeBoundingBox(Children);
    }
    protected int GetLevel(RNode<T> root)
    {
        if (this.Equals(root))
            return 0;
        return 1 + Parent.GetLevel(root);
    }
    protected void CondenseTree(ref RNode<T> root, List<(RNode<T>, int)> eliminatedNodes)
    {
        //Stop and reinsert, when root is reached
        if (this.Equals(root))
        {
            RecalculateMbb();
            List<T> leafEntriesToBeAdded = [];
            foreach ((RNode<T> node, int level) in eliminatedNodes)
            {
                node.Parent = null; //ensure node can be removed from memory
                if (node is RLeafNode<T> leafNode)
                {
                    leafEntriesToBeAdded.AddRange(leafNode.LeafEntries);
                }
                else
                {
                    foreach (RNode<T> subNode in ((RNonLeafNode<T>)node).NodeEntries)
                    {
                        root.ReInsert(subNode, level + 1, ref root);
                    }
                }
            }
            foreach (var entry in leafEntriesToBeAdded)
            {
                root.Insert(entry, ref root);
            }
            return;
        }

        RNonLeafNode<T> parentNode = (RNonLeafNode<T>)Parent; //parent can't be a leaf
        if (Count < MinSize)
        {
            if (parentNode.NodeEntries.Remove(this) == false)
                throw new Exception("node was not present in its parent");
            eliminatedNodes.Add((this, this.GetLevel(root)));
        }
        else
        {
            //adjust mbb of node
            RecalculateMbb();
        }

        parentNode.CondenseTree(ref root, eliminatedNodes);
    }
    protected void AdjustTree(RNode<T> adjustedNode, RNode<T>? newSiblingNode, ref RNode<T> root)
    {
        if (adjustedNode.Equals(root)) //stop condition
        {
            if(newSiblingNode == null)//Then root can stay the way it is
                return;
            //This means the root was split so a new root should be installed
            RNonLeafNode<T> newRoot = new RNonLeafNode<T>(MinSize, MaxSize);
            newRoot.NodeEntries = [adjustedNode, newSiblingNode];
            newRoot.Mbb = adjustedNode.Mbb.Enlarged(newSiblingNode.Mbb);
            adjustedNode.Parent = newRoot;
            newSiblingNode.Parent = newRoot;
            root = newRoot;
            return;
        }
        RNonLeafNode<T> nodeParent = (RNonLeafNode<T>)adjustedNode.Parent; //a parent of a node should never be a leaf node.
        if (newSiblingNode == null)
        {
            nodeParent.Mbb = nodeParent.Mbb.Enlarged(adjustedNode.Mbb); 
            nodeParent.AdjustTree(nodeParent, null, ref root);
            return;
        }
        if (nodeParent.Count < MaxSize)
        { 
            nodeParent.Mbb = nodeParent.Mbb.Enlarged(adjustedNode.Mbb); 
            nodeParent.NodeEntries.Add(newSiblingNode);
            newSiblingNode.Parent = nodeParent;
            nodeParent.Mbb = nodeParent.Mbb.Enlarged(newSiblingNode.Mbb);
            nodeParent.AdjustTree(nodeParent, null, ref root);
            return;
        }
        (RNode<T> n, RNode<T> nn) = nodeParent.SplitNode(newSiblingNode);
        AdjustTree(n, nn, ref root);
    }
}