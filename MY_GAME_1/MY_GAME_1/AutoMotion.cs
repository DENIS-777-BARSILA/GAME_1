using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MY_GAME_1;
using SharpDX.Direct2D1.Effects;
using SimpleLinkedList;

namespace Components;



public enum TypesMovement
{
    Simple,
    Patrol,
    AlgorithmMovement_Flying
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
            TypesMovement.AlgorithmMovement_Flying => new AlgorithmMovement_Flying(),
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


public class AlgorithmMovement_Flying : AlgorithmMovement
{
    private const int MaxSearchDepth = 10;
    private TileMap tileMap = GameWorld.TileMap;
    private PathFinder pathFinder;
    private float lastPathUpdate;
    private const float pathUpdateInterval = 0.5f;
    private SimpleLinkedList<TilePosition> currentPath;

    public AlgorithmMovement_Flying()
    {
        pathFinder = new PathFinder(tileMap);
    }

    public void Update(PositionComponent positionComp, MotionComponent motionComp, RenderComponent render)
    {
        try
        {
            float currentTime = (float)GameWorld.GameTime.TotalGameTime.TotalSeconds;

            if (currentTime - lastPathUpdate > pathUpdateInterval || currentPath == null)
            {
                UpdatePath(positionComp);
                lastPathUpdate = currentTime;
            }

            FollowPath(positionComp, motionComp);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in flying movement: {ex.Message}");
        }
    }

    private void UpdatePath(PositionComponent positionComp)
    {
        var playerPos = GameWorld.player.PositionComp.Position;
        var monsterPos = positionComp.Position;

        var playerTile = WorldToTile(playerPos);
        var monsterTile = WorldToTile(monsterPos);

        if (!tileMap.InBounds(playerTile) || !tileMap.InBounds(monsterTile))
            return;

        currentPath = pathFinder.FindPaths(monsterTile, playerTile, MaxSearchDepth).FirstOrDefault();
    }

    private void FollowPath(PositionComponent positionComp, MotionComponent motionComp)
    {
        if (currentPath == null) return;

        var nextPoint = currentPath.Previous?.Value;
        if (nextPoint == null) return;

        var nextPos = tileMap.GetPosition(nextPoint);
        var currentPos = positionComp.Position;

        Vector2 direction = nextPos - currentPos;
        if (direction != Vector2.Zero)
            direction.Normalize();

        motionComp.Move(direction);

        if (Vector2.Distance(currentPos, nextPos) < 10f)
        {
            currentPath = currentPath.Previous;
        }
    }

    private TilePosition WorldToTile(Vector2 worldPos)
    {
        return new TilePosition
        {
            X = (int)(worldPos.X / tileMap.TileSize),
            Y = (int)(worldPos.Y / tileMap.TileSize)
        };
    }
}

public class PathFinder
{
    private TileMap _tileMap;

    public PathFinder(TileMap tileMap)
    {
        _tileMap = tileMap;
    }

    public IEnumerable<SimpleLinkedList<TilePosition>> FindPaths(TilePosition start, TilePosition target, int maxDepth)
    {
        var visited = new HashSet<TilePosition>();
        var queue = new Queue<SimpleLinkedList<TilePosition>>();
        queue.Enqueue(new SimpleLinkedList<TilePosition>(start));
        visited.Add(start);

        int currentDepth = 0;

        while (queue.Count > 0 && currentDepth < maxDepth)
        {
            currentDepth++;
            var current = queue.Dequeue();

            if (current.Value.Equals(target))
            {
                yield return current;
                yield break;
            }

            foreach (var neighbor in GetValidNeighbors(current.Value))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(new SimpleLinkedList<TilePosition>(neighbor, current));
                }
            }
        }
    }

    private IEnumerable<TilePosition> GetValidNeighbors(TilePosition point)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;

                var neighbor = new TilePosition { X = point.X + dx, Y = point.Y + dy };

                if (_tileMap.InBounds(neighbor) && _tileMap.IsEmpthyCell(neighbor))
                {
                    yield return neighbor;
                }
            }
        }
    }
}


