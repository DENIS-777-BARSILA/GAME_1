using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Components;
using MY_GAME_1;
using System.Collections.Generic;
using SharpDX.Direct2D1.Effects;


namespace MY_GAME_1;

public class PlatformCreator
{
    private readonly TileMap tileMap = GameWorld.TileMap;
    readonly Viewport Viewport;
    public List<Platform_1> Platforms { get; private set; }

    public readonly float scale;
    private readonly Dictionary<TileMap.MapsObject, Texture2D> platformTextures;


    public PlatformCreator(Viewport viewport, Dictionary<TileMap.MapsObject, Texture2D> platformTextures)
    {
        Viewport = viewport;
        Platforms = new List<Platform_1>();

        this.platformTextures = platformTextures;
    }

    public void MakePlatform_(int tileX, int tileY, TileMap.MapsObject platformType)
    {
        if (tileX < 0 || tileY < 0 ||
            tileX > GameWorld.TileMap.HorizontalTiles ||
            tileY > GameWorld.TileMap.VerticalTiles)
        {
            Console.WriteLine($"Недопустимые координаты платформы ({tileX}, {tileY})");
            return;
        }

        if (GameWorld.TileMap.TileData[tileX, tileY] != TileMap.MapsObject.Empty)
        {
            Console.WriteLine($"Клетка уже занята ({tileX}, {tileY})");
            return;
        }

        Vector2 position = GameWorld.TileMap.GetPosition(tileX, tileY);
        GameWorld.TileMap.TileData[tileX, tileY] = platformType;
        Console.WriteLine($"Created platform at ({tileX},{tileY}) with type ");
        InitializePlatform(position, platformType);
    }

    public void MakePlatform_(TileMap.MapsObject platformType, params Vector2[] TileCells)
    {
        foreach (var pos in TileCells)
        {
            MakePlatform_((int)pos.X, (int)pos.Y, platformType);
        }
    }

    public void InitializePlatformsFromTileMap()
    {
        for (int x = 0; x < GameWorld.TileMap.HorizontalTiles; x++)
        {
            for (int y = 0; y < GameWorld.TileMap.VerticalTiles; y++)
            {
                var tileType = GameWorld.TileMap.TileData[x, y];
                if (tileType != TileMap.MapsObject.Empty &&
                    platformTextures.ContainsKey(tileType))
                {
                    Vector2 position = GameWorld.TileMap.GetPosition(x, y);
                    InitializePlatform(position, tileType);
                }
            }
        }
    }

    private void InitializePlatform(Vector2 position, TileMap.MapsObject platformType)
    {
        if (!platformTextures.ContainsKey(platformType))
            platformType = TileMap.MapsObject.Platform_1;

        float scale = GameWorld.TileMap.CalculateScale(platformTextures[platformType]);
        Platform_1 newPlatform = new Platform_1(position, platformTextures[platformType], Viewport, scale);
        Platforms.Add(newPlatform);
        GameWorld.ColisionObjects.Add(newPlatform);
        Console.WriteLine($"Created platform at ({position.X},{position.Y}) with type {platformType}");
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        for (int i = Platforms.Count - 1; i >= 0; i--)
        {
            Platforms[i].Draw(spriteBatch, gameTime);
        }
    }
}

public class MonsterCreator
{
    readonly Viewport Viewport;
    public List<IGameObject> Monsters { get; private set; }

    readonly Texture2D Texture;
    public float Scale { get; private set; }
    public readonly float TileScale;



    public MonsterCreator(Texture2D texture, Viewport viewport, float scale)
    {
        Texture = texture;
        Viewport = viewport;
        Monsters = new List<IGameObject>();
        TileScale = GameWorld.TileMap.CalculateScale(Texture);
        Scale = scale;
    }


    public void MakeMonster(int tileX, int tileY)
    {
        if (tileX < 0 || tileY < 0 ||
        tileX > GameWorld.TileMap.HorizontalTiles ||
        tileY > GameWorld.TileMap.VerticalTiles)
        {
            Console.WriteLine($"Недопустимые координаты платформы ({tileX}, {tileY})");
            return;
        }

        if (GameWorld.TileMap.TileData[tileX, tileY] != TileMap.MapsObject.Empty)
        {
            Console.WriteLine($"Клетка уже занята ({tileX}, {tileY})");
            return;
        }

        Vector2 position = GameWorld.TileMap.GetPosition(tileX, tileY);

        InitializeMonster(position);
    }

    public void MakeMonster(params Vector2[] TileCells)
    {
        foreach (var pos in TileCells)
        {
            MakeMonster(GameWorld.TileMap.GetPosition((int)pos.X, (int)pos.Y));
        }
    }

    private void InitializeMonster(Vector2 position)
    {
        Monster_1 newMonster = new Monster_1(position, 100, 100, Texture,
       4, 0, Viewport,TileScale* Scale);
        Monsters.Add(newMonster);
        GameWorld.ColisionObjects.Add(newMonster);

    }

    public void Update()
    {
        for (int i = Monsters.Count - 1; i >= 0; i--)
        {
            Monsters[i].Update();
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        for (int i = Monsters.Count - 1; i >= 0; i--)
        {
            Monsters[i].Draw(spriteBatch, gameTime);
            Console.WriteLine($"Monster at {Monsters[i].PositionComp.Position}, scale: {Monsters[i].RenderComp.Scale}");
        }

    }
}