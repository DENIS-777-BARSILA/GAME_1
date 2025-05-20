using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MY_GAME_1;
using SharpDX.Direct2D1.Effects;

namespace Components;



public enum TypesMovement
{
    Simple,
    Patrol,
    Chase
}


public interface AlgorithmMovement
{
    void Update(PositionComponent positionComp, MotionComponent motionComp, RenderComponent renderComp);
}

public class AutoMotionComponent
{
    private PositionComponent positionComp;
    private MotionComponent motionComp;
    private RenderComponent renderComp;
    private AlgorithmMovement currentAlgorithmMovement;

    public AutoMotionComponent(
        PositionComponent position,
        MotionComponent motion,
        RenderComponent render,
        AlgorithmMovement algorithmMovement)
    {
        this.positionComp = position;
        this.motionComp = motion;
        renderComp = render;
        currentAlgorithmMovement = algorithmMovement;
    }

    public void Update()
    {
        currentAlgorithmMovement.Update(positionComp, motionComp, renderComp);
    }

    public void SetAlgorithmMovement(AlgorithmMovement newMovement)
    {
        currentAlgorithmMovement = newMovement;
    }

    public static AlgorithmMovement CreateMovement(TypesMovement type)
    {
        return type switch
        {
            TypesMovement.Patrol => new PatrolMovement(),
            TypesMovement.Chase => new AlgorithmMovement_Simple(),
            _ => new AlgorithmMovement_Simple()
        };
    }
}

public class AlgorithmMovement_Simple : AlgorithmMovement
{
    private readonly Player player;
    private bool IsJump = false;
    private Side CurrentDirection = Side.None;


    public AlgorithmMovement_Simple()
    {
        player = GameWorld.player;
    }

    private void CalculateMotion(PositionComponent positionComp)
    {
        const int coeffForMotion = 5;
        const int coeffForJump = 50;

        float DistanceToPlayer = positionComp.Position.X - player.PositionComp.Position.X;

        if (Math.Abs(DistanceToPlayer) < coeffForMotion)
            CurrentDirection = Side.None;
        else if (DistanceToPlayer < 0)
            CurrentDirection = Side.Right;
        else
            CurrentDirection = Side.Left;


        if (positionComp.Position.Y - player.PositionComp.Position.Y > coeffForJump && DistanceToPlayer < coeffForJump)
            IsJump = true;
        else
            IsJump = false;
    }

    public void Update(PositionComponent positionComp, MotionComponent motionComp, RenderComponent renderComp)
    {
        CalculateMotion(positionComp);
        motionComp.Move(CurrentDirection);
        if (IsJump) motionComp.Jump();
    }
}

public class PatrolMovement : AlgorithmMovement
{
    private const float PatrolRange = 1200f;
    private float? startX;
    private bool movingRight = true;
    private int directionChangeCooldown = 0;
    private const int DirectionChangeCooldownFrames = 10;
    private readonly Random random = new Random();

    private PhysicalComponent physicalComp;

    public void Update(PositionComponent position, MotionComponent motion, RenderComponent render)
    {
        if (startX == null)
        {
            startX = position.Position.X;
            movingRight = random.Next(2) == 1;
        }

        physicalComp ??= new PhysicalComponent(position, render);

        if (directionChangeCooldown > 0)
        {
            directionChangeCooldown--;
            return;
        }

        bool shouldTurn = ShouldTurnAround(position, motion);

        if (shouldTurn)
        {
            ChangeDirection();
        }
        else
        {
            motion.Move(movingRight ? Side.Right : Side.Left);
        }
    }

    private bool ShouldTurnAround(PositionComponent position, MotionComponent motion)
    {
        if ((movingRight && position.Position.X > startX + PatrolRange) ||
            (!movingRight && position.Position.X < startX))
        {
            return true;
        }
        Vector2 movement = movingRight ? Vector2.UnitX : -Vector2.UnitX;
        Vector2 newPosition = position.Position + movement * motion.MaxSpeedX;

        var collision = physicalComp.GetCollisionSide(newPosition);

        return (movingRight && collision.Side == Side.Right) ||
               (!movingRight && collision.Side == Side.Left);
    }

    private void ChangeDirection()
    {
        movingRight = !movingRight;
        directionChangeCooldown = DirectionChangeCooldownFrames;
    }
}


public class AlgorithmMovement_2 : AlgorithmMovement
{
    TileMap tileMap = GameWorld.TileMap;

    public void Update(PositionComponent position, MotionComponent motion, RenderComponent render)
    {


        {
            motion.Move(Side.Left);
        }
    }


}