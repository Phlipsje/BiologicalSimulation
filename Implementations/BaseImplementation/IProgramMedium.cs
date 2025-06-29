using Continuum;
using Continuum.Simulation;
using Continuum.Datastructures;

namespace Implementations.BaseImplementation;

public interface IProgramMedium
{
    public Simulation Simulation { get; set; }
    public World World { get; set; }
    public DataStructure DataStructure { get; set; }
    public void StartProgram();
    public void StopProgram();
    public void FileWriten(string filePath, string fileContents);
}