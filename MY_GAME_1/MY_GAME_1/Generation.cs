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

    private readonly Dictionary<PlatformTypeData, Texture2D> platformTextures;


    public PlatformCreator(Viewport viewport, Dictionary<PlatformTypeData, Texture2D> _platformTextures)
    {
        Viewport = viewport;
        Platforms = new List<Platform_1>();

        this.platformTextures = _platformTextures;
    }

    public void MakePlatform(int tileX, int tileY, PlatformTypeData platformData)
    {
        if (!tileMap.IsEmpthyCell(tileX, tileY)) return;

        Vector2 position = GameWorld.TileMap.GetPosition(tileX, tileY);
        tileMap.TileData[tileX, tileY] = platformData;
        InitializePlatform(position, platformData);
    }

    private void InitializePlatform(Vector2 position, PlatformTypeData platformData)
    {
        float tileScale = tileMap.CalculateScale(platformTextures[platformData]);

        Platform_1 newPlatform = new Platform_1(
            position,
            platformTextures[platformData],
            Viewport,
            tileScale
            );

        Platforms.Add(newPlatform);
        GameWorld.ColisionObjects.Add(newPlatform);

        Console.WriteLine($"Created platform at ({position.X},{position.Y}");
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
    private readonly TileMap tileMap = GameWorld.TileMap;
    public List<IGameObject> Monsters { get; private set; }
    private readonly Dictionary<MonsterTypeData, Texture2D> monsterTextures;

    public MonsterCreator(Viewport viewport, Dictionary<MonsterTypeData, Texture2D> _monsterTextures)
    {
        Viewport = viewport;
        Monsters = new List<IGameObject>();

        monsterTextures = _monsterTextures;
    }

    public void MakeMonster(int tileX, int tileY, MonsterTypeData monsterData)
    {
        if (!tileMap.IsEmpthyCell(tileX, tileY)) return;

        Vector2 position = tileMap.GetPosition(tileX, tileY);

        InitializeMonster(position, monsterData);
    }

    private void InitializeMonster(Vector2 position, MonsterTypeData monsterData)
    {
        float tileScale = tileMap.CalculateScale(monsterTextures[monsterData]);

        Monster_1 newMonster = new Monster_1(
            position,
            monsterData.Health,
            monsterTextures[monsterData],
             monsterData.SpeedX,
             monsterData.SpeedY,
             Viewport,
             monsterData.Scale * tileScale,
             monsterData.algorithmMovement);
        Monsters.Add(newMonster);
        GameWorld.ColisionObjects.Add(newMonster);
    }

    public void Remove(Monster_1 monster)
    {
        GameWorld.Level.monsterCreator.Monsters.Remove(monster);
        GameWorld.ColisionObjects.Remove(monster);
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

public class CollectebleCreator
{
    readonly Viewport Viewport;
    private readonly TileMap tileMap = GameWorld.TileMap;
    public List<IGameObject> CollectibleObjs { get; private set; }
    private readonly Dictionary<CollectibleTypeData, Texture2D> collectebleTextures;

    public CollectebleCreator(Viewport viewport, Dictionary<CollectibleTypeData, Texture2D> _collectebleTextures)
    {
        Viewport = viewport;
        CollectibleObjs = new List<IGameObject>();

        collectebleTextures = _collectebleTextures;
    }

    public void MakeCollecteble(int tileX, int tileY, CollectibleTypeData collectebleData)
    {
        if (!tileMap.IsEmpthyCell(tileX, tileY)) return;

        Vector2 position = tileMap.GetPosition(tileX, tileY);

        InitializeCollecteble(position, collectebleData);
    }

    private void InitializeCollecteble(Vector2 position, CollectibleTypeData collectebleData)
    {
        float tileScale = tileMap.CalculateScale(collectebleTextures[collectebleData]);
        ICollectible newIcollectible = CollectibleFactory.Create(collectebleData, position, collectebleTextures[collectebleData], tileMap);

        CollectibleObjs.Add(newIcollectible);
        GameWorld.CollectibleObjects.Add(newIcollectible);
    }

    public void Remove(ICollectible CollectibleObj)
    {
        CollectibleObjs.Remove(CollectibleObj);
        GameWorld.CollectibleObjects.Remove(CollectibleObj);
    }

    public void Update()
    {
        for (int i = CollectibleObjs.Count - 1; i >= 0; i--)
        {
            CollectibleObjs[i].Update();
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        for (int i = CollectibleObjs.Count - 1; i >= 0; i--)
        {
            CollectibleObjs[i].Draw(spriteBatch, gameTime);
            Console.WriteLine($"Monster at {CollectibleObjs[i].PositionComp.Position}, scale: {CollectibleObjs[i].RenderComp.Scale}");
        }

    }
}



public static class CollectibleFactory
    {
        public static ICollectible Create(CollectibleTypeData data, Vector2 position, Texture2D texture, TileMap tileMap)
        {
            float tileScale = tileMap.CalculateScale(texture);

            return data.Type switch
            {
                "MedKit" => new MedKit(position, texture, tileScale),
                "Ammo" => new Ammo(position, texture, tileScale)
            };
            
        }
    }