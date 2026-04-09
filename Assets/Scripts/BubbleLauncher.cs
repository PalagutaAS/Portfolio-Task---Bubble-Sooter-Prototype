using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleLauncher : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _nextBubblePoint;
    [SerializeField] private int _maxShots = 10;
    
    private IBubbleFactoryRandom _bubbleFactory;
    
    private Bubble _currentBubble;
    private Queue<Bubble> _bubbleQueue;
    private int _shotsRemaining;
    
    public Bubble CurrentBubble => _currentBubble;
    
    public Transform FirePoint => _firePoint;
    
    public void Constructor(IBubbleFactoryRandom bubbleFactory)
    {
        _bubbleFactory = bubbleFactory;
        
        Clear();
        LoadInitialBubbles();
    }
    
    private void LoadInitialBubbles()
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
    
    public Bubble Shoot()
    {
        if (_shotsRemaining <= 0)
        {
            Debug.LogWarning("Нет доступных выстрелов! Достигнут лимит.");
            return null;
        }
        
        if (_currentBubble == null)
        {
            Debug.LogWarning("Нет текущего пузыря для выстрела!");
            return null;
        }

        Bubble shotBubble = _currentBubble;
        _shotsRemaining--;

        StartCoroutine(ReloadCoroutine());
        
        return shotBubble;
    }

    private IEnumerator ReloadCoroutine()
    {
        _currentBubble = null;
        yield return new WaitForSeconds(1f);
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
        _shotsRemaining = _maxShots;
    }
}