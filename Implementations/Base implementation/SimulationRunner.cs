using System;
using System.Numerics;
using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;

namespace Implementations;

/// <summary>
/// This class does all the less interesting setup for a simulation and connects it to a type of program that can run it.
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
    

    /// <summary>
    /// This will start the simulation.
    /// </summary>
    public void Start(int randomSeed = 0)
    {
        Random random = randomSeed == 0 ? new Random() : new Random(randomSeed);
        Simulation.CreateSimulation(World, random);
        Simulation.SetDataStructure(DataStructure);
        
        Simulation.OnEnd += StopProgram;
        Simulation.OnFileWrite += FileWriten;
        
        Simulation.StartSimulation();
        
        ProgramMedium.Simulation = Simulation;
        ProgramMedium.DataStructure = DataStructure;
        ProgramMedium.World = World;
        
        ProgramMedium.StartProgram();
    }
    
    /// <summary>
    /// When the simulation has ended, this will cause the program medium to stop as well.
    /// </summary>
    /// <param name="world"></param>
    private void StopProgram(World world)
    {
        //Simulation has already stopped before this
        ProgramMedium.StopProgram();
    }

    /// <summary>
    /// When the simulation writes to a file, this will cause the program medium to be able to react to it.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileContents"></param>
    private void FileWriten(string filePath, string fileContents)
    {
        ProgramMedium.FileWriten(filePath, fileContents);
    }
    
    /// <summary>
    /// This gets called on cleanup, is needed because this can run executables.
    /// Should not be called by the user.
    /// </summary>
    public void Dispose()
    {
        
    }
}