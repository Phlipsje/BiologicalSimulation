using System;
using System.Collections.Generic;
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
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private const int screenWidth = 1280;
    private const int screenHeight = 720;

    private RenderManager renderManager;
    private static Random random = new Random();

    public static Color BackgroundColor
    {
        get {
        return new Color(200, 200, random.Next(50, 150));
    }
    }

    public VisualSimulation()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = screenWidth;
        _graphics.PreferredBackBufferHeight = screenHeight;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

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
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        renderManager.Render(_spriteBatch);
        
        base.Draw(gameTime);
    }
}