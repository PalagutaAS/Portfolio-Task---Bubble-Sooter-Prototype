using System.Collections.Generic;
using UnityEngine;

public interface IFloatingBubbleRemover
{
    /// <summary>
    /// Удаляет все пузыри, которые не соединены с верхним рядом (row = 0).
    /// Возвращает количество удалённых пузырей.
    /// </summary>
    int RemoveFloatingBubbles();
}


public class FloatingBubbleRemover : IFloatingBubbleRemover
{
    private readonly IBubbleGridStorage _storage;
    private readonly IBubbleNeighborFinder _neighborFinder;

    public FloatingBubbleRemover(IBubbleGridStorage storage, IBubbleNeighborFinder neighborFinder)
    {
        _storage = storage;
        _neighborFinder = neighborFinder;
    }

    public int RemoveFloatingBubbles()
    {
        // 1. Собираем все пузыри из верхнего ряда (row == 0)
        var rootIndices = new List<Vector2Int>();
        foreach (var bubble in _storage.GetAllBubbles())
        {
            if (_storage.TryGetIndices(bubble, out Vector2Int idx) && idx.x == 0)
            {
                rootIndices.Add(idx);
            }
        }

        // Если в верхнем ряду нет пузырей, то удаляем всё
        if (rootIndices.Count == 0)
        {
            int totalCount = 0;
            var allBubbles = _storage.GetAllBubbles();
            foreach (var bubble in allBubbles)
            {
                _storage.RemoveBubble(bubble);
                totalCount++;
            }
            return totalCount;
        }

        // 2. BFS от всех корневых индексов для поиска всех связанных с потолком пузырей
        var connectedIndices = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();

        foreach (var root in rootIndices)
        {
            if (!connectedIndices.Contains(root))
            {
                queue.Enqueue(root);
                connectedIndices.Add(root);
            }
        }

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            var neighbors = _neighborFinder.GetExistingNeighbors(current);

            foreach (var (neighborIdx, _) in neighbors)
            {
                if (!connectedIndices.Contains(neighborIdx))
                {
                    connectedIndices.Add(neighborIdx);
                    queue.Enqueue(neighborIdx);
                }
            }
        }

        // 3. Удаляем все пузыри, индексы которых НЕ в connectedIndices
        var bubblesToRemove = new List<Bubble>();
        foreach (var bubble in _storage.GetAllBubbles())
        {
            if (_storage.TryGetIndices(bubble, out Vector2Int idx) && !connectedIndices.Contains(idx))
            {
                bubblesToRemove.Add(bubble);
            }
        }

        foreach (var bubble in bubblesToRemove)
        {
            _storage.RemoveBubble(bubble);
        }

        return bubblesToRemove.Count;
    }
}
