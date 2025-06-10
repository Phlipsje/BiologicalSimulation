using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace BioSim.Datastructures;

public class Chunk2D
{
    public Vector2 Center { get; }
    public float HalfDimension { get; } //Size from center (so half of full length)
    public int OrganismCount { get; private set; }
    public LinkedList<Organism> Organisms { get; }
    public ConcurrentQueue<Organism> CheckToBeAdded; //This is a queue, because emptied every frame
    public Chunk2D[] ConnectedChunks; //Connected chunks is at most a list of 26 (9+8+9 for each chunk touching this chunk (also diagonals))
    private bool stepping = false;

    public Chunk2D(Vector2 center, float size)
    {
        Center = center;
        HalfDimension = size/2f;
        Organisms = new LinkedList<Organism>();
        CheckToBeAdded = new ConcurrentQueue<Organism>();
    }
    
    public void Initialize(Chunk2D[] connectedChunks)
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
        
        //Check what should be added to chunk
        //No removals happen during this
        CheckNewPossibleAdditions();
        
        //Run update loop
        for (LinkedListNode<Organism> organismNode = Organisms.First!; organismNode != null; organismNode = organismNode.Next!)
        {
            Organism organism = organismNode.Value;
            
            //Move and run step for organism (organism does collision check with knowledge of exclusively what this chunk knows (which is enough)
            organism.Step();
        }
        
        //Update what should and should not be in this chunk
        //No additions happen during this (to this chunk)
        Queue<LinkedListNode<Organism>> toRemove = new Queue<LinkedListNode<Organism>>();
        for (LinkedListNode<Organism> organismNode = Organisms.First!; organismNode != null; organismNode = organismNode.Next!)
        {
            //Get organism at this index
            Organism organism = organismNode.Value;
            
            CheckPosition(organism, organismNode);
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
                Organisms.AddLast(organism);
                OrganismCount++;
            }
        }
    }
    
    /// <summary>
    /// Checks if an organism should be within this chunk based off of it's recently updated position, also checks if it should be added to a neighbouring chunk
    /// O(#connectedChunks) = O(26)
    /// </summary>
    /// <param name="organism"></param>
    /// <param name="organismNode"></param>
    private void CheckPosition(Organism organism, LinkedListNode<Organism> organismNode)
    {
        //Set the largest of the distances per axis, that is enough to check if it should be within or not
        float singleAxisDistance = SingleAxisDistance(organism);
        
        if (singleAxisDistance > HalfDimension)
        {
            //Send to neighbouring chunk for checking
            foreach (Chunk2D chunk in ConnectedChunks)
            {
                chunk.CheckToBeAdded.Enqueue(organism);
            }
            
            //Removing via node if faster
            Organisms.Remove(organismNode);
            OrganismCount--;
        }
    }

    [Pure]
    private float SingleAxisDistance(Organism organism)
    {
        return Math.Max(Math.Abs(organism.Position.X - Center.X), Math.Abs(organism.Position.Y - Center.Y));
    }
    
    /// <summary>
    /// Only call this via AddOrganism in DataStructure, should not be used to communicate between chunks
    /// </summary>
    /// <param name="organism"></param>
    public void DirectlyInsertOrganism(Organism organism)
    {
        Organisms.AddLast(organism);
        OrganismCount++;
    }
}