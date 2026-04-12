using UnityEngine;

public interface ICollisionDetector
{
    bool FindFirstCollision(Vector2 p1, Vector2 p2, out Vector2 collisionCenter, out Bubble hitBubble);
}


public class CollisionDetector : ICollisionDetector
{
    private readonly IBubbleGridStorage _storage;
    private readonly float _bubbleRadius;

    public CollisionDetector(IBubbleGridStorage storage, float bubbleRadius)
    {
        _storage = storage;
        _bubbleRadius = bubbleRadius;
    }

    public bool FindFirstCollision(Vector2 p1, Vector2 p2, out Vector2 collisionCenter, out Bubble hitBubble)
    {
        collisionCenter = Vector2.zero;
        hitBubble = null;
        float minT = float.MaxValue;

        Vector2 delta = p2 - p1;
        float length = delta.magnitude;
        if (length < Mathf.Epsilon) return false;

        Vector2 dir = delta / length;

        foreach (Bubble bubble in _storage.GetAllBubbles())
        {
            if (!_storage.TryGetPosition(bubble, out Vector2 center)) continue;

            Vector2 rel = p1 - center;
            float a = 1f;
            float b = 2f * Vector2.Dot(rel, dir);
            float c = rel.sqrMagnitude - (2f * _bubbleRadius) * (2f * _bubbleRadius);
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
}


