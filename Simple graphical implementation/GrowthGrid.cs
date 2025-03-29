using System;
using BioSim.Datastructures;
using Microsoft.Xna.Framework;

namespace Simple_graphical_implementation;

/// <summary>
/// Used to determine growth properties of organisms, only for this specific visual implementation, not a general system we support
/// </summary>
public static class GrowthGrid
{
    private static GridValues[,,] values;
    private static Vector3 minPosition;
    private static Vector3 chunkSize;
    private static (int, int, int) maxIndices;

    public static void Initialize(Vector3 minPosition, Vector3 maxPosition, Vector3 chunkSize)
    {
        minPosition = minPosition - chunkSize * 2;
        maxPosition = maxPosition + chunkSize * 2;
        int chunkCountX = (int)Math.Ceiling((maxPosition.X - minPosition.X) / chunkSize.X);
        int chunkCountY = (int)Math.Ceiling((maxPosition.Y - minPosition.Y) / chunkSize.Y);
        int chunkCountZ = (int)Math.Ceiling((maxPosition.Z - minPosition.Z) / chunkSize.Z);
        values = new GridValues[chunkCountX, chunkCountY, chunkCountZ];
        GrowthGrid.minPosition = minPosition;
        GrowthGrid.chunkSize = chunkSize;
        maxIndices = (chunkCountX, chunkCountY, chunkCountZ);

        for (int i = 0; i < chunkCountX; i++)
        {
            for (int j = 0; j < chunkCountY; j++)
            {
                for (int k = 0; k < chunkCountZ; k++)
                {
                    values[i, j, k] = new GridValues();
                }
            }
        }
    }

    public static void Step()
    {
        float dR = 0.2f;
        float dBB1 = 0.2f;
        float dBB2 = 0.2f;
        float decayR = 0.001f;
        float decayBB = 0.001f;

        for (int x = 0; x < maxIndices.Item1; x++)
        {
            for (int y = 0; y < maxIndices.Item2; y++)
            {
                for (int z = 0; z < maxIndices.Item3; z++)
                {
                    GridValues oldValues = values[x, y, z];
                    values[x,y,z].R = oldValues.R*(1-decayR);
                    values[x,y,z].BB1 = oldValues.BB1 * (1-decayBB);
                    values[x,y,z].BB2 = oldValues.BB2 * (1-decayBB);
                }
            }
        }
    }

    public static GridValues GetValues(Vector3 position)
    {
        (int chunkX, int chunkY, int chunkZ) = GetChunk(position);
        return values[chunkX, chunkY, chunkZ];
    }

    public static void SetRValue(Vector3 position, float r)
    {
        (int chunkX, int chunkY, int chunkZ) = GetChunk(position);
        values[chunkX, chunkY, chunkZ].R = r;
    }
    
    public static void SetBB1Valus(Vector3 position, float bbA)
    {
        (int chunkX, int chunkY, int chunkZ) = GetChunk(position);
        values[chunkX, chunkY, chunkZ].BB1 = bbA;
    }
    
    public static void SetBB2Valus(Vector3 position, float bbB)
    {
        (int chunkX, int chunkY, int chunkZ) = GetChunk(position);
        values[chunkX, chunkY, chunkZ].BB2 = bbB;
    }
    
    private static (int, int, int) GetChunk(Vector3 position)
    {
        int chunkX = (int)Math.Ceiling((position.X - minPosition.X) / chunkSize.X);
        int chunkY = (int)Math.Ceiling((position.Y - minPosition.Y) / chunkSize.Y);
        int chunkZ = (int)Math.Ceiling((position.Z - minPosition.Z) / chunkSize.Z);
        return (chunkX, chunkY, chunkZ);
    }
}

public struct GridValues
{
    public float R; //No idea what this is
    public float BB1; //Don't know what BB is
    public float BB2; //Don't know what BB is

    public GridValues()
    {
        R = 0;
        BB1 = 0;
        BB2 = 0;
    }
}