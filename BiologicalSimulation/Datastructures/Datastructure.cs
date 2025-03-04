using System.Collections;
using System.Numerics;

namespace BioSim.Datastructures;
/// <summary>
/// An abstract class used to define an object that can help in more efficiently running position based queries
/// </summary>
public abstract class DataStructure : IEnumerable<IOrganism>
{
    protected World World { get; }

    public DataStructure(World world)
    {
        World = world;
    }

    public abstract Organism ClosestNeighbour(Organism organism);
    
    //Define this method to explain how the data structure can be accessed as something that is iterable, easiest is a List or array
    protected abstract IEnumerator<IOrganism> ToEnumerator();

    IEnumerator<IOrganism> IEnumerable<IOrganism>.GetEnumerator()
    {
        return ToEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        return ToEnumerator();
    }
}