
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Components;
using System;


namespace MY_GAME_1;


public static class InterfaceObjects
{
    public static Menu MainMenu { get; set; }
    public static Menu PauseMenu { get; set; }
    public static Menu GameOverMenu { get; set; }




    public static HealthBar PlayerHealthBar;
    public static AmmoCounter AmmoCounter;
    public static SpriteFont MenuFont;
    public static SpriteFont Font { get; private set; }
    public static void InitializeInterface()
    {
        HealthBar playerHealthBar = new HealthBar(new Vector2(20, 20),
            200, height: 20, healthComp: GameWorld.player.HealthComp);
        InterfaceObjects.PlayerHealthBar = playerHealthBar;

        AmmoCounter = new AmmoCounter(Font, GameWorld.player, GameWorld.GraphicsDevice.Viewport);

        GameWorld.Draw += (gameTime) => playerHealthBar.Draw(GameWorld._spriteBatch, gameTime);
        GameWorld.Draw += (gameTime) => AmmoCounter.Draw(GameWorld._spriteBatch, gameTime);
    }


    public static void InitializeMenus(Action exitAction)
    {
        InitializeMainMenu(exitAction);
        InitializePauseMenu();
        InitializeGameOverMenu();
    }

    private static void InitializeMainMenu(Action exitAction)
    {
        MainMenu = new Menu(MenuFont);
        MainMenu.AddMenuItem("Start Game", (Action)(() =>
        {
            GameState.CurrentState = GameStates.Playing;
        }));
        MainMenu.AddMenuItem("Exit", exitAction);
    }

    public static void InitializePauseMenu()
    {
        PauseMenu = new Menu(MenuFont);
        PauseMenu.AddMenuItem("Resume", (Action)(() =>
        {
            GameState.CurrentState = GameStates.Playing;
        }));
        PauseMenu.AddMenuItem("Exit to Menu", (Action)(() =>
        {
            GameWorld.Level.InitializeNewGame();
            GameState.CurrentState = GameStates.MainMenu;

            System.Threading.Thread.Sleep(100);
        }));
    }

    public static void InitializeGameOverMenu()
    {
        GameOverMenu = new Menu(MenuFont);
        GameOverMenu.AddMenuItem("New Game", (Action)(() =>
        {
            GameWorld.Level.InitializeNewGame();
            GameState.CurrentState = GameStates.Playing;
        }));
        GameOverMenu.AddMenuItem("Exit to Menu", (Action)(() =>
        {
            GameWorld.Level.InitializeNewGame();
            GameState.CurrentState = GameStates.MainMenu;

            System.Threading.Thread.Sleep(100);
        }));
    }

    

    public static void LoadContent()
    {
        Font = GameWorld.Content.Load<SpriteFont>("font_1");
        MenuFont = GameWorld.Content.Load<SpriteFont>("font_menu");
    }

}

public class HealthBar
{
    private Texture2D backgroundTexture;
    private Texture2D healthTexture;
    private Vector2 position;
    private int width;
    private int height;
    private HealthComponent healthComp;

    public HealthBar(Vector2 position, int width, int height,
                   HealthComponent healthComp)
    {
        this.position = position;
        this.width = width;
        this.height = height;
        this.healthComp = healthComp;

        backgroundTexture = new Texture2D(GameWorld.GraphicsDevice, 1, 1);
        healthTexture = new Texture2D(GameWorld.GraphicsDevice, 1, 1);
        backgroundTexture.SetData(new[] { Color.Gray });

        healthTexture.SetData(new[] { Color.Red });
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(backgroundTexture,
            new Rectangle((int)position.X, (int)position.Y, width, height),
            Color.White);

        float healthPercentage = (float)healthComp.Health / healthComp.MaxHealth;
        int currentWidth = (int)(width * healthPercentage);

        spriteBatch.Draw(healthTexture,
            new Rectangle((int)position.X, (int)position.Y, currentWidth, height),
            Color.White);
    }
}

public class AmmoCounter
{
    private SpriteFont font;
    private Vector2 position;
    private Player player;

    public AmmoCounter(SpriteFont font, Player player, Viewport viewport)
    {
        this.font = font;
        this.player = player;
        position = new Vector2(viewport.Width - 150, 20);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        string ammoText = $"Ammo: {player.AmmoCount}";
        Vector2 textSize = font.MeasureString(ammoText);

        Texture2D pixelTexture = new Texture2D(GameWorld.GraphicsDevice, 1, 1);
        pixelTexture.SetData(new[] { Color.Yellow });

        spriteBatch.Draw(
            pixelTexture,
            new Rectangle(
                (int)position.X - 5,
                (int)position.Y - 2,
                (int)textSize.X + 10,
                (int)textSize.Y + 4),
            new Color(0, 0, 0, 128));

        spriteBatch.DrawString(
            font,
            ammoText,
            position,
            Color.Yellow);
    }
}