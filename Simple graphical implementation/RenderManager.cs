using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simple_graphical_implementation;

public class RenderManager
{
    GraphicsDevice graphicsDevice;
    List<Renderer> renderers;
    public RenderManager(GraphicsDevice graphicsDevice, List<Renderer> renderers)
    {
        this.graphicsDevice = graphicsDevice;
        this.renderers = renderers;
    }

    public void Render(SpriteBatch spriteBatch)
    {
        //First let every render target be formed
        foreach (Renderer renderer in renderers)
        {
            renderer.Render(graphicsDevice, spriteBatch);
        }
        
        graphicsDevice.Clear(Color.Black);
        
        //Create a new buffer to draw to (the screen in the case)
        spriteBatch.Begin();

        foreach (Renderer renderer in renderers)
        {
            spriteBatch.Draw(renderer.RenderTarget, renderer.DisplayRectangle, Color.White);
        }
        
        //Stop drawing to the buffer and flush the output to the gpu
        spriteBatch.End();
    }
}