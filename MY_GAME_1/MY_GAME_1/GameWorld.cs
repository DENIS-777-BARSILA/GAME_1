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
    public static GraphicsDevice GraphicsDevice;
    public static ContentManager Content;
    public static GameTime GameTime;
    public static SpriteBatch _spriteBatch;


    public static Action Update;
    public static Action<GameTime> Draw;

    public static Viewport viewport;

    public static Background background;
    public static Player player;


    public static List<Bullet> Bullets = new List<Bullet>();
    public static List<IGameObject> GameObjects = new List<IGameObject>();

    public static Level Level;
}

public class Level
{


    public Texture2D BackgroundImage { get; private set; }
    public Texture2D TexturePlayer { get; private set; }

    public Texture2D TexturePlatform { get; private set; }
    public Texture2D TextureBullet { get; private set; }
    public Texture2D TextureTest { get; private set; }

    public Texture2D TextureMonster { get; private set; }


    public PlatfotmCreator platformCreator { get; private set; }
    public MonsterCreator monsterCreator { get; private set; }

    public Level(GraphicsDevice graphicsDevice, ContentManager content)
    {
        GameWorld.GraphicsDevice = graphicsDevice;
        GameWorld.Content = content;
    }

    public void LoadContent()
    {
        BackgroundImage = GameWorld.Content.Load<Texture2D>("Background_0");
        TexturePlayer = GameWorld.Content.Load<Texture2D>("player");

        TextureBullet = GameWorld.Content.Load<Texture2D>("bullet");
        TextureMonster = GameWorld.Content.Load<Texture2D>("monster_1");

        TexturePlatform = GameWorld.Content.Load<Texture2D>("test");
    }

    public void Initialize()
    {
        platformCreator = new PlatfotmCreator(TexturePlatform, GameWorld.GraphicsDevice.Viewport);
        monsterCreator = new MonsterCreator(TextureMonster, GameWorld.GraphicsDevice.Viewport);

        GameWorld.player = new Player(new Vector2(50, 50), 100, 100, TexturePlayer, 10, 10, GameWorld.GraphicsDevice.Viewport, 0.2f);

        platformCreator.MakePlatform_(0.3f,
           new Vector2(100, 800),
           new Vector2(200, 700),
           new Vector2(400, 600),
           new Vector2(600, 500),
           new Vector2(800, 300));

        monsterCreator.MakeMonster(0.5f, new Vector2(200, 200)); //new Vector2(700, 400));

        GameWorld.GameObjects.AddRange(platformCreator.Platforms);
        GameWorld.background = new Background(BackgroundImage, 2);


        GameWorld.Update += () => monsterCreator.Update();
        GameWorld.Update += () => GameWorld.player.Update();
        GameWorld.Update += () => Bullet.Update(GameWorld.Bullets);

        GameWorld.Draw += (gameTime) => GameWorld.background.RenderComp.Draw(GameWorld._spriteBatch, gameTime);
        GameWorld.Draw += (gameTime) => GameWorld.player.RenderComp.Draw(GameWorld._spriteBatch, gameTime);
        GameWorld.Draw += (gameTime) => monsterCreator.Draw(GameWorld._spriteBatch, gameTime);
        GameWorld.Draw += (gameTime) => platformCreator.Draw(GameWorld._spriteBatch, gameTime);
    }



    public void Update()
    {

    }

}