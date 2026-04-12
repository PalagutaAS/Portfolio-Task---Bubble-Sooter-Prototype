using System.Collections.Generic;
using UnityEngine;

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
}

public class TrajectoryPredictor
{
    private readonly IBubbleGridStorage _storage;
    private readonly IBubbleNeighborFinder _neighborFinder;
    private readonly Bounds _fieldBounds;
    private readonly IGridPositionService _gridPositions;
    private readonly TrajectorySettings _settings;

    public TrajectoryPredictor(IBubbleGridStorage storage, IBubbleNeighborFinder neighborFinder, 
        IGridPositionService gridPositions, TrajectorySettings trajectorySettings, Bounds fieldBounds)
    {
        _gridPositions = gridPositions;
        _storage = storage;
        _neighborFinder = neighborFinder;
        _settings = trajectorySettings;
        _fieldBounds = fieldBounds;
    }

    public ShotResult Predict(
        Vector2 startPos,
        Vector2 direction,
        float speed,
        float maxTime = 3f)
    {
        Vector2 pos = startPos;
        Vector2 vel = direction * speed;
        List<Vector2> trajectory = new List<Vector2> { pos };
        float elapsed = 0f;

        float radius = _settings.radiusBubble;
        float dt = _settings.predictDeltaTime;
        
        while (elapsed < maxTime)
        {
            Vector2 newPos = pos + vel * dt;

            // ---- Проверка столкновения с шарами сетки на отрезке [pos, newPos] ----
            if (TryFindCollision(pos, newPos, out Vector2 collisionPoint, out Bubble hitBubble))
            {
                trajectory.Add(collisionPoint);
                float tHit = (collisionPoint - pos).magnitude / vel.magnitude;
                elapsed += tHit;

                // Определяем целевую пустую ячейку (как у вас)
                Vector2Int targetCell = FindNearestEmptyCell(hitBubble, collisionPoint);
                if (targetCell.x >= 0 && targetCell.y >= 0)
                {
                    Vector2 cellPos = _gridPositions.GetPosition(targetCell.x, targetCell.y);
                    trajectory.Add(cellPos);
                    float distToCell = Vector2.Distance(collisionPoint, cellPos);
                    float timeToCell = distToCell / vel.magnitude;
                    elapsed += timeToCell;
                }
                else
                {
                    return new ShotResult
                    {
                        hit = false,
                        trajectory = trajectory,
                        duration = elapsed,
                        shotSpeed = speed
                    };
                }

                return new ShotResult { 
                    hit = true,
                    hitPoint = collisionPoint,
                    hitBubble = hitBubble,
                    targetCell = targetCell,
                    trajectory = trajectory,
                    duration = elapsed,
                    shotSpeed = speed
                };
            }

            // ---- Проверка вылета за нижнюю границу (без отскока) ----
            if (newPos.y - radius <= _fieldBounds.min.y)
            {
                // Вычисляем точное время пересечения нижней границы
                float t = (_fieldBounds.min.y + radius - pos.y) / vel.y;
                Vector2 exitPoint = pos + vel * t;
                trajectory.Add(exitPoint);
                elapsed += t;
                return new ShotResult
                {
                    hit = false,
                    trajectory = trajectory,
                    duration = elapsed,
                    shotSpeed = speed,
                    exitPoint = exitPoint
                };
            }

            // ---- Отскок от остальных стен (левая, правая, верхняя) ----
            bool wallHit = false;
            Vector2 wallPoint = Vector2.zero;
            Vector2 newVel = vel;

            // Левая / правая
            if (newPos.x - radius <= _fieldBounds.min.x)
            {
                wallPoint.x = _fieldBounds.min.x + radius;
                float t = (wallPoint.x - pos.x) / vel.x;
                wallPoint.y = pos.y + vel.y * t;
                newVel.x = -vel.x;
                wallHit = true;
            }
            else if (newPos.x + radius >= _fieldBounds.max.x)
            {
                wallPoint.x = _fieldBounds.max.x - radius;
                float t = (wallPoint.x - pos.x) / vel.x;
                wallPoint.y = pos.y + vel.y * t;
                newVel.x = -vel.x;
                wallHit = true;
            }
            // Верхняя стена (отскок)
            else if (newPos.y + radius >= _fieldBounds.max.y)
            {
                wallPoint.y = _fieldBounds.max.y - radius;
                float t = (wallPoint.y - pos.y) / vel.y;
                wallPoint.x = pos.x + vel.x * t;
                newVel.y = -vel.y;
                wallHit = true;
            }

            if (wallHit)
            {
                trajectory.Add(wallPoint);
                pos = wallPoint;
                vel = newVel;
                elapsed += dt;
                continue;
            }

            // ---- Обычное движение ----
            pos = newPos;
            trajectory.Add(pos);
            elapsed += dt;
            vel.y -= _settings.gravity * dt;
        }

        // Не попал и не улетел за maxTime
        return new ShotResult { hit = false, trajectory = trajectory, duration = elapsed, shotSpeed = speed };
    }

    // Проверка пересечения отрезка с окружностями всех шаров
    private bool TryFindCollision(Vector2 p1, Vector2 p2, out Vector2 collisionCenter, out Bubble hitBubble)
    {
        collisionCenter = Vector2.zero;
        hitBubble = null;
        float minT = float.MaxValue;
        float radius = _settings.radiusBubble;
        
        Vector2 delta = p2 - p1;
        float length = delta.magnitude;
        Vector2 dir = delta / length; // нормализованное направление

        foreach (Bubble bubble in _storage.GetAllBubbles())
        {
            if (!_storage.TryGetPosition(bubble, out Vector2 center)) continue;

            Vector2 rel = p1 - center;
            float a = 1f;
            float b = 2f * Vector2.Dot(rel, dir);
            float c = rel.sqrMagnitude - (2f * radius) * (2f * radius);
            float disc = b * b - 4f * a * c;
            if (disc < 0) continue;

            float sqrtDisc = Mathf.Sqrt(disc);
            float t1 = (-b - sqrtDisc) / (2f * a);
            float t2 = (-b + sqrtDisc) / (2f * a);

            float t = -1f;
            if (t1 >= 0 && t1 <= length) t = t1;
            else if (t2 >= 0 && t2 <= length) t = t2;

            if (t >= 0 && t < minT)
            {
                minT = t;
                collisionCenter = p1 + dir * t;
                hitBubble = bubble;
            }
        }
        return hitBubble != null;
    }
    
    private Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 point)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }

    // Выбор ближайшей пустой ячейки к точке касания
    private Vector2Int FindNearestEmptyCell(Bubble hitBubble, Vector2 contactPoint)
    {
        var emptyCells = _neighborFinder.GetEmptyCellNeighbors(hitBubble);
        if (emptyCells.Count == 0) 
            return new Vector2Int(-1, -1);

        Vector2Int best = emptyCells[0];
        float bestDist = Vector2.Distance(contactPoint, _gridPositions.GetPosition(best.x, best.y));
        for (int i = 1; i < emptyCells.Count; i++)
        {
            float d = Vector2.Distance(contactPoint, _gridPositions.GetPosition(emptyCells[i].x, emptyCells[i].y));
            if (d < bestDist)
            {
                bestDist = d;
                best = emptyCells[i];
            }
        }
        return best;
    }
}