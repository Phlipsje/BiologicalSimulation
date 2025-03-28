using System;
using BioSim.Datastructures;
using Microsoft.Xna.Framework;

namespace Simple_graphical_implementation;

/// <summary>
/// Used to determine growth properties of organisms, only for this specific visual implementation, not a general system we support
/// </summary>
public class GrowthGrid
{
    private int[,,] foodRemaining;
    private Vector3 minPosition;
    private Vector3 chunkSize;

    public GrowthGrid(Vector3 minPosition, Vector3 maxPosition, Vector3 chunkSize, float largestOrganismSize)
    {
        minPosition = minPosition - chunkSize * 2;
        maxPosition = maxPosition + chunkSize * 2;
        int chunkCountX = (int)Math.Ceiling((maxPosition.X - minPosition.X) / chunkSize.X);
        int chunkCountY = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / chunkSize.Y);
        int chunkCountZ = (int)Math.Ceiling((maxPosition.Z - minPosition.Z) / chunkSize.Z);
        foodRemaining = new int[chunkCountX, chunkCountY, chunkCountZ];
        this.minPosition = minPosition;
        this.chunkSize = chunkSize;

        for (int i = 0; i < chunkCountX; i++)
        {
            for (int j = 0; j < chunkCountY; j++)
            {
                for (int k = 0; k < chunkCountZ; k++)
                {
                    foodRemaining[i, j, k] = 0; //TODO do something with this
                }
            }
        }
    }

    public int GetFood(Vector3 position)
    {
        (int chunkX, int chunkY, int chunkZ) = GetChunk(position);
        return foodRemaining[chunkX, chunkY, chunkZ];
    }
    
    private (int, int, int) GetChunk(Vector3 position)
    {
        int chunkX = (int)Math.Ceiling((position.X - minPosition.X) / chunkSize.X);
        int chunkY = (int)Math.Ceiling((position.Y - minPosition.Y) / chunkSize.Y);
        int chunkZ = (int)Math.Ceiling((position.Z - minPosition.Z) / chunkSize.Z);
        return (chunkX, chunkY, chunkZ);
    }
}