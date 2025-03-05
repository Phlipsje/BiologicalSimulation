using System;
using BioSim;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Simple_graphical_implementation;

public class Renderer
{
    public RenderTarget2D RenderTarget { get; }
    public Rectangle DisplayRectangle { get; }
    private ViewDirection viewDirection;
    private Texture2D organismTexture;
    private Color OrganismColor { get; } = Color.Green;
    private int axesIndex0;
    private int axesIndex1;
    
    public Renderer(RenderTarget2D renderTarget, ViewDirection viewDirection, Rectangle displayRectangle)
    {
        this.RenderTarget = renderTarget;
        this.viewDirection = viewDirection;
        this.DisplayRectangle = displayRectangle;

        switch (viewDirection)
        {
            case ViewDirection.XYPlane:
                axesIndex0 = 0; //X
                axesIndex1 = 1; //Y
                break;
            case ViewDirection.YZPlane:
                axesIndex0 = 1; //Y
                axesIndex1 = 2; //Z
                break;
            case ViewDirection.XZPlane:
                axesIndex0 = 0; //X
                axesIndex1 = 2; //Z
                break;
        }
    }

    public void LoadContent(ContentManager content)
    {
        organismTexture = content.Load<Texture2D>("Big circle");
    }

    public void Render(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, World world, ViewingInformation viewingInformation)
    {
        //Tell buffer we are drawing to a render target
        graphicsDevice.SetRenderTarget(RenderTarget);

        //Remove everything that is set in the render target and set it to a singular background color
        graphicsDevice.Clear(VisualSimulation.BackgroundColor);

        //Start a new buffer to draw to
        spriteBatch.Begin();

        foreach (Organism organism in world.Organisms)
        {
            float posAxis0 = (organism.Position[axesIndex0] - viewingInformation.Position[axesIndex0] - organism.Size/2) * viewingInformation.Scale + viewingInformation.Width/2;
            float posAxis1 = (organism.Position[axesIndex1] - viewingInformation.Position[axesIndex1] - organism.Size/2) * viewingInformation.Scale + viewingInformation.Height/2;

            float organismPixelSize = organism.Size * viewingInformation.Scale;
            
            //Skip if out of scope
            if (posAxis0 < -organismPixelSize || posAxis0 > viewingInformation.Width + organismPixelSize)
                continue;
            
            //Skip if out of scope
            if (posAxis1 < -organismPixelSize || posAxis0 > viewingInformation.Height + organismPixelSize)
                continue;

            Vector2 position = new Vector2(posAxis0, posAxis1);
            float scale = viewingInformation.Scale / 1000f; //1000 because the size of the organism sprite is 1000x1000
            spriteBatch.Draw(organismTexture, position, null, OrganismColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
        }
        
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