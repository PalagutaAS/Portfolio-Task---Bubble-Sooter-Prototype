using System.Linq;
using UnityEngine;

public class GridGenerator
{
    private readonly GridSettings _settings;
    private readonly BubbleFactory _bubbleFactory;
    private readonly Transform _parent;
    private readonly IBubbleGridStorage _storage;
    private readonly GridPositions _gridPositions;
    
    public int Rows => _settings.rows;
    public int Columns => _settings.columns;

    public GridGenerator(GridSettings gridSettings, BubbleFactory bubbleFactory, GridPositions gridPositionCalculator, Transform parent, IBubbleGridStorage storage)
    {
        _settings = gridSettings;
        _bubbleFactory = bubbleFactory;
        _parent = parent;
        _gridPositions = gridPositionCalculator;
        _storage = storage;
    }

    public void GenerateRandomBubbles()
    {
        if (_settings == null || _settings.Prefab == null)
        {
            Debug.LogError("Настройки или префаб не назначены!");
            return;
        }

        ClearGrid();
        
        for (int row = 0; row < _settings.rows; row++)
        {
            for (int col = 0; col < _settings.columns; col++)
            {
                Vector2? posNullable = _gridPositions.Positions[row, col];
                if (posNullable.HasValue)
                {
                    Vector2 pos = posNullable.Value;
                    if (row < 10)
                    {
                        Bubble bubble = _bubbleFactory.CreateRandomBubble(pos, _parent);
                        Vector2Int indexes = new Vector2Int(row, col);
                        _storage.AddBubble(indexes, bubble);
                    }
                }
                else
                {
                    Debug.Log($"Несуществующий индекс под номером:  {row} {col}.");
                }
            }
        }

        Debug.Log($"Сгенерировано {_storage.GetAllBubbles().Count()} ячеек.");
    }

    private void GenerateFromLevelData()
    {
        
    }

    private void ClearGrid()
    {
        foreach (var bubble in _storage.GetAllBubbles())
        {
            if (bubble != null)
                Object.Destroy(bubble.gameObject);
        }
        _storage.Clear();
    }

}