using System.Diagnostics.Contracts;
using System.Numerics;

namespace BioSim.Datastructures.NewDSAttempt;

public class Chunk3D
{
    public Vector3 Center { get; }
    public float HalfDimension { get; } //Size from center (so half of full length)
    private float dimenstionExtensionForCheck = 0.5f; //TODO set dynamically later
    public LinkedList<Organism> Organisms { get; }
    private LinkedList<Organism> extendedCheck;
    public Queue<Organism> CheckToBeAdded; //This is a queue, because emptied every frame
    private Chunk3D[] connectedChunks; //Connected chunks is at most a list of 26 (9+8+9 for each chunk touching this chunk (also diagonals))

    public Chunk3D(Vector3 center, float halfDimension, float largestOrganismSize)
    {
        Center = center;
        HalfDimension = halfDimension;
        Organisms = new LinkedList<Organism>();
        extendedCheck = new LinkedList<Organism>();
        CheckToBeAdded = new Queue<Organism>();
        dimenstionExtensionForCheck = largestOrganismSize;
    }

    public void Initialize(Chunk3D[] connectedChunks)
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
        LinkedListNode<Organism> organismNode = Organisms.First!;
        for (int i = 0; i < Organisms.Count; i++)
        {
            Organism organism = organismNode.Value;
            organismNode = organismNode.Next!;
            
            //Move and run step for organism (organism does collision check with knowledge of exclusively what this chunk knows (which is enough)
            organism.Step(Organisms, extendedCheck);
        }
        
        //Update what should and should not be in this chunk
        //No additions happen during this (to this chunk)
        organismNode = Organisms.First!;
        for (int i = 0; i < Organisms.Count; i++)
        {
            //Get organism at this index
            Organism organism = organismNode.Value;
            
            CheckPosition(organism, organismNode);
            
            //Goto next
            organismNode = organismNode.Next!;
        }
        organismNode = extendedCheck.First!;
        for (int i = 0; i < extendedCheck.Count; i++)
        {
            //Get organism at this index
            Organism organism = organismNode.Value;
            
            CheckRemoveFromExtension(organism, organismNode);
            
            //Goto next
            organismNode = organismNode.Next!;
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

            if (singleAxisDistance <= HalfDimension && !Organisms.Contains(organism))
            {
                Organisms.AddLast(organism);
                continue;
            }
            
            if (singleAxisDistance <= HalfDimension + dimenstionExtensionForCheck && !extendedCheck.Contains(organism))
            {
                extendedCheck.AddLast(organism);
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
        float singleAxisDistance = Math.Max(Math.Max(Math.Abs(organism.Position.X - Center.X), Math.Abs(organism.Position.Y - Center.Y)), Math.Abs(organism.Position.Z - Center.Z));
        
        if (singleAxisDistance > HalfDimension)
        {
            //Send to neighbouring chunk for checking
            foreach (Chunk3D chunk in connectedChunks)
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
                foreach (Chunk3D chunk in connectedChunks)
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
        float singleAxisDistance = Math.Max(Math.Max(Math.Abs(organism.Position.X - Center.X), Math.Abs(organism.Position.Y - Center.Y)), Math.Abs(organism.Position.Z - Center.Z));
        
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
        return Math.Max(Math.Max(Math.Abs(organism.Position.X - Center.X), Math.Abs(organism.Position.Y - Center.Y)), Math.Abs(organism.Position.Z - Center.Z));
    }
}