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


    public static Background game_background;
    public static Background menu_background;
    public static Player player;

    public static List<Bullet> Bullets = new List<Bullet>();
    public static List<IGameObject> ColisionObjects = new List<IGameObject>();
    public static List<ICollectible> CollectibleObjects = new List<ICollectible>();
}

