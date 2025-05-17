using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MY_GAME_1;
using SharpDX.Direct2D1.Effects;

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

    public void CheckDamageFromBullet(IGameObject gameObject)
    {
        for (int i = GameWorld.Bullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = GameWorld.Bullets[i];
            if (PhysicalComponent.CheckColisionBetweenObjects(gameObject, bullet))
            {
                ChangeHealth(-(int)Bullet.Damage);
                GameWorld.Bullets.RemoveAt(i);
            }
        }
    }

    public void Update(IGameObject gameObject)
    {
        CheckDamageFromBullet(gameObject);

        if (health > 0)
            return;

        if (gameObject is Monster_1)
            GameWorld.Level.monsterCreator.Remove((Monster_1)gameObject);
    }
}

public class RenderComponent
{
    public readonly Texture2D texture;
    public PositionComponent PositionComp;
    public MotionComponent MotionComp;

    public float Rotation { get; set; }
    public float Scale = 1;


    public int Width
    {
        get => (int)(frameWidth * Scale);
    }

    public int Height
    {
        get => (int)(frameHeight * Scale);
    }

    public SpriteEffects SpriteEffect { get; private set; } = SpriteEffects.None;

    private bool isAnimation;
    private readonly int frameWidth;
    private readonly int frameHeight;
    private int countFrameX = 1;
    private int countFrameY = 1;
    private int frameX = 0;
    private int frameY = 0;

    private float timeSinceLastFrame = 0f;
    private float frameTime = 0.1f;
    public float FrameTime
    {
        get => frameTime;
        set => frameTime = value;
    }
    private Rectangle currentFrame;

    public RenderComponent(Texture2D texture, PositionComponent positionComp, float scale)  //without animation
    {
        this.texture = texture;
        PositionComp = positionComp;

        this.Scale = scale;

        isAnimation = false;
        frameWidth = texture.Width;
        frameHeight = texture.Height;
        currentFrame = new Rectangle(0, 0, texture.Width, texture.Height);
    }

    public RenderComponent(Texture2D texture, PositionComponent positionComp,
     float scale, int countFrameX, int countFrameY, float frameTime) //with animation 
    {
        this.texture = texture;

        PositionComp = positionComp;

        this.Scale = scale;

        isAnimation = true;
        this.countFrameX = countFrameX;
        this.countFrameY = countFrameY;

        frameWidth = texture.Width / countFrameX;
        frameHeight = texture.Height / countFrameY;

        currentFrame = new Rectangle(0, 0, frameWidth, frameHeight);
        this.frameTime = frameTime;
    }

    private void UpdateCurrentFrame(GameTime gameTime)
    {
        timeSinceLastFrame += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (timeSinceLastFrame >= frameTime && MotionComp.SideMoion != Side.None)
        {
            timeSinceLastFrame = 0f;

            frameX = (frameX + 1) % countFrameX;
            if (frameX == 0)
            {
                frameY = (frameY + 1) % countFrameY;
            }

            if (MotionComp.SideMoion == Side.Left)
                SpriteEffect = SpriteEffects.FlipHorizontally;
            else
                SpriteEffect = SpriteEffects.None;
        }

        else if (MotionComp.SideMoion == Side.None)
        {
            frameX = 0; frameY = 0;
        }

        currentFrame = new Rectangle(
                frameX * frameWidth,
                frameY * frameHeight,
                frameWidth,
                frameHeight);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (isAnimation && IsCorrectForAnimation())
            UpdateCurrentFrame(gameTime);

        spriteBatch.Draw(
            texture,
            PositionComp.Position,
            currentFrame,
            Color.White,
            Rotation,
            Vector2.Zero,
            Scale,
            SpriteEffect,
            0f);
    }

    private bool IsCorrectForAnimation()
    {
        return PositionComp != null && MotionComp != null;
    }


    public Rectangle GetRectangleBounds()
    {
        Rectangle bounds = new Rectangle((int)PositionComp.Position.X, (int)PositionComp.Position.Y,
        Width, Height);
        return bounds;
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

    public const float Gravity = 6;

    public PhysicalComponent(PositionComponent positionComp, RenderComponent renderComp)
    {
        PositionComp = positionComp;
        RenderComp = renderComp;
        GameObjects = GameWorld.ColisionObjects;
        Viewport = GameWorld.viewport;
    }

    public static Rectangle GetBounds(IGameObject gameObject)
    {
        if (gameObject?.PositionComp == null || gameObject.RenderComp == null)
        {
            throw new ArgumentNullException("GameObjects components are null");
        }

        return new Rectangle(
            (int)gameObject.PositionComp.Position.X,
            (int)gameObject.PositionComp.Position.Y,
            gameObject.RenderComp.Width,
            gameObject.RenderComp.Height);
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

    public ColisionData GetCollisionSide(Vector2 newPosition)
    {
        Rectangle newBounds = new Rectangle((int)newPosition.X, (int)newPosition.Y,
            RenderComp.Width, RenderComp.Height);

        return GetCollisionSide(newBounds);
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
            if (obj is Player || obj is Monster_1) continue;
            if (obj.PositionComp == this.PositionComp) continue;

            Rectangle otherBounds = GetBounds(obj);
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

    private static ColisionData GetNearestCollision(params ColisionData[] collisions)
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




    public static bool CheckColisionBetweenObjects(IGameObject gameObject_1, IGameObject gameObject_2)
    {
        Side sideCollision = GetColisionBetweenObjects(gameObject_1, gameObject_2);

        return sideCollision != Side.None;
    }

    public static Side GetColisionBetweenObjects(IGameObject gameObject_1, IGameObject gameObject_2)
    {
        Side sideCollision = Side.None;

        Rectangle bounds_1 = gameObject_1.RenderComp.GetRectangleBounds();
        Rectangle bounds_2 = gameObject_2.RenderComp.GetRectangleBounds();

        sideCollision = GetCollisionSideWithObjectBounds(bounds_1, bounds_2);

        return sideCollision;
    }

    public static bool CheckObjectCollisionWithFilter(IGameObject gameObject, Func<IGameObject, bool> filter = null)
    {
        foreach (var obj in GameWorld.ColisionObjects)
        {
            if (filter != null && !filter(obj)) continue;

            return CheckColisionBetweenObjects(gameObject, obj);
        }
        return false;
    }

    public static bool CheckBoundsCollisionWithFilter(Rectangle bounds, Func<IGameObject, bool> filter = null)
    {
        foreach (var obj in GameWorld.ColisionObjects)
        {
            if (filter != null && !filter(obj)) continue;

            return CheckCollisionSideWithObjectBounds(bounds, GetBounds(obj));
        }
        return false;
    }


    public static bool CheckCollisionSideWithObjectBounds(Rectangle bounds, Rectangle otherBounds)
    {
         Side sideCollision = GetCollisionSideWithObjectBounds(bounds, otherBounds);

        return sideCollision != Side.None;
    }

    public static Side GetCollisionSideWithObjectBounds(Rectangle bounds, Rectangle otherBounds)
    {
        Rectangle intersection = Rectangle.Intersect(bounds, otherBounds);

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

    public bool CheckVievportCollision(Vector2 Position, float indent)
    {
        return
        Position.X < 0 - indent ||
        Position.X > GameWorld.viewport.Width + indent ||
        Position.Y < 0 - indent ||
        Position.Y > GameWorld.viewport.Height + indent;
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
        return collisionData.Penetration > 0;
    }


    public void Update()
    {
        if (!IsGrounded())
        {
            Vector2 newPosition = PositionComp.Position + new Vector2(0, Gravity);
            Move(newPosition);
        }
    }
}

public class MotionComponent
{
    public float MaxSpeedX { get; private set; }
    public float MaxSpeedY { get; private set; }

    private float _speedX;
    public float SpeedX
    {
        get => _speedX;
        set => _speedX = Math.Clamp(value, -MaxSpeedX, MaxSpeedX);
    }
    private float _speedY;
    public float SpeedY
    {
        get => _speedY;
        set => _speedY = Math.Clamp(value, -MaxSpeedY, MaxSpeedY);
    }

    public float JumpHeight = -195;
    private float JumpSpeed = 0f;
    public bool IsJumping { get; private set; }
    public Side SideMoion { get; private set; }


    public readonly bool IsKeyboardOperation;

    private PositionComponent PositionComp;
    private PhysicalComponent PhysicalComp;

    public MotionComponent(float maxSpeedX, float maxSpeedY, PositionComponent positionComp, PhysicalComponent physicalComp, bool isKeyboardOperation)
    {
        MaxSpeedX = maxSpeedX;
        MaxSpeedY = maxSpeedY;
        PositionComp = positionComp;
        PhysicalComp = physicalComp;
        IsKeyboardOperation = isKeyboardOperation;
    }

    public void Update(KeyboardState keyboardState)
    {
        SideMoion = Side.None;

        if (IsKeyboardOperation)
            KeybordMotion(keyboardState);


        UpdateJump();
    }

    public void Update()
    {
        UpdateJump();
    }

    public void Update(Vector2 speed, bool jump)
    {
        SpeedY = speed.Y;
        SpeedX = speed.X;

        Move(new Vector2(SpeedX, SpeedY));
        if (jump)
            Jump();
        UpdateJump();
    }

    private void UpdateJump()
    {
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


    private void KeybordMotion(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Left))
        {
            Vector2 newPosition = PositionComp.Position - new Vector2(MaxSpeedX, 0);
            if (PhysicalComp.Move(newPosition))
                SideMoion = Side.Left;
        }

        if (keyboardState.IsKeyDown(Keys.Right))
        {
            Vector2 newPosition = PositionComp.Position + new Vector2(MaxSpeedX, 0);
            if (PhysicalComp.Move(newPosition))
                SideMoion = Side.Right;
        }

        if (keyboardState.IsKeyDown(Keys.Up))
            Jump();


    }

    public void Move(Vector2 newPosition)
    {
        bool movedX = PhysicalComp.Move(PositionComp.Position + new Vector2(newPosition.X, 0));

        if (movedX)
            SideMoion = newPosition.X > 0 ? Side.Right : Side.Left;
        else
            SideMoion = Side.None;
    }

    public void Move(Side side)
    {
        if (side == Side.None)
            return;
        float curspeed = side == Side.Right ? MaxSpeedX : MaxSpeedX * -1;


        bool movedX = PhysicalComp.Move(PositionComp.Position + new Vector2(curspeed, 0));

        SideMoion = side;
    }

    public void Jump()
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

    private RenderComponent RenderComp;

    public readonly float Speed = 1;

    private float _lastShotTime;
    private const float ShotDelay = 0.5f;
    private readonly float Scale;

    public ShootingComponent(PositionComponent positionComp, RenderComponent renderComp, float speed, float scale)
    {
        PositionComp = positionComp;
        RenderComp = renderComp;
        Speed = speed;
        Scale = scale;
    }


    public void Update(MouseState mouseState)
    {

        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            float currentTime = (float)GameWorld.GameTime.TotalGameTime.TotalSeconds;
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

        Vector2 gunPosition = new Vector2(PositionComp.Position.X + PositionComp.Width / 2, PositionComp.Position.Y + PositionComp.Height / 3);
        if (RenderComp.SpriteEffect == SpriteEffects.FlipHorizontally)
            gunPosition.X = PositionComp.Position.X;


        Vector2 direction = target - gunPosition;
        direction = Vector2.Normalize(direction);

        GameWorld.Bullets.Add(new Bullet(gunPosition, GameWorld.Level.TextureBullet, GameWorld.viewport, Scale,
         direction.X * Speed, direction.Y * Speed));
    }

}



