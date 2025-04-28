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
    public static Background background;
    public static Player player;
    public static Monster_1 Monster;

    public static PlatfotmCreator platformCreater;
    public static MonsterCreator monsterCreater;

    public static List<Bullet> Bullets = new List<Bullet>();

    public static List<IGameObject> GameObjects = new List<IGameObject>();
    public static Level Level;

    public static Viewport viewport;
}

public class Level
{
    private GraphicsDevice GraphicsDevice;
    private readonly ContentManager Content;
    public GameTime GameTime { get; set; }

    public Texture2D BackgroundImage { get; private set; }
    public Texture2D TexturePlayer { get; private set; }

    public Texture2D TexturePlatform { get; private set; }
    public Texture2D TextureBullet { get; private set; }
    public Texture2D TextureTest { get; private set; }

    public Texture2D TextureMonster { get; private set; }

    public Level(GraphicsDevice graphicsDevice, ContentManager content)
    {
        GraphicsDevice = graphicsDevice;
        Content = content;
    }

    public void LoadContent()
    {
        BackgroundImage = Content.Load<Texture2D>("Background_0");
        TexturePlayer = Content.Load<Texture2D>("player");

        TextureBullet = Content.Load<Texture2D>("bullet");
        TextureMonster = Content.Load<Texture2D>("monster_1");

        TexturePlatform = Content.Load<Texture2D>("test");
    }

    public void Initialize()
    {
        GameWorld.platformCreater = new PlatfotmCreator(TexturePlatform, GraphicsDevice.Viewport);
        GameWorld.monsterCreater = new MonsterCreator(TextureMonster, GraphicsDevice.Viewport);

        GameWorld.player = new Player(new Vector2(50, 50), 100, 100, TexturePlayer, 10, 10, GraphicsDevice.Viewport, 0.2f);
       // GameWorld.Monster = (new Monster_1(new Vector2(150, 50), 100, 100, TextureMonster, 10, 10, GraphicsDevice.Viewport, 0.5f));

        GameWorld.platformCreater.MakePlatform_(0.3f,
           new Vector2(100, 800),
           new Vector2(200, 700),
           new Vector2(400, 600),
           new Vector2(600, 500),
           new Vector2(800, 300));

         GameWorld.monsterCreater.MakeMonster(new Vector2(400, 400), 0.5f);



        GameWorld.GameObjects.AddRange(GameWorld.platformCreater.Platforms);
        GameWorld.background = new Background(BackgroundImage, 2);
    }

    public void Update(GameTime gameTime)
    {
        GameTime = gameTime;
    }

}