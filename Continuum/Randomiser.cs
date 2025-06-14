namespace Continuum;

public static class Randomiser
{
    private static Random? _random;
    public static bool IsUsingSeed { get; private set; } = false;

    private static Random Random
    {
        get { return _random ??= new Random(); }
    }

    public static void SetSeed(int seed)
    {
        IsUsingSeed = true;
        _random = new Random(seed);
    }

    public static int Next()
    {
        return Random.Next();
    }

    public static int Next(int minValue, int maxValue)
    {
        return Random.Next(minValue, maxValue);
    }

    public static float NextSingle()
    {
        return Random.NextSingle();
    }

    public static double NextDouble()
    {
        return Random.NextDouble();
    }
}