using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Components;
using System.Linq;


namespace MY_GAME_1;


public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;

    private SpriteBatch _spriteBatch;
    private Level _level;


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        GameWorld.viewport = GraphicsDevice.Viewport;
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        GameWorld._spriteBatch = _spriteBatch;
        _level = new Level(GraphicsDevice, Content);
        _level.LoadContent();
        _level.Initialize();
        GameWorld.Level = _level;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        GameWorld.GameTime = gameTime;
        GameWorld.Update();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        GameWorld.Draw(gameTime); 
       
        GameWorld.Bullets.ForEach(b => b.Draw(_spriteBatch, gameTime));




        Texture2D debugTexture = new Texture2D(GraphicsDevice, 1, 1);
        debugTexture.SetData(new[] { Color.Red });

        // хитбокс игрока
        //  Rectangle playerBounds = new Rectangle(
        //     (int)GameWorld.player.PositionComp.Position.X,
        //     (int)GameWorld.player.PositionComp.Position.Y,
        //   GameWorld.player.RenderComp.Width,
        //   GameWorld.player.RenderComp.Height);
        //   _spriteBatch.Draw(debugTexture, playerBounds, Color.Red * 0.5f);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
