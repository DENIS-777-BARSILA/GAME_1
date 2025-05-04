using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Components;
using System.Collections.Generic;

namespace MY_GAME_1;


public class Background
{
    public readonly PositionComponent PositionComp;
    public readonly RenderComponent RenderComp;

    public Background(Texture2D bacgroundImage, float scale)
    {
        RenderComp = new RenderComponent(bacgroundImage, null, scale);
        PositionComp = new PositionComponent(Vector2.Zero, RenderComp);
        RenderComp.PositionComp = PositionComp;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        RenderComp.Draw(spriteBatch, gameTime);
    }
}


public class Player : IGameObject
{
    public PositionComponent PositionComp { get; } = null;
    public HealthComponent HealthComp { get; }
    public RenderComponent RenderComp { get; } = null;
    public MotionComponent MotionComp { get; }
    public PhysicalComponent PhysicalComp { get; }
    public ShootingComponent ShootingComp { get; }

    public Player(Vector2 position, int health, int maxHealth, Texture2D texture, float speedX, float speedY, Viewport viewport, float scale)
    {
        RenderComp = new RenderComponent(texture, null, scale, 1, 1, 0.1f);


        PositionComp = new PositionComponent(position, RenderComp);



        HealthComp = new HealthComponent(maxHealth, health);
        PhysicalComp = new PhysicalComponent(PositionComp, RenderComp);
        MotionComp = new MotionComponent(speedX, speedY, PositionComp, PhysicalComp, true);

        ShootingComp = new ShootingComponent(PositionComp, RenderComp, 50);

        RenderComp.PositionComp = PositionComp;
        RenderComp.MotionComp = MotionComp;
    }

    public void Update()
    {
        MotionComp.Update(Keyboard.GetState());
        PhysicalComp.Update();
        ShootingComp.Update(Mouse.GetState());
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        RenderComp.Draw(spriteBatch, gameTime);
    }
}

public class Monster_1 : IGameObject
{
    public PositionComponent PositionComp { get; } = null;
    public HealthComponent HealthComp { get; }
    public RenderComponent RenderComp { get; } = null;
    public MotionComponent MotionComp { get; }
    public PhysicalComponent PhysicalComp { get; }

    public Monster_1(Vector2 position, int health, int maxHealth, Texture2D texture, float speedX, float speedY, Viewport viewport, float scale)
    {
        RenderComp = new RenderComponent(texture, null, scale, 5, 2, 0.1f); //, 


        PositionComp = new PositionComponent(position, RenderComp);



        HealthComp = new HealthComponent(maxHealth, health);
        PhysicalComp = new PhysicalComponent(PositionComp, RenderComp);
        MotionComp = new MotionComponent(speedX, speedY, PositionComp, PhysicalComp, false);


        RenderComp.PositionComp = PositionComp;
        RenderComp.MotionComp = MotionComp;
    }

    public void Update()
    {
        Console.WriteLine($"{PositionComp.Position.X}  {PositionComp.Position.Y}");
        MotionComp.Update(new Vector2(1, 0), true);
        PhysicalComp.Update();

    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        RenderComp.Draw(spriteBatch, gameTime);
    }


}


public class Platform_1 : IGameObject
{
    public PositionComponent PositionComp { get; } = null;
    public RenderComponent RenderComp { get; } = null;
    public PhysicalComponent PhysicalComp { get; }

    public Platform_1(Vector2 position, Texture2D texture, Viewport viewport, float scale)
    {
        RenderComp = new RenderComponent(texture, null, scale);
        PositionComp = new PositionComponent(position, RenderComp);
        RenderComp.PositionComp = PositionComp;
        PhysicalComp = new PhysicalComponent(PositionComp, RenderComp);
    }

    public void Update()
    {

    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        RenderComp.Draw(spriteBatch, gameTime);
    }
}


public class Bullet : IGameObject
{
    public PositionComponent PositionComp { get; } = null;
    public RenderComponent RenderComp { get; } = null;
    public PhysicalComponent PhysicalComp { get; }
    public MotionComponent MotionComp { get; }


    readonly Vector2 Direction;
    float SpeedX;
    float SpeedY;

    public Bullet(Vector2 position, Texture2D texture, Viewport viewport, float scale, float speedX, float speedY)
    {
        RenderComp = new RenderComponent(texture, null, scale);
        PositionComp = new PositionComponent(position, RenderComp);
        RenderComp.PositionComp = PositionComp;
        PhysicalComp = new PhysicalComponent(PositionComp, RenderComp);
        MotionComp = new MotionComponent(speedX, speedY, PositionComp, PhysicalComp, false);
        SpeedX = speedX;
        SpeedY = speedY;


        Direction = new Vector2(speedX, speedY);
        if (Direction != Vector2.Zero) Direction.Normalize();
        RenderComp.Rotation = (float)Math.Atan2(Direction.Y, Direction.X);
    }

    public void Update()
    {
        Vector2 newPosition = PositionComp.Position + new Vector2(SpeedX, SpeedY);
        Rectangle newBounds = new Rectangle((int)newPosition.X, (int)newPosition.Y,
            RenderComp.Width, RenderComp.Height);

        if (PhysicalComp.GetObjectCollisions(newBounds, new Vector2(SpeedX, SpeedY)).Side != Side.None)
            GameWorld.Bullets.Remove(this);

        if (PhysicalComp.CheckVievportCollision(PositionComp.Position, 50))
            GameWorld.Bullets.Remove(this);

        PositionComp.Position = newPosition;
    }

    public static void Update(List<Bullet> bullets)
    {
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            bullets[i].Update();
        }
    }


    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        RenderComp.Draw(spriteBatch, gameTime);
    }
}




public interface IGameObject
{
    PositionComponent PositionComp { get; }
    RenderComponent RenderComp { get; }
    PhysicalComponent PhysicalComp { get; }

    void Update();
    void Draw(SpriteBatch spriteBatch, GameTime gameTime);
}




