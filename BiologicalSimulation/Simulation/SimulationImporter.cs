namespace BioSim.Simulation;

public class SimulationImporter
{
    public void ImportFromFile(string filePath)
    {
        string fileContents = File.ReadAllText(filePath);
        ImportFromString(fileContents);
    }

    public void ImportFromString(string s)
    {
        
    }
}