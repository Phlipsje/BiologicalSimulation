using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;

namespace Implementations;

public interface IProgramMedium
{
    public Simulation Simulation { get; set; }
    public World World { get; set; }
    public DataStructure DataStructure { get; set; }
    public void StartProgram();
    public void StopProgram();
}