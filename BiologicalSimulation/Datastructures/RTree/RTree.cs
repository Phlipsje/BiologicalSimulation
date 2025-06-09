using System.Collections;
using BiologicalSimulation.Datastructures.RTree;

namespace BioSim.Datastructures;

public class RTree<T>(int m, int M)
    where T : IMinimumBoundable 
{
    public RNode<T> Root => root != null ? root : new RLeafNode<T>(_m, _M); //public for testing
    
    private int _m = m, _M = M;
    private RNode<T>? root = null;
    public List<T> Search(Mbb searchArea)
    {
        if (root == null)
            return [];
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
}