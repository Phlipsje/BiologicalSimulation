using System.Numerics;
using BiologicalSimulation.Datastructures.RTree;
using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;
using Implementations;
using Implementations.Console_implementation;
using Implementations.Monogame2DRenderer;
using Implementations.OpenTK3DRenderer;

namespace BasicImplementation;

/// <summary>
/// Controls the custom simulation
/// </summary>
class Program
{
    //Easiest way to implement global counter, not most safe way of doing it
    public static int OrganismACount = 0;
    public static int OrganismBCount = 0;
    
    static void Main(string[] args)
    {
        SimulationRunner runner = new SimulationRunner();
        
        Simulation simulation = SimulationRunner.Simulation;
        
        //Choose the data structure that is used to speed up the simulation
        float worldHalfSize = 16f;
        float organismSize = 0.5f;
        SimulationRunner.DataStructure = new RTreeDataStructure(MathF.Sqrt(3f * 0.1f * 0.1f) + 0.18f);
        //SimulationRunner.DataStructure = new NoDataStructure();
        //SimulationRunner.DataStructure = new MultithreadedChunk3DDataStructure(new Vector3(-worldHalfSize), new Vector3(worldHalfSize), 4f, organismSize);
        
        //Create a world which implements the data structure and defines rules such as:
        // where organisms start in the simulation, what the bounds are of the virtual environment and when to stop the simulation automatically
        SimulationRunner.World = new TestWorld(SimulationRunner.DataStructure, simulation, worldHalfSize);
        
        //Decide if, and when, to save the contents of the simulation to a file
        simulation.FileWritingEnabled = false;
        simulation.TicksPerFileWrite = 100;
        //Also decide where to save the contents to
        simulation.WriteToSameFile = true;
        SimulationExporter.FileName = "testing";
        SimulationExporter.SaveDirectory = "Content\\Testing";
        //This is a small debug to show the user where the save file is located
        SimulationExporter.ShowExportFilePath = true;
        
        //Choose in what form the simulation is run (this decides if you get a Console view, 2D view or 3D view)
        SimulationRunner.ProgramMedium = new ConsoleApp();

        //Starts the simulation, add an integer value as a parameter to set a seed,
        // doing so will cause the simulation to play out exactly the same every time (with exception to multithreading data structures due to race conditions).
        runner.Start(500);
    }
}