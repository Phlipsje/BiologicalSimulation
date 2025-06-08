using System;
using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;
using Microsoft.Xna.Framework;
using Vector3  = System.Numerics.Vector3;

namespace Simple_graphical_implementation;

/// <summary>
/// This class does the setup for a world and data structure and chooses a medium to run it in
/// </summary>
public class Main : IDisposable
{
    private static World world;
    private static DataStructure dataStructure;
    private static Simulation simulation;
    private static IProgramMedium programMedium;
    public static int Tick => simulation.Tick;

    //Easiest way to implement global counter, not most safe way of doing it
    public static int OrganismACount = 0;
    public static int OrganismBCount = 0;

    public Main()
    {
        Initialize();

        programMedium = new VisualSimulation();
        programMedium.Simulation = simulation;
        programMedium.DataStructure = dataStructure;
        programMedium.World = world;
        ((VisualSimulation)programMedium).Run();
    }

    public void Initialize()
    {
        simulation = new Simulation();
        Random random = new Random(); //Can enter seed here
        float worldHalfSize = 12f;
        float organismSize = 0.5f;
        dataStructure = new MultithreadedChunk3DFixedDataStructure(new Vector3(-worldHalfSize), 
            new Vector3(worldHalfSize), 4f, organismSize);
        world = new TestWorld(dataStructure, simulation, worldHalfSize);
        //TestOrganism exampleOrganism = new TestOrganism(Vector3.Zero, organismSize, world, dataStructure, random);
        //OrganismManager.RegisterOrganism(exampleOrganism.Key, exampleOrganism.CreateNewOrganism);
        simulation.CreateSimulation(world, random);
        simulation.SetDataStructure(dataStructure);
        simulation.DrawingEnabled = true;
        simulation.SetDrawFrequency(1);
        
        //For saving to file
        simulation.FileWritingEnabled = true;
        simulation.WriteToSameFile = true;
        simulation.SetFileWriteFrequency(100);
        SimulationExporter.FileName = "testing";
        SimulationExporter.SaveDirectory = "Content\\Testing";
        SimulationExporter.ShowExportFilePath = true;
        SimulationExporter.ClearDirectory = true;
        
        simulation.OnEnd += StopProgram;
        simulation.OnDraw += OnDrawCall;
        
        OrganismACount = 0;
        OrganismBCount = 0;
        GrowthGrid.Initialize(new Vector3(-worldHalfSize), 
            new Vector3(worldHalfSize), new Vector3(0.5f));
        simulation.StartSimulation();
    }
    
    private void StopProgram(World world)
    {
        //Simulation has already stopped before this
        programMedium.StopProgram();
    }

    private void OnDrawCall(World world)
    {
        programMedium.DrawCall();
    }
    
    public void Dispose()
    {
        
    }
}