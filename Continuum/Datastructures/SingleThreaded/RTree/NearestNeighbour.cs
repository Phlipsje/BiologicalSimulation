namespace Continuum.Datastructures.SingleThreaded.RTree;

public class NearestNeighbour<T>(T? entry, float distance)
{
    public T? Entry = entry;
    public float Distance = distance;
}