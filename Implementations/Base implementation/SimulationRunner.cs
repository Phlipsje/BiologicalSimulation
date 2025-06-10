using System;
using System.Numerics;
using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;

namespace Implementations;

/// <summary>
/// This class does the setup for a world and data structure and chooses a medium to run it in
/// </summary>
public class SimulationRunner : IDisposable
{
    public static World World;
    public static DataStructure DataStructure;
    public static Simulation Simulation;
    public static IProgramMedium ProgramMedium;
    public static int Tick => Simulation.Tick;

    public SimulationRunner()
    {
        //Sets culture to US-English, specific language does not matter, but because we set this, using float.Parse and writing floats to file always use '.' as decimal point.
        System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
        System.Threading.Thread.CurrentThread.CurrentCulture = ci;
        
        Simulation = new Simulation();
    }

    public void Initialize()
    {
        Random random = new Random(); //Can enter seed here
        Simulation.CreateSimulation(World, random);
        Simulation.SetDataStructure(DataStructure);
        
        Simulation.OnEnd += StopProgram;
        Simulation.OnFileWrite += FileWriten;
        
        Simulation.StartSimulation();
    }

    public void Start()
    {
        Initialize();
        
        ProgramMedium.Simulation = Simulation;
        ProgramMedium.DataStructure = DataStructure;
        ProgramMedium.World = World;
        
        ProgramMedium.StartProgram();
    }
    
    private void StopProgram(World world)
    {
        //Simulation has already stopped before this
        ProgramMedium.StopProgram();
    }

    private void FileWriten(string filePath, string fileContents)
    {
        ProgramMedium.FileWriten(filePath, fileContents);
    }
    
    public void Dispose()
    {
        
    }
}