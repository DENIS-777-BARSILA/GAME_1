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


public enum TileObjects
{
    Empty,
    Platform
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


    public TileObjects[,] TileData;
    private int tileSize;
    private float scale;

    public TileMap()
    {
        TileData = new TileObjects[HorizontalTiles, VerticalTiles];
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
        return TileSize * new Vector2(position.X, position.Y);
    }

    public TilePosition GetTilePosition(Vector2 position)
    {
        int tileX = (int)(position.X / TileSize);
        int tileY = (int)(position.Y / TileSize);
        var _position = new TilePosition { X = tileX, Y = tileY };

        if (!InBounds(_position))
            Console.WriteLine($"{tileX} {tileY}  out of bound");

        return _position;
    }

    public TilePosition GetTilePosition(Vector2 position, float Width, float Height)
    {
        var centerPos = position + new Vector2(Width / 2, Height / 2);

        return GetTilePosition(centerPos);
    }


    public bool IsEmpthyCell(int tileX, int tileY)
    {
        if (!InBounds(new TilePosition { X = tileX, Y = tileY }))
        {
            Console.WriteLine($"{tileX} {tileY}  out of bound");
            return false;
        }

        return GameWorld.TileMap.TileData[tileX, tileY] == TileObjects.Empty;
    }

    public bool IsEmpthyCell(TilePosition position)
    {
        return IsEmpthyCell(position.X, position.Y);
    }

    public bool InBounds(TilePosition position)
    {
        return position.X >= 0 && position.X < HorizontalTiles
            && position.Y >= 0 && position.Y < VerticalTiles;
    }

    Vector2 GetTileCenter(int tileX, int tileY)
    {
        return new Vector2(tileX * TileSize + TileSize / 2,
                          tileY * TileSize + TileSize / 2);
    }

    bool IsPointInTile(Vector2 point, int tileX, int tileY)
    {
        return point.X >= tileX * TileSize &&
               point.X < (tileX + 1) * TileSize &&
               point.Y >= tileY * TileSize &&
               point.Y < (tileY + 1) * TileSize;
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
        var currentTileData = new TileObjects[tileMap.HorizontalTiles, tileMap.VerticalTiles];


        string[] lines = System.IO.File.ReadAllLines(filePath);


        for (int y = 0; y < tileMap.VerticalTiles; y++)
        {
            string line = lines[y];
            if (line.Length != tileMap.HorizontalTiles)
            {
                throw new Exception($"line {y} is not valid lenght: ({line.Length} expected: {tileMap.HorizontalTiles})");
            }

            for (int x = 0; x < tileMap.HorizontalTiles; x++)
            {
                InitializeTypesObj(x, y, levelData, line[x], tileMap, currentTileData);
            }
        }

        tileMap.TileData = currentTileData;
    }


    private static void InitializeTypesObj(int tileX, int tileY, LevelData levelData, char type, TileMap tileMap, TileObjects[,] currentTileData)
    {

        if (levelData.MonsterTypes.ContainsKey(type))
        {
            GameWorld.Level.gameObjectCreator.MakeMonster(tileX, tileY, levelData.MonsterTypes[type]);

        }

        else if (levelData.PlatformTypes.ContainsKey(type))
        {
            GameWorld.Level.gameObjectCreator.MakePlatform(tileX, tileY, levelData.PlatformTypes[type]);
            currentTileData[tileX, tileY] = TileObjects.Platform;
        }

        else if (levelData.CollectibleTypes.ContainsKey(type))
        {
            GameWorld.Level.gameObjectCreator.MakeCollectible(tileX, tileY, levelData.CollectibleTypes[type]);
        }

        else
        {
            currentTileData[tileX, tileY] = TileObjects.Empty;
        }
    }
}