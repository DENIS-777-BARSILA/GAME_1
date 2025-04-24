using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MY_GAME_1;

namespace Components;

public enum Side
{
    None,
    Top,
    Bottom,
    Left,
    Right
}

public class ColisionData
{
    public readonly Side Side;
    public readonly int Penetration;
    public ColisionData(Side side, int penetration)
    {
        Side = side;
        Penetration = penetration;
    }

}


public class HealthComponent
{
    private int health;
    private int maxHealth;

    public int Health
    {
        get { return health; }
    }

    public int MaxHealth
    {
        get { return maxHealth; }
    }

    public HealthComponent(int mh, int h)
    {
        maxHealth = mh;
        health = h;
    }

    public void ChangeHealth(int difference)
    {
        SetHealth(health + difference);
    }

    public void SetHealth(int newHealth)
    {
        health = newHealth <= maxHealth ? newHealth : maxHealth;
        health = newHealth > 0 ? newHealth : 0;
    }

    public void ChangeMaxHealth(int difference)
    {
        SetHealth(maxHealth + difference);
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        if (health < maxHealth)
            health = maxHealth;
    }
}

public class RenderComponent
{
    public readonly Texture2D texture;
    public PositionComponent PositionComp;


    private int height;
    private int width;
    private float scale = 1.0f;
    public float Scale
    {
        get => scale;
        set
        {
            scale = value;
            Width = (int)(texture.Width * scale);
            Height = (int)(texture.Height * scale);
        }
    }

    public int Width { get; private set; }
    public int Height { get; private set; }

    public RenderComponent(Texture2D texture, PositionComponent positionComp, float scale)
    {
        this.texture = texture;
        PositionComp = positionComp;

        height = texture.Height;
        width = texture.Width;

        this.Scale = scale;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            texture,
            PositionComp.Position,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            0f);

    }
}

public class PositionComponent
{
    public Vector2 Position { get; set; }

    private RenderComponent RenderComp;
    public int Height { get { return RenderComp.Height; } }
    public int Width { get { return RenderComp.Width; } }

    public PositionComponent(Vector2 vector, RenderComponent renderComp)
    {
        Position = vector;
        RenderComp = renderComp;
    }

    public void Update(GameTime gameTime)
    {
    }

}

public class PhysicalComponent
{
    private PositionComponent PositionComp;
    private RenderComponent RenderComp;
    private List<IGameObject> GameObjects;
    private readonly Viewport Viewport;

    public static readonly float Gravity = 6;

    public PhysicalComponent(PositionComponent positionComp, RenderComponent renderComp)
    {
        PositionComp = positionComp;
        RenderComp = renderComp;
        GameObjects = GameWorld.GameObjects;
        Viewport = GameWorld.viewport;
    }

    public bool IsGrounded()
    {
        Rectangle feetCheck = new Rectangle(
           (int)PositionComp.Position.X,
           (int)PositionComp.Position.Y + RenderComp.Height,
           RenderComp.Width,
           10);

        return CheckCollision(feetCheck) || feetCheck.Bottom >= Viewport.Height;
    }

    public bool CheckCollision(Vector2 newPosition)
    {
        Rectangle newBounds = new Rectangle((int)newPosition.X, (int)newPosition.Y,
            RenderComp.Width, RenderComp.Height);

        return CheckCollision(newBounds);
    }

    public bool CheckCollision(Rectangle bounds)
    {
        return GetCollisionSide(bounds).Side != Side.None;
    }

    public ColisionData GetCollisionSide(Rectangle newBounds)
    {
        Vector2 direction = new Vector2(newBounds.X - PositionComp.Position.X,
                                       newBounds.Y - PositionComp.Position.Y);
        if (direction == Vector2.Zero)
            return new ColisionData(Side.None, 0);

        ColisionData objectCollision = GetObjectCollisions(newBounds, direction);

        ColisionData viewportCollision = GetViewportCollisions(newBounds, direction);

        return GetNearestCollision(objectCollision, viewportCollision);
    }

    public ColisionData GetObjectCollisions(Rectangle newBounds, Vector2 direction)
    {
        ColisionData nearestCollision = new ColisionData(Side.None, int.MaxValue);

        foreach (var obj in GameObjects)
        {
            if (obj == this) continue;

            Rectangle otherBounds = new Rectangle(
                (int)obj.PositionComp.Position.X,
                (int)obj.PositionComp.Position.Y,
                obj.RenderComp.Width,
                obj.RenderComp.Height);

            if (!newBounds.Intersects(otherBounds)) continue;

            int penetration = 0;
            Side currentSide = Side.None;

            if (Math.Abs(direction.X) > Math.Abs(direction.Y))
            {
                if (newBounds.Right > otherBounds.Left && direction.X > 0)
                {
                    penetration = otherBounds.Left - newBounds.Right;
                    currentSide = Side.Right;
                }
                else if (newBounds.Left < otherBounds.Right && direction.X < 0)
                {
                    penetration = newBounds.Left - otherBounds.Right;
                    currentSide = Side.Left;
                }
            }
            else
            {
                if (newBounds.Bottom > otherBounds.Top && direction.Y > 0)
                {
                    penetration = otherBounds.Top - newBounds.Bottom;
                    currentSide = Side.Bottom;
                }
                else if (newBounds.Top < otherBounds.Bottom && direction.Y < 0)
                {
                    penetration = otherBounds.Bottom - newBounds.Top;
                    currentSide = Side.Top;
                }
            }

            if (Math.Abs(penetration) < Math.Abs(nearestCollision.Penetration))
            {
                nearestCollision = new ColisionData(currentSide, penetration);
            }
        }

        return nearestCollision;
    }

    public ColisionData GetViewportCollisions(Rectangle newBounds, Vector2 direction)
    {
        ColisionData viewportCollision = new ColisionData(Side.None, int.MaxValue);

        if (Math.Abs(direction.X) > Math.Abs(direction.Y))
        {
            if (direction.X > 0 && newBounds.Right > Viewport.Width)
            {
                int penetration = Viewport.Width - newBounds.Right;
                viewportCollision = new ColisionData(Side.Right, penetration);
            }
            else if (direction.X < 0 && newBounds.Left < 0)
            {
                int penetration = newBounds.Left;
                viewportCollision = new ColisionData(Side.Left, penetration);
            }
        }
        else
        {
            if (direction.Y < 0 && newBounds.Top < 0)
            {
                int penetration = newBounds.Top;
                viewportCollision = new ColisionData(Side.Top, penetration);
            }
            else if (direction.Y > 0 && newBounds.Bottom > Viewport.Height)
            {
                int penetration = Viewport.Height - newBounds.Bottom;
                viewportCollision = new ColisionData(Side.Bottom, penetration);
            }
        }

        return viewportCollision;
    }

    private ColisionData GetNearestCollision(params ColisionData[] collisions)
    {
        ColisionData result = new ColisionData(Side.None, int.MaxValue);

        foreach (var collision in collisions)
        {
            if (collision.Side == Side.None) continue;

            if (Math.Abs(collision.Penetration) < Math.Abs(result.Penetration))
            {
                result = collision;
            }
        }

        return result.Side != Side.None ? result : new ColisionData(Side.None, 0);
    }



    public Side GetCollisionSideWithObjectBounds(Rectangle bounds, Rectangle otherBounds)
    {
        Rectangle currentBounds = new Rectangle(
            (int)PositionComp.Position.X,
            (int)PositionComp.Position.Y,
            RenderComp.Width,
            RenderComp.Height);

        Rectangle intersection = Rectangle.Intersect(currentBounds, otherBounds);

        if (intersection.Width > intersection.Height)
        {
            if (bounds.Top < otherBounds.Top)
                return Side.Bottom;
            else
                return Side.Top;
        }
        else if (intersection.Height > intersection.Width)
        {
            if (bounds.Left < otherBounds.Left)
                return Side.Right;
            else
                return Side.Left;
        }

        return Side.None;
    }

    public ColisionData GetCollisionSide(Vector2 newPosition)
    {
        Rectangle newBounds = new Rectangle((int)newPosition.X, (int)newPosition.Y,
            RenderComp.Width, RenderComp.Height);

        return GetCollisionSide(newBounds);
    }


    public bool Move(Vector2 newPosition)
    {
        ColisionData collisionData = GetCollisionSide(newPosition);

        if (collisionData.Side == Side.None)
        {
            PositionComp.Position = newPosition;
            return true;
        }

        Vector2 correctedPosition = newPosition;

        switch (collisionData.Side)
        {
            case Side.Top:
                correctedPosition.Y += collisionData.Penetration;
                correctedPosition.X = newPosition.X;
                break;

            case Side.Bottom:
                correctedPosition.Y += collisionData.Penetration;
                break;

            case Side.Left:
                correctedPosition.X -= collisionData.Penetration;
                correctedPosition.Y = newPosition.Y;
                break;

            case Side.Right:

                correctedPosition.X += collisionData.Penetration;
                correctedPosition.Y = newPosition.Y;
                break;
        }

        PositionComp.Position = correctedPosition;
        return false;
    }


    public void Update()
    {
        if (!IsGrounded())
        {
            Vector2 newPosition = PositionComp.Position + new Vector2(0, Gravity);
            Move(newPosition);
        }
    }

    public bool CheckVievportCollision(Vector2 Position, float indent)
    {
        return
        Position.X < 0 - indent ||
        Position.X > GameWorld.viewport.Width + indent ||
        Position.Y < 0 - indent ||
        Position.Y > GameWorld.viewport.Height + indent;
    }
}

public class MotionComponent
{
    public float SpeedX;

    public float SpeedY;
    public float JumpHeight = -195;
    private float JumpSpeed = 0f;
    private bool IsJumping;

    public readonly bool IsKeyboardOperation;

    private PositionComponent PositionComp;
    private PhysicalComponent PhysicalComp;

    public MotionComponent(float speedX, float speedY, PositionComponent positionComp, PhysicalComponent physicalComp, bool isKeyboardOperation)
    {
        SpeedX = speedX;
        SpeedY = speedY;
        PositionComp = positionComp;
        PhysicalComp = physicalComp;
        IsKeyboardOperation = isKeyboardOperation;
    }

    public void Update(KeyboardState keyboardState)
    {
        if (IsKeyboardOperation)
            KeybordMotion(keyboardState);

        if (IsJumping)
        {
            JumpSpeed += PhysicalComponent.Gravity;
            Vector2 newPosition = PositionComp.Position + new Vector2(0, JumpSpeed * 0.1f);
            PhysicalComp.Move(newPosition);

            if (PhysicalComp.IsGrounded())
            {
                IsJumping = false;
                JumpSpeed = 0f;
            }
        }
    }

    public void Update()
    {
        Vector2 newPosition = PositionComp.Position + new Vector2(SpeedX, SpeedY);
        Move(newPosition);
    }


    private void KeybordMotion(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Left))
        {
            Vector2 newPosition = PositionComp.Position - new Vector2(SpeedX, 0);
            PhysicalComp.Move(newPosition);
        }

        if (keyboardState.IsKeyDown(Keys.Right))
        {
            Vector2 newPosition = PositionComp.Position + new Vector2(SpeedX, 0);
            PhysicalComp.Move(newPosition);
        }

        if (keyboardState.IsKeyDown(Keys.Space))
            Jump();
    }

    private void Move(Vector2 newPosition)
    {
        PhysicalComp.Move(new Vector2(newPosition.X, 0));
        PhysicalComp.Move(new Vector2(0, newPosition.Y));
    }

    private void Jump()
    {
        if (!IsJumping && PhysicalComp.IsGrounded())
        {
            IsJumping = true;
            JumpSpeed = JumpHeight;
        }
    }

}

public class ShootingComponent
{
    private PositionComponent PositionComp;

    public readonly float Speed = 1;

    private float _lastShotTime;
    private const float ShotDelay = 0.5f;

    public ShootingComponent(PositionComponent positionComp, float speed)
    {
        PositionComp = positionComp;
        Speed = speed;
    }


    public void Update(MouseState mouseState)
    {

        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            float currentTime = (float)GameWorld.Level.GameTime.TotalGameTime.TotalSeconds;
            if (currentTime - _lastShotTime > ShotDelay)
            {
                MakeBullet(mouseState);
                _lastShotTime = currentTime;
            }

        }
    }

    public void MakeBullet(MouseState mouseState)
    {
        Vector2 target = new Vector2(mouseState.X, mouseState.Y);
        Vector2 gunPosition = new Vector2(PositionComp.Position.X + PositionComp.Width / 2, PositionComp.Position.Y + PositionComp.Height / 2);

        Vector2 direction = target - gunPosition;
        direction = Vector2.Normalize(direction);

        GameWorld.Bullets.Add(new Bullet(gunPosition, GameWorld.Level.TextureBullet, GameWorld.viewport, 0.1f,
         direction.X * Speed, direction.Y * Speed));

          Console.WriteLine($"Direction: {direction}, SpeedX: {direction.X * Speed}, SpeedY: {direction.Y * Speed}");
    }

}