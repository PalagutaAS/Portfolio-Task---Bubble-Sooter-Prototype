using UnityEngine;

/// <summary>
/// Конфигурация сетки. Может быть прикреплена к тому же объекту, что и GridGenerator.
/// </summary>

[CreateAssetMenu(fileName = "Grid Setting", menuName = "Game Tools/Settings", order = 0)]

public class GridSettings : ScriptableObject
{
    [Header("Размеры сетки")]
    public int rows = 5;
    public int columns = 8;

    [Header("Размер ячейки")]
    public float cellSize = 1f;

    [Header("Смещение чётных рядов")]
    public float offsetX = 0.5f;
    
    [Header("Смещение рядов по Y")]
    public float verticalOverlap = 0.2f;

    [Header("Префаб")]
    public Bubble cellPrefab;
}