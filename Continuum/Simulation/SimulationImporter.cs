using System.Numerics;
using Continuum;

namespace Continuum.Simulation;

public class SimulationImporter
{
    private static Dictionary<string, Func<Vector3, Organism>> organismCreationFunctions = new();

    /// <summary>
    /// Call this before trying to import a type of organism
    /// </summary>
    /// <param name="organismKey"></param>
    /// <param name="organismCreationFunction"></param>
    public static void RegisterOrganism(string organismKey, Func<Vector3, Organism> organismCreationFunction)
    {
        organismCreationFunctions.Add(organismKey, organismCreationFunction);
    }
    
    /// <summary>
    /// This actually creates an organism
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
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
    
    /// <summary>
    /// Load a file into a world
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="world"></param>
    /// <param name="timestampIndex"></param>
    public static void FromFileToOrganisms(string filePath, World world, int timestampIndex = 0)
    {
        string fileContents = File.ReadAllText(filePath);
        FromStringToOrganisms(fileContents, world, timestampIndex);
    }
    
    /// <summary>
    /// Load a file into a list of organisms
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="timestampIndex"></param>
    /// <returns></returns>
    public static Organism[] FromFileToOrganisms(string filePath, int timestampIndex = 0)
    {
        string fileContents = File.ReadAllText(filePath);
        return FromStringToOrganisms(fileContents, timestampIndex);
    }

    /// <summary>
    /// Load a file into a list of any type
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="parseString"></param>
    /// <param name="timestampIndex"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] FromFileToObjectType<T>(string filePath, Func<string, string, T> parseString, int timestampIndex = 0)
    {
        string fileContents = File.ReadAllText(filePath);
        return FromStringToObjects(fileContents, parseString, timestampIndex);
    }

    /// <summary>
    /// Load a string into a world
    /// </summary>
    /// <param name="s"></param>
    /// <param name="world"></param>
    /// <param name="timestampIndex"></param>
    public static void FromStringToOrganisms(string s, World world, int timestampIndex)
    {
        world.Clear();
        
        string[] timeStampStrings = s.Split('\n');
        string timeStamp = timeStampStrings[timestampIndex];
        string[] organismStrings = timeStamp.Split(ImportExportHelper.OrganismSeparator);
        foreach (string organismString in organismStrings)
        {
            string[] keySplit = organismString.Split(ImportExportHelper.KeySeperator);
            Organism organism = CreateOrganism(keySplit[0]);
            organism.FromString(keySplit[1]);
            world.AddOrganism(organism);
        }
    }
    
    /// <summary>
    /// Load a string into a list of organisms
    /// </summary>
    /// <param name="s"></param>
    /// <param name="timestampIndex"></param>
    /// <returns></returns>
    public static Organism[] FromStringToOrganisms(string s, int timestampIndex)
    {
        LinkedList<Organism> organisms = new();
        
        string[] timeStampStrings = s.Split('\n');
        string timeStamp = timeStampStrings[timestampIndex];
        string[] organismStrings = timeStamp.Split(ImportExportHelper.OrganismSeparator);
        foreach (string organismString in organismStrings)
        {
            string[] keySplit = organismString.Split(ImportExportHelper.KeySeperator);
            Organism organism = CreateOrganism(keySplit[0]);
            organism.FromString(keySplit[1]);
            organisms.AddLast(organism);
        }
        
        return organisms.ToArray();
    }

    /// <summary>
    /// Load a string into a list of any type
    /// </summary>
    /// <param name="s"></param>
    /// <param name="parseString"></param>
    /// <param name="timestampIndex"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] FromStringToObjects<T>(string s, Func<string, string, T> parseString, int timestampIndex)
    {
        LinkedList<T> objects = new();
        
        string[] timeStampStrings = s.Split('\n');
        string timeStamp = timeStampStrings[timestampIndex];
        string[] organismStrings = timeStamp.Split(ImportExportHelper.OrganismSeparator);
        foreach (string objectString in organismStrings)
        {
            if (objectString is "" or "\r")
                continue;
            
            string[] keySplit = objectString.Split(ImportExportHelper.KeySeperator);
            //keySplit[0] = Key, keySplit[1] = organism contents
            objects.AddLast(parseString(keySplit[0], keySplit[1]));
        }
        
        return objects.ToArray();
    }
}