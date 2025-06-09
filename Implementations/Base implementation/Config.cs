using System.Numerics;
using BioSim.Datastructures;
using BioSim.Simulation;
using Implementations.Console_implementation;

namespace Implementations;

/// <summary>
/// Use this class to make adjustments to how the simulation is run
/// </summary>
public class Config
{
    public void Setup()
    {
        Simulation simulation = Main.Simulation;
        
        //Choose the data structure that is used to speed up the simulation
        float worldHalfSize = 12f;
        float organismSize = 0.5f;
        Main.DataStructure = new MultithreadedChunk3DFixedDataStructure(new Vector3(-worldHalfSize), 
            new Vector3(worldHalfSize), 4f, organismSize);
        
        //Create a world which implements the data structure and defines rules such as:
        // where organisms start in the simulation, what the bounds are of the virtual environment and when to stop the simulation automatically
        Main.World = new TestWorld(Main.DataStructure, simulation, worldHalfSize);
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
        Main.ProgramMedium = new ConsoleApp();
    }
}