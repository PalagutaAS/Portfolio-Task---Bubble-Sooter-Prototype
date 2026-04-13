using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleLauncher : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _nextBubblePoint;
    [SerializeField] private int _maxShots = 10;
    
    private IBubbleFactoryRandom _bubbleFactory;
    private IBubbleGridStorage _bubbleStorage;
    private ITrajectoryPredictor _trajectoryPredictor;
    private IStickyBubbleService _stickyBubbleService;
    
    private Bubble _currentBubble;
    private Queue<Bubble> _bubbleQueue;
    private int _shotsRemaining;
    
    public Bubble CurrentBubble => _currentBubble;
    
    public Transform FirePoint => _firePoint;
    
    public void Constructor(IBubbleFactoryRandom bubbleFactory, ITrajectoryPredictor trajectoryPredictor, IBubbleGridStorage bubbleGridStorage, IStickyBubbleService stickyBubbleService)
    {
        _bubbleFactory = bubbleFactory;
        _trajectoryPredictor = trajectoryPredictor;
        _bubbleStorage = bubbleGridStorage;
        _stickyBubbleService = stickyBubbleService;
        
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
    
    public void Shoot(Vector2 direction, float shotSpeed)
    {
        if (_shotsRemaining <= 0 || _currentBubble == null) return;

        _shotsRemaining--;
        Bubble shotBubble = _currentBubble;
        _currentBubble = null;

        ShotResult result = _trajectoryPredictor.Predict(_firePoint.position, direction, shotSpeed);
        
        StartCoroutine(AnimateShot(shotBubble, result));
    }
    
    private IEnumerator AnimateShot(Bubble bubble, ShotResult result)
    {
        List<Vector2> path = result.trajectory;
        if (path.Count < 2) yield break;

        float speed = result.shotSpeed;
        
        for (int i = 1; i < path.Count; i++)
        {
            Vector2 start = path[i - 1];
            Vector2 end = path[i];
            float duration = result.duration / path.Count;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                bubble.transform.position = Vector2.Lerp(start, end, t);
                yield return null;
            }
            bubble.transform.position = end;
        }

        if (result.hit)
        {
            _stickyBubbleService.AttachToCell(bubble, result.targetCell);
        }
        else
        {
            Destroy(bubble.gameObject);
        }
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
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