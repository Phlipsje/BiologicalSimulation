using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;
using Microsoft.Win32.SafeHandles;

namespace Implementations.Console_implementation;

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
    //We have a separate thread for input, because otherwise Console.ReadLine() would block running the simulation
    private Thread inputThread;
    public void StartProgram()
    {
        AllocConsole();
        RedirectConsoleIO();
        
        Console.WriteLine("Running Biological Simulation");
        Console.WriteLine($"Using data structure: {DataStructure.GetType()}.");
        Console.WriteLine($"Starting organism count: {World.GetOrganismCount()}.");
        if (Simulation.FileWritingEnabled)
        {
            Console.WriteLine($"Data will be written to file every: {Simulation.TicksPerFileWrite} ticks.");
            Console.WriteLine($"Data will be stored in:");
            Console.WriteLine(Path.GetFullPath(SimulationExporter.SaveDirectory));
        }
        else
            Console.WriteLine("[WARNING] File writing is disabled.");
        
        Console.WriteLine("A limited overview will about the current simulation state will be given every time a save is made.");
        
        Console.WriteLine("Press 'q' to abort the simulation, all data writen to files will persist and one final save will be made.");
        Console.WriteLine("Press 'h' for help commands while simulation is running.");

        //Thread will call ReadInput()
        inputThread = new Thread(ReadInput);
        //It will run in the background (thus never blocking a process or action)
        inputThread.IsBackground = true;
        inputThread.Start();
        
        //Actually start program
        CoreLoop();
    }

    private void CoreLoop()
    {
        while (looping)
        {
            if (!Simulation.FileWritingEnabled && Simulation.Tick % 250 == 0)
                PrintSimulationStats();
            Simulation.Step();
        }
    }

    private void ReadInput()
    {
        while (looping)
        {
            char c = Console.ReadKey().KeyChar;

            switch (c)
            {
                case 'q':
                    Simulation.AbortSimulation();
                    break;
                case 'h':
                    PrintHelp();
                    break;
            }
        }
    }

    public void PrintHelp()
    {
        //Stuff like, print stats and other stuff
        throw new NotImplementedException();
    }

    public void PrintSimulationStats()
    {
        //TODO add fps, or real time running
        string[] lines =
        [
            $"|Tick: {Simulation.Tick}|",
            $"|Organisms: {World.GetOrganismCount()}|",
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
        
        FreeConsole();
    }

    public void FileWriten(string filePath, string fileContents)
    {
        PrintSimulationStats();
    }

    #region Console handling stuff
    /// <summary>
    /// Extra stuff here is to make sure the console in windows is used and not the IDE
    /// </summary>
    
    //This makes it so Console.WriteLine is sent to the console and not to the IDE
    private void RedirectConsoleIO()
    {
        IntPtr stdOutHandle = CreateFile("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
        IntPtr stdInHandle = CreateFile("CONIN$", GENERIC_READ, FILE_SHARE_READ, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);

        SetStdHandle(STD_OUTPUT_HANDLE, stdOutHandle);
        SetStdHandle(STD_ERROR_HANDLE, stdOutHandle);
        SetStdHandle(STD_INPUT_HANDLE, stdInHandle);

        var outStream = new FileStream(new SafeFileHandle(stdOutHandle, true), FileAccess.Write);
        var inStream = new FileStream(new SafeFileHandle(stdInHandle, true), FileAccess.Read);

        var writer = new StreamWriter(outStream) { AutoFlush = true };
        Console.SetOut(writer);
        Console.SetError(writer);
        Console.SetIn(new StreamReader(inStream));
    }

    const int STD_OUTPUT_HANDLE = -11;
    const int STD_INPUT_HANDLE = -10;
    const int STD_ERROR_HANDLE = -12;

    //With this we can create a console application
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();
    
    //With this we can forcefully close the console
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeConsole();
    
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetStdHandle(int nStdHandle, IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    const uint GENERIC_READ = 0x80000000;
    const uint GENERIC_WRITE = 0x40000000;
    const uint OPEN_EXISTING = 3;
    const uint FILE_ATTRIBUTE_NORMAL = 0x80;
    const uint FILE_SHARE_READ = 1;
    const uint FILE_SHARE_WRITE = 2;
    #endregion
}