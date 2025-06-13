using System.Text;

namespace BioSim.Simulation;

public class SimulationExporter
{
    /// <summary>
    /// The name of the file the simulation contents are stored in.
    /// </summary>
    public static string FileName { get; set; }
    
    /// <summary>
    /// The directory in which the save file is stored.
    /// </summary>
    public static string SaveDirectory { get; set; }
    
    /// <summary>
    /// Used as a debugging tool, if true will print the absolute path where the file is stored in the console.
    /// </summary>
    public static bool ShowExportFilePath { get; set; } = false;

    /// <summary>
    /// Saves all organisms in the simulation to a file
    /// </summary>
    /// <param name="world"></param>
    /// <returns>Returns 2 strings, first is the file path, second is the file contents</returns>
    internal (string, string) SaveToSeparateFiles(World world, Simulation simulation)
    {
        StringBuilder sb = new StringBuilder();
        
        world.GetOrganisms(out var organisms).Wait();
        foreach (Organism organism in organisms)
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

    /// <summary>
    /// Saves the simulation and writes all save data of all previously occured time steps to the same file.
    /// </summary>
    /// <param name="world"></param>
    /// <param name="simulation"></param>
    /// <returns>Returns 2 strings, first is the file path, second is the file contents</returns>
    internal (string, string) SaveToSameFile(World world, Simulation simulation)
    {
        StringBuilder sb = new StringBuilder();
        
        world.GetOrganisms(out var organisms).Wait();
        foreach (Organism organism in organisms)
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
        string filePath = SaveDirectory + "\\" + FileName + ".txt";
        string resultingString = sb.ToString();
        
        //Save to file
        if (File.Exists(filePath))
        {
            string previousString = File.ReadAllText(filePath);
            
            resultingString = previousString + $"\r\n{simulation.Tick} " + resultingString;
        }
        else
        {
            resultingString = $"{simulation.Tick} " + resultingString;
        }
        
        File.WriteAllText(filePath, resultingString);
        return (Path.GetFullPath(filePath), resultingString);
    }
}