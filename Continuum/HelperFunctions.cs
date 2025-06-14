namespace Continuum;

internal static class HelperFunctions
{
    public static void KnuthShuffle<T>(IList<T> list) //O(n) algorithm for randomising the order of a list.
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Randomiser.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}