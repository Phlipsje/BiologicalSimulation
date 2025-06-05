using System;
using System.Collections.Generic;
using BioSim.Datastructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector3 = System.Numerics.Vector3;

namespace RTreeTester;

public class RTreeTester : Game
{
    private const float Scale = 4.8f;
    private const int TestSize = 10;
    private Random random = new Random();
    private KeyboardState lastKeyState;
    private int currentN = 0;
    private Texture2D pixel;
    private class TestObject(int n, Mbb mbb) : IMinimumBoundable
    {
        public Mbb Mbb = mbb;
        public int N = n;
        public Mbb GetMbb()
        {
            return mbb;
        }
    }
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RTree<TestObject> rTree;
    
    public RTreeTester()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        rTree = new RTree<TestObject>(2, 10);
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData([Color.White]);
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        KeyboardState keyState = Keyboard.GetState();
        if (keyState.IsKeyUp(Keys.A) && lastKeyState.IsKeyDown(Keys.A) || keyState.IsKeyDown(Keys.Space))
        {
            Vector3 pos = new Vector3(random.NextSingle() * 100, random.NextSingle() * 100,
                random.NextSingle() * 100 - 50);
            Mbb mbb = new Mbb(pos, pos + new Vector3(TestSize));
            rTree.Insert(new TestObject(currentN++, mbb));
        }
            
        lastKeyState = keyState;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();
        List<(Mbb,int)> mbbs = rTree.GetMbbsWithLevel();
        int maxLevel = 0;
        foreach (var (_, level) in mbbs)
        {
            maxLevel = Math.Max(maxLevel, level);
        }
        foreach (var (mbb, level) in mbbs)
        {
            float colorStep = maxLevel != 0 ? 1f / maxLevel : 0;
            Color color = new Color(1 - level * colorStep, 1, 1);
            if (level == maxLevel)
                color = new Color(0, 1f, 0);
            DrawMbb(mbb, color);
        }
        
        // TODO: Add your drawing code here
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    void DrawMbb(Mbb mbb, Microsoft.Xna.Framework.Color color, int thickness = 1)
    {
        DrawRectangle(new Vector2(mbb.Minimum.X, mbb.Minimum.Y), new Vector2(mbb.Maximum.X, mbb.Maximum.Y), color, thickness);
    }

    void DrawRectangle(Vector2 minimum, Vector2 maximum, Color color, int thickness = 1)
    {
        Vector2 topLeft = new Vector2(minimum.X, maximum.Y) * Scale;
        Vector2 topRight = new Vector2(maximum.X, maximum.Y) * Scale;
        Vector2 bottomLeft = new Vector2(minimum.X, minimum.Y) * Scale;
        Vector2 bottomRight = new Vector2(maximum.X, minimum.Y) * Scale;
        DrawLine(topLeft, topRight, color);
        DrawLine(topRight, bottomRight, color);
        DrawLine(bottomRight, bottomLeft, color);
        DrawLine(bottomLeft, topLeft, color);
    }

    void DrawLine(Vector2 point1, Vector2 point2, Microsoft.Xna.Framework.Color color, int thickness = 1)
    {
        float distance = Vector2.Distance(point1, point2);
        float angle = MathF.Atan2(point2.Y - point1.Y, point2.X - point1.X);
        DrawLine(point1, distance, angle, color, thickness);
    }
    void DrawLine(Vector2 point, float length, float angle, Color color, float thickness = 1)
    {
        Vector2 origin = new Vector2(0, 0.5f);
        Vector2 scale = new Vector2(length, thickness);
        _spriteBatch.Draw(pixel, point, null, color, angle, origin, scale, SpriteEffects.None, 0);
        
    }

    Microsoft.Xna.Framework.Vector3 Convert(System.Numerics.Vector3 vector)
    {
        return new Microsoft.Xna.Framework.Vector3(vector.X, vector.Y, vector.Z);
    }
    Microsoft.Xna.Framework.Vector2 Convert(System.Numerics.Vector2 vector)
    {
        return new Microsoft.Xna.Framework.Vector2(vector.X, vector.Y);
    }
}