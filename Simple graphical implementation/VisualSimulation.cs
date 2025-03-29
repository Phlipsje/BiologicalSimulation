using System;
using System.Collections.Generic;
using System.Diagnostics;
using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;
using System.Numerics;
using BiologicalSimulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Simple_graphical_implementation;

/// <summary>
/// This project contains a visual implementation of the biological simulation library.
/// Used to test visually test that everything works as intended.
/// </summary>
public class VisualSimulation : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private const int screenWidth = 1280;
    private const int screenHeight = 720;
    private RenderManager renderManager;
    private bool updateDrawnImage = true;

    public static Color BackgroundColor = Color.CornflowerBlue;

    private World world;
    private Simulation simulation;
    private ViewingInformation viewingInformation;
    public new int Tick => simulation.Tick;

    //Easiest way to implement global counter, not most safe way of doing it
    public static int OrganismACount = 0;
    public static int OrganismBCount = 0;
    
    //For tracking fps performance
    public static float AverageFps { get; private set; }
    private float tallyFps;
    private int fpsCounter;
    private const int ticksPerUpdate = 15;
    private float secondsCount = 0f;
    
    public VisualSimulation()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        graphics.PreferredBackBufferWidth = screenWidth;
        graphics.PreferredBackBufferHeight = screenHeight;

        IsFixedTimeStep = false;
        AverageFps = 60;
    }

    protected override void Initialize()
    {
        base.Initialize();

        viewingInformation = new ViewingInformation();
        viewingInformation.Position = Vector3.Zero;
        viewingInformation.Scale = 50;
        viewingInformation.Width = screenWidth;
        viewingInformation.Height = screenHeight;
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        int sizeX = screenWidth / 2;
        int sizeY = screenHeight / 2;
        renderManager = new RenderManager(this, GraphicsDevice, new List<Renderer>()
        {
            new Renderer(new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight), 
                ViewDirection.XYPlane, new Rectangle(0, 0, sizeX, sizeY)),
            new Renderer(new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight),
                ViewDirection.YZPlane, new Rectangle(sizeX, 0, sizeX, sizeY)),
            new Renderer(new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight),
                ViewDirection.XZPlane, new Rectangle(0, sizeY, sizeX, sizeY))
        });
        renderManager.DrawBorders = true;
        renderManager.Draw = true;
        renderManager.LoadContent(Content);
        
        simulation = new Simulation();
        Random random = new Random(); //Can enter seed here
        float worldHalfSize = 10f;
        world = new TestWorld(simulation, worldHalfSize);
        float organismSize = 0.5f;
        DataStructure dataStructure = new Chunk3DFixedDataStructure(world, new Vector3(-worldHalfSize, -worldHalfSize, -worldHalfSize), 
            new Vector3(worldHalfSize, worldHalfSize, worldHalfSize), new Vector3(1f, 1f, 1f), organismSize);
        TestOrganism exampleOrganism = new TestOrganism(Vector3.Zero, organismSize, world, dataStructure, random);
        OrganismManager.RegisterOrganism(exampleOrganism.Key, exampleOrganism.CreateNewOrganism);
        simulation.CreateSimulation(world, random);
        simulation.SetDataStructure(dataStructure);
        simulation.DrawingEnabled = true;
        simulation.SetDrawFrequency(1);
        
        //For saving to file
        simulation.FileWritingEnabled = false;
        simulation.SetFileWriteFrequency(100);
        SimulationExporter.FileName = "simulation";
        SimulationExporter.SaveDirectory = "Content\\nothing";
        SimulationExporter.ShowExportFilePath = true;
        SimulationExporter.ClearDirectory = true;

        simulation.OnDraw += OnDrawCall;
        simulation.OnEnd += StopProgram;
        
        OrganismACount = 0;
        OrganismBCount = 0;
        GrowthGrid.Initialize(new Vector3(-worldHalfSize, -worldHalfSize, -worldHalfSize), 
            new Vector3(worldHalfSize, worldHalfSize, worldHalfSize), new Vector3(0.5f, 0.5f, 0.5f));
        simulation.StartSimulation();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            simulation.AbortSimulation();
            Exit();
        }
        
        #region Performance tracking
        tallyFps += 1 / (float)gameTime.ElapsedGameTime.TotalSeconds; 
        fpsCounter++;
        if (fpsCounter >= ticksPerUpdate) 
        { 
            AverageFps = tallyFps / fpsCounter;
            if (AverageFps is Single.PositiveInfinity)
                AverageFps = 0;
                
            fpsCounter = 0;
            tallyFps = 0;
        }
        //Now can write average fps in render manager
        #endregion

        simulation.Step();
        GrowthGrid.Step();

        secondsCount += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (simulation.Tick == 4000)
        {
            Console.WriteLine("Simulation reached 4000 ticks in:");
            Console.WriteLine(secondsCount + " seconds"); //Elapsed seconds
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (!updateDrawnImage)
            return;
        
        GraphicsDevice.Clear(Color.CornflowerBlue);

        renderManager.Render(spriteBatch, world, viewingInformation);
        
        updateDrawnImage = false;
        
        base.Draw(gameTime);
    }

    private void OnDrawCall(World world)
    {
        updateDrawnImage = true;
    }

    private void StopProgram(World world)
    {
        //Simulation has already stopped before this
        Exit();
    }
}

public class ViewingInformation
{
    public System.Numerics.Vector3 Position { get; set; } //Where the center of the viewpoint is
    public float Width { get; set; }
    public float Height { get; set; }
    public float Scale { get; set; } //How many pixels are equal to a unit of distance in the simulation
}