using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Components;
using MY_GAME_1;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.IO;
using SharpDX.MediaFoundation;



namespace MY_GAME_1;

public static class GameWorld
{
   


    public static GraphicsDevice GraphicsDevice;
    public static ContentManager Content;
    public static GameTime GameTime;
    public static SpriteBatch _spriteBatch;
    public static Viewport viewport;



    public static Action Update;
    public static Action<GameTime> Draw;


    public static TileMap TileMap;
    public static Level Level;


    public static Background background;
    public static Player player;

    public static List<Bullet> Bullets = new List<Bullet>();
    public static List<IGameObject> ColisionObjects = new List<IGameObject>();
    public static List<ICollectible> CollectibleObjects = new List<ICollectible>();
}

public class TileMap
{
    public float TileSize
    {
        set { }
        get
        {
            return GameWorld.viewport.Width / HorizontalTiles;
        }
    }

    public readonly int HorizontalTiles = 32;
    public readonly int VerticalTiles = 18;


    public IGameObjectData[,] TileData;
    private int tileSize;
    private float scale;

    public TileMap()
    {
        TileData = new IGameObjectData[HorizontalTiles, VerticalTiles];
    }

    public float CalculateScale(Texture2D texture)
    {
        float scaleX = TileSize / texture.Width;
        float scaleY = TileSize / texture.Height;

        return Math.Min(scaleX, scaleY);
    }

    public Vector2 GetPosition(int tileX, int tileY)
    {
        return TileSize * new Vector2(tileX, tileY);
    }

    public Vector2 GetPosition(TilePosition position)
    {
        return TileSize * new Vector2(position.TileX, position.TileY);
    }

    public bool IsEmpthyCell(int tileX, int tileY)
    {
        if (tileX < 0 || tileY < 0 ||
            tileX > GameWorld.TileMap.HorizontalTiles ||
            tileY > GameWorld.TileMap.VerticalTiles)
        {
            Console.WriteLine($"Недопустимые координаты платформы ({tileX}, {tileY})");
            return false;
        }

        if (GameWorld.TileMap.TileData[tileX, tileY] != null)
        {
            Console.WriteLine($"Клетка уже занята ({tileX}, {tileY})");
            return false;
        }

        return true;
    }

    public void LoadFromTextFile(string filePath, LevelData levelData)
    {
        TileMapObjectsInitializer.InitializeObjects(filePath, levelData, this);
    }
}

public static class TileMapObjectsInitializer
{
    public static void InitializeObjects(string filePath, LevelData levelData, TileMap tileMap)
    {
        string[] lines = System.IO.File.ReadAllLines(filePath);

        for (int y = 0; y < tileMap.VerticalTiles; y++)
        {
            string line = lines[y];
            if (line.Length != tileMap.HorizontalTiles)
            {
                throw new Exception($"Строка {y} имеет неверную длину ({line.Length} вместо {tileMap.HorizontalTiles})");
            }

            for (int x = 0; x < tileMap.HorizontalTiles; x++)
            {
                TileMapObjectsInitializer.InitializeTypesObj(x, y, levelData, line[x], tileMap);
            }
        }
    }


    private static void InitializeTypesObj(int tileX, int tileY, LevelData levelData, char type, TileMap tileMap)
    {
        if (levelData.MonsterTypes.ContainsKey(type))
            GameWorld.Level.gameObjectCreator.MakeMonster(tileX, tileY, levelData.MonsterTypes[type]);

        else if (levelData.PlatformTypes.ContainsKey(type))
            GameWorld.Level.gameObjectCreator.MakePlatform(tileX, tileY, levelData.PlatformTypes[type]);

        else if (levelData.CollectibleTypes.ContainsKey(type))
            GameWorld.Level.gameObjectCreator.MakeCollectible(tileX, tileY, levelData.CollectibleTypes[type]);

    }
}

