using System.Collections.Generic;
using UnityEngine;

public class BubbleMatchFinder : IBubbleMatchFinder
{
    private readonly IBubbleGridStorage _storage;
    private readonly IBubbleNeighborFinder _neighborFinder;

    public BubbleMatchFinder(IBubbleGridStorage storage, IBubbleNeighborFinder neighborFinder)
    {
        _storage = storage;
        _neighborFinder = neighborFinder;
    }

    public List<Bubble> FindMatchingBubbles(Bubble startBubble)
    {
        if (startBubble == null) 
            return new List<Bubble>();
        
        if (!_storage.TryGetIndices(startBubble, out Vector2Int startIdx))
            return new List<Bubble>();

        var matchingIndices = FindMatchingIndices(startIdx);
        var result = new List<Bubble>(matchingIndices.Count);
        
        foreach (var idx in matchingIndices)
        {
            if (_storage.TryGetBubble(idx, out var bubble))
                result.Add(bubble);
        }
        return result;
    }

    public List<Vector2Int> FindMatchingIndices(Vector2Int startIndices)
    {
        if (!_storage.TryGetBubble(startIndices, out var startBubble))
            return new List<Vector2Int>();

        var targetColor = startBubble.Color;
        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        
        queue.Enqueue(startIndices);
        visited.Add(startIndices);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var neighbors = _neighborFinder.GetExistingNeighbors(current);
            
            foreach (var (neighborIdx, neighborBubble) in neighbors)
            {
                if (visited.Contains(neighborIdx)) continue;
                if (!neighborBubble.Color.Equals(targetColor)) continue;
                
                visited.Add(neighborIdx);
                queue.Enqueue(neighborIdx);
            }
        }

        return new List<Vector2Int>(visited);
    }
}

public interface IBubbleMatchFinder
{
    /// <summary>
    /// Возвращает список пузырей того же цвета, связанных с начальным (включая его самого).
    /// </summary>
    List<Bubble> FindMatchingBubbles(Bubble startBubble);
    
    /// <summary>
    /// Возвращает список индексов пузырей того же цвета, связанных с указанными индексами.
    /// </summary>
    List<Vector2Int> FindMatchingIndices(Vector2Int startIndices);
}