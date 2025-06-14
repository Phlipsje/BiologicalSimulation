using Continuum.Datastructures;
using Continuum.Datastructures.MultiThreaded;
using Continuum.Datastructures.SingleThreaded;

namespace Continuum.Simulation;

public class Simulation
{
    /// <summary>
    /// Defines the rules that the organisms in the simulation follow
    /// </summary>
    private World world;
    
    /// <summary>
    /// Holds the data structure that the simulation makes use of.
    /// </summary>
    private bool isMultithreaded;
    private DataStructure dataStructure;
    private SingleThreadedDataStructure singleThreadedDataStructure;
    private MultiThreadedDataStructure multiThreadedDataStructure;
    
    /// <summary>
    /// Used to stop the simulation.
    /// </summary>
    private bool abort = false;
    
    /// <summary>
    /// The current Step() iteration the program is on.
    /// </summary>
    public int Tick { get; private set; } = 0;
    
    /// <summary>
    /// Decides if the contents of the simulation should be written to a file.
    /// </summary>
    public bool FileWritingEnabled { get; set; } = false;
    
    /// <summary>
    /// Only relevant if file writing is enabled.
    /// Decides if each iteration of file writing is all accumulated in a singular file,
    /// or if false will write every save to a separate file in the same directory.
    /// </summary>
    public bool WriteToSameFile { get; set; } = false;
    
    /// <summary>
    /// Only relevant if file writing is enabled.
    /// Decides how many ticks the simulation should wait before saving to a file.
    /// Set to 0 or less to not write to files except for on simulation end.
    /// </summary>
    public int TicksPerFileWrite { get; set; }= 0;
    
    /// <summary>
    /// Handles the writing of a simulation's contents to file
    /// </summary>
    private SimulationExporter simulationExporter;
    
    /// <summary>
    /// This gets called at the end of every tick
    /// </summary>
    public delegate void OnTickEventHandler(World world);
    
    /// <summary>
    /// This gets called at the end of every tick
    /// </summary>
    public event OnTickEventHandler? OnTick;
    
    /// <summary>
    /// This gets called when a file is writen to
    /// </summary>
    public delegate void OnFileWriteEventHandler(string filePath, string fileContents);
    
    /// <summary>
    /// This gets called when a file is writen to
    /// </summary>
    public event OnFileWriteEventHandler? OnFileWrite;
    
    /// <summary>
    /// This gets called when a simulation is aborted/finished.
    /// </summary>
    public delegate void OnEndEventHandler(World world);
    
    /// <summary>
    /// This gets called when a simulation is aborted/finished.
    /// </summary>
    public event OnEndEventHandler? OnEnd;

    /// <summary>
    /// This creates a simulation with the defined World.
    /// After this the DataStructure still has to be set (or it defaults to NoDataStructure).
    /// </summary>
    /// <param name="world"></param>
    public void CreateSimulation(World world)
    {
        this.world = world;
        isMultithreaded = false;
        singleThreadedDataStructure = new NoDataStructure(); //Default data structure has no optimizations
        dataStructure = singleThreadedDataStructure;
        simulationExporter = new SimulationExporter();
        
        //Sets culture to US-English, specific language does not matter, but because we set this, using float.Parse and writing floats to file always use '.' as decimal point.
        System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
        System.Threading.Thread.CurrentThread.CurrentCulture = ci;
    }
    
    /// <summary>
    /// Decides which data structure the simulation uses
    /// </summary>
    /// <param name="dataStructure"></param>
    public void SetDataStructure(DataStructure dataStructure)
    {
        if (dataStructure.IsMultithreaded)
            multiThreadedDataStructure = (MultiThreadedDataStructure)dataStructure;
        else
            singleThreadedDataStructure = (SingleThreadedDataStructure)dataStructure;
        this.dataStructure = dataStructure;
    }

    /// <summary>
    /// Once this is called the simulation starts, depending on the implementation using it, it will run until completion or until halted before that point.
    /// Will save the contents of the simulation once, to allow the user to see the world's starting conditions.
    /// </summary>
    public void StartSimulation()
    {
        world.Initialize();
        world.StartingDistribution();
        dataStructure.Initialize();
        
        CheckWarnings();
        CheckErrors();

        if (FileWritingEnabled)
        {
            Save();
        }
    }

    /// <summary>
    /// Moves the simulation one timestep forward, meaning all organisms get to do 1 'action'.
    /// This version should be used if the data structure is single threaded!
    /// </summary>
    public void Step()
    {
        if (abort || world.StopCondition())
        {
            OnSimulationEnd();
            return;
        }
        
        Tick++;
        world.Step();
        if (dataStructure.IsMultithreaded)
            throw new ArgumentException("Running non asynchronous step while using multi threading!");
        else
            singleThreadedDataStructure.Step();
        OnTick?.Invoke(world);

        //Save file and invoke event letting know that it happened
        if (FileWritingEnabled && TicksPerFileWrite > 0 && Tick % TicksPerFileWrite == 0)
        {
            Save();
        }
    }
    
    /// <summary>
    /// Moves the simulation one timestep forward, meaning all organisms get to do 1 'action'.
    /// This version should be used if the data structure is multi threaded!
    /// </summary>
    public Task StepAsync()
    {
        if (abort || world.StopCondition())
        {
            OnSimulationEnd();
            return Task.CompletedTask;
        }
        
        Tick++;
        world.Step();
        if(dataStructure.IsMultithreaded)
            multiThreadedDataStructure.Step().Wait();
        else
            throw new ArgumentException("Running asynchronous step while using single threading!");
        OnTick?.Invoke(world);

        //Save file and invoke event letting know that it happened
        if (FileWritingEnabled && TicksPerFileWrite > 0 && Tick % TicksPerFileWrite == 0)
        {
            Save();
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Prematurely stops the simulation.
    /// Once this is called the contents of the simulation will be writen to file if it is enabled.
    /// </summary>
    public void AbortSimulation()
    {
        abort = true;
    }

    /// <summary>
    /// Handles the ending of the simulation.
    /// </summary>
    private void OnSimulationEnd()
    {
        if (FileWritingEnabled)
        {
            //Save simulation to file
            Save();
        }
        
        //Tell the user that the simulation is over
        OnEnd?.Invoke(world);
    }

    /// <summary>
    /// Makes a save to a previously specified file location.
    /// Will always try to write, even if file writing is disabled.
    /// </summary>
    public void Save()
    {
        (string filePath, string fileContents) = WriteToSameFile ? simulationExporter.SaveToSameFile(dataStructure, world, this) : simulationExporter.SaveToSeparateFiles(dataStructure, world, this);
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