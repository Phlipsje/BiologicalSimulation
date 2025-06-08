using System;
using System.Numerics;
using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;

namespace Implementations;

/// <summary>
/// This class does the setup for a world and data structure and chooses a medium to run it in
/// </summary>
public class Main : IDisposable
{
    public static World World;
    public static DataStructure DataStructure;
    public static Simulation Simulation;
    public static IProgramMedium ProgramMedium;
    public static int Tick => Simulation.Tick;

    //Easiest way to implement global counter, not most safe way of doing it
    public static int OrganismACount = 0;
    public static int OrganismBCount = 0;

    public Main()
    {
        Simulation = new Simulation();
        
        Config config = new Config();
        config.Setup();
        
        Initialize();
        
        ProgramMedium.Simulation = Simulation;
        ProgramMedium.DataStructure = DataStructure;
        ProgramMedium.World = World;
        ProgramMedium.StartProgram();
    }

    public void Initialize()
    {
        Random random = new Random(); //Can enter seed here
        Simulation.CreateSimulation(World, random);
        Simulation.SetDataStructure(DataStructure);
        
        Simulation.OnEnd += StopProgram;
        
        OrganismACount = 0;
        OrganismBCount = 0;
        Simulation.StartSimulation();
    }
    
    private void StopProgram(World world)
    {
        //Simulation has already stopped before this
        ProgramMedium.StopProgram();
    }
    
    public void Dispose()
    {
        
    }
}