using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

public interface ITrajectoryPredictor
{
    ShotResult Predict(
        Vector2 startPos,
        Vector2 direction,
        float speed,
        float maxTime = 3f);
}


public struct ShotResult
{
    public bool hit;
    public Vector2 hitPoint;
    public Bubble hitBubble;
    public Vector2Int targetCell;
    public List<Vector2> trajectory;
    public float duration;
    public Vector2 exitPoint;
    public float shotSpeed;

    // Фабричный метод для случая промаха
    public static ShotResult Miss(List<Vector2> trajectory, float duration, float speed, Vector2 exitPoint = default)
    {
        return new ShotResult
        {
            hit = false,
            trajectory = trajectory,
            duration = duration,
            shotSpeed = speed,
            exitPoint = exitPoint
        };
    }

    // Фабричный метод для попадания
    public static ShotResult Hit(Vector2 hitPoint, Bubble hitBubble, Vector2Int targetCell, 
                                 List<Vector2> trajectory, float duration, float speed)
    {
        return new ShotResult
        {
            hit = true,
            hitPoint = hitPoint,
            hitBubble = hitBubble,
            targetCell = targetCell,
            trajectory = trajectory,
            duration = duration,
            shotSpeed = speed
        };
    }
}

public class TrajectoryPredictor : ITrajectoryPredictor
{
    private readonly IBubbleGridStorage _storage;
    private readonly IBubbleNeighborFinder _neighborFinder;
    private readonly Bounds _fieldBounds;
    private readonly IGridPositionService _gridPositions;
    private readonly ICollisionDetector _collisionDetector;
    private readonly PhysicalSimulateSettings _settings;

    public TrajectoryPredictor(
        IBubbleGridStorage storage,
        IBubbleNeighborFinder neighborFinder,
        IGridPositionService gridPositions,
        PhysicalSimulateSettings physicalSimulateSettings,
        ICollisionDetector collisionDetector,
        Bounds fieldBounds)
    {
        _gridPositions = gridPositions;
        _storage = storage;
        _neighborFinder = neighborFinder;
        _settings = physicalSimulateSettings;
        _fieldBounds = fieldBounds;
        _collisionDetector = collisionDetector;
    }

    public ShotResult Predict(Vector2 startPos, Vector2 direction, float speed, float maxTime = 3f)
    {
        Vector2 position = startPos;
        Vector2 velocity = direction * speed;
        List<Vector2> trajectory = new List<Vector2> { position };
        float elapsedTime = 0f;

        float radius = _settings.radiusBubble;
        float dt = _settings.predictDeltaTime;

        while (elapsedTime < maxTime)
        {
            Vector2 newPosition = position + velocity * dt;

            // 1. Проверка столкновения с пузырём
            if (TryHandleBubbleCollision(position, newPosition, velocity,
                                         ref elapsedTime, ref trajectory, out ShotResult? bubbleResult))
            {
                return bubbleResult.Value;
            }

            // 2. Проверка вылета за нижнюю границу
            if (TryHandleBottomExit(position, newPosition, velocity,
                                    ref elapsedTime, ref trajectory, out ShotResult? bottomResult))
            {
                return bottomResult.Value;
            }

            // 3. Проверка отскока от стен (левая, правая, верхняя)
            if (TryHandleWallBounce(position, newPosition, ref velocity, ref trajectory))
            {
                position = trajectory[^1];
                elapsedTime += dt;
                continue;
            }

            // 4. Обычное движение с гравитацией
            position = newPosition;
            trajectory.Add(position);
            elapsedTime += dt;
            velocity.y -= _settings.gravity * dt;
        }

        // Время вышло, пузырь никуда не попал
        return ShotResult.Miss(trajectory, elapsedTime, speed);
    }

    /// <summary>
    /// Проверяет и обрабатывает столкновение с пузырём на отрезке от oldPos до newPos.
    /// </summary>
    private bool TryHandleBubbleCollision(
        Vector2 oldPos,
        Vector2 newPos,
        Vector2 velocity,
        ref float elapsedTime,
        ref List<Vector2> trajectory,
        out ShotResult? result)
    {
        result = null;

        if (!_collisionDetector.FindFirstCollision(oldPos, newPos, out Vector2 collisionPoint, out Bubble hitBubble))
            return false;

        trajectory.Add(collisionPoint);
        float timeToHit = Vector2.Distance(oldPos, collisionPoint) / velocity.magnitude;
        elapsedTime += timeToHit;

        Vector2Int targetCell = FindNearestEmptyCell(hitBubble, collisionPoint);
        if (targetCell.x < 0 || targetCell.y < 0)
        {
            // Нет свободной соседней ячейки – промах (пузырь не может прилипнуть)
            result = ShotResult.Miss(trajectory, elapsedTime, velocity.magnitude);
            return true;
        }

        // Добавляем финальную позицию в целевой ячейке
        Vector2 cellPos = _gridPositions.GetPosition(targetCell.x, targetCell.y);
        trajectory.Add(cellPos);
        float timeToCell = Vector2.Distance(collisionPoint, cellPos) / velocity.magnitude;
        elapsedTime += timeToCell;

        result = ShotResult.Hit(collisionPoint, hitBubble, targetCell, trajectory, elapsedTime, velocity.magnitude);
        return true;
    }

    /// <summary>
    /// Проверяет выход пузыря за нижнюю границу поля.
    /// </summary>
    private bool TryHandleBottomExit(
        Vector2 oldPos,
        Vector2 newPos,
        Vector2 velocity,
        ref float elapsedTime,
        ref List<Vector2> trajectory,
        out ShotResult? result)
    {
        result = null;
        float radius = _settings.radiusBubble;
        
        float bottomBoundary = _fieldBounds.min.y + radius;
        if (newPos.y > bottomBoundary)
            return false;

        float t = (bottomBoundary - oldPos.y) / velocity.y;
        Vector2 exitPoint = oldPos + velocity * t;

        trajectory.Add(exitPoint);
        elapsedTime += t;

        result = ShotResult.Miss(trajectory, elapsedTime, velocity.magnitude, exitPoint);
        return true;
    }

    /// <summary>
    /// Проверяет и обрабатывает отскок от левой, правой или верхней стены.
    /// </summary>
    private bool TryHandleWallBounce(
        Vector2 oldPos,
        Vector2 newPos,
        ref Vector2 velocity,
        ref List<Vector2> trajectory)
    {
        bool bounced = false;
        Vector2 bouncePoint = Vector2.zero;
        float radius = _settings.radiusBubble;
        
        // Левая стена
        if (newPos.x - radius <= _fieldBounds.min.x)
        {
            bouncePoint.x = _fieldBounds.min.x + radius;
            float t = (bouncePoint.x - oldPos.x) / velocity.x;
            bouncePoint.y = oldPos.y + velocity.y * t;
            velocity.x = -velocity.x;
            bounced = true;
        }
        // Правая стена
        else if (newPos.x + radius >= _fieldBounds.max.x)
        {
            bouncePoint.x = _fieldBounds.max.x - radius;
            float t = (bouncePoint.x - oldPos.x) / velocity.x;
            bouncePoint.y = oldPos.y + velocity.y * t;
            velocity.x = -velocity.x;
            bounced = true;
        }
        // Верхняя стена
        else if (newPos.y + radius >= _fieldBounds.max.y)
        {
            bouncePoint.y = _fieldBounds.max.y - radius;
            float t = (bouncePoint.y - oldPos.y) / velocity.y;
            bouncePoint.x = oldPos.x + velocity.x * t;
            velocity.y = -velocity.y;
            bounced = true;
        }

        if (bounced)
        {
            trajectory.Add(bouncePoint);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Выбирает ближайшую пустую ячейку-соседа к точке контакта.
    /// </summary>
    private Vector2Int FindNearestEmptyCell(Bubble hitBubble, Vector2 contactPoint)
    {
        var emptyCells = _neighborFinder.GetEmptyCellNeighbors(hitBubble);
        if (emptyCells.Count == 0)
            return new Vector2Int(-1, -1);

        Vector2Int best = emptyCells[0];
        float bestDist = Vector2.Distance(contactPoint, _gridPositions.GetPosition(best.x, best.y));

        for (int i = 1; i < emptyCells.Count; i++)
        {
            float dist = Vector2.Distance(contactPoint, _gridPositions.GetPosition(emptyCells[i].x, emptyCells[i].y));
            if (dist < bestDist)
            {
                bestDist = dist;
                best = emptyCells[i];
            }
        }

        return best;
    }
}