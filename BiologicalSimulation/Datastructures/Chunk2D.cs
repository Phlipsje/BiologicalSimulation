using System.Diagnostics.Contracts;
using System.Numerics;

namespace BioSim.Datastructures;

public class Chunk2D
{
    public Vector2 Center { get; }
    public float HalfDimension { get; } //Size from center (so half of full length)
    private float dimenstionExtensionForCheck;
    public LinkedList<Organism> Organisms { get; }
    private LinkedList<Organism> extendedCheck;
    public Queue<Organism> CheckToBeAdded; //This is a queue, because emptied every frame
    private HashSet<ulong> containingOrganisms; //Used for Contains checks
    private HashSet<ulong> extendedContainingOrganisms; //Used for Contains checks in extended linked list
    private Chunk2D[] connectedChunks; //Connected chunks is at most a list of 26 (9+8+9 for each chunk touching this chunk (also diagonals))
    private List<LinkedList<Organism>> listsToCheck;

    public Chunk2D(Vector2 center, float halfDimension, float largestOrganismSize)
    {
        Center = center;
        HalfDimension = halfDimension;
        Organisms = new LinkedList<Organism>();
        extendedCheck = new LinkedList<Organism>();
        CheckToBeAdded = new Queue<Organism>();
        containingOrganisms = new HashSet<ulong>();
        extendedContainingOrganisms = new HashSet<ulong>();
        dimenstionExtensionForCheck = largestOrganismSize;
        listsToCheck = new List<LinkedList<Organism>>(){Organisms, extendedCheck};
    }

    public void Initialize(Chunk2D[] connectedChunks)
    {
        //Connected chunks is at most a list of 26 (9+8+9 for each chunk touching this chunk (also diagonals))
        this.connectedChunks = connectedChunks;
    }
    
    public void Step()
    {
        //Check what should be added to chunk
        //No removals happen during this
        CheckNewPossibleAdditions();
        
        //Run update loop
        for (LinkedListNode<Organism> organismNode = Organisms.First; organismNode != null; organismNode = organismNode.Next)
        {
            Organism organism = organismNode.Value;
            
            //Move and run step for organism (organism does collision check with knowledge of exclusively what this chunk knows (which is enough)
            organism.Step(listsToCheck);
        }
        
        //Update what should and should not be in this chunk
        //No additions happen during this (to this chunk)
        for (LinkedListNode<Organism> organismNode = Organisms.First; organismNode != null; organismNode = organismNode.Next)
        {
            //Get organism at this index
            Organism organism = organismNode.Value;
            
            CheckPosition(organism, organismNode);
        }
        for (LinkedListNode<Organism> organismNode = Organisms.First; organismNode != null; organismNode = organismNode.Next)
        {
            //Get organism at this index
            Organism organism = organismNode.Value;
            
            CheckRemoveFromExtension(organism, organismNode);
        }
    }
    
    /// <summary>
    /// Check every entry in checkToBeAdded queue for possible addition
    /// O(#checkToBeAdded * #Organisms)
    /// </summary>
    private void CheckNewPossibleAdditions()
    {
        while (CheckToBeAdded.Count > 0)
        {
            Organism organism = CheckToBeAdded.Dequeue();
            
            float singleAxisDistance = SingleAxisDistance(organism);

            if (singleAxisDistance <= HalfDimension && !containingOrganisms.Contains(organism.Id))
            {
                Organisms.AddLast(organism);
                containingOrganisms.Add(organism.Id);
                continue;
            }
            
            if (singleAxisDistance <= HalfDimension + dimenstionExtensionForCheck && !extendedContainingOrganisms.Contains(organism.Id))
            {
                extendedCheck.AddLast(organism);
                extendedContainingOrganisms.Add(organism.Id);
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
            foreach (Chunk2D chunk in connectedChunks)
            {
                chunk.CheckToBeAdded.Enqueue(organism);
            }
            //Removing via node if faster
            Organisms.Remove(organismNode);
        }
        else //If a bit deeper within chunk, then only send for check, not for removal (so that neighbouring chunks can add to extended range)
        {
            //Send to neighbouring chunks for checking
            if (singleAxisDistance > HalfDimension - dimenstionExtensionForCheck)
            {
                foreach (Chunk2D chunk in connectedChunks)
                {
                    chunk.CheckToBeAdded.Enqueue(organism);
                }
            }
        }
    }

    /// <summary>
    /// Checks if an organism should be removed from extended list in this chunk based off of it's recently updated position
    /// O(1)
    /// </summary>
    /// <param name="organism"></param>
    /// <param name="organismNode"></param>
    private void CheckRemoveFromExtension(Organism organism, LinkedListNode<Organism> organismNode)
    {
        //Set the largest of the distances per axis, that is enough to check if it should be within or not
        float singleAxisDistance = Math.Max(Math.Abs(organism.Position.X - Center.X), Math.Abs(organism.Position.Y - Center.Y));
        
        //Remove if too far gone, don't try to add to neighbours because they already have it
        if (singleAxisDistance > HalfDimension + dimenstionExtensionForCheck)
        {
            //Removing via node if faster
            extendedCheck.Remove(organismNode);
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
        containingOrganisms.Add(organism.Id);
    }
}