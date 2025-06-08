using System.Net.Mime;
using System.Runtime.InteropServices;
using _3D_Renderer;
using BioSim;
using BioSim.Simulation;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

class RayTracer : GameWindow
{
    public Sphere[] Spheres;
    private int shader, _quadVAO, _sphereSSBO;
    private int fileTimestampIndex = 0;
    private string readFilePath = "../../../Past simulations/testing.txt";
    
    Camera _camera;
    Vector2 _lastMousePos;
    bool _firstMouse = true;
    private bool fullWindow = false;

    public RayTracer()
        : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
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
        int vs = CompileShader(ShaderType.VertexShader, "../../../Shaders/shader.vert");
        int fs = CompileShader(ShaderType.FragmentShader, "../../../Shaders/shader.frag");
        GL.AttachShader(shader, vs);
        GL.AttachShader(shader, fs);
        GL.LinkProgram(shader);

        //Uploads the list of spheres to the GPU using a SSBO
        _sphereSSBO = GL.GenBuffer();
        UpdateSphereBuffer();
        
        // Vertex data for a full screen quad
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
        
        _camera = new Camera(new Vector3(0, 0, -3));
        CursorState = CursorState.Grabbed;
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        
        //Exit program on escape press
        if (IsKeyDown(Keys.Escape))
            Close();
        
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

        if (IsKeyPressed(Keys.Left))
        {
            fileTimestampIndex--;
            fileTimestampIndex = Math.Max(fileTimestampIndex, 0);
            UpdateSphereBuffer();
        }
        if (IsKeyPressed(Keys.Right))
        {
            fileTimestampIndex++;
            fileTimestampIndex = Math.Min(fileTimestampIndex, File.ReadLines(readFilePath).Count()-1);
            UpdateSphereBuffer();
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
        
        // for (int i = 0; i < Organisms.Length; i++)
        // {
        //     var sphere = Organisms[i];
        //     GL.Uniform3(GL.GetUniformLocation(shader, $"spheres[{i}].center"), sphere.Center);
        //     GL.Uniform1(GL.GetUniformLocation(shader, $"spheres[{i}].radius"), sphere.Radius);
        // }
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
        //Testing.txt uses the format specified in Implementations.sln, if another system is used, then this must change as well to render it
        Spheres = SimulationImporter.FromFileToObjectType<Sphere>(readFilePath, (string key, string contents) =>
        {
            Sphere sphere = new Sphere();
            if (key == "A")
            {
                sphere.Color = new Vector3(0.4f, 0.8f, 0.4f); //Green
            }
            else if(key == "B")
            {
                sphere.Color = new Vector3(0.4f, 0.8f, 0.8f); //Yellow
            }
            sphere.Radius = 0.5f; //Does not change in our example
            string[] values = contents.Split(' ');
            sphere.Center = new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
            return sphere;
        }, fileTimestampIndex);

        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _sphereSSBO);

        int bufferSize = Spheres.Length * Marshal.SizeOf<Sphere>();

        // Resize buffer if needed
        GL.BufferData(BufferTarget.ShaderStorageBuffer, bufferSize, Spheres, BufferUsageHint.DynamicDraw);

        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, _sphereSSBO);
    }
}