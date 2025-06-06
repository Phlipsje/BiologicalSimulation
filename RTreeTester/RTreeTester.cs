using System;
using System.Collections.Generic;
using System.Linq;
using BioSim.Datastructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector3 = System.Numerics.Vector3;

namespace RTreeTester;

public class RTreeTester : Game
{
    private const int screenWidth = 1920;
    private const int screenHeight = 1200;
    private const int m = 2, M = 50;
    private const float Scale = 0.01f;
    private const int TestSize = 10;
    private Random random = new Random(33419680);
    private KeyboardState lastKeyState;
    private int currentN = 0;
    private Texture2D pixel;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RTree<TestObject> rTree;
    private List<TestObject> list = [];
    
    public RTreeTester()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        rTree = new RTree<TestObject>(m, M);
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = screenWidth;
        _graphics.PreferredBackBufferHeight = screenHeight;
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

        int disconnectedParents = rTree.DisconnectedParentCount();
        if (disconnectedParents > 0)
            throw new Exception("disconnected parents detected");
        int wrongParents = rTree.WrongParentCount();
        if (wrongParents > 0)
            throw new Exception("Wrong parents detected");
        
        int size = 50000;
        KeyboardState keyState = Keyboard.GetState();
        if (keyState.IsKeyUp(Keys.A) && lastKeyState.IsKeyDown(Keys.A))
        {
            float spread = 50000;
            Vector3 pos = new Vector3(random.NextSingle() * spread, random.NextSingle() * spread,
                random.NextSingle() * spread);
            Mbb mbb = new Mbb(pos, pos + new Vector3(TestSize));
            TestObject obj = new TestObject(currentN++, mbb); 
            rTree.Insert(obj);
            list.Add(obj);
        }

        if (keyState.IsKeyUp(Keys.Space) && lastKeyState.IsKeyDown(Keys.Space))
        {
            for (int i = 0; i < size; i++)
            {
                float spread = 50000;
                Vector3 pos = new Vector3(random.NextSingle() * spread, random.NextSingle() * spread,
                    random.NextSingle() * spread);
                Mbb mbb = new Mbb(pos, pos + new Vector3(TestSize));
                TestObject obj = new TestObject(currentN++, mbb);
                rTree.Insert(obj);
                list.Add(obj);
            }
        }
        if (keyState.IsKeyUp(Keys.H) && lastKeyState.IsKeyDown(Keys.H))
        {
            for (int i = 0; i < 270; i++)
            {
                float spread = 50000;
                Vector3 pos = new Vector3(random.NextSingle() * spread, random.NextSingle() * spread,
                    random.NextSingle() * spread);
                Mbb mbb = new Mbb(pos, pos + new Vector3(TestSize));
                TestObject obj = new TestObject(currentN++, mbb);
                rTree.Insert(obj);
                list.Add(obj);
            }
        }
        if (keyState.IsKeyUp(Keys.R) && lastKeyState.IsKeyDown(Keys.R) && currentN > 0)
        {
            for (int i = 0; i < size && currentN > 0; i++)
            {
                rTree.Delete(list[--currentN]);
                //list.Remove(list[currentN]);
            }
            list = [];
        }
        if (keyState.IsKeyUp(Keys.Y) && lastKeyState.IsKeyDown(Keys.Y) && currentN > 0)
        {
            for (int i = 0; i < 210 && currentN > 0; i++)
            {
                rTree.Delete(list[--currentN]);
                list.Remove(list[currentN]);
            }
        }
        if (keyState.IsKeyUp(Keys.D) && lastKeyState.IsKeyDown(Keys.D) && currentN > 0)
        {
            rTree.Delete(list[--currentN]);
            list.Remove(list[currentN]);
        }

        if (keyState.IsKeyUp(Keys.T) && lastKeyState.IsKeyDown(Keys.T))
        {
            int randomSeed = 0;
            try
            {
                for (int sim = 0; sim < 100000000; sim++)
                {
                    randomSeed = sim * 10;
                    random = new Random(randomSeed);
                    for (int i = 0; i < size; i++)
                    {
                        float spread = 50000;
                        Vector3 pos = new Vector3(random.NextSingle() * spread, random.NextSingle() * spread,
                            random.NextSingle() * spread);
                        Mbb mbb = new Mbb(pos, pos + new Vector3(TestSize));
                        TestObject obj = new TestObject(currentN++, mbb);
                        rTree.Insert(obj);
                        list.Add(obj);
                    }

                    for (int i = 0; i < size && currentN > 0; i++)
                    {
                        rTree.Delete(list[--currentN]);
                        //list.Remove(list[currentN]);
                    }

                    list = [];
                }
            }
            catch (Exception e)
            {
                int seed = randomSeed;
                throw e;
            }
            Exit();
        }
        if (keyState.IsKeyUp(Keys.K) && lastKeyState.IsKeyDown(Keys.K))
        {
            for (int sim = 0; sim < 60; sim++)
            {
                for (int i = 0; i < size; i++)
                {
                    float spread = 50000;
                    Vector3 pos = new Vector3(random.NextSingle() * spread, random.NextSingle() * spread,
                        random.NextSingle() * spread);
                    Mbb mbb = new Mbb(pos, pos + new Vector3(TestSize));
                    TestObject obj = new TestObject(currentN++, mbb);
                    rTree.Insert(obj);
                    list.Add(obj);
                }

                for (int i = 0; i < size && currentN > 0; i++)
                {
                    rTree.Delete(list[--currentN]);
                    //list.Remove(list[currentN]);
                }
                list = [];
            }
            Exit();
        }
        if (keyState.IsKeyUp(Keys.L) && lastKeyState.IsKeyDown(Keys.L))
        {
            for (int sim = 0; sim < 60; sim++)
            {
                for (int i = 0; i < currentN; i++)
                {
                    rTree.Delete(list[i]);
                    rTree.Insert(list[i]);
                }
            }
            Exit();
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
        
        //draw graph representation.
        DrawNode(rTree.Root, 0, 0, Vector2.Zero);
        
        
        // TODO: Add your drawing code here
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    void DrawNode(RNode<TestObject> node, int level, float offset, Vector2 parentPos)
    {
        float levelHeight = 50;
        float totalWidth = screenWidth / 2f;
        float levelWidth = totalWidth / MathF.Pow(M, level);
        Vector2 graphOrigin = new Vector2(3 * (screenWidth / 4f), screenHeight / 2f);
        Vector2 drawPos = graphOrigin + new Vector2(offset, level * levelHeight);
        Vector2 halfSize = new Vector2(5f);
        if (node is RLeafNode<TestObject>)
        {
            //draw 
            DrawRectangle(drawPos - halfSize, drawPos + halfSize, Color.Green);
        }
        else
        {
            //draw node
            DrawRectangle(drawPos - halfSize, drawPos + halfSize, Color.White);
            RNonLeafNode<TestObject> nonLeaf = (RNonLeafNode<TestObject>)node;
            for(int i = 0; i < nonLeaf.Children.Count; i++)
            {
                DrawNode(nonLeaf.Children[i], level + 1, offset + i * (levelWidth / nonLeaf.Children.Count) - levelWidth / 2f, drawPos);
            }
        }
        if (level != 0)
        {
            DrawLine(drawPos, parentPos, Color.Aqua);
        }
    }
    void DrawMbb(Mbb mbb, Microsoft.Xna.Framework.Color color, int thickness = 1)
    {
        DrawRectangle(new Vector2(mbb.Minimum.X * Scale, mbb.Minimum.Y * Scale + screenHeight / 2f), new Vector2(mbb.Maximum.X * Scale, mbb.Maximum.Y * Scale + screenHeight / 2f), color, thickness);
        DrawRectangle(new Vector2(mbb.Minimum.X * Scale + screenWidth / 2f, mbb.Minimum.Z * Scale), new Vector2(mbb.Maximum.X * Scale + screenWidth / 2f, mbb.Maximum.Z * Scale), color, thickness);
        DrawRectangle(new Vector2(mbb.Minimum.Y * Scale, mbb.Minimum.Z * Scale), new Vector2(mbb.Maximum.Y * Scale, mbb.Maximum.Z * Scale), color, thickness);
    }

    void DrawRectangle(Vector2 minimum, Vector2 maximum, Color color, int thickness = 1)
    {
        Vector2 topLeft = new Vector2(minimum.X, maximum.Y);
        Vector2 topRight = new Vector2(maximum.X, maximum.Y);
        Vector2 bottomLeft = new Vector2(minimum.X, minimum.Y);
        Vector2 bottomRight = new Vector2(maximum.X, minimum.Y);
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
    
    private class TestObject(int n, Mbb mbb) : IMinimumBoundable
    {
        public Mbb Mbb = mbb;
        public int N = n;
        public Mbb GetMbb()
        {
            return mbb;
        }
    }
}