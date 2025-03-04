using System;
using System.Collections.Generic;
using BioSim;
using BioSim.Datastructures;
using BioSim.Simulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

    private Simulation simulation;
    
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
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        int sizeX = screenWidth / 2;
        int sizeY = screenHeight / 2;
        renderManager = new RenderManager(GraphicsDevice, new List<Renderer>()
        {
            new Renderer(new RenderTarget2D(GraphicsDevice, screenWidth / 2, screenHeight / 2), 
                ViewDirection.XYPlane, new Rectangle(0, 0, sizeX, sizeY)),
            new Renderer(new RenderTarget2D(GraphicsDevice, screenWidth / 2, screenHeight / 2),
                ViewDirection.YZPlane, new Rectangle(sizeX, 0, sizeX, sizeY)),
            new Renderer(new RenderTarget2D(GraphicsDevice, screenWidth / 2, screenHeight / 2),
                ViewDirection.XZPlane, new Rectangle(0, sizeY, sizeX, sizeY))
        });
        renderManager.DrawBorders = true;
        renderManager.LoadContent(Content);
        
        simulation = new Simulation();
        World world = new TestWorld();
        DataStructure dataStructure = new NoDataStructure(world);
        simulation.CreateSimulation(world);
        simulation.DrawingEnabled = true;
        simulation.FileWritingEnabled = false;
        simulation.SetDrawFrequency(1);

        simulation.OnDraw += OnDrawCall;
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

        renderManager.Render(spriteBatch);

        updateDrawnImage = false;
        
        base.Draw(gameTime);
    }

    private void OnDrawCall(World world)
    {
        updateDrawnImage = true;
    }
}