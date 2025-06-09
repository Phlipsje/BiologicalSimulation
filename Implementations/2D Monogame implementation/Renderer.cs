using System;
using System.Collections.Generic;
using BioSim;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Implementations.Monogame2DRenderer;

public class Renderer
{
    public RenderTarget2D RenderTarget { get; }
    public Rectangle DisplayRectangle { get; }
    private ViewDirection viewDirection;
    private Texture2D organismTexture;
    private SpriteFont font;
    private int axisIndex0;
    private int axisIndex1;
    private int topDownAxis;
    private char axis0Char;
    private char axis1Char;
    
    public Renderer(RenderTarget2D renderTarget, ViewDirection viewDirection, Rectangle displayRectangle)
    {
        this.RenderTarget = renderTarget;
        this.viewDirection = viewDirection;
        this.DisplayRectangle = displayRectangle;

        switch (viewDirection)
        {
            case ViewDirection.XYPlane:
                axisIndex0 = 0; //X
                axis0Char = 'X';
                axisIndex1 = 1; //Y
                axis1Char = 'Y';
                topDownAxis = 2; //Z
                break;
            case ViewDirection.YZPlane:
                axisIndex0 = 1; //Y
                axis0Char = 'Y';
                axisIndex1 = 2; //Z
                axis1Char = 'Z';
                topDownAxis = 0; //X
                break;
            case ViewDirection.XZPlane:
                axisIndex0 = 0; //X
                axis0Char = 'X';
                axisIndex1 = 2; //Z
                axis1Char = 'Z';
                topDownAxis = 1; //Y
                break;
        }
    }

    public void LoadContent(ContentManager content)
    {
        organismTexture = content.Load<Texture2D>("Big circle");
        font = content.Load<SpriteFont>("Font");
    }

    public void Render(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Organism[] organisms, ViewingInformation viewingInformation)
    {
        //Tell buffer we are drawing to a render target
        graphicsDevice.SetRenderTarget(RenderTarget);

        //Remove everything that is set in the render target and set it to a singular background color
        graphicsDevice.Clear(Monogame2DRenderer.BackgroundColor);

        //Start a new buffer to draw to
        spriteBatch.Begin();

        
        foreach (Organism organism in organisms)
        {
            float posAxis0 = (organism.Position[axisIndex0] - viewingInformation.Position[axisIndex0] - organism.Size/2) * viewingInformation.Scale + viewingInformation.Width/2;
            float posAxis1 = (organism.Position[axisIndex1] - viewingInformation.Position[axisIndex1] - organism.Size/2) * viewingInformation.Scale + viewingInformation.Height/2;

            float organismPixelSize = organism.Size * viewingInformation.Scale;
            
            //Skip if out of scope
            if (posAxis0 < -organismPixelSize || posAxis0 > viewingInformation.Width + organismPixelSize)
                continue;
            
            //Skip if out of scope
            if (posAxis1 < -organismPixelSize || posAxis1 > viewingInformation.Height + organismPixelSize)
                continue;

            //TODO base scale, color and layerDepth off of what is in the foreground (and don't draw what is behind the camera)
            float minDistanceToCamera = -3f;
            float maxDistanceToCamera = 3f;
            float layerDepth = (organism.Position[topDownAxis] - minDistanceToCamera) / (maxDistanceToCamera - minDistanceToCamera);
            Vector2 position = new Vector2(posAxis0, posAxis1);
            float scale = viewingInformation.Scale / 1000f; //1000 because the size of the organism sprite is 1000x1000
            Color color = new Color(organism.Color.X * byte.MaxValue, organism.Color.Y * byte.MaxValue,
                organism.Color.Z * byte.MaxValue);
            spriteBatch.Draw(organismTexture, position, null, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        }
        
        spriteBatch.DrawString(font, axis0Char + " ->", new Vector2(50, 20), Color.White);
        spriteBatch.DrawString(font, axis1Char.ToString(), new Vector2(20, 50), Color.White);
        spriteBatch.DrawString(font, "|", new Vector2(30, 80), Color.White);
        spriteBatch.DrawString(font, "V", new Vector2(20, 105), Color.White);
        
        //Finish the buffer and flush output to gpu
        spriteBatch.End();
        
        //Clear the render target from the buffer to draw to screen again
        graphicsDevice.SetRenderTarget(null);
    }
}

public enum ViewDirection
{
    XYPlane,
    YZPlane,
    XZPlane,
}