using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Continuum;
using Continuum.Datastructures;
using Continuum.Simulation;
using Implementations.BaseImplementation;

namespace Implementations.ConsoleImplementation;

/// <summary>
/// This implementation only opens a console window to avoid having to draw the organisms in any way.
/// This is the fastest implementation available and should be used in every situation where rendering the organisms is not strictly necessary.
/// 
/// </summary>
public class ConsoleApp : IProgramMedium
{
    public Simulation Simulation { get; set; }
    public World World { get; set; }
    public DataStructure DataStructure { get; set; }
    private bool looping = true;
    private int startOrganismCount = 0;
    
    //We have a separate thread for input, because otherwise Console.ReadLine() would block running the simulation
    private Thread inputThread;
    
    private int ticksPerPrint; //Set to 0 to disable (only used when file writing is disabled)
    private bool print = true;
    
    //For tracking fps performance
    private Stopwatch stopwatch;
    public static float TimeRunning { get; private set; }
    public static float AverageFps { get; private set; }
    private float tallyFps;
    private int fpsCounter;
    private const int ticksPerUpdate = 15;

    public ConsoleApp()
    {
        //Start with all default values
        DefaultValues();
    }

    public ConsoleApp(string[] args)
    {
        DefaultValues();
        foreach (string s in args)
        {
            try
            {
                string field = s.Split('=')[0];
                string value = s.Split('=')[1];
                //input sanitizing
                field = field.Trim();
                value = value.Trim();
                field = field.ToLower();
                value = value.ToLower();
                switch (field)
                {
                    case "tpp":
                        ticksPerPrint = int.Parse(value);
                        break;
                    case "print":
                        print = value is "true" or "t";
                        break;
                    default:
                        Console.WriteLine("Invalid argument: " + s);
                        break;
                }
            }
            catch //Just ignore anything invalid
            {
                Console.WriteLine("Invalid argument: " + s);
            }
        }
    }

    private void DefaultValues()
    {
        TimeRunning = 0;
        ticksPerPrint = 500;
    }
    
    public void StartProgram()
    {
        int count;
        if (DataStructure.IsMultithreaded)
        {
            World.GetOrganismCountAsync(out int c).Wait();
            count = c;
        }
        else
            count = World.GetOrganismCount();
        startOrganismCount = count;
        
        Console.WriteLine("Running Biological Simulation");
        
        PrintSimulationInfo();
        
        Console.WriteLine("A limited overview will about the current simulation state will be given every time a save is made");
        
        Console.WriteLine("Press 'q' to abort the simulation, all data writen to files will persist and one final save will be made");
        Console.WriteLine("Press 'h' for help commands while simulation is running");

        //Thread will call ReadInput()
        inputThread = new Thread(ReadInput);
        //It will run in the background (thus never blocking a process or action)
        inputThread.IsBackground = true;
        inputThread.Start();
        
        stopwatch = new Stopwatch();
        stopwatch.Start();
        
        //Actually start program

        if (DataStructure.IsMultithreaded)
            CoreLoopAsync().Wait();
        else
            CoreLoop();
    }

    private void CoreLoop()
    {
        while (looping)
        {
            Simulation.Step();

            // Performance Tracking
            TimeRunning += (float)stopwatch.Elapsed.TotalSeconds;
            tallyFps += 1 / (float)stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            fpsCounter++;
            if (fpsCounter >= ticksPerUpdate)
            {
                AverageFps = tallyFps / fpsCounter;
                if (AverageFps is float.PositiveInfinity)
                    AverageFps = 0;
                fpsCounter = 0;
                tallyFps = 0;
            }

            //Does not run on FileWritingEnabled as that is already handled by FileWriten()
            if (print && !Simulation.FileWritingEnabled && ticksPerPrint > 0 && Simulation.Tick % ticksPerPrint == 0)
                PrintSimulationStats();
        }

        stopwatch.Stop();
    }
    
    private Task CoreLoopAsync()
    {
        while (looping)
        {
            Simulation.StepAsync().Wait();

            // Performance Tracking
            TimeRunning += (float)stopwatch.Elapsed.TotalSeconds;
            tallyFps += 1 / (float)stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            fpsCounter++;
            if (fpsCounter >= ticksPerUpdate)
            {
                AverageFps = tallyFps / fpsCounter;
                if (AverageFps is float.PositiveInfinity)
                    AverageFps = 0;
                fpsCounter = 0;
                tallyFps = 0;
            }

            //Does not run on FileWritingEnabled as that is already handled by FileWriten()
            if (print && !Simulation.FileWritingEnabled && ticksPerPrint > 0 && Simulation.Tick % ticksPerPrint == 0)
                PrintSimulationStats();
        }

        stopwatch.Stop();

        return Task.CompletedTask;
    }

    private void ReadInput()
    {
        while (looping)
        {
            char c = Console.ReadKey(true).KeyChar;

            switch (c)
            {
                case 'q':
                    Simulation.AbortSimulation();
                    break;
                case 'h':
                    PrintHelp();
                    break;
                case 'o':
                    PrintSimulationStats();
                    break;
                case 'i':
                    PrintSimulationInfo();
                    break;
                case 'p':
                    if (looping)
                        Console.WriteLine("Simulation is currently running, Tick: " + Simulation.Tick);
                    else
                        Console.WriteLine("Simulation is NOT currently active");
                    break;
                case 's':
                    Console.WriteLine("[Simulation manually saved]");
                    Simulation.Save();
                    break;
            }
        }
    }

    public void PrintHelp()
    {
        //Stuff like, print stats and other stuff
        Console.WriteLine("------COMMANDS------");
        Console.WriteLine("- q: ABORT simulation");
        Console.WriteLine("- i: INFO about simulation");
        Console.WriteLine("- o: STATS about simulation");
        Console.WriteLine("- p: PING process");
        Console.WriteLine("- s: SAVE simulation manually");
    }

    public void PrintSimulationInfo()
    {
        Console.WriteLine($"[{DateTime.Now}]");
        Console.WriteLine($"Simulating world: {World.GetType()}");
        Console.WriteLine($"Using data structure: {DataStructure.GetType()}");
        Console.WriteLine($"Starting organism count: {startOrganismCount}");
        if (Simulation.FileWritingEnabled)
        {
            Console.WriteLine($"Data will be written to file every: {Simulation.TicksPerFileWrite} ticks");
            Console.WriteLine($"Data will be stored in:");
            Console.WriteLine(Path.GetFullPath(SimulationExporter.SaveDirectory));
        }
        else
            Console.WriteLine("[WARNING] File writing is disabled");
    }

    public void PrintSimulationStats()
    {
        int count;
        if (DataStructure.IsMultithreaded)
        {
            World.GetOrganismCountAsync(out int c).Wait();
            count = c;
        }
        else
            count = World.GetOrganismCount();
        startOrganismCount = count;
        string[] lines =
        [
            $"|[{DateTime.Now.ToString("HH:mm:ss")}]|",
            $"|Tick: {Simulation.Tick}|",
            $"|Organisms: {count}|",
            $"|Runtime: {Math.Round(TimeRunning, 2)}s|",
            $"|Tick/Sec: {Math.Round(AverageFps, 2)}/s|",
            $"|Sec/Tick: {Math.Round(1/AverageFps, 3)}s|"
        ];

        int length = lines.Select(s =>
        {
            return s.Length;
        }).Max();

        //Reformatting
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Substring(0, lines[i].Length-1) + new String(' ', length - lines[i].Length) + '|';
        }
        
        //Some pretty printing
        Console.WriteLine('+' + new String('-', length-2) + '+');
        foreach (string line in lines)
        {
            Console.WriteLine(line);
        }
        Console.WriteLine('+' + new String('-', length-2) + '+');
    }
    
    public void StopProgram()
    {
        looping = false;
        PrintSimulationStats();

        Console.WriteLine("Press any key to close the console...");
        
        //Readline is here to await input
        Console.ReadKey();
        
        Environment.Exit(0);
    }

    public void FileWriten(string filePath, string fileContents)
    {
        if(print)
            PrintSimulationStats();
    }
}