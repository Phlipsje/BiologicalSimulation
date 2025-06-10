using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Implementations.OpenTK3DRenderer;

public class OpenTK3DRenderer : GameWindow, IProgramMedium
{
    public Simulation Simulation { get; set; }
    public World World { get; set; }
    public DataStructure DataStructure { get; set; }
    
    public Sphere[] Spheres;
    private int shader, _quadVAO, _sphereSSBO;
    
    Camera _camera;
    Vector2 _lastMousePos;
    bool _firstMouse = true;
    private bool fullWindow = false;

    public OpenTK3DRenderer() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        //Set window settings
        Location = new Vector2i(400, 300);
        Size = new Vector2i(800, 600);
        GL.Viewport(0, 0, 800, 600);
        Title = "Biological Simulation 3D Renderer";
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        
        // Compile shaders
        shader = GL.CreateProgram();
        int vs = CompileShader(ShaderType.VertexShader, GetSourceRelativePath("Shaders/shader.vert"));
        int fs = CompileShader(ShaderType.FragmentShader, GetSourceRelativePath("Shaders/shader.frag"));
        GL.AttachShader(shader, vs);
        GL.AttachShader(shader, fs);
        GL.LinkProgram(shader);

        //Uploads the list of spheres to the GPU using a SSBO
        _sphereSSBO = GL.GenBuffer();
        UpdateSphereBuffer();
        
        // Vertex data for a full screen quad (used so that we have a screen)
        float[] quadVertices = {
            -1f, -1f,
            1f, -1f,
            1f,  1f,
            -1f,  1f
        };
        uint[] indices = { 0, 1, 2, 2, 3, 0 };

        int vbo = GL.GenBuffer();
        _quadVAO = GL.GenVertexArray();
        int ebo = GL.GenBuffer();

        GL.BindVertexArray(_quadVAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        
        _camera = new Camera(new Vector3(0, 10, 10));
        //This means that your cursor is used by the program (so you can't move it around anymore)
        CursorState = CursorState.Grabbed;
    }

    public static string GetSourceRelativePath(string relativePath, [System.Runtime.CompilerServices.CallerFilePath] string callerFile = "")
    {
        return Path.Combine(Path.GetDirectoryName(callerFile), relativePath);
    }
    
    protected override async void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        
        HandleInput(args);
        
        await Simulation.Step();
        
        UpdateSphereBuffer();
    }

    private void HandleInput(FrameEventArgs args)
    {
        //Exit program on escape press
        if (IsKeyDown(Keys.Escape))
        {
            Simulation.AbortSimulation();
            Close();
        }
        
        //Allow switching between fullscreen and windowed
        if (IsKeyPressed(Keys.F))
        {
            if (!fullWindow)
            {
                Location = new Vector2i(0, 0);
                Size = new Vector2i(1920, 1080); 
                GL.Viewport(0, 0, 1920, 1080);
            }
            else
            {
                Location = new Vector2i(400, 300);
                Size = new Vector2i(800, 600); 
                GL.Viewport(0, 0, 800, 600);
            }
            
            fullWindow = !fullWindow;
        }
        
        _camera.UpdateKeyboard(KeyboardState, (float)args.Time);

        var mouse = MouseState.Position;

        if (_firstMouse)
        {
            _lastMousePos = mouse;
            _firstMouse = false;
        }

        Vector2 delta = mouse - _lastMousePos;
        _lastMousePos = mouse;

        _camera.UpdateMouse(delta.X, delta.Y);
    }

    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.UseProgram(shader);
        
        GL.Uniform1(GL.GetUniformLocation(shader, "sphereCount"), Spheres.Length);
        
        GL.Uniform3(GL.GetUniformLocation(shader, "cameraPos"), _camera.Position);
        GL.Uniform3(GL.GetUniformLocation(shader, "cameraFront"), _camera.Front);
        GL.Uniform3(GL.GetUniformLocation(shader, "cameraUp"), _camera.Up);
        GL.Uniform3(GL.GetUniformLocation(shader, "cameraRight"), _camera.Right);
        GL.Uniform1(GL.GetUniformLocation(shader, "aspect"), Size.X / (float)Size.Y);
        
        GL.BindVertexArray(_quadVAO);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        GL.DeleteProgram(shader);
        GL.DeleteVertexArray(_quadVAO);
    }
    
    string LoadShader(string path)
    {
        return File.ReadAllText(path);
    }

    int CompileShader(ShaderType type, string path)
    {
        string src = LoadShader(path);
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, src);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status != (int)All.True)
        {
            string log = GL.GetShaderInfoLog(shader);
            throw new Exception($"Shader compile error ({path}): {log}");
        }
        return shader;
    }
    
    void UpdateSphereBuffer()
    {
        Spheres = World.GetOrganisms().Select(o =>
        {
            Vector3 pos = new Vector3(o.Position.X, o.Position.Y, o.Position.Z);
            Vector3 color = new Vector3(o.Color.X, o.Color.Y, o.Color.Z);
            Sphere sphere = new Sphere(pos, o.Size, color);
            return sphere;
        }).ToArray();

        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _sphereSSBO);

        int bufferSize = Spheres.Length * Marshal.SizeOf<Sphere>();

        // Resize buffer if needed
        GL.BufferData(BufferTarget.ShaderStorageBuffer, bufferSize, Spheres, BufferUsageHint.DynamicDraw);

        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, _sphereSSBO);
    }
    
    public void StartProgram()
    {
        Run();
    }

    public void StopProgram()
    {
        Close();
    }

    public void FileWriten(string filePath, string fileContents)
    {
        //Nothing to do
    }
}