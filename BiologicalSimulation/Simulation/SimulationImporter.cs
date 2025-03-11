using BiologicalSimulation;

namespace BioSim.Simulation;

public class SimulationImporter
{
    public void ImportFromFile(string filePath, World world)
    {
        string fileContents = File.ReadAllText(filePath);
        ImportFromString(fileContents, world);
    }

    public void ImportFromString(string s, World world)
    {
        string[] organismStrings = s.Split(ImportExportHelper.OrganismSeparator);
        foreach (string organismString in organismStrings)
        {
            string[] keySplit = organismString.Split(ImportExportHelper.KeySeperator);
            Organism organism = OrganismManager.CreateOrganism(keySplit[0]);
            organism.FromString(keySplit[1]);
            world.AddOrganism(organism);
        }
    }
}