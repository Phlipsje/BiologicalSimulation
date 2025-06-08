using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;

namespace Simple_graphical_implementation;

public interface IProgramMedium
{
    public Simulation Simulation { get; set; }
    public World World { get; set; }
    public DataStructure DataStructure { get; set; }
    public void StartProgram();
    public void StopProgram();
    public void DrawCall();
}