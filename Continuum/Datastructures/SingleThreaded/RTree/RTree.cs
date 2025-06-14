namespace Continuum.Datastructures.SingleThreaded.RTree;

public class RTree<T>(int minNodeSize, int maxNodeSize)
    where T : IMinimumBoundable 
{
    public RNode<T> Root => root ?? new RLeafNode<T>(minNodeSize, maxNodeSize); //public for testing
    private RNode<T>? root = null;
    private List<T> resultsList = [];
    public List<T> Search(Mbb searchArea)
    {
        if (root == null)
            return [];
        resultsList.Clear();
        root.Search(searchArea, resultsList);
        return resultsList;
    }
    public void Insert(T entry)
    {
        if (root == null)
        {
            RLeafNode<T> newRoot = new RLeafNode<T>(minNodeSize, maxNodeSize);
            newRoot.LeafEntries.Add(entry);
            newRoot.Mbb = entry.GetMbb();
            root = newRoot;
            return;
        }
        root.Insert(entry, ref root);
    }

    public bool Delete(T entry)
    {
        if (root != null && root.Delete(entry, ref root))
        {
            if (root.Count == 0)
                root = null;
            return true;
        }
        return false;
    }

    public bool UpdateMbb(T entry, Mbb newMbb)
    {
        if (root != null)
            return root.UpdateMbb(entry, newMbb, ref root);
        return false;
    }

    public void ForEach(Action<T> action)
    {
        if (root != null)
            root.ForEach(action);
    }
    
    public List<T> ToList()
    {
        if (root == null)
            return [];
        List<T> list = [];
        root.GetAllLeafEntries(list);
        return list;
    }

    public List<(Mbb,int)> GetMbbsWithLevel() //for debugging
    {
        List<(Mbb, int)> list = [];
        if (root != null)
            root.GetMbbsWithLevel(list, root);
        return list;
    }

    public T? NearestNeighbour(T searchEntry, Func<T,T,float> distanceFunc)
    {
        if (root == null)
            return default;
        T? dummy = default(T); 
        NearestNeighbour<T> initial = new NearestNeighbour<T>(dummy, float.MaxValue);
        root.NearestNeighbour(searchEntry, initial, distanceFunc);
        return initial.Entry; //contains result of query now
    }

    public void Clear()
    {
        //No more references to root, so entire tree gets garbage collected
        root = null;
    }
}