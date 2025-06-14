using System.Numerics;

namespace Continuum.Datastructures;

public abstract class DataStructure
{
    public abstract bool IsMultithreaded { get; }
    
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
    /// Gets called once on simulation start
    /// </summary>
    public virtual void Initialize()
    {
        
    }
    
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
    /// Finds all organisms within a range
    /// </summary>
    /// <param name="organism"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public abstract IEnumerable<Organism> OrganismsWithinRange(Organism organism, float radius);

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

    /// <summary>
    /// Finds the closest intersection out of a list of organisms
    /// </summary>
    /// <param name="organism"></param>
    /// <param name="normalizedDirection"></param>
    /// <param name="length"></param>
    /// <param name="otherOrganisms"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    protected static bool FindMinimumIntersection(Organism organism, Vector3 normalizedDirection, float length, IEnumerable<Organism> otherOrganisms, out float t)
    {
        return FindMinimumIntersection(organism, normalizedDirection, length, otherOrganisms,
            otherOrganism => organism == otherOrganism, out t);
    }
    
    protected static bool FindMinimumIntersection(Organism organism, Vector3 normalizedDirection, float length, 
        IEnumerable<Organism> otherOrganisms, Func<Organism, bool> skipOrganism, out float t)
    {
        bool hit = false;
        t = float.MaxValue;
        foreach (Organism otherOrganism in otherOrganisms)
        {
            if (skipOrganism(otherOrganism))
                continue;
            
            if (RayIntersects(organism.Position, normalizedDirection, length, otherOrganism.Position,
                    organism.Size + otherOrganism.Size, out float tHit))
            {
                if (tHit < t)
                {
                    t = tHit;
                    hit = true;
                }
                    
            }
        }

        float epsilon = 0.01f;
        t -= epsilon;
        
        //Return if there even was a collision
        return hit;
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