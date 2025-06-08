using System;
using System.Collections.Generic;
using System.Diagnostics;
using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;
using System.Numerics;
using BiologicalSimulation;
using BioSim.Datastructures;
using BioSim.Datastructures.Datastructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Simple_graphical_implementation;

/// <summary>
/// This project contains a visual implementation of the biological simulation library.
/// Used to test visually test that everything works as intended.
/// </summary>
public class VisualSimulation : Game, IProgramMedium
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private const int screenWidth = 1280;
    private const int screenHeight = 720;
    private RenderManager renderManager;
    private bool updateDrawnImage = true;

    public static Color BackgroundColor = Color.CornflowerBlue;
    private ViewingInformation viewingInformation;

    
    
    //For tracking fps performance
    public static float AverageFps { get; private set; }
    private float tallyFps;
    private int fpsCounter;
    private const int ticksPerUpdate = 15;
    
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

    public Simulation Simulation { get; set; }
    public World World { get; set; }
    public DataStructure DataStructure { get; set; }

    public void StartProgram()
    {
        Run();
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
        renderManager.Draw = false;
        renderManager.LoadContent(Content);
        
        
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Simulation.AbortSimulation();
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

        Simulation.Step();
        GrowthGrid.Step();
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (!updateDrawnImage)
            return;
        
        GraphicsDevice.Clear(Color.CornflowerBlue);

        renderManager.Render(spriteBatch, World, viewingInformation);
        
        updateDrawnImage = false;
        
        base.Draw(gameTime);
    }
    
    private void StopProgram(World world)
    {
        //Simulation has already stopped before this
        Exit();
    }

    public void StopProgram()
    {
        Exit();
    }

    public void DrawCall()
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