using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleLauncher : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _nextBubblePoint;
    [SerializeField] private int _maxShots = 10;
    
    private IBubbleFactoryRandom _bubbleFactory;
    private ITrajectoryPredictor _trajectoryPredictor;
    
    private Bubble _currentBubble;
    private Queue<Bubble> _bubbleQueue;
    private int _shotsRemaining;

    private int ShotsRemaining
    {
        get => _shotsRemaining;
        set
        {
            _shotsRemaining = value;
            OnShotsCountChanged?.Invoke(_shotsRemaining);
            if (_shotsRemaining == 0)
                OnOutOfShots?.Invoke();
        }
    }

    public Bubble CurrentBubble => _currentBubble;
    public event Action<Bubble, ShotResult> OnShotProcessed;
    public event Action OnOutOfShots;
    public event Action<int> OnShotsCountChanged;
    public Transform FirePoint => _firePoint;
    
    public void Constructor(IBubbleFactoryRandom bubbleFactory, ITrajectoryPredictor trajectoryPredictor)
    {
        _bubbleFactory = bubbleFactory;
        _trajectoryPredictor = trajectoryPredictor;
        
        Clear();
    }

    public void LoadInitialBubbles()
    {
        if (_bubbleFactory == null)
        {
            Debug.LogError("BubbleLauncher: фабрика не инициализирована!");
            return;
        }

        _bubbleQueue = new Queue<Bubble>(_maxShots);
        for (int i = 0; i < _maxShots; i++)
        {
            _bubbleQueue.Enqueue(CreateRandomBubbleAtPoint(_nextBubblePoint.position));
        }

        ReplaceCurrentWithNext();
    }
    
    private Bubble CreateRandomBubbleAtPoint(Vector3 position)
    {
        var bubble = _bubbleFactory.CreateBubble(position);
        bubble.gameObject.SetActive(false);
        return bubble;
    } 
    
    public void Shoot(Vector2 direction, float shotSpeed)
    {
        if (ShotsRemaining <= 0 || _currentBubble == null) return;

        
        Bubble shotBubble = _currentBubble;
        _currentBubble = null;

        ShotResult result = _trajectoryPredictor.Predict(_firePoint.position, direction, shotSpeed);
        OnShotProcessed?.Invoke(shotBubble, result);
        
        ShotsRemaining--;

    }
    
    public void Reload()
    {
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        ReplaceCurrentWithNext();
    }
    
    private void ReplaceCurrentWithNext()
    {
        if (_bubbleQueue.Count == 0)
            return;

        _currentBubble = _bubbleQueue.Dequeue();
        _currentBubble.transform.position = _firePoint.position;
        _currentBubble.gameObject.SetActive(true);

        if (_bubbleQueue.Count > 0)
        {
            _bubbleQueue.Peek().gameObject.SetActive(true);
        }
    }
    
    public void Clear()
    {
        if (_currentBubble != null)
            Destroy(_currentBubble.gameObject);
        _currentBubble = null;
        ShotsRemaining = _maxShots;
    }

    public void ResetCurrentBubblePosition()
    {
        if (CurrentBubble == null)
            return;
        
        CurrentBubble.transform.position = FirePoint.position;
    }
}