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

    public GameObjectCreator gameObjectCreator { get; private set; }
    private LevelData levelData;

    public Level(GraphicsDevice graphicsDevice, ContentManager content, int levelNumber)
    {
        GameWorld.GraphicsDevice = graphicsDevice;
        GameWorld.Content = content;

        InitializeNewGame();
    }

    private void LoadLevelData(int levelNumber)
    {
        var jsonPath = $"Levels/Level_{levelNumber}.json";

        string json = File.ReadAllText(Path.Combine(GameWorld.Content.RootDirectory, jsonPath));

        levelData = JsonConvert.DeserializeObject<LevelData>(json);
    }

    public void LoadContent()
    {
        InterfaceObjects.LoadContent();

        BackgroundImage = GameWorld.Content.Load<Texture2D>(levelData.BackgroundTexture);
        TexturePlayer = GameWorld.Content.Load<Texture2D>(levelData.PlayerTexture);
        TextureBullet = GameWorld.Content.Load<Texture2D>(levelData.BulletTexture);

        TexturesPlatforms = new Dictionary<PlatformTypeData, Texture2D>();
        foreach (var type in levelData.PlatformTypes)
            TexturesPlatforms[type.Value] = GameWorld.Content.Load<Texture2D>(type.Value.Texture);

        TexturesMonsters = new Dictionary<MonsterTypeData, Texture2D>();
        foreach (var type in levelData.MonsterTypes)
            TexturesMonsters[type.Value] = GameWorld.Content.Load<Texture2D>(type.Value.Texture);

        TexturesCollectible = new Dictionary<CollectibleTypeData, Texture2D>();
        foreach (var type in levelData.CollectibleTypes)
            TexturesCollectible[type.Value] = GameWorld.Content.Load<Texture2D>(type.Value.Texture);
    }


    private void ResetGameState()
    {
        GameWorld.Bullets.Clear();
        GameWorld.ColisionObjects.Clear();
        GameWorld.CollectibleObjects.Clear();

        GameWorld.Update = null;
        GameWorld.Draw = null;
    }

    public void InitializeNewGame()
    {
        ResetGameState();
        LoadLevelData(1);
        LoadContent();
        Initialize();
        InitializePlayer();
        InterfaceObjects.InitializeInterface();
        
    }

    public void SetLevelNumber(int levelNumber)
    {
        ResetGameState();
        LoadLevelData(levelNumber);
        LoadContent();
        InitializePlayer();
        Initialize();
        InterfaceObjects.InitializeInterface();
    }


    public void Initialize()
    {
        GameWorld.TileMap = new TileMap();

        GameWorld.background = new Background(BackgroundImage, 2f);

        var platformFactory = new PlatformFactory(GameWorld.GraphicsDevice.Viewport, TexturesPlatforms);
        var monsterFactory = new MonsterFactory(GameWorld.GraphicsDevice.Viewport, TexturesMonsters);
        var collectibleFactory = new CollectibleFactory(GameWorld.GraphicsDevice.Viewport, TexturesCollectible);

        gameObjectCreator = new GameObjectCreator(
            GameWorld.TileMap,
            platformFactory,
            monsterFactory,
            collectibleFactory
        );

        GameWorld.Level = this;

        string mapPath = Path.Combine(GameWorld.Content.RootDirectory, "Levels", levelData.TileMapFile);
        GameWorld.TileMap.LoadFromTextFile(mapPath, levelData);

        InitializeGameProcess();
    }

    private void InitializePlayer()
    {
        var playerPosition = GameWorld.TileMap.GetPosition(levelData.PlayerStartPosition);
        GameWorld.player = new Player(playerPosition, 180, 200, TexturePlayer, 5, 5,
                                   GameWorld.GraphicsDevice.Viewport, 0.05f);

        
    }

    private void InitializeGameProcess()
    {
        GameWorld.Update += () => gameObjectCreator.Update();
        GameWorld.Update += () => GameWorld.player.Update();
        GameWorld.Update += () => Bullet.Update(GameWorld.Bullets);

        GameWorld.Draw += (gameTime) => GameWorld.background.RenderComp.Draw(GameWorld._spriteBatch, gameTime);
        GameWorld.Draw += (gameTime) => GameWorld.player.RenderComp.Draw(GameWorld._spriteBatch, gameTime);
        GameWorld.Draw += (gameTime) => gameObjectCreator.Draw(GameWorld._spriteBatch, gameTime);

        GameWorld.Draw += (gameTime) =>
        {
            foreach (var bullet in GameWorld.Bullets)
            {
                bullet.RenderComp.Draw(GameWorld._spriteBatch, gameTime);
            }
        };
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

public class PlatformTypeData : IGameObjectData
{
    public string Texture;
}

public class CollectibleTypeData : IGameObjectData
{
    public string Texture { get; set; }
    public string Type { get; set; }
    public float Value { get; set; }
}

public interface IGameObjectData;
