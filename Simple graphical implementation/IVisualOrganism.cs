using BioSim;
using BioSim.Datastructures;
using Microsoft.Xna.Framework;
using Vector3 = System.Numerics.Vector3;

namespace Simple_graphical_implementation;

/// <summary>
/// This is added to the organisms in this visual representation,
/// so that a difference can be made in color (which wouldn't make sense when only looking at the simulation aspect)
/// </summary>
public abstract class VisualOrganism : Organism
{
    protected VisualOrganism(Vector3 startingPosition, float size, World world, DataStructure dataStructure) : base(startingPosition, size, world, dataStructure)
    {
    }

    public abstract Color Color { get; }
}