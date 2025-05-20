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


public enum GameStates
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public static class GameState
{
    public static GameStates CurrentState { get; set; } = GameStates.GameOver;
    private static int currentLevel = 1;

    public static int CurrentLevel
    {
        get { return currentLevel; }
        set
        {
            if (currentLevel != value)
            {
                currentLevel = value;
                GameWorld.Level.SetLevelNumber(value);
            }
        }
    }

    public static void RestartGame()
    {
        CurrentLevel = 0;
        ResetGameState();
    }

    private static void ResetGameState()
    {
        GameWorld.Bullets.Clear();
        GameWorld.ColisionObjects.Clear();
        GameWorld.CollectibleObjects.Clear();

        GameWorld.Update = null;
        GameWorld.Draw = null;

        GameWorld.Level.Initialize();

        InterfaceObjects.InitializeMenus(() => Environment.Exit(0));
    }
}


public class MenuItem
{


    public string Text { get; set; }
    public Vector2 Position { get; set; }
    public Color Color { get; set; }
    public Action Action { get; set; }

    public MenuItem(string text, Vector2 position, Action action)
    {
        Text = text;
        Position = position;
        Color = Color.White;
        Action = action;
    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font, bool isSelected)
    {
        Color drawColor = isSelected ? Color.Yellow : Color.White;
        spriteBatch.DrawString(font, Text, Position, drawColor);
    }
}

public class Menu
{
    private readonly SpriteFont font;
    private readonly List<MenuItem> menuItems;
    private int selectedIndex;

    public Menu(SpriteFont font)
    {
        this.font = font;
        menuItems = new List<MenuItem>();
        selectedIndex = 0;
    }

    public void AddMenuItem(string text, Action action)
    {
        menuItems.Add(new MenuItem(text, Vector2.Zero, action));
        menuItems[^1].Position = CalculateMenuPosition(menuItems.Count - 1);
    }

    private Vector2 CalculateMenuPosition(int itemIndex)
    {
        float itemHeight = font.MeasureString(" ").Y * 1.5f;

        float startY = GameWorld.viewport.Height / 3;

        float x = (GameWorld.viewport.Width - font.MeasureString(menuItems[itemIndex].Text).X) / 2;

        float y = startY + itemIndex * itemHeight;

        return new Vector2(x, y);
    }

    public void Update(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Down) && selectedIndex < menuItems.Count - 1)
        {
            selectedIndex++;
        }
        else if (keyboardState.IsKeyDown(Keys.Up) && selectedIndex > 0)
        {
            selectedIndex--;
        }

        if (keyboardState.IsKeyDown(Keys.Enter))
        {
            menuItems[selectedIndex].Action?.Invoke();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            menuItems[i].Draw(spriteBatch, font, i == selectedIndex);
        }
    }
}