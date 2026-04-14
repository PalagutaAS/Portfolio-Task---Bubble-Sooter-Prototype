using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Находит соседей для гексагональной сетки со смещёнными чётными рядами.
/// </summary>
public class HexNeighborFinder : IBubbleNeighborFinder
{
    private readonly int _rows;
    private readonly int _columns;
    private readonly IBubbleGridStorage _storage;

    public HexNeighborFinder(int rows, int columns, IBubbleGridStorage storage)
    {
        _rows = rows;
        _columns = columns;
        _storage = storage;
    }
    
    public List<(Vector2Int indices, Bubble bubble)> GetExistingNeighbors(Vector2Int center)
    {
        List<Vector2Int> potentialNeighbors = GetPotentialNeighborIndices(center);
        List<(Vector2Int, Bubble)> result = new List<(Vector2Int, Bubble)>();

        foreach (var idx in potentialNeighbors)
        {
            if (_storage.TryGetBubble(idx, out Bubble bubble))
                result.Add((idx, bubble));
        }

        return result;
    }

    /// <summary>
    /// Возвращает список индексов потенциальных соседей (без проверки существования ячейки).
    /// </summary>
    private List<Vector2Int> GetPotentialNeighborIndices(Vector2Int center)
    {
        int row = center.x;
        int col = center.y;
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Горизонтальные соседи
        if (col - 1 >= 0)
            neighbors.Add(new Vector2Int(row, col - 1));
        if (col + 1 < GetColumnCountForRow(row))
            neighbors.Add(new Vector2Int(row, col + 1));

        bool isEvenRow = (row % 2 == 0);

        // Верхние соседи
        if (row - 1 >= 0)
        {
            int upperRowCols = GetColumnCountForRow(row - 1);
            
            // Чётная строка: соседи сверху (col-1) и (col)
            // Нечётная строка: соседи сверху (col) и (col+1)
            int chekCol = isEvenRow ? col - 1 : col + 1;

            if (chekCol >= 0 && chekCol < upperRowCols)
                neighbors.Add(new Vector2Int(row - 1, chekCol));
            
            if (col < upperRowCols)
                neighbors.Add(new Vector2Int(row - 1, col));
        }

        // Нижние соседи
        if (row + 1 < _rows)
        {
            int lowerRowCols = GetColumnCountForRow(row + 1);
            
            // Чётная строка: соседи снизу (col-1) и (col)
            // Нечётная строка: соседи снизу (col) и (col+1)
            int chekCol = isEvenRow ? col - 1 : col + 1;

            if (chekCol >= 0 && chekCol < lowerRowCols)
                neighbors.Add(new Vector2Int(row + 1, chekCol));
            if (col < lowerRowCols)
                neighbors.Add(new Vector2Int(row + 1, col));

        }

        return neighbors;
    }

    /// <summary>
    /// Возвращает индексы существующих соседей (без самих объектов Bubble).
    /// </summary>
    private List<Vector2Int> GetExistingNeighborIndices(Vector2Int center)
    {
        List<Vector2Int> potential = GetPotentialNeighborIndices(center);
        List<Vector2Int> existing = new List<Vector2Int>();
        foreach (var idx in potential)
        {
            if (_storage.TryGetBubble(idx, out _)) // нам нужны только индексы, сам bubble не требуется
                existing.Add(idx);
        }
        return existing;
    }

    public bool IsConnectedToTopRow(Vector2Int startIndices)
    {
        if (startIndices.x == 0) return true; // сам уже в верхнем ряду

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(startIndices);
        visited.Add(startIndices);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            // Получаем реально существующих соседей (только те, у которых есть Bubble)
            List<Vector2Int> neighborIndices = GetExistingNeighborIndices(current);

            foreach (var neighbor in neighborIndices)
            {
                if (visited.Contains(neighbor)) continue;

                if (neighbor.x == 0) return true; // достигли верхнего ряда

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return false;
    }
    
    
    /// <summary>
    /// Возвращает количество столбцов в указанном ряду.
    /// Чётные ряды (row % 2 == 0) — полные, нечётные — на один столбец короче.
    /// </summary>
    private int GetColumnCountForRow(int row)
    {
        return (row % 2 == 0) ? _columns : _columns - 1;
    }

    public List<Vector2Int> GetEmptyCellNeighbors(Bubble bubble)
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        
        if (_storage.TryGetIndices(bubble, out Vector2Int idx))
        {
            List<Vector2Int> potential = GetPotentialNeighborIndices(idx);
            foreach (var cell in potential)
            {
                if (_storage.TryGetBubble(cell, out _))
                    continue;

                emptyCells.Add(cell);
            }
        }

        return emptyCells;
    }
}

public interface IBubbleNeighborFinder
{
    /// <summary>
    /// Возвращает список существующих соседей для заданного индекса.
    /// Каждый элемент — пара (индексы, Bubble).
    /// </summary>
    public List<(Vector2Int indices, Bubble bubble)> GetExistingNeighbors(Vector2Int center);
    
    /// <summary>
    /// Проверяет, связан ли пузырёк с заданными индексами с верхним рядом (row = 0) через соседние пузыри.
    /// Вынести в отдельный сервис
    /// </summary>
    bool IsConnectedToTopRow(Vector2Int startIndices);

    List<Vector2Int> GetEmptyCellNeighbors(Bubble bubble);
}