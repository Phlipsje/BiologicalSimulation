using System.Numerics;
using BioSim;

namespace BiologicalSimulation;

public static class OrganismManager
{
    private static Dictionary<string, Func<Vector3, Organism>> organismCreationFunctions = new();

    public static void RegisterOrganism(string organismKey, Func<Vector3, Organism> organismCreationFunction)
    {
        organismCreationFunctions.Add(organismKey, organismCreationFunction);
    }
    
    public static Organism CreateOrganism(string key)
    {
        Vector3 position = Vector3.Zero;
        bool success = organismCreationFunctions.TryGetValue(key, out Func<Vector3, Organism> creationFunction);
        if (!success)
        {
            throw new KeyNotFoundException();
        }
        
        Organism organism = creationFunction!.Invoke(position);
        return organism;
    }
}