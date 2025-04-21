using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Components;
using MY_GAME_1;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;


namespace MY_GAME_1;

public static class GameWorld
{
    public static Player player;
    public static Platform_1 platform;

    public static Background background;
    public static List<IGameObject> GameObjects = new List<IGameObject>();


}


public class Level
{
    private GraphicsDevice GraphicsDevice;
    private readonly ContentManager Content;

    public Texture2D BackgroundImage { get; private set; }
    public Texture2D TexturePlayer { get; private set; }
    public Texture2D TexturePlatform { get; private set; }
    public Texture2D TextureTest { get; private set; }

    public PlatfotmCreator PlatfotmCreator { get; private set; }




    public Level(GraphicsDevice graphicsDevice, ContentManager content)
    {
        GraphicsDevice = graphicsDevice;
        Content = content;
    }


    public void LoadContent()
    {
        TexturePlayer = Content.Load<Texture2D>("mobokot");
        TexturePlatform = Content.Load<Texture2D>("test");
        BackgroundImage = Content.Load<Texture2D>("Background_0");

    }

    public void Initialize()
    {

        PlatfotmCreator = new PlatfotmCreator(TexturePlatform, GraphicsDevice.Viewport);

        GameWorld.player = new Player(
            new Vector2(50, 50),
            100, 100,
            TexturePlayer,
            10, 10,
            GraphicsDevice.Viewport,
            0.1f);

        PlatfotmCreator.MakePlatform_(0.3f,
            new Vector2(100, 800),
            new Vector2(200, 700),
            new Vector2(400, 600),
            new Vector2(600, 500),
            new Vector2(800, 300));

        GameWorld.GameObjects.AddRange(PlatfotmCreator.Platforms);
        GameWorld.background = new Background(BackgroundImage, GraphicsDevice.Viewport);
    }



}