using System.Numerics;

namespace Continuum.Datastructures.SingleThreaded.RTree;

public struct Mbb(Vector3 minimum, Vector3 maximum)
{
    private const float Epsilon = 0.1f; //extra offset to contains to account for floating point errors
    public Vector3 Minimum = minimum;
    public Vector3 Maximum = maximum;
    public Vector3 Position => Maximum - (Maximum - Minimum) / 2;
    public float Area => (Maximum.X - Minimum.X) * (Maximum.Y - Minimum.Y) * (Maximum.Z - Minimum.Z);

    public bool Intersects(Mbb other)
    {
        return Minimum.X < other.Maximum.X && other.Minimum.X < Maximum.X &&
               Minimum.Y < other.Maximum.Y && other.Minimum.Y < Maximum.Y &&
               Minimum.Z < other.Maximum.Z && other.Minimum.Z < Maximum.Z;
    }

    public bool Contains(Mbb other)
    {
        return Minimum.X <= other.Minimum.X + Epsilon && Minimum.Y <= other.Minimum.Y + Epsilon && Minimum.Z <= other.Minimum.Z + Epsilon &&
               Maximum.X >= other.Maximum.X - Epsilon && Maximum.Y >= other.Maximum.Y - Epsilon && Maximum.Z >= other.Maximum.Z - Epsilon;
    }
    
    //calculate as described in Nearest Neighbor Queries by Nick Roussopoulos Stephen Kelley and Frederic Vincent
    public float MinDist(Vector3 point)
    {
        float minDist = 0;
        for (int d = 0; d < 3; d++)//iterate over the 3 dimensions
        {
            float p = point[d];
            float r = p < Minimum[d] ? Minimum[d] : p > Maximum[d] ? Maximum[d] : p;
            float intermediate = p - r;
            minDist += intermediate * intermediate;
        }
        return minDist;
    }
    
    //the enlargement to this minimum bounding box needed to fit the other mbb inside it. Also returns the new Mbb
    public float Enlargement(Mbb other)
    {
        float enlargement = Enlarged(other).Area - Area;
        return enlargement;
    }

    public Mbb Enlarged(Mbb other)
    {
        Vector3 newMinimum = new Vector3(MathF.Min(Minimum.X, other.Minimum.X), MathF.Min(Minimum.Y, other.Minimum.Y),
            MathF.Min(Minimum.Z, other.Minimum.Z));
        Vector3 newMaximum = new Vector3(MathF.Max(Maximum.X, other.Maximum.X), MathF.Max(Maximum.Y, other.Maximum.Y),
            MathF.Max(Maximum.Z, other.Maximum.Z));
        return new Mbb(newMinimum, newMaximum);
    }

    public static Mbb ComputeBoundingBox<T>(IEnumerable<T> entries) where T : IMinimumBoundable
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float maxZ = float.MinValue;
        foreach (T entry in entries)
        {
            Vector3 min = entry.GetMbb().Minimum;
            Vector3 max = entry.GetMbb().Maximum;
            if (min.X < minX)
                minX = min.X;
            if (min.Y < minY)
                minY = min.Y;
            if (min.Z < minZ)
                minZ = min.Z;
            if (max.X > maxX)
                maxX = max.X;
            if (max.Y > maxY)
                maxY = max.Y;
            if (max.Z > maxZ)
                maxZ = max.Z;
        }
        return new Mbb(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
    }
}

public interface IMinimumBoundable
{
    public Mbb GetMbb();
    public void SetMbb(Mbb newMbb);
}