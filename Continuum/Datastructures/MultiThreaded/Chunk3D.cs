using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace Continuum.Datastructures.MultiThreaded;

internal class Chunk3D
{
    internal World World { get; set; }
    public Vector3 Center { get; }
    public float HalfDimension { get; } //Size from center (so half of full length)
    public int OrganismCount { get; private set; }
    public List<Organism> Organisms { get; }
    public ConcurrentQueue<Organism> CheckToBeAdded; //This is a queue, because emptied every frame
    public Chunk3D[] ConnectedChunks; //Connected chunks is at most a list of 26 (9+8+9 for each chunk touching this chunk (also diagonals))
    private bool stepping = false;
    
    public Chunk3D(Vector3 center, float size)
    {
        Center = center;
        HalfDimension = size/2f;
        Organisms = new List<Organism>(50);
        CheckToBeAdded = new ConcurrentQueue<Organism>();
    }
    
    public void Initialize(Chunk3D[] connectedChunks)
    {
        //Connected chunks is at most a list of 26 (9+8+9 for each chunk touching this chunk (also diagonals))
        this.ConnectedChunks = connectedChunks;
    }
    
    public Task Step()
    {
        if (stepping)
        {
            //Eventually remove this, but now keep to check if problem is truely gone
            throw new Exception("Multithreading caused duplicate steps, this is a problem!");
        }

        stepping = true;
        
        if (World.RandomisedExecutionOrder)
            HelperFunctions.KnuthShuffle(Organisms);
        
        //Check what should be added to chunk
        //No removals happen during this
        CheckNewPossibleAdditions();
        
        //Run update loop
        for (int i = 0; i < Organisms.Count; i++)
        {
            //Move and run step for organism (organism does collision check with knowledge of exclusively what this chunk knows (which is enough)
            Organisms[i].Step();
        }
        
        //Update what should and should not be in this chunk
        //No additions happen during this (to this chunk)
        for (int i = 0; i < Organisms.Count; i++)
        {
            bool removed = CheckPosition(Organisms[i]);
            if (removed)
                i--;
        }

        stepping = false;
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Check every entry in checkToBeAdded queue for possible addition
    /// O(#checkToBeAdded * #Organisms)
    /// </summary>
    private void CheckNewPossibleAdditions()
    {
        while (CheckToBeAdded.Count > 0)
        {
            bool success = CheckToBeAdded.TryDequeue(out Organism organism);
            if (!success)
                continue;
            
            float singleAxisDistance = SingleAxisDistance(organism);

            //Might not even need the contains, but keeping it for now to be safe
            if (singleAxisDistance <= HalfDimension && !Organisms.Contains(organism))
            {
                Organisms.Add(organism);
                OrganismCount++;
            }
        }
    }
    
    /// <summary>
    /// Checks if an organism should be within this chunk based off of it's recently updated position, also checks if it should be added to a neighbouring chunk
    /// O(#connectedChunks) = O(26)
    /// </summary>
    /// <param name="organism"></param>
    private bool CheckPosition(Organism organism)
    {
        //Set the largest of the distances per axis, that is enough to check if it should be within or not
        float singleAxisDistance = SingleAxisDistance(organism);
        
        if (singleAxisDistance > HalfDimension)
        {
            //Send to neighbouring chunk for checking
            foreach (Chunk3D chunk in ConnectedChunks)
            {
                chunk.CheckToBeAdded.Enqueue(organism);
            }
            
            //Removing via node if faster
            Organisms.Remove(organism);
            OrganismCount--;
            return true;
        }

        return false;
    }

    [Pure]
    private float SingleAxisDistance(Organism organism)
    {
        return Math.Max(Math.Max(Math.Abs(organism.Position.X - Center.X), Math.Abs(organism.Position.Y - Center.Y)), Math.Abs(organism.Position.Z - Center.Z));
    }
    
    /// <summary>
    /// Only call this via AddOrganism in DataStructure, should not be used to communicate between chunks
    /// </summary>
    /// <param name="organism"></param>
    public void DirectlyInsertOrganism(Organism organism)
    {
        Organisms.Add(organism);
        OrganismCount++;
    }
}