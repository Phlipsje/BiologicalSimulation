using System;
using System.Collections.Generic;
using BioSim;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Implementations;

public class RenderManager
{
    VisualSimulation visualSimulation;
    GraphicsDevice graphicsDevice;
    List<Renderer> renderers;
    private Texture2D pixel;
    private SpriteFont font;
    public bool DrawBorders { get; set; }
    public bool Draw { get; set; } = true;
    public RenderManager(VisualSimulation visualSimulation, GraphicsDevice graphicsDevice, List<Renderer> renderers)
    {
        this.visualSimulation = visualSimulation;
        this.graphicsDevice = graphicsDevice;
        this.renderers = renderers;
    }

    public void LoadContent(ContentManager content)
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.LoadContent(content);
        }
        
        pixel = content.Load<Texture2D>("Square");
        font = content.Load<SpriteFont>("Font");
    }

    public void Render(SpriteBatch spriteBatch, World world, ViewingInformation viewingInformation)
    {
        if (Draw)
        {
            //First let every render target be formed
            foreach (Renderer renderer in renderers)
            {
                renderer.Render(graphicsDevice, spriteBatch, world, viewingInformation);
            } 
        }
        
        graphicsDevice.Clear(Color.Black);
        
        //Create a new buffer to draw to (the screen in the case)
        spriteBatch.Begin();

        if (Draw)
        {
            foreach (Renderer renderer in renderers)
            {
                spriteBatch.Draw(renderer.RenderTarget, renderer.DisplayRectangle, Color.White);
                if (DrawBorders)
                {
                    DrawRectangle(spriteBatch, renderer.DisplayRectangle, Color.Red);
                }
            } 
        }
        
        //Draw some extra information on screen
        spriteBatch.DrawString(font, $"FPS: {Math.Round(VisualSimulation.AverageFps)}", new Vector2(660, 380), Color.White);
        spriteBatch.DrawString(font, $"Tick: {Main.Tick}", new Vector2(660, 410), Color.White);
        spriteBatch.DrawString(font, $"Total Organisms: {Main.OrganismACount + Main.OrganismBCount}", new Vector2(660, 440), Color.White);
        spriteBatch.DrawString(font, $"Green(A) Organisms: {Main.OrganismACount}", new Vector2(660, 470), Color.White);
        spriteBatch.DrawString(font, $"Yellow(B) Organisms: {Main.OrganismBCount}", new Vector2(660, 500), Color.White);
        
        //Stop drawing to the buffer and flush the output to the gpu
        spriteBatch.End();
    }

    private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
    {
        spriteBatch.Draw(pixel, new Rectangle(rectangle.Location, new Point(1, rectangle.Height)), color);
        spriteBatch.Draw(pixel, new Rectangle(rectangle.Location, new Point(rectangle.Width, 1)), color);
        spriteBatch.Draw(pixel, new Rectangle(new Point(rectangle.X + rectangle.Width - 1, rectangle.Y), new Point(1, rectangle.Height)), color);
        spriteBatch.Draw(pixel, new Rectangle(new Point(rectangle.X, rectangle.Y + rectangle.Height - 1), new Point(rectangle.Width, 1)), color);
    }
}