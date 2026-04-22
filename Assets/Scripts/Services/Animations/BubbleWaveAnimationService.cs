using System.Collections.Generic;
using DG.Tweening;
using ScriptableObjects;
using UnityEngine;

public interface IBubbleWaveAnimationService
{
    void AnimateNeighbors(Vector2Int centerIndices);
    
    void AnimateNeighbors(Bubble bubble);
}

public class BubbleWaveAnimationService : IBubbleWaveAnimationService
{
    private readonly IBubbleNeighborFinder _neighborFinder;
    private readonly IGridPositionService _gridPositions;
    private readonly IBubbleGridStorage _storage;
    private readonly BubbleAnimationSettings _settings;

    public BubbleWaveAnimationService(
        IBubbleNeighborFinder neighborFinder,
        IGridPositionService gridPositions,
        IBubbleGridStorage storage,
        BubbleAnimationSettings settings)
    {
        _neighborFinder = neighborFinder;
        _gridPositions = gridPositions;
        _storage = storage;
        _settings = settings;
    }

    public void AnimateNeighbors(Vector2Int centerIndices)
    {
        Vector2 centerPos = _gridPositions.GetPosition(centerIndices.x, centerIndices.y);

        // --- ПЕРВАЯ ВОЛНА: прямые соседи ---
        List<(Vector2Int indices, Bubble bubble)> firstWave = _neighborFinder.GetExistingNeighbors(centerIndices);
        HashSet<Vector2Int> firstWaveIndices = new HashSet<Vector2Int>();

        foreach (var (idx, bubble) in firstWave)
        {
            if (bubble == null) continue;
            firstWaveIndices.Add(idx);

            if (!_storage.TryGetPosition(idx, out Vector2 originalPos))
                continue;

            Vector2 direction = (originalPos - centerPos).normalized;
            float distance = Vector2.Distance(centerPos, originalPos);
            Vector2 pushedPos = originalPos + direction * (distance * _settings.pushStrength);

            bubble.transform.DOKill();
            Sequence seq = DOTween.Sequence();
            seq.Append(bubble.transform.DOMove(pushedPos, _settings.pushDuration).SetEase(_settings.pushEase));
            seq.AppendInterval(_settings.returnDelay);
            float returnDur = _settings.returnDuration > 0 ? _settings.returnDuration : _settings.pushDuration;
            seq.Append(bubble.transform.DOMove(originalPos, returnDur).SetEase(_settings.returnEase));
            seq.Play();
        }

        // --- ВТОРАЯ ВОЛНА: соседи соседей (кроме центра и первой волны) ---
        HashSet<Vector2Int> secondWaveIndices = new HashSet<Vector2Int>();

        foreach (var (firstIdx, _) in firstWave)
        {
            var neighborsOfFirst = _neighborFinder.GetExistingNeighbors(firstIdx);
            foreach (var (secondIdx, _) in neighborsOfFirst)
            {
                // Исключаем центр и уже анимированные первой волной
                if (secondIdx == centerIndices || firstWaveIndices.Contains(secondIdx))
                    continue;

                secondWaveIndices.Add(secondIdx);
            }
        }

        // Анимация второй волны
        float secondStrength = _settings.pushStrength * _settings.secondWaveStrengthMultiplier;
        float returnDur2 = _settings.returnDuration > 0 ? _settings.returnDuration : _settings.pushDuration;

        foreach (var secondIdx in secondWaveIndices)
        {
            if (!_storage.TryGetBubble(secondIdx, out Bubble secondBubble) || secondBubble == null)
                continue;

            if (!_storage.TryGetPosition(secondIdx, out Vector2 originalPos))
                continue;

            // Направление толчка — от центральной ячейки
            Vector2 direction = (originalPos - centerPos).normalized;
            float distance = Vector2.Distance(centerPos, originalPos);
            Vector2 pushedPos = originalPos + direction * (distance * secondStrength);

            secondBubble.transform.DOKill();
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(_settings.secondWaveDelay);
            seq.Append(secondBubble.transform.DOMove(pushedPos, _settings.pushDuration).SetEase(_settings.pushEase));
            seq.AppendInterval(_settings.returnDelay);
            seq.Append(secondBubble.transform.DOMove(originalPos, returnDur2).SetEase(_settings.returnEase));
            seq.Play();
        }
    }

    public void AnimateNeighbors(Bubble bubble)
    {
        if (bubble == null)
        {
            Debug.LogWarning("Попытка анимировать соседей для null пузыря");
            return;
        }

        if (_storage.TryGetIndices(bubble, out Vector2Int indices))
        {
            AnimateNeighbors(indices);
            return;
        }

        
        Debug.LogWarning($"Пузырь {bubble.name} не найден в хранилище сетки");
        
    }
}