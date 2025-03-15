using System;
using System.Collections.Generic;
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

    //Easiest way to implement global counter, not most safe way of doing it
    public static int OrganismACount = 0;
    public static int OrganismBCount = 0;
    
    public VisualSimulation()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        graphics.PreferredBackBufferWidth = screenWidth;
        graphics.PreferredBackBufferHeight = screenHeight;
    }

    protected override void Initialize()
    {
        base.Initialize();

        viewingInformation = new ViewingInformation();
        viewingInformation.Position = Vector3.Zero;
        viewingInformation.Scale = 100;
        viewingInformation.Width = screenWidth;
        viewingInformation.Height = screenHeight;
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        int sizeX = screenWidth / 2;
        int sizeY = screenHeight / 2;
        renderManager = new RenderManager(GraphicsDevice, new List<Renderer>()
        {
            new Renderer(new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight), 
                ViewDirection.XYPlane, new Rectangle(0, 0, sizeX, sizeY)),
            new Renderer(new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight),
                ViewDirection.YZPlane, new Rectangle(sizeX, 0, sizeX, sizeY)),
            new Renderer(new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight),
                ViewDirection.XZPlane, new Rectangle(0, sizeY, sizeX, sizeY))
        });
        renderManager.DrawBorders = true;
        renderManager.LoadContent(Content);
        
        simulation = new Simulation();
        world = new TestWorld();
        DataStructure dataStructure = new NoDataStructure(world);
        TestOrganism exampleOrganism = new TestOrganism(Vector3.Zero, 0.5f, world, dataStructure);
        OrganismManager.RegisterOrganism(exampleOrganism.Key, exampleOrganism.CreateNewOrganism);
        simulation.CreateSimulation(world);
        simulation.DrawingEnabled = true;
        simulation.FileWritingEnabled = false;
        simulation.SetDrawFrequency(1);

        simulation.OnDraw += OnDrawCall;

        OrganismACount = 0;
        OrganismBCount = 0;
        simulation.StartSimulation();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        simulation.Step();
        
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
}

public class ViewingInformation
{
    public System.Numerics.Vector3 Position { get; set; } //Where the center of the viewpoint is
    public float Width { get; set; }
    public float Height { get; set; }
    public float Scale { get; set; } //How many pixels are equal to a unit of distance in the simulation
}