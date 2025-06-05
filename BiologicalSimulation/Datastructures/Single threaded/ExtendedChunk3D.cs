using System.Diagnostics.Contracts;
using System.Numerics;
using BiologicalSimulation.Datastructures;

namespace BioSim.Datastructures;

/// <summary>
/// Used by Chunk3DFixedDataStructure.cs
/// A chunk is a cube that stores all organisms within it.
/// The chunk has 2 sizes, the smaller of the 2 sizes is its own size,
/// the larger size is an extension overlapping with neighbouring chunks that is of minimal size to include all organisms in other chunks that are relevant for collision within this chunk.
/// Organisms are checked for inclusion/removal every frame, but are only actually removed/inserted if it falls outside the boundaries.
/// </summary>
public class ExtendedChunk3D
{
    public Vector3 Center { get; }
    public float HalfDimension { get; } //Size from center (so half of full length)
    private float dimenstionExtensionForCheck;
    public int OrganismCount { get; private set; }
    public LinkedList<Organism> Organisms { get; }
    public LinkedList<Organism> ExtendedCheck;
    public Queue<Organism> CheckToBeAdded; //This is a queue, because emptied every frame
    private ExtendedChunk3D[] connectedChunks; //Connected chunks is at most a list of 26 (9+8+9 for each chunk touching this chunk (also diagonals))
    
    public ExtendedChunk3D(Vector3 center, float size, float largestOrganismSize)
    {
        Center = center;
        HalfDimension = size/2f;
        Organisms = new LinkedList<Organism>();
        ExtendedCheck = new LinkedList<Organism>();
        CheckToBeAdded = new Queue<Organism>();
        dimenstionExtensionForCheck = largestOrganismSize * 2;
    }

    public void Initialize(ExtendedChunk3D[] connectedChunks)
    {
        //Connected chunks is at most a list of 26 (9+8+9 for each chunk touching this chunk (also diagonals))
        this.connectedChunks = connectedChunks;
    }
    
    /// <summary>
    /// Does logic for this frame
    /// </summary>
    public void Step()
    {
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
        for (LinkedListNode<Organism> organismNode = Organisms.First!; organismNode != null; organismNode = organismNode.Next!)
        {
            //Get organism at this index
            Organism organism = organismNode.Value;
            
            CheckPosition(organism, organismNode);
        }
        for (LinkedListNode<Organism> organismNode = Organisms.First!; organismNode != null; organismNode = organismNode.Next!)
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

            if (singleAxisDistance <= HalfDimension && !Organisms.Contains(organism))
            {
                Organisms.AddLast(organism);
                OrganismCount++;
                continue;
            }
            
            if (singleAxisDistance <= HalfDimension + dimenstionExtensionForCheck && !ExtendedCheck.Contains(organism))
            {
                ExtendedCheck.AddLast(organism);
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
            foreach (ExtendedChunk3D chunk in connectedChunks)
            {
                chunk.CheckToBeAdded.Enqueue(organism);
            }
            
            if (organismNode.Previous == null && organismNode.Next == null)
                return;
            
            //Removing via node if faster
            Organisms.Remove(organismNode);
            OrganismCount--;
        }
        else //If a bit deeper within chunk, then only send for check, not for removal (so that neighbouring chunks can add to extended range)
        {
            //Send to neighbouring chunks for checking
            if (singleAxisDistance > HalfDimension - dimenstionExtensionForCheck)
            {
                foreach (ExtendedChunk3D chunk in connectedChunks)
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
            ExtendedCheck.Remove(organismNode);
        }
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
        Organisms.AddLast(organism);
        OrganismCount++;
    }
}