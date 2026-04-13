using System.Collections.Generic;
using UnityEngine;

public interface IStickyBubbleService
{
    void AttachToCell(Bubble bubble, Vector2Int resultTargetCell);
}

public class StickyBubbleService : IStickyBubbleService
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

    public void AttachToCell(Bubble bubble, Vector2Int cellIndices)
    {
        _bubbleStorage.AddBubble(cellIndices, bubble);
        if (_bubbleStorage.TryGetPosition(cellIndices, out Vector2 pos))
        {
            bubble.transform.position = pos;
        }

        List<Bubble> listBubble = _matchFinder.FindMatchingBubbles(bubble);
        
        if (listBubble.Count >= 3)
        {
            foreach (Bubble b in listBubble)
            {
                _bubbleStorage.RemoveBubble(b);
            }
            
            // Запускаем проверку на соприкосновение с потолком,
            // но не знаю как именно это сделать/реализовать, снаю что нужен отдельный сервис на это
        }
    }
}
