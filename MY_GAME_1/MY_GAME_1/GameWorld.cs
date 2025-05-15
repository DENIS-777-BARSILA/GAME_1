using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Components;
using MY_GAME_1;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.IO;



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

    public static Background background;
    public static Player player;

    public static List<Bullet> Bullets = new List<Bullet>();
    public static List<IGameObject> ColisionObjects = new List<IGameObject>();
    public static TileMap TileMap;

    public static Level Level;
}





public class TileMap
{

    public enum MapsObject
    {
        Empty,
        Platform_1,
        Platform_2,
        Platform_3
    }

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


    public MapsObject[,] TileData;
    private int tileSize;
    private float scale;

    public TileMap()
    {
        TileData = new MapsObject[HorizontalTiles, VerticalTiles];
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

    public void LoadFromTextFile(string filePath)
    {
        string[] lines = System.IO.File.ReadAllLines(filePath);

        if (lines.Length != VerticalTiles)
        {
            throw new Exception($"Ожидается {VerticalTiles} строк в файле карты");
        }

        for (int y = 0; y < VerticalTiles; y++)
        {
            string line = lines[y];
            if (line.Length != HorizontalTiles)
            {
                throw new Exception($"Строка {y} имеет неверную длину ({line.Length} вместо {HorizontalTiles})");
            }

            for (int x = 0; x < HorizontalTiles; x++)
            {
                TileData[x, y] = ParseTileChar(line[x]);
            }
        }
    }

    private MapsObject ParseTileChar(char c)
    {
        return c switch
        {
            '0' => MapsObject.Empty,
            '1' => MapsObject.Platform_1,
            '2' => MapsObject.Platform_2,
            '3' => MapsObject.Platform_3,
            'P' => MapsObject.Platform_1,

            _ => MapsObject.Empty
        };
    }

    public List<Vector2> GetListCells(MapsObject typeMapsObject)
    {
        List<Vector2> result = new List<Vector2>();

        for (int i = 0; i < HorizontalTiles; i++)
        {
            for (int j = 0; j < VerticalTiles; j++)
            {
                if (TileData[i, j] == typeMapsObject)
                    result.Add(new Vector2(i, j));

            }
        }

        return result;
    }
}