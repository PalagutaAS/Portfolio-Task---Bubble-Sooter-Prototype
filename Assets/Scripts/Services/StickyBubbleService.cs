using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyBubbleService
{
    private readonly IBubbleMatchFinder _matchFinder;
    private readonly IBubbleNeighborFinder _neighborFinder;
    private readonly IBubbleGridStorage _bubbleStorage;
    private readonly IGridPositionService _gridPositions;

    public StickyBubbleService(IBubbleMatchFinder matchFinder, IBubbleNeighborFinder neighborFinder,
        IBubbleGridStorage bubbleStorage, IGridPositionService gridPositions)
    {
        _matchFinder = matchFinder;
        _neighborFinder = neighborFinder;
        _bubbleStorage = bubbleStorage;
        _gridPositions = gridPositions;
    }

    public void OnCollisionBubbles(Bubble bubbleStick, Bubble bubbleStay)
    {
        var emptyList = _neighborFinder.GetEmptyCellNeighbors(bubbleStay);
        //Debug.Log($"Текущая позиция для поиска: {bubbleStick.transform.position}");

        if (emptyList.Count == 0)
            return;
        
        //Debug.Log($"Найденно {emptyList.Count} пустых ячеек");

        List<Vector2> potentialPositions = new List<Vector2>();
        foreach (Vector2Int inds in emptyList)
        {
            potentialPositions.Add(_gridPositions.GetPosition(inds.x, inds.y));
        }
        
        if (potentialPositions.Count == 0)
        {
            Debug.LogWarning("Нет пустых соседних ячеек");
            return;
        }
        
        Vector2 stickPos = bubbleStick.transform.position;
        Vector2 closestPos = potentialPositions[0];
        float minSqrDist = (closestPos - stickPos).sqrMagnitude;

        int index = 0;
        //Debug.Log($"Проверяем ячейку {emptyList[index]} c позицией: {potentialPositions[index]}");
        for (int i = 1; i < potentialPositions.Count; i++)
        {
            float sqrDist = (potentialPositions[i] - stickPos).sqrMagnitude;
            //Debug.Log($"Проверяем ячейку {emptyList[i]} c позицией: {potentialPositions[i]}");
            if (sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                closestPos = potentialPositions[i];
                index = i;
            }
        }
        Debug.Log($"Ближайшая позиция: {closestPos} c ячейкой {emptyList[index]}");
        
        bubbleStick.StartCoroutine(RepositionBubble(bubbleStick, emptyList[index]));
        
    }
    
    private IEnumerator RepositionBubble(Bubble bubble, Vector2Int indices)
    {
        _bubbleStorage.AddBubble(indices, bubble);
        if (_bubbleStorage.TryGetPosition(indices, out Vector2 position))
        {
            bubble.GetComponent<BubbleProjectile>().Launch(Vector2.zero, 0);
            Vector3 startPos = bubble.transform.position;
            Vector3 targetPos = position;
            float duration = 0.05f;
            Debug.Log($"Move to: {targetPos.ToString()} in grid indices: {indices}");
            float speed = Vector3.Distance(startPos, targetPos) / duration;
            while (Vector3.Distance(bubble.transform.position, targetPos) > 0.01f)
            {
                bubble.transform.position = Vector3.MoveTowards(bubble.transform.position, targetPos, speed * Time.deltaTime);
                yield return null;
            }
        }
    }
}
