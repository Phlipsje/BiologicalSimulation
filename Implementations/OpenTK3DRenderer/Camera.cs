using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Implementations.OpenTK3DRenderer;

public class Camera
{
    public Vector3 Position { get; set; }
    public float Pitch { get; private set; }
    public float Yaw { get; private set; }

    public Camera(Vector3 startPosition)
    {
        Position = startPosition;
        Pitch = 0.0f;
        Yaw = -90.0f; // Looking down -Z
    }

    public Vector3 Front => Vector3.Normalize(new Vector3(
        MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch)),
        MathF.Sin(MathHelper.DegreesToRadians(Pitch)),
        MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch))
    ));

    public Vector3 Right => Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
    public Vector3 Up => Vector3.Normalize(Vector3.Cross(Right, Front));

    public void UpdateKeyboard(KeyboardState input, float deltaTime)
    {
        float speed = 2.0f * deltaTime;

        if (input.IsKeyDown(Keys.W))
            Position += Front * speed;
        if (input.IsKeyDown(Keys.S))
            Position -= Front * speed;
        if (input.IsKeyDown(Keys.A))
            Position -= Right * speed;
        if (input.IsKeyDown(Keys.D))
            Position += Right * speed;
        if (input.IsKeyDown(Keys.Space))
            Position += Up * speed;
        if (input.IsKeyDown(Keys.LeftShift))
            Position -= Up * speed;
    }

    public void UpdateMouse(float deltaX, float deltaY)
    {
        float sensitivity = 0.2f;
        Yaw += deltaX * sensitivity;
        Pitch -= deltaY * sensitivity;
        Pitch = MathHelper.Clamp(Pitch, -89f, 89f);
    }
}