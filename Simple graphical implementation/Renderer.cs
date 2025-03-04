using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simple_graphical_implementation;

public class Renderer
{
    public RenderTarget2D RenderTarget { get; }
    public Rectangle DisplayRectangle { get; }
    private ViewDirection viewDirection;
    
    public Renderer(RenderTarget2D renderTarget, ViewDirection viewDirection, Rectangle displayRectangle)
    {
        this.RenderTarget = renderTarget;
        this.viewDirection = viewDirection;
        this.DisplayRectangle = displayRectangle;
    }

    public void Render(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        //Tell buffer we are drawing to a render target
        graphicsDevice.SetRenderTarget(RenderTarget);

        //Remove everything that is set in the render target and set it to a singular background color
        graphicsDevice.Clear(VisualSimulation.BackgroundColor);

        //Start a new buffer to draw to
        spriteBatch.Begin();
        
        //TODO have the plane be drawn
        
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