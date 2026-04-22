using ScriptableObjects;
using UnityEngine;

public interface IGridPositionService
{
    Vector2 GetPosition(int row, int col);
    int GetLength(int dimension = 0);
}

public class GridPositions : IGridPositionService
{
    private GridSettings _settings;
    private Transform _parent;

    private Vector2[,] _positions;

    public GridPositions(GridSettings settings, Transform parent)
    {
        _settings = settings;
        _parent = parent;
        _positions = CalculatePositionArray(
            _settings.rows,
            _settings.columns,
            _settings.cellSize,
            _settings.offsetX,
            _settings.verticalOverlap);
    }

    /// <summary>
    /// Создаёт двумерный массив позиций ячеек (сверху вниз, слева направо).
    /// Верхний ряд (row = 0) имеет Y = 0 (локально, центр ячейки).
    /// Для чётных рядов (row % 2 == 1) последняя ячейка отсутствует (null).
    /// </summary>
    private Vector2[,] CalculatePositionArray(int rows, int columns, float cellSize, float offsetX, float verticalOverlap)
    {
        Vector2[,] grid = new Vector2[rows, columns];

        float verticalStep = cellSize - verticalOverlap;

        float upperRowMinX = cellSize / 2f;
        float upperRowMaxX = cellSize / 2f + (columns - 1) * cellSize;
        float shiftX = -(upperRowMinX + upperRowMaxX) / 2f;

        for (int row = 0; row < rows; row++)
        {
            int cellsInRow = (row % 2 == 0) ? columns : columns - 1;
            float xOffset = (row % 2 == 1) ? offsetX : 0f;

            float yPos = -row * verticalStep - cellSize/2;

            for (int col = 0; col < cellsInRow; col++)
            {
                float xPos = xOffset + cellSize / 2f + col * cellSize;
                Vector2 pos = new Vector2(xPos + shiftX, yPos);
                grid[row, col] = _parent.TransformPoint(pos);
            }
        }

        return grid;
    }

    public Vector2 GetPosition(int row, int col)
    {
        try
        {
            return _positions[row, col];
        }
        catch (System.IndexOutOfRangeException)
        {
            Debug.LogError($"Запрошен недопустимый индекс: row={row}, col={col}");
            return Vector2.zero;
        }
    }

    public int GetLength(int dimension = 0)
    {
        return _positions.GetLength(dimension);
    }
}