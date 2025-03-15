using System.Text;

namespace BioSim.Simulation;

public class SimulationExporter
{
    public static string FileName { get; set; }
    public static string SaveDirectory { get; set; }
    public static bool ShowExportFilePath { get; set; } = false;

    /// <summary>
    /// Saves all organisms in the simulation to a file
    /// </summary>
    /// <param name="world"></param>
    /// <returns>Returns 2 strings, first is the file path, second is the file contents</returns>
    public (string, string) SaveToFile(World world, Simulation simulation)
    {
        //This should write to a file, not tested yet
        
        StringBuilder sb = new StringBuilder();
        
        foreach (Organism organism in world.Organisms)
        {
            //A string builder is a lot faster at concatenating a lot of string together than using the + operation on strings
            sb.Append(organism.Key);
            sb.Append(ImportExportHelper.KeySeperator);
            sb.Append(organism.ToString());
            sb.Append(ImportExportHelper.OrganismSeparator);
        }

        //Create the directory if it does not yet exist
        if (!Directory.Exists(SaveDirectory))
        {
            Directory.CreateDirectory(SaveDirectory);
        }

        //Get file path
        string filePath = SaveDirectory + "\\" + FileName + $" {simulation.Tick}.txt";
        string resultingString = sb.ToString();
        
        //Save to file
        File.WriteAllText(filePath, resultingString);

        if (ShowExportFilePath)
        {
            Console.WriteLine(Path.GetFullPath(filePath));   
            ShowExportFilePath = false;
        }
        
        return (Path.GetFullPath(filePath),resultingString);
    }
}