using BiologicalSimulation;

namespace BioSim.Simulation;

public class SimulationImporter
{
    public static void FromFileToOrganisms(string filePath, World world, int timestampIndex = 0)
    {
        string fileContents = File.ReadAllText(filePath);
        FromStringToOrganisms(fileContents, world, timestampIndex);
    }
    
    public static Organism[] FromFileToOrganisms(string filePath, int timestampIndex = 0)
    {
        string fileContents = File.ReadAllText(filePath);
        return FromStringToOrganisms(fileContents, timestampIndex);
    }

    public static T[] FromFileToObjectType<T>(string filePath, Func<string, string, T> parseString, int timestampIndex = 0)
    {
        string fileContents = File.ReadAllText(filePath);
        return FromStringToObjects(fileContents, parseString, timestampIndex);
    }

    public static void FromStringToOrganisms(string s, World world, int timestampIndex)
    {
        world.Clear();
        
        string[] timeStampStrings = s.Split('\n');
        string timeStamp = timeStampStrings[timestampIndex];
        string[] organismStrings = timeStamp.Split(ImportExportHelper.OrganismSeparator);
        foreach (string organismString in organismStrings)
        {
            string[] keySplit = organismString.Split(ImportExportHelper.KeySeperator);
            Organism organism = OrganismManager.CreateOrganism(keySplit[0]);
            organism.FromString(keySplit[1]);
            world.AddOrganism(organism);
        }
    }
    
    public static Organism[] FromStringToOrganisms(string s, int timestampIndex)
    {
        LinkedList<Organism> organisms = new();
        
        string[] timeStampStrings = s.Split('\n');
        string timeStamp = timeStampStrings[timestampIndex];
        string[] organismStrings = timeStamp.Split(ImportExportHelper.OrganismSeparator);
        foreach (string organismString in organismStrings)
        {
            string[] keySplit = organismString.Split(ImportExportHelper.KeySeperator);
            Organism organism = OrganismManager.CreateOrganism(keySplit[0]);
            organism.FromString(keySplit[1]);
            organisms.AddLast(organism);
        }
        
        return organisms.ToArray();
    }

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