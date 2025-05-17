using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Components;
using MY_GAME_1;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.IO;
namespace MY_GAME_1;

public class HealthBar
{
    private Texture2D backgroundTexture;
    private Texture2D healthTexture;
    private Vector2 position;
    private int width;
    private int height;
    private HealthComponent healthComp;
    
    public HealthBar(Vector2 position, int width, int height, 
                   HealthComponent healthComp)
    {
        this.position = position;
        this.width = width;
        this.height = height;
        this.healthComp = healthComp;
        
        backgroundTexture = new Texture2D(GameWorld.GraphicsDevice, 1, 1);
        healthTexture = new Texture2D(GameWorld.GraphicsDevice, 1, 1);
        backgroundTexture.SetData(new[] { Color.Gray });
        
        healthTexture.SetData(new[] { Color.Red });
    }
    
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(backgroundTexture, 
            new Rectangle((int)position.X, (int)position.Y, width, height), 
            Color.White);
            
        float healthPercentage = (float)healthComp.Health / healthComp.MaxHealth;
        int currentWidth = (int)(width * healthPercentage);
        
        spriteBatch.Draw(healthTexture, 
            new Rectangle((int)position.X, (int)position.Y, currentWidth, height), 
            Color.White);
    }
}