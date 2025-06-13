using System.Collections;
using System.Numerics;

namespace BioSim.Datastructures;
/// <summary>
/// An abstract class used to define an object that can help in more efficiently running position based queries
/// </summary>
public abstract class DataStructure
{
    /// <summary>
    /// To connect with what is happening in the world.
    /// </summary>
    protected World World { get; private set; }

    /// <summary>
    /// Sets the world to keep as a reference.
    /// </summary>
    /// <param name="world"></param>
    internal void SetWorld(World world)
    {
        World = world;
    }
    
    /// <summary>
    /// Gets called every tick, after the updating of World.cs
    /// </summary>
    /// <returns></returns>
    public abstract Task Step();
    
    /// <summary>
    /// Removes all Organisms from the simulation.
    /// </summary>
    /// <returns></returns>
    public abstract Task Clear();
    
    /// <summary>
    /// Adds a new organism to the simulation.
    /// </summary>
    /// <param name="organism"></param>
    public abstract void AddOrganism(Organism organism);
    
    /// <summary>
    /// Removes an organism from the simulation.
    /// </summary>
    /// <param name="organism"></param>
    /// <returns></returns>
    public abstract bool RemoveOrganism(Organism organism);
    
    /// <summary>
    /// Gets a list of all currently active organisms.
    /// </summary>
    /// <returns></returns>
    public abstract Task GetOrganisms(out IEnumerable<Organism> organisms);
    
    /// <summary>
    /// Gets the total amount of organisms currently active.
    /// </summary>
    /// <returns></returns>
    public abstract Task GetOrganismCount(out int count);
    
    /// <summary>
    /// Checks if an organisms would collide with the world bounds or another organisms by moving to the newly given position.
    /// </summary>
    /// <param name="organism"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public abstract bool CheckCollision(Organism organism, Vector3 position);
    
    /// <summary>
    /// Finds the Organism closest to the given organism.
    /// NOTE: Depending on the data structure that implements it, can return null if the range to a nearest organism is too great.
    /// </summary>
    /// <param name="organism"></param>
    /// <returns></returns>
    public abstract Organism? NearestNeighbour(Organism organism);

    /// <summary>
    /// Find the first intersection when the organism tries to move in the given direction up to the given length.
    /// Returns a boolean based on if an intersection takes place at all.
    /// The outed value t is where the collision took place.
    /// </summary>
    /// <param name="organism"></param>
    /// <param name="normalizedDirection"></param>
    /// <param name="length"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public abstract bool FindFirstCollision(Organism organism, Vector3 normalizedDirection, float length,
        out float t);

    protected static bool FindMinimumIntersection(Organism organism, Vector3 normalizedDirection, float length, IEnumerable<Organism> otherOrganisms, out float t)
    {
        t = float.MaxValue;
        foreach (Organism otherOrganism in otherOrganisms)
        {
            if (RayIntersects(organism.Position, normalizedDirection, length, otherOrganism.Position,
                    organism.Size + otherOrganism.Size, out float tHit))
            {
                if (tHit < t)
                    t = tHit;
            }
        }
        
        //Return if there even was a collision
        return Math.Abs(t - float.MaxValue) > 1f;
    }
    
    /// <summary>
    /// Helper function for collision checks with spheres
    /// </summary>
    /// <param name="rayOrigin"></param>
    /// <param name="rayDir"></param>
    /// <param name="maxDistance"></param>
    /// <param name="sphereCenter"></param>
    /// <param name="sphereRadius"></param>
    /// <param name="tHit"></param>
    /// <returns></returns>
    protected static bool RayIntersects(Vector3 rayOrigin, Vector3 rayDir, float maxDistance,
        Vector3 sphereCenter, float sphereRadius, out float tHit)
    {
        Vector3 oc = rayOrigin - sphereCenter;
        float r = sphereRadius;

        float a = Vector3.Dot(rayDir, rayDir);
        float b = 2.0f * Vector3.Dot(oc, rayDir);
        float c = Vector3.Dot(oc, oc) - r * r;

        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            tHit = float.MaxValue;
            return false;
        }

        float sqrtDiscriminant = MathF.Sqrt(discriminant);
        float t0 = (-b - sqrtDiscriminant) / (2 * a);
        float t1 = (-b + sqrtDiscriminant) / (2 * a);

        // Find the first valid hit within range
        if (t0 >= 0 && t0 <= maxDistance)
        {
            tHit = t0;
            return true;
        }
        if (t1 >= 0 && t1 <= maxDistance)
        {
            tHit = t1;
            return true;
        }

        tHit = float.MaxValue;
        return false;
    }
}