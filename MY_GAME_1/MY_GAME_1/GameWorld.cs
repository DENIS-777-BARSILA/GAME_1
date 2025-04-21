using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Components;
using MY_GAME_1;
using System.Collections.Generic;


namespace MY_GAME_1;

public static class GameWorld
{
    public static Player player;
    public static Platform_1 platform;

    public static Background background;
    public static List<IGameObject> GameObjects = new List<IGameObject>();


}
