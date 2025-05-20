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
        _level = new Level(GraphicsDevice, Content, GameState.CurrentLevel);

        InterfaceObjects.InitializeMenus(() => this.Exit());
    }

    protected override void Update(GameTime gameTime)
    {
        GameWorld.GameTime = gameTime;

        var keyboardState = Keyboard.GetState();

        switch (GameState.CurrentState)
        {
            case GameStates.MainMenu:
                if (InterfaceObjects.MainMenu != null)
                    InterfaceObjects.MainMenu.Update(keyboardState);
                break;

            case GameStates.Playing:
                if (keyboardState.IsKeyDown(Keys.Escape))
                    GameState.CurrentState = GameStates.Paused;
                else
                    GameWorld.Update?.Invoke();
                break;

            case GameStates.Paused:
                if (InterfaceObjects.PauseMenu != null)
                    InterfaceObjects.PauseMenu.Update(keyboardState);
                break;

            case GameStates.GameOver:
                if (InterfaceObjects.GameOverMenu != null)
                    InterfaceObjects.GameOverMenu.Update(keyboardState);
                break;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();

        if (GameWorld.background != null)
        {
            GameWorld.background.RenderComp.Draw(_spriteBatch, gameTime);
        }

        switch (GameState.CurrentState)
        {
            case GameStates.MainMenu:
                if (InterfaceObjects.MainMenu != null)
                    InterfaceObjects.MainMenu.Draw(_spriteBatch);
                break;

            case GameStates.Playing:
                GameWorld.Draw?.Invoke(gameTime);
                break;

            case GameStates.Paused:
                GameWorld.Draw?.Invoke(gameTime);

                var pixel = new Texture2D(GraphicsDevice, 1, 1);
                pixel.SetData(new[] { Color.Black });
                _spriteBatch.Draw(pixel,
                    new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                    new Color(0, 0, 0, 150));

                if (InterfaceObjects.PauseMenu != null)
                    InterfaceObjects.PauseMenu.Draw(_spriteBatch);
                break;

            case GameStates.GameOver:
                if (InterfaceObjects.GameOverMenu != null)
                    InterfaceObjects.GameOverMenu.Draw(_spriteBatch);
                break;
        }

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
