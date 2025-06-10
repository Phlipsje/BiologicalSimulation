using BioSim.Datastructures;

namespace BioSim.Simulation;

public partial class Simulation
{
    private World world;
    private DataStructure dataStructure;
    private Random random;
    private bool abort = false;
    public int Tick { get; private set; } = 0; //The current tick we are on
    public bool FileWritingEnabled { get; set; } = false;
    public bool WriteToSameFile { get; set; } = false;
    public int TicksPerFileWrite = 0;
    
    private SimulationExporter simulationExporter;
    
    //This event happens at the end of every tick
    public delegate void OnTickEventHandler(World world);
    public event OnTickEventHandler? OnTick;
    
    //This event happens at depending on if it is turned on and if it's frequency, basically a lesser called OnTick
    public delegate void OnFileWriteEventHandler(string filePath, string fileContents);
    public event OnFileWriteEventHandler? OnFileWrite;
    
    //This event happens when the simulation is finished
    public delegate void OnEndEventHandler(World world);
    public event OnEndEventHandler? OnEnd;

    public void CreateSimulation(World world, Random random)
    {
        this.world = world;
        this.random = random;
        dataStructure = new NoDataStructure(); //Default data structure has no optimizations
        simulationExporter = new SimulationExporter();
        
        //Sets culture to US-English, specific language does not matter, but because we set this, using float.Parse and writing floats to file always use '.' as decimal point.
        System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
        System.Threading.Thread.CurrentThread.CurrentCulture = ci;
    }

    public void StartSimulation()
    {
        world.StartingDistribution(random);
        world.Initialize();
        
        CheckWarnings();
        CheckErrors();

        if (FileWritingEnabled)
        {
            (string filePath, string fileContents) = WriteToSameFile ? simulationExporter.SaveToSameFile(world, this) : simulationExporter.SaveToSeparateFiles(world, this);
            OnFileWrite?.Invoke(filePath, fileContents);
        }
    }

    public async Task Step()
    {
        if (abort || world.StopCondition())
        {
            OnSimulationEnd();
            return;
        }
        
        Tick++;
        await dataStructure.Step();
        world.Step();
        OnTick?.Invoke(world);

        //Save file and invoke event letting know that it happened
        if (FileWritingEnabled && Tick % TicksPerFileWrite == 0)
        {
            (string filePath, string fileContents) = WriteToSameFile ? simulationExporter.SaveToSameFile(world, this) : simulationExporter.SaveToSeparateFiles(world, this);
            OnFileWrite?.Invoke(filePath, fileContents);
        }
    }

    public void AbortSimulation()
    {
        abort = true;
    }

    /// <summary>
    /// What to do when the simulation is over
    /// </summary>
    private void OnSimulationEnd()
    {
        //Save simulation to file
        (string filePath, string fileContents) = WriteToSameFile ? simulationExporter.SaveToSameFile(world, this) : simulationExporter.SaveToSeparateFiles(world, this);
        OnFileWrite?.Invoke(filePath, fileContents);
        
        //Tell the user that the simulation is over
        OnEnd?.Invoke(world);
    }

    //Forcefully make a save to a specified file location
    public void Save()
    {
        (string filePath, string fileContents) = WriteToSameFile ? simulationExporter.SaveToSameFile(world, this) : simulationExporter.SaveToSeparateFiles(world, this);
        OnFileWrite?.Invoke(filePath, fileContents);
    }

    #region Warnings and errors

    private void CheckWarnings()
    {
        if(dataStructure is NoDataStructure)
            Console.WriteLine("Warning: no data structure is being used");
    }

    private void CheckErrors()
    {
        
    }
    

    #endregion
}