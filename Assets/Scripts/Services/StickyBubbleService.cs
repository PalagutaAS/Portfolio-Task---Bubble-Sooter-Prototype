using System.Collections.Generic;
using UnityEngine;

public interface IStickyBubbleService
{
    List<Bubble> AttachToCell(Bubble bubble, Vector2Int resultTargetCell);
}

public class StickyBubbleService : IStickyBubbleService
{
    private readonly IBubbleMatchFinder _matchFinder;
    private readonly IBubbleGridStorage _bubbleStorage;
    private readonly IFloatingBubbleRemover _floatingRemover;
    private readonly IBubbleWaveAnimationService _bubbleWaveAnimation;
    private readonly IScoreService _scoreService;

    public StickyBubbleService(IBubbleMatchFinder matchFinder,
        IBubbleGridStorage bubbleStorage, IFloatingBubbleRemover floatingRemover, IBubbleWaveAnimationService bubbleWaveAnimation, IScoreService scoreService)
    {
        _matchFinder = matchFinder;
        _bubbleStorage = bubbleStorage;
        _floatingRemover = floatingRemover;
        _bubbleWaveAnimation = bubbleWaveAnimation;
        _scoreService = scoreService;
    }

    public List<Bubble> AttachToCell(Bubble bubble, Vector2Int cellIndices)
    {
        _bubbleStorage.AddBubble(cellIndices, bubble);
        if (_bubbleStorage.TryGetPosition(cellIndices, out Vector2 pos))
        {
            bubble.transform.position = pos;
        }

        List<Bubble> listBubble = _matchFinder.FindMatchingBubbles(bubble);
        int countMatch = listBubble.Count;
        if (countMatch >= 3)
        {
            foreach (Bubble b in listBubble)
            {
                _bubbleStorage.RemoveBubble(b);
            }
            
            int floatingRemoved = _floatingRemover.RemoveFloatingBubbles();
            _scoreService.CalculateScore(countMatch, floatingRemoved);
            
            // Запускаем проверку на соприкосновение с потолком,
            // но не знаю как именно это сделать/реализовать, знаю что нужен отдельный сервис на это
            // Возможно вынести часть в отдельные сервисы, много ответственности
        }

        _bubbleWaveAnimation.AnimateNeighbors(cellIndices);
        return listBubble;
    }
}
