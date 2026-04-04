using System.Linq;
using UnityEngine;

public class GridGenerator
{
    private readonly GridSettings _settings;
    private readonly BubbleFactory _bubbleFactory;
    private readonly Transform _parent;
    private readonly IBubbleGridStorage _storage;
    private readonly PositionCalculator _positionsCalc;

    private Vector2?[,] _positions;

    public GridGenerator(GridSettings gridSettings, BubbleFactory bubbleFactory, PositionCalculator positionCalculator, Transform parent, IBubbleGridStorage storage)
    {
        _settings = gridSettings;
        _bubbleFactory = bubbleFactory;
        _parent = parent;
        _positionsCalc = positionCalculator;
        _storage = storage;
    }

    public void GenerateGrid()
    {
        if (_settings == null )
        {
            Debug.LogError("Настройкиy не назначены!");
            return;
        }

        ClearGrid();
        
        for (int row = 0; row < _settings.rows; row++)
        {
            for (int col = 0; col < _settings.columns; col++)
            {
                Vector2? posNullable = _positionsCalc.Positions[row, col];
                if (posNullable.HasValue)
                {
                    Vector2 pos = posNullable.Value;
                    Bubble bubble = _bubbleFactory.CreateRandomBubble(_settings.cellPrefab, pos, _parent);
                    Vector2Int indexes = new Vector2Int(row, col);
                    _storage.AddBubble(indexes, bubble);
                }
            }
        }

        Debug.Log($"Сгенерировано {_storage.GetAllBubbles().Count()} ячеек.");
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