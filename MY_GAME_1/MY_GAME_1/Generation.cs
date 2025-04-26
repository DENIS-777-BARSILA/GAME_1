using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Components;
using MY_GAME_1;
using System.Collections.Generic;


namespace MY_GAME_1;

public class PlatfotmCreator
{
    readonly Texture2D Texture;
    readonly Viewport Viewport;
    public List<Platform_1> Platforms { get; private set; }

    public PlatfotmCreator(Texture2D texture, Viewport viewport)
    {
        Texture = texture;
        Viewport = viewport;
        Platforms = new List<Platform_1>();
    }

    public void MakePlatform_(Vector2 position, float scale)
    {
        Platform_1 newPlatform = new Platform_1(position, Texture, Viewport, scale);
        Platforms.Add(newPlatform);
    }

    public void MakePlatform_(float scale, params Vector2[] positions)
    {
        foreach (var pos in positions)
        {
            Platform_1 newPlatform = new Platform_1(pos, Texture, Viewport, scale);
            Platforms.Add(newPlatform);
        }
    }
}

public class MonsterCreator
{
    readonly Texture2D Texture;
    readonly Viewport Viewport;
    public List<IGameObject> Monsters { get; private set; }

    public MonsterCreator(Texture2D texture, Viewport viewport)
    {
        Texture = texture;
        Viewport = viewport;
        Monsters = new List<IGameObject>();
    }

    public void MakeMonster(Vector2 position, float scale)
    {
        Monster_1 newMonster = new Monster_1(position, 100, 100, Texture,
        20, 20, Viewport, scale);
        Monsters.Add(newMonster);

       // GameWorld.GameObjects.Add(newMonster);
    }

    public void MakeMonster(float scale, params Vector2[] positions)
    {
        foreach (var pos in positions)
        {
            Monster_1 newMonster = new Monster_1(pos, 100, 100, Texture,
        20, 20, Viewport, scale);
            Monsters.Add(newMonster);

            GameWorld.GameObjects.Add(newMonster);
        }
    }
}