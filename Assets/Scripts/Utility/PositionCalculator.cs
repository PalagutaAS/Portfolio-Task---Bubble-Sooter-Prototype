using UnityEngine;

public static class PositionCalculator
{
    /// <summary>
    /// Создаёт двумерный массив позиций ячеек (сверху вниз, слева направо).
    /// Верхний ряд (row = 0) имеет Y = 0 (локально, центр ячейки).
    /// Для чётных рядов (row % 2 == 1) последняя ячейка отсутствует (null).
    /// </summary>
    public static Vector2?[,] CalculatePositionArray(int rows, int columns, float cellSize, float offsetX, float verticalOverlap)
    {
        Vector2?[,] grid = new Vector2?[rows, columns];

        float verticalStep = cellSize - verticalOverlap;

        // Вычисляем смещение X для центрирования верхнего ряда относительно родителя
        float upperRowMinX = cellSize / 2f;
        float upperRowMaxX = cellSize / 2f + (columns - 1) * cellSize;
        float shiftX = -(upperRowMinX + upperRowMaxX) / 2f;

        for (int row = 0; row < rows; row++)
        {
            int cellsInRow = (row % 2 == 0) ? columns : columns - 1;
            float xOffset = (row % 2 == 1) ? offsetX : 0f;

            // Y позиция: верхний ряд (row = 0) -> Y = 0, каждый следующий ряд опускается на verticalStep вниз
            float yPos = -row * verticalStep - cellSize/2;

            for (int col = 0; col < cellsInRow; col++)
            {
                float xPos = xOffset + cellSize / 2f + col * cellSize;
                Vector2 pos = new Vector2(xPos + shiftX, yPos);
                grid[row, col] = pos;
            }
        }

        return grid;
    }
}