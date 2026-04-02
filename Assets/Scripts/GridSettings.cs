using UnityEngine;

/// <summary>
/// Конфигурация сетки. Может быть прикреплена к тому же объекту, что и GridGenerator.
/// </summary>
public class GridSettings : MonoBehaviour
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
    public GameObject cellPrefab; 

    [Header("Родительский объект (опционально)")]
    public Transform parentTransform;
}