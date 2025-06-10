namespace Implementations.OpenTK3DRenderer;

using System.Runtime.InteropServices;
using OpenTK.Mathematics;

[StructLayout(LayoutKind.Sequential)]
public struct Sphere
{
    public Vector3 Center;
    public float Radius;
    public Vector3 Color;
    public float Padding; //Padding exists to have amount of bytes sent be power of 2, which is a lot more efficient when sending to the GPU

    public Sphere(Vector3 center, float radius, Vector3 color)
    {
        Center = center;
        Radius = radius;
        Color = color;
        Padding = 0;
    }
}
