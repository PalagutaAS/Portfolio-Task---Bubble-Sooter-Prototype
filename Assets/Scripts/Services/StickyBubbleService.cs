using System.Collections.Generic;
using UnityEngine;

public interface IStickyBubbleService
{
    void AttachToCell(Bubble bubble, Vector2Int resultTargetCell);
}

public class StickyBubbleService : IStickyBubbleService
{
    private readonly IBubbleMatchFinder _matchFinder;
    private readonly IBubbleGridStorage _bubbleStorage;
    private readonly IFloatingBubbleRemover _floatingRemover;
    private readonly IBubbleWaveAnimationService _bubbleWaveAnimation;

    public StickyBubbleService(IBubbleMatchFinder matchFinder,
        IBubbleGridStorage bubbleStorage, IFloatingBubbleRemover floatingRemover, IBubbleWaveAnimationService bubbleWaveAnimation)
    {
        _matchFinder = matchFinder;
        _bubbleStorage = bubbleStorage;
        _floatingRemover = floatingRemover;
        _bubbleWaveAnimation = bubbleWaveAnimation;
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
            
            int floatingRemoved = _floatingRemover.RemoveFloatingBubbles();
            Debug.Log($"Удалено висячих пузырей: {floatingRemoved}");
            
            // Запускаем проверку на соприкосновение с потолком,
            // но не знаю как именно это сделать/реализовать, знаю что нужен отдельный сервис на это
        }

        _bubbleWaveAnimation.AnimateNeighbors(cellIndices);
    }
}
