using UnityEngine;
using System.Collections.Generic;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GridSettings settings;

    // Словарь: позиция (Vector2) -> созданный объект
    private Dictionary<Vector2, Bubble> cellMap = new Dictionary<Vector2, Bubble>();
    private Vector2?[,] _positions;

    private void Start()
    {
        GenerateGrid();
    }

    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        if (settings == null || settings.cellPrefab == null)
        {
            Debug.LogError("Настройки или префаб не назначены!");
            return;
        }

        ClearGrid();

        _positions = PositionCalculator.CalculatePositionArray(
            settings.rows,
            settings.columns,
            settings.cellSize,
            settings.offsetX,
            settings.verticalOverlap
        );

        Transform parent = settings.parentTransform != null ? settings.parentTransform : transform;

        for (int row = 0; row < settings.rows; row++)
        {
            for (int col = 0; col < settings.columns; col++)
            {
                Vector2? posNullable = _positions[row, col];
                if (posNullable.HasValue)
                {
                    Vector2 pos = posNullable.Value;
                    Bubble cell = Instantiate(settings.cellPrefab, parent).GetComponent<Bubble>();
                    cell.transform.localPosition = posNullable.Value;
                    cellMap[pos] = cell;
                }
            }
        }

        Debug.Log($"Сгенерировано {cellMap.Count} ячеек.");
    }

    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        foreach (var cell in cellMap.Values)
        {
            if (cell != null)
                DestroyImmediate(cell);
        }
        cellMap.Clear();
    }

    /// <summary>
    /// Получить объект ячейки по её позиции (центру).
    /// </summary>
    public Bubble GetCellAtPosition(Vector2 position)
    {
        cellMap.TryGetValue(position, out Bubble cell);
        return cell;
    }
}