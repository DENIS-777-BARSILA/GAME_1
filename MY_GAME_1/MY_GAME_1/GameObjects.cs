using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Components;

namespace MY_GAME_1;


public class Background
{
    public readonly PositionComponent PositionComp;
    public readonly RenderComponent RenderComp;

    public Background(Texture2D bacgroundImage, Viewport viewport)
    {
        PositionComp = new PositionComponent(Vector2.Zero, RenderComp);

        RenderComp = new RenderComponent(bacgroundImage, PositionComp, 1);
    }


}


public class Player : IGameObject
{
    public PositionComponent PositionComp { get; } = null;
    public HealthComponent HealthComp { get; }
    public RenderComponent RenderComp { get; } = null;
    public MotionComponent MotionComp { get; }
    public PhysicalComponent PhysicalComp { get; }
    public ShootingComponent ShootingComp{ get; }

    public Player(Vector2 position, int health, int maxHealth, Texture2D texture, float speedX, float speedY, Viewport viewport, float scale)
    {
        RenderComp = new RenderComponent(texture, null, scale);


        PositionComp = new PositionComponent(position, RenderComp);


        RenderComp.PositionComp = PositionComp;

        HealthComp = new HealthComponent(maxHealth, health);
        PhysicalComp = new PhysicalComponent(PositionComp, RenderComp);
        MotionComp = new MotionComponent(speedX, speedY, PositionComp, PhysicalComp, true);

        ShootingComp = new ShootingComponent(PositionComp, 50);

    }

    public void Update()
    {
        MotionComp.Update(Keyboard.GetState());
        PhysicalComp.Update();
        ShootingComp.Update(Mouse.GetState());
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
}


public class Bullet : IGameObject
{
    public PositionComponent PositionComp { get; } = null;
    public RenderComponent RenderComp { get; } = null;
    public PhysicalComponent PhysicalComp { get; }
    public MotionComponent MotionComp { get; }

    float SpeedX;
    float SpeedY;

    public Bullet(Vector2 position, Texture2D texture, Viewport viewport, float scale, float speedX, float speedY)
    {
        RenderComp = new RenderComponent(texture, null, scale);
        PositionComp = new PositionComponent(position, RenderComp);
        RenderComp.PositionComp = PositionComp;
        PhysicalComp = new PhysicalComponent(PositionComp, RenderComp);
        MotionComp = new MotionComponent(speedX, speedY, PositionComp, PhysicalComp, true);
        SpeedX = speedX;
        SpeedY = speedY;
    }

    public void Update()
    {
        MotionComp.Update();
    }
}

public interface IGameObject
{
    PositionComponent PositionComp { get; }
    RenderComponent RenderComp { get; }
    PhysicalComponent PhysicalComp { get; }

    void Update();
}




