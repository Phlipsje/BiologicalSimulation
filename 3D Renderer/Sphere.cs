using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace _3D_Renderer;

[StructLayout(LayoutKind.Sequential)]
public struct Sphere
{
    public Vector3 Center;
    public float Radius;

    public Sphere(Vector3 center, float radius)
    {
        Center = center;
        Radius = radius;
    }
}
