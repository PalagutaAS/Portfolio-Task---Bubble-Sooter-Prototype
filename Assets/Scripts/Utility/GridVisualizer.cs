using ScriptableObjects;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridVisualizer : MonoBehaviour
{
    [Header("Grid Settings Reference")]
    public GridSettings settings; // Перетащите сюда объект с настройками сетки

    [Header("Visualization Options")]
    public bool drawCells = true;
    public bool drawRowColors = true;
    public bool drawRowNumbers = false;
    public bool drawOnlySelected = false;

    [Header("Appearance")]
    public float cellSizeMultiplier = 0.8f; // Размер отображаемой фигуры относительно ячейки
    public Color evenRowColor = new Color(0.2f, 0.6f, 1f, 0.5f);
    public Color oddRowColor = new Color(0.8f, 0.3f, 0.3f, 0.5f);

    private GridPositions gridPositions;

    private void OnValidate()
    {
        if (settings == null)
        {
            Debug.LogWarning("GridVisualizer: настройки сетки не назначены!");
        }
    }

    private void OnDrawGizmos()
    {
        if (settings == null) return;
        if (drawOnlySelected) return;
        DrawGrid();
    }

    private void OnDrawGizmosSelected()
    {
        if (settings == null) return;
        DrawGrid();
    }

    private void DrawGrid()
    {
        if (gridPositions == null || SettingsChanged())
        {
            gridPositions = new GridPositions(settings, transform);
        }

        int rows = settings.rows;
        int cols = settings.columns;

        // 1. Рисуем ячейки
        if (drawCells)
        {
            for (int row = 0; row < rows; row++)
            {
                Color rowColor = drawRowColors ? (row % 2 == 0 ? evenRowColor : oddRowColor) : Color.white;
                Gizmos.color = rowColor;

                for (int col = 0; col < cols; col++)
                {
                    Vector2 pos = gridPositions.GetPosition(row, col);
                    if (pos == Vector2.zero)
                        continue;

                    Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
                    float size = settings.cellSize * cellSizeMultiplier;
                    Gizmos.DrawCube(worldPos, new Vector3(size, size, 0.01f));
                }
            }
        }
        
        // 2. Рисуем номера рядов (только если выбран объект и в режиме Scene View)
        if (drawRowNumbers && SelectionContainsThis())
        {
            DrawRowNumbers(rows);
        }
    }

    private void DrawRowNumbers(int rows)
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 14;
        style.alignment = TextAnchor.MiddleCenter;

        for (int row = 0; row < rows; row++)
        {
            Vector3 worldPos = gridPositions.GetPosition(row, 0);
            Vector3 labelPos = worldPos + new Vector3(-settings.cellSize * 0.9f, 0, 0);
#if UNITY_EDITOR
            Handles.Label(labelPos, row.ToString(), style);
#else
            return;
#endif
            
        }
    }

    private bool SettingsChanged()
    {
        return gridPositions == null;
    }

    private bool SelectionContainsThis()
    {
#if UNITY_EDITOR
        return Selection.activeGameObject == gameObject;
#else
        return false;
#endif
    }
}