using ScriptableObjects;
using UnityEngine;

public class GridGenerator
{
    private readonly GridSettings _settings;
    private readonly IBubbleFactoryRandom _bubbleFactory;
    private readonly IBubbleGridStorage _storage;
    private readonly IGridPositionService _gridPositions;
    
    public int Rows => _settings.rows;
    public int Columns => _settings.columns;

    public GridGenerator(GridSettings gridSettings, IBubbleFactoryRandom bubbleFactory, IGridPositionService gridPositionCalculator, IBubbleGridStorage storage)
    {
        _settings = gridSettings;
        _bubbleFactory = bubbleFactory;
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
                Vector2 pos = _gridPositions.GetPosition(row, col);
                if (pos != Vector2.zero)
                {
                    if (row < 12)
                    {
                        Bubble bubble = _bubbleFactory.CreateBubble(pos);
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