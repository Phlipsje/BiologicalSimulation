using System.Data;
using BioSim.Datastructures;

namespace BioSim.Simulation;

public partial class Simulation
{
    public void SetDataStructure(DataStructure dataStructure)
    {
        this.dataStructure = dataStructure;
    }

    public void SetDrawFrequency(int ticksPerDrawCall)
    {
        if (ticksPerDrawCall <= 0)
        {
            throw new Exception("Ticks per draw call must be greater than zero.");
        }
        
        this.ticksPerDrawCall = ticksPerDrawCall;
    }
    
    public void SetFileWriteFrequency(int ticksPerFileWrite)
    {
        if (ticksPerFileWrite <= 0)
        {
            throw new Exception("Ticks per draw call must be greater than zero.");
        }
        
        this.ticksPerFileWrite = ticksPerFileWrite;
    }
}