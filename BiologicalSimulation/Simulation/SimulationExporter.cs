using System.Text;

namespace BioSim.Simulation;

public class SimulationExporter
{
    public static string FileName;
    public static string SaveDirectory;

    /// <summary>
    /// Saves all organisms in the simulation to a file
    /// </summary>
    /// <param name="world"></param>
    /// <returns>Returns 2 strings, first is the file path, second is the file contents</returns>
    public (string, string) SaveToFile(World world)
    {
        //This should write to a file, not tested yet
        
        StringBuilder sb = new StringBuilder();
        
        foreach (Organism organism in world.Organisms)
        {
            //A string builder is a lot faster at concatenating a lot of string together than using the + operation on strings
            sb.Append(organism.ToString());
            sb.Append(ImportExportHelper.OrganismSeparator);
        }

        string filePath = SaveDirectory + FileName;
        string resultingString = sb.ToString();
        
        File.WriteAllText(filePath, resultingString);
        return (Path.GetFullPath(filePath),resultingString);
    }
}