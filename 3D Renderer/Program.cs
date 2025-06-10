namespace _3D_Renderer;

public static class Program
{
    public static void Main(string[] args)
    {
        using var window = new RayTracer();
        window.Run();
    }
}