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


namespace MY_GAME_1;


public class Level
{
    public Texture2D BackgroundImage { get; private set; }
    public Texture2D TexturePlayer { get; private set; }

    public Dictionary<TileMap.MapsObject, Texture2D> TexturesPlatforms { get; private set; }
    public Texture2D TextureBullet { get; private set; }
    public Texture2D TextureTest { get; private set; }

    public Texture2D TextureMonster { get; private set; }

    public PlatformCreator platformCreator { get; private set; }
    public MonsterCreator monsterCreator { get; private set; }

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
        TextureMonster = GameWorld.Content.Load<Texture2D>(levelData.MonsterTexture);

        TexturesPlatforms = new Dictionary<TileMap.MapsObject, Texture2D>();
        foreach (var kvp in levelData.PlatformTextures)
        {
            var platformType = (TileMap.MapsObject)int.Parse(kvp.Key);
            TexturesPlatforms[platformType] = GameWorld.Content.Load<Texture2D>(kvp.Value);
        }
    }

    public void Initialize()
    {
        GameWorld.TileMap = new TileMap();

        platformCreator = new PlatformCreator(GameWorld.GraphicsDevice.Viewport, TexturesPlatforms);
        monsterCreator = new MonsterCreator(TextureMonster, GameWorld.GraphicsDevice.Viewport, 10f);

        var playerPosition = GameWorld.TileMap.GetPosition(levelData.PlayerStartPosition);
        GameWorld.player = new Player(playerPosition, 100, 100, TexturePlayer, 5, 5,
                                   GameWorld.GraphicsDevice.Viewport, 0.1f);


        foreach (var monsterPos in levelData.Monsters)
            monsterCreator.MakeMonster(monsterPos.TileX, monsterPos.TileY);

        GameWorld.background = new Background(BackgroundImage, 2);



        string mapPath = Path.Combine(GameWorld.Content.RootDirectory, "Levels", levelData.TileMapFile);
        GameWorld.TileMap.LoadFromTextFile(mapPath);



        platformCreator.InitializePlatformsFromTileMap();

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

public class LevelData
{
    public string BackgroundTexture { get; set; }
    public string PlayerTexture { get; set; }
    public string BulletTexture { get; set; }
    public string MonsterTexture { get; set; }
    public Dictionary<string, string> PlatformTextures { get; set; }

    public List<TilePosition> Monsters { get; set; }

    public TilePosition PlayerStartPosition;

    public string TileMapFile { get; set; }



}

public class TilePosition
{
    public int TileX { get; set; }
    public int TileY { get; set; }
}