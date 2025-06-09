using BiologicalSimulation.Datastructures.RTree;

namespace BioSim.Datastructures;

public static class SplitUtils
{
    public static (T, T) LinearPickSeeds<T>(List<T> entries) where T : IMinimumBoundable
    {
        //Find extreme rectangles along all dimensions and record total width of entries
        T highLowX = entries[0];
        T lowHighX = entries[0];
        T highLowY = entries[0];
        T lowHighY = entries[0];
        T highLowZ = entries[0];
        T lowHighZ = entries[0];
        Mbb mbb = entries[0].GetMbb();
        float minX = mbb.Minimum.X;
        float maxX = mbb.Maximum.X;
        float minY = mbb.Minimum.Y;
        float maxY = mbb.Maximum.Y;
        float minZ = mbb.Minimum.Z;
        float maxZ = mbb.Maximum.Z;
        for (int i = 1; i < entries.Count; i++)
        {
            T entry = entries[i];
            Mbb entryMbb = entry.GetMbb();
            if (entryMbb.Minimum.X > highLowX.GetMbb().Minimum.X)
                highLowX = entry;
            if (entryMbb.Maximum.X < lowHighX.GetMbb().Maximum.X)
                lowHighX = entry;
            if (entryMbb.Minimum.Y > highLowY.GetMbb().Minimum.Y)
                highLowY = entry;
            if (entryMbb.Maximum.Y < lowHighY.GetMbb().Maximum.Y)
                lowHighY = entry;
            if (entryMbb.Minimum.Z > highLowZ.GetMbb().Minimum.Z)
                highLowZ = entry;
            if (entryMbb.Maximum.Z < lowHighZ.GetMbb().Maximum.Z)
                lowHighZ = entry;
            if (entryMbb.Minimum.X < minX)
                minX = entryMbb.Minimum.X;
            if (entryMbb.Minimum.Y < minY)
                minY = entryMbb.Minimum.Y;
            if (entryMbb.Minimum.Z < minZ)
                minZ = entryMbb.Minimum.Z;
            if (entryMbb.Maximum.X > maxX)
                maxX = entryMbb.Maximum.X;
            if (entryMbb.Maximum.Y > maxY)
                maxY = entryMbb.Maximum.Y;
            if (entryMbb.Maximum.Z > maxZ)
                maxZ = entryMbb.Maximum.Z;
        }
        //Adjust for shape of the rectangle cluster
        float widthX = maxX - minX;
        float widthY = maxY - minX;
        float widthZ = maxZ - minZ;
        float normSepX = (highLowX.GetMbb().Minimum.X - lowHighX.GetMbb().Maximum.X) / widthX;
        float normSepY = (highLowY.GetMbb().Minimum.Y - lowHighY.GetMbb().Maximum.Y) / widthY;
        float normSepZ = (highLowZ.GetMbb().Minimum.Z - lowHighZ.GetMbb().Maximum.Z) / widthZ;

        //select the pair with greatest seperation
        (T, T) pair = normSepX > normSepY && normSepX > normSepZ ? 
            (highLowX, lowHighX) : normSepY > normSepZ ? (highLowY, lowHighY) : (highLowZ, lowHighZ);

        if (pair.Item1.Equals(pair.Item2)) //edge case where we should fall back on quadratic pick seeds
            pair = QuadraticPickSeeds(entries);
        
        return pair;
    }
    public static (T, T) QuadraticPickSeeds<T>(List<T> entries) where T : IMinimumBoundable
    {
        (T, T) mostWasteful = (entries[0], entries[0]); //placeholder
        float largestD = float.MinValue;
        for (int i = 0; i < entries.Count; i++)
        {
            for (int j = i + 1; j < entries.Count; j++)
            {
                Mbb e1 = entries[i].GetMbb();
                Mbb e2 = entries[i].GetMbb();
                Mbb J = e1.Enlarged(e2);
                float d = J.Area - e1.Area - e2.Area;
                if(d > largestD)
                    mostWasteful = (entries[i], entries[j]);
            }
        }
        return mostWasteful;
    }

    public static void DistributeEntries<TGroup,TEntry>(List<TEntry> entries, TGroup group1, TGroup group2, 
        Action<TGroup, TEntry> addToGroup, Func<TGroup, int> groupCount, int minSize) where TGroup : IMinimumBoundable where TEntry : IMinimumBoundable
    {
        for (int i = 0; i < entries.Count; i++)
        {
            TEntry currentEntry = entries[i];

            //if it is required to put all remaining entries into a group to ensure that group is filled to size m do so
            TGroup? groupToFill = default(TGroup);
            if (groupCount(group1) + (entries.Count - i) <= minSize)
                groupToFill = group1;
            if (groupCount(group2) + (entries.Count - i) <= minSize)
                groupToFill = group2;
            if (groupToFill != null)
            {
                for (int j = i; j < entries.Count; j++)
                {
                    currentEntry = entries[j];
                    addToGroup(groupToFill, currentEntry);
                }

                break;
            }
            Mbb group1Enlarged = group1.GetMbb().Enlarged(currentEntry.GetMbb());
            Mbb group2Enlarged = group2.GetMbb().Enlarged(currentEntry.GetMbb());
            if (group1Enlarged.Area < group2Enlarged.Area)
            {
                addToGroup(group1, currentEntry);
                continue;
            }
            if (group1Enlarged.Area == group2Enlarged.Area)
            {
                if (groupCount(group1) < groupCount(group2))
                {
                    addToGroup(group1, currentEntry);
                }
                else
                {
                    addToGroup(group2, currentEntry);
                }
                continue;
            }
            addToGroup(group2, currentEntry);
        }
    }
}