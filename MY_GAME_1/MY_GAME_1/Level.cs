using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Components;
using MY_GAME_1;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace MY_GAME_1;


public class Level
{
    public Texture2D BackgroundImage { get; private set; }
    public Texture2D TexturePlayer { get; private set; }

    public Dictionary<PlatformTypeData, Texture2D> TexturesPlatforms { get; private set; }
    public Dictionary<MonsterTypeData, Texture2D> TexturesMonsters { get; private set; }
    public Dictionary<CollectibleTypeData, Texture2D> TexturesCollectible { get; private set; }


    public Texture2D TextureBullet { get; private set; }
    public Texture2D TextureTest { get; private set; }

    public PlatformCreator platformCreator { get; private set; }
    public MonsterCreator monsterCreator { get; private set; }
    public CollectebleCreator collectebleCreator { get; private set; }

    private LevelData levelData;

    public Level(GraphicsDevice graphicsDevice, ContentManager content, string jsonLevelPath)
    {
        GameWorld.GraphicsDevice = graphicsDevice;
        GameWorld.Content = content;

        LoadLevelData(jsonLevelPath);
    }

    private void LoadLevelData(string jsonPath)
    {
        string json = File.ReadAllText(Path.Combine(GameWorld.Content.RootDirectory, jsonPath));

        levelData = JsonConvert.DeserializeObject<LevelData>(json);
    }

    public void LoadContent()
    {
        BackgroundImage = GameWorld.Content.Load<Texture2D>(levelData.BackgroundTexture);
        TexturePlayer = GameWorld.Content.Load<Texture2D>(levelData.PlayerTexture);
        TextureBullet = GameWorld.Content.Load<Texture2D>(levelData.BulletTexture);


        TexturesPlatforms = new Dictionary<PlatformTypeData, Texture2D>();
        foreach (var type in levelData.PlatformTypes)
        {
            TexturesPlatforms[type.Value] = GameWorld.Content.Load<Texture2D>(type.Value.Texture);
        }

        TexturesMonsters = new Dictionary<MonsterTypeData, Texture2D>();
        foreach (var type in levelData.MonsterTypes)
        {
            TexturesMonsters[type.Value] = GameWorld.Content.Load<Texture2D>(type.Value.Texture);
        }

        TexturesCollectible = new Dictionary<CollectibleTypeData, Texture2D>();
        foreach (var type in levelData.CollectibleTypes)
        {
            TexturesCollectible[type.Value] = GameWorld.Content.Load<Texture2D>(type.Value.Texture);
        }
    }

    public void Initialize()
    {
        GameWorld.TileMap = new TileMap();

        var playerPosition = GameWorld.TileMap.GetPosition(levelData.PlayerStartPosition);
        GameWorld.player = new Player(playerPosition, 180, 200, TexturePlayer, 5, 5,
                                   GameWorld.GraphicsDevice.Viewport, 0.05f);

        GameWorld.background = new Background(BackgroundImage, 2f);

        platformCreator = new PlatformCreator(GameWorld.GraphicsDevice.Viewport, TexturesPlatforms);
        monsterCreator = new MonsterCreator(GameWorld.GraphicsDevice.Viewport, TexturesMonsters);
        collectebleCreator = new CollectebleCreator(GameWorld.GraphicsDevice.Viewport, TexturesCollectible);

        GameWorld.Level = this;

        string mapPath = Path.Combine(GameWorld.Content.RootDirectory, "Levels", levelData.TileMapFile);
        GameWorld.TileMap.LoadFromTextFile(mapPath, levelData);

        InitializeGameProcess();
        InitializeInterface();
    }

    private void InitializeGameProcess()
    {
        GameWorld.Update += () => monsterCreator.Update();
        GameWorld.Update += () => collectebleCreator.Update();
        GameWorld.Update += () => GameWorld.player.Update();
        GameWorld.Update += () => Bullet.Update(GameWorld.Bullets);
        GameWorld.Update += () => Bullet.Update(GameWorld.Bullets);

        GameWorld.Draw += (gameTime) => GameWorld.background.RenderComp.Draw(GameWorld._spriteBatch, gameTime);
        GameWorld.Draw += (gameTime) => GameWorld.player.RenderComp.Draw(GameWorld._spriteBatch, gameTime);
        GameWorld.Draw += (gameTime) => monsterCreator.Draw(GameWorld._spriteBatch, gameTime);
        GameWorld.Draw += (gameTime) => platformCreator.Draw(GameWorld._spriteBatch, gameTime);
        GameWorld.Draw += (gameTime) => collectebleCreator.Draw(GameWorld._spriteBatch, gameTime);
    }

    private void InitializeInterface()
    {
        HealthBar playerHealthBar = new HealthBar(new Vector2(20, 20),
     200, height: 20, healthComp: GameWorld.player.HealthComp);

        InterfaceObjects.PlayerHealthBar = playerHealthBar;

        GameWorld.Draw += (gameTime) => playerHealthBar.Draw(GameWorld._spriteBatch, gameTime);
    }

    public void RemoveGameObject(IGameObject gameObject)
    {
        if (gameObject is Monster_1)
            monsterCreator.Remove((Monster_1)gameObject);
        if (gameObject is ICollectible)
        {
            GameWorld.CollectibleObjects.Remove((ICollectible)gameObject);

        }
    }
}

public class LevelData
{
    public string BackgroundTexture { get; set; }
    public string PlayerTexture { get; set; }
    public string BulletTexture { get; set; }

    public Dictionary<char, PlatformTypeData> PlatformTypes { get; set; }
    public Dictionary<char, MonsterTypeData> MonsterTypes { get; set; }
    public Dictionary<char, CollectibleTypeData> CollectibleTypes { get; set; }

    public TilePosition PlayerStartPosition;

    public string TileMapFile { get; set; }

}

public class TilePosition
{
    public int TileX { get; set; }
    public int TileY { get; set; }
}

public class MonsterTypeData
{
    public string Texture;
    public int Health;

    public float SpeedX;

    public float SpeedY;

    public float Scale;

    public TypesMovement algorithmMovement;
}

public class PlatformTypeData : ITileMapObject
{
    public string Texture;
}

public class CollectibleTypeData : ITileMapObject
{
    public string Texture { get; set; }
    public string Type { get; set; }
    public float Value { get; set; }
}

public interface ITileMapObject;


