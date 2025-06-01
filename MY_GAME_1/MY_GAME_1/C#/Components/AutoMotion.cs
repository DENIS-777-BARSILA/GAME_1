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
    Patrol_Flying,
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
    public AlgorithmMovement currentAlgorithmMovement;

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
            TypesMovement.Patrol => new PatrolMovement(false),
            TypesMovement.Patrol_Flying => new PatrolMovement(true),
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
    private const int CountTileRange = 10;
    private float? startX;
    private bool IsFlying = false;
    private bool movingRight = true;
    private int directionChangeCooldown = 0;
    private const int DirectionChangeCooldownFrames = 10;
    private readonly Random random = new Random();

    private readonly TileMap tileMap = GameWorld.TileMap;

    private PhysicalComponent physicalComp;

    public PatrolMovement(bool isFlying)
    {
        IsFlying = isFlying;
    }

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

        if (ShouldTurnAround(position, motion))
        {
            ChangeDirection();
        }

        else
            motion.Move(movingRight ? Side.Right : Side.Left);
    }

    private bool ShouldTurnAround(PositionComponent positionComp, MotionComponent motionComp)
    {
        if ((movingRight && positionComp.Position.X > startX + CountTileRange * tileMap.TileSize) ||
            (!movingRight && positionComp.Position.X < startX))
        {
            return true;
        }

        Vector2 movement = movingRight ? Vector2.UnitX : -Vector2.UnitX;
        Vector2 newPosition = positionComp.Position + movement * motionComp.MaxSpeedX;
        var collision = physicalComp.GetCollisionSide(newPosition);


        float checkAhead = movingRight ? tileMap.TileSize / 2 : -tileMap.TileSize / 2;
        Vector2 checkPosition = positionComp.Position + new Vector2(checkAhead, tileMap.TileSize);
        TilePosition nextGroundTile = tileMap.GetTilePosition(checkPosition);
        Console.WriteLine($"Auto  {checkPosition.X} {checkPosition.Y}   Tile  {nextGroundTile.X} {nextGroundTile.Y}");


        return (movingRight && collision.Side == Side.Right) ||
               (!movingRight && collision.Side == Side.Left)
               || (tileMap.IsEmpthyCell(nextGroundTile) && !IsFlying);
    }

    private void ChangeDirection()
    {
        movingRight = !movingRight;
        directionChangeCooldown = DirectionChangeCooldownFrames;
    }
}

public class AlgorithmMovement_Flying : AlgorithmMovement
{
    private AlgorithmMovement PatrolMovementAlgorithm;

    private readonly float ShiftDistance = GameWorld.TileMap.TileSize / 5;
    private const int MaxSearchDepth = 2000;
    private TileMap tileMap = GameWorld.TileMap;
    private PathFinder pathFinder;
    private float lastPathUpdate;
    private const float pathUpdateInterval = 0.5f;
    private SimpleLinkedList<TilePosition> currentPath;

    public AlgorithmMovement_Flying()
    {
        pathFinder = new PathFinder(tileMap);
        PatrolMovementAlgorithm = new PatrolMovement(false);
    }

    public void Update(PositionComponent positionComp, MotionComponent motionComp, RenderComponent render)
    {
        float currentTime = (float)GameWorld.GameTime.TotalGameTime.TotalSeconds;

        if (currentTime - lastPathUpdate > pathUpdateInterval || currentPath == null)
        {
            UpdatePath(positionComp, motionComp);
            lastPathUpdate = currentTime;
        }


        if (currentPath != null)
        {
            FollowPath(positionComp, motionComp);
        }

        else
        {
            PatrolMovementAlgorithm.Update(positionComp, motionComp, render);
        }

    }

    private void UpdatePath(PositionComponent positionComp, MotionComponent motionComp)
    {
        var playerPositionComp = GameWorld.player.PositionComp;

        var playerTile = GameWorld.TileMap.GetTilePosition(playerPositionComp.Position, playerPositionComp.Width, playerPositionComp.Width);
        var monsterTile = GameWorld.TileMap.GetTilePosition(positionComp.Position, positionComp.Width, positionComp.Height);

        if (!tileMap.InBounds(playerTile)) return;

        var paths = pathFinder.FindPaths(monsterTile, playerTile, MaxSearchDepth);
        currentPath = paths.FirstOrDefault();
    }

    private void FollowPath(PositionComponent positionComp, MotionComponent motionComp)
    {
        if (currentPath == null || currentPath.Previous == null)
            return;

        var currentPos = positionComp.Position;

        var nextPoint = currentPath.Previous.Value;
        var nextPos = tileMap.GetPosition(nextPoint);

        Vector2 centeredPos = nextPos + new Vector2(tileMap.TileSize / 2 - positionComp.Width / 2,
            tileMap.TileSize / 2 - positionComp.Height / 2);

        Vector2 playerCenteredPos = GameWorld.player.PositionComp.Position + new Vector2(tileMap.TileSize / 2 - positionComp.Width / 2,
            tileMap.TileSize / 2 - positionComp.Height / 2);

        var direction = centeredPos - currentPos;

        if (Vector2.DistanceSquared(currentPos, GameWorld.player.PositionComp.Position) <= tileMap.TileSize)
        {
            motionComp.Move(GameWorld.player.PositionComp.Position - currentPos);
            return;
        }

        if (direction != Vector2.Zero)
        {
            direction.Normalize();

            if (Math.Abs(direction.X) > Math.Abs(direction.Y))
                direction = new Vector2(Math.Sign(direction.X), 0);

            else
                direction = new Vector2(0, Math.Sign(direction.Y));

            motionComp.Move(direction);

            if (Vector2.Distance(currentPos, nextPos) < ShiftDistance)
                currentPath = currentPath.Previous;
        }
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

            foreach (var neighbor in GetNeighbors(current.Value))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(new SimpleLinkedList<TilePosition>(neighbor, current));
                }
            }
        }
    }

    private IEnumerable<TilePosition> GetNeighbors(TilePosition point)
    {
        var directions = new (int dx, int dy)[] { (0, -1), (1, 0), (0, 1), (-1, 0) };

        foreach (var dir in directions)
        {
            var neighbor = new TilePosition { X = point.X + dir.dx, Y = point.Y + dir.dy };
            if (_tileMap.InBounds(neighbor) && _tileMap.IsEmpthyCell(neighbor))
            {
                yield return neighbor;
            }
        }
    }
}


