using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Components;
using MY_GAME_1;
using System.Collections.Generic;
using SharpDX.Direct2D1.Effects;


namespace MY_GAME_1;

public class GameObjectCreator
{
    private readonly TileMap _tileMap;
    private readonly IPlatformFactory _platformFactory;
    private readonly IMonsterFactory _monsterFactory;
    private readonly ICollectibleFactory _collectibleFactory;

    public List<Platform_1> Platforms { get; } = new();
    public List<Monster_1> Monsters { get; } = new();
    public List<ICollectible> Collectibles { get; } = new();

    public GameObjectCreator
    (
        TileMap tileMap,
        IPlatformFactory platformFactory,
        IMonsterFactory monsterFactory,
        ICollectibleFactory collectibleFactory)
    {
        _tileMap = tileMap;
        _platformFactory = platformFactory;
        _monsterFactory = monsterFactory;
        _collectibleFactory = collectibleFactory;
    }

    public void MakeGameObject(int tileX, int tileY, IGameObjectData data)
    {
        if (!_tileMap.IsEmpthyCell(tileX, tileY)) return;

        var position = _tileMap.GetPosition(tileX, tileY);

        switch (data)
        {
            case PlatformTypeData platformData:
                MakePlatform(tileX, tileY, platformData);
                break;

            case MonsterTypeData monsterData:
                MakeMonster(tileX, tileY, monsterData);
                break;

            case CollectibleTypeData collectibleData:
                MakeCollectible(tileX, tileY, collectibleData);
                break;
        }
    }


    public void MakePlatform(int tileX, int tileY, PlatformTypeData data)
    {
        if (!_tileMap.IsEmpthyCell(tileX, tileY)) return;

        var position = _tileMap.GetPosition(tileX, tileY);
        var platform = _platformFactory.Create(position, data);

        Platforms.Add(platform);
        GameWorld.ColisionObjects.Add(platform);
    }

    public void MakeMonster(int tileX, int tileY, MonsterTypeData data)
    {
        if (!_tileMap.IsEmpthyCell(tileX, tileY)) return;

        var position = _tileMap.GetPosition(tileX, tileY);
        var monster = _monsterFactory.Create(position, data);

        Monsters.Add(monster);
    }

    public void MakeCollectible(int tileX, int tileY, CollectibleTypeData data)
    {
        if (!_tileMap.IsEmpthyCell(tileX, tileY)) return;

        var position = _tileMap.GetPosition(tileX, tileY);
        var collectible = _collectibleFactory.Create(position, data);

        Collectibles.Add(collectible);
        GameWorld.CollectibleObjects.Add(collectible);
    }

    public void Remove(IGameObject obj)
    {
    switch (obj)
        {
            case Platform_1 platform:
                if (Platforms.Contains(platform))
                {
                    Platforms.Remove(platform);
                    GameWorld.ColisionObjects.Remove(platform);
                }
                break;

            case Monster_1 monster:
                if (Monsters.Contains(monster))
                {
                    Monsters.Remove(monster);
                    GameWorld.ColisionObjects.Remove(monster);
                }
                break;

            case ICollectible collectible:
                if (Collectibles.Contains(collectible))
                {
                    Collectibles.Remove(collectible);
                    GameWorld.CollectibleObjects.Remove(collectible);
                }
                break;
        }
    }

    public void Update()
    {
        var monstersCopy = new List<Monster_1>(Monsters);
        foreach (var monster in monstersCopy)
        {
            if (Monsters.Contains(monster))
                monster.Update();
        }

        var collectiblesCopy = new List<ICollectible>(Collectibles);
        foreach (var collectible in collectiblesCopy)
        {
            if (Collectibles.Contains(collectible))
                collectible.Update();
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        foreach (var platform in Platforms) platform.Draw(spriteBatch, gameTime);
        foreach (var monster in Monsters) monster.Draw(spriteBatch, gameTime);
        foreach (var collectible in Collectibles) collectible.Draw(spriteBatch, gameTime);
    }
}

public interface IGameObjectFactory<TData, TObject> where TObject : IGameObject
{
    TObject Create(Vector2 position, TData data);
}

public interface IPlatformFactory : IGameObjectFactory<PlatformTypeData, Platform_1> { }
public interface IMonsterFactory : IGameObjectFactory<MonsterTypeData, Monster_1> { }
public interface ICollectibleFactory : IGameObjectFactory<CollectibleTypeData, ICollectible> { }


public class PlatformFactory : IPlatformFactory
{
    private readonly Viewport _viewport;
    private readonly Dictionary<PlatformTypeData, Texture2D> _textures;

    public PlatformFactory(Viewport viewport, Dictionary<PlatformTypeData, Texture2D> textures)
    {
        _viewport = viewport;
        _textures = textures;
    }

    public Platform_1 Create(Vector2 position, PlatformTypeData data)
    {
        var texture = _textures[data];
        float scale = GameWorld.TileMap.CalculateScale(texture);
        return new Platform_1(position, texture, _viewport, scale);
    }
}

public class MonsterFactory : IMonsterFactory
{
    private readonly Viewport _viewport;
    private readonly Dictionary<MonsterTypeData, Texture2D> _textures;

    public MonsterFactory(Viewport viewport, Dictionary<MonsterTypeData, Texture2D> textures)
    {
        _viewport = viewport;
        _textures = textures;
    }

    public Monster_1 Create(Vector2 position, MonsterTypeData data)
    {
        var texture = _textures[data];
        float tileScale = GameWorld.TileMap.CalculateScale(texture);
        return new Monster_1(
            position,
            data.Health,
            texture,
            data.SpeedX,
            data.SpeedY,
            _viewport,
            data.Scale * tileScale,
            data.algorithmMovement);
    }
}

public class CollectibleFactory : ICollectibleFactory
{
    private readonly Viewport _viewport;
    private readonly Dictionary<CollectibleTypeData, Texture2D> _textures;

    public CollectibleFactory(Viewport viewport, Dictionary<CollectibleTypeData, Texture2D> textures)
    {
        _viewport = viewport;
        _textures = textures;
    }

    public ICollectible Create(Vector2 position, CollectibleTypeData data)
    {
        var texture = _textures[data];
        float scale = GameWorld.TileMap.CalculateScale(texture);

        return data.Type switch
        {
            "MedKit" => new MedKit(position, texture, scale),
            "Ammo" => new Ammo(position, texture, scale),
            "Door" => new ExitDoor(position, texture, scale),
            _ => throw new ArgumentException($"Unknown collectible type: {data.Type}")
        };
    }
}