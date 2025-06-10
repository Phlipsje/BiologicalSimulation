using System.Numerics;
using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;
using Implementations;
using Implementations.Monogame2DRenderer;

namespace GrowthGridImplementation;

/// <summary>
/// More detailed implementation that BasicImplementation.csproj, but does NOT work with MULTITHREADING
/// This is due to the GrowthGrid.cs being a single static class that is called from all threads whenever they feel like it, without any locking mechanism
/// It will succesfully run with multithreaded data structures, but the results regarding growth WILL BE INCORRECT
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
        float worldHalfSize = 12f;
        float organismSize = 0.5f;
        SimulationRunner.DataStructure = new MultithreadedChunk3DFixedDataStructure(new Vector3(-worldHalfSize), 
            new Vector3(worldHalfSize), 4f, organismSize);
        
        //Create a world which implements the data structure and defines rules such as:
        // where organisms start in the simulation, what the bounds are of the virtual environment and when to stop the simulation automatically
        SimulationRunner.World = new TestWorld(SimulationRunner.DataStructure, simulation, worldHalfSize);
        
        //For this specific simulation we have made use of an extra class for which we also do the setup here, this is not needed for every type of simulation
        GrowthGrid.Initialize(new Vector3(-worldHalfSize), 
            new Vector3(worldHalfSize), new Vector3(0.5f));
        
        //Decide if, and when, to save the contents of the simulation to a file
        simulation.FileWritingEnabled = false;
        simulation.SetFileWriteFrequency(100);
        //Also decide where to save the contents to
        simulation.WriteToSameFile = true;
        SimulationExporter.FileName = "testing";
        SimulationExporter.SaveDirectory = "Content\\Testing";
        SimulationExporter.ClearDirectory = true;
        //This is a small debug to show the user where the save file is located
        SimulationExporter.ShowExportFilePath = true;
        
        //Choose in what form the simulation is run (this decides if you get a Console view, 2D view or 3D view)
        SimulationRunner.ProgramMedium = new Monogame2DRenderer();

        SimulationRunner.Simulation.OnTick += Step;

        runner.Start();
    }

    private static void Step(World world)
    {
        GrowthGrid.Step();
    }
}