using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BubbleGridStorage : IBubbleGridStorage
{
    private readonly Dictionary<Vector2Int, Bubble> _positionToBubble = new ();
    private readonly Dictionary<Bubble, Vector2Int> _bubbleToIndexes = new ();
    private IGridPositionService _gridPositions;

    public BubbleGridStorage(IGridPositionService gridPositions)
    {
        _gridPositions = gridPositions;
    }

    public void AddBubble(Vector2Int indexesOnGrid, Bubble bubble)
    {
        if (bubble == null) return;
        _positionToBubble[indexesOnGrid] = bubble;
        _bubbleToIndexes[bubble] = indexesOnGrid;
    }

    public void RemoveBubble(Bubble bubble)
    {
        if (bubble == null) return;
        if (_bubbleToIndexes.TryGetValue(bubble, out Vector2Int indexes))
        {
            _positionToBubble.Remove(indexes);
            _bubbleToIndexes.Remove(bubble);
            Object.Destroy(bubble.gameObject);
        }
    }

    public void Clear()
    {
        _positionToBubble.Clear();
        _bubbleToIndexes.Clear();
    }

    public bool TryGetBubble(Vector2Int position, out Bubble bubble)
    {
        return _positionToBubble.TryGetValue(position, out bubble);
    }

    public bool TryGetPosition(Bubble bubble, out Vector2 position)
    {
        if (!_bubbleToIndexes.TryGetValue(bubble, out Vector2Int indices))
        {
            position = default;
            return false;
        }


        if (indices.x < 0 || indices.x >= _gridPositions.GetLength(0)) 
        {
            position = default;
            return false;
        }

        Vector2 foundPos = _gridPositions.GetPosition(indices.x, indices.y);
        if (foundPos == Vector2.zero)
        {
            position = default;
            return false;
        }
        
        position = foundPos;
        return true;
    }

    public bool TryGetPosition(Vector2Int indices, out Vector2 position)
    {
        Vector2 foundPos = _gridPositions.GetPosition(indices.x, indices.y);
        if (foundPos == Vector2.zero)
        {
            position = default;
            return false;
        }
        
        position = foundPos;
        return true;
    }

    public bool TryGetIndices(Bubble bubble, out Vector2Int indexes)
    {
        return _bubbleToIndexes.TryGetValue(bubble, out indexes);
    }

    public IEnumerable<Bubble> GetAllBubbles()
    {
        return _bubbleToIndexes.Keys.ToList();
    }
}


public interface IBubbleGridStorage
{
    void AddBubble(Vector2Int indexesOnGrid, Bubble bubble);
    void RemoveBubble(Bubble bubble);
    void Clear();
    
    bool TryGetBubble(Vector2Int position, out Bubble bubble);
    
    bool TryGetPosition(Bubble bubble, out Vector2 position);
    bool TryGetPosition(Vector2Int indices, out Vector2 position);

    bool TryGetIndices(Bubble bubble, out Vector2Int position);


    /// <summary>
    /// Получить все пузыри (опционально).
    /// </summary>
    IEnumerable<Bubble> GetAllBubbles();
}