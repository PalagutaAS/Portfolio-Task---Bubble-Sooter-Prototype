using UnityEngine;

public class BubbleLauncher : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _nextBubblePoint;
    [SerializeField] private int _maxShots = 10;
    
    private BubbleFactory _bubbleFactory;
    private Transform _parentForBubbles;
    
    private Bubble _currentBubble;
    private Bubble _nextBubble;
    private int _shotsRemaining;
    
    public Bubble CurrentBubble => _currentBubble;
    public Bubble NextBubble => _nextBubble;
    public Transform FirePoint => _firePoint;
    
    /// <summary>
    /// Инициализация лаунчера. Вызывать после создания фабрики.
    /// </summary>
    public void Constructor(BubbleFactory bubbleFactory, Transform parentForBubbles)
    {
        _bubbleFactory = bubbleFactory;
        _parentForBubbles = transform;

        LoadInitialBubbles();
        _shotsRemaining = _maxShots;
    }
    
    private void LoadInitialBubbles()
    {
        if (_bubbleFactory == null)
        {
            Debug.LogError("BubbleLauncher: фабрика не инициализирована!");
            return;
        }
        
        _currentBubble = CreateRandomBubbleAtPoint(_firePoint.position);
        _nextBubble = CreateRandomBubbleAtPoint(_nextBubblePoint.position);
    }
    
    private Bubble CreateRandomBubbleAtPoint(Vector3 position)
    {
        Bubble bubble = _bubbleFactory.CreateRandomBubble(position, _parentForBubbles);
        return bubble;
    }
    
    /// <summary>
    /// Выстрелить текущим пузырём. Возвращает пузырь, который должен полететь.
    /// После вызова лаунчер автоматически заменяет текущий пузырь следующим и генерирует новый следующий.
    /// </summary>
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
        
        ReplaceCurrentWithNext();
        
        GenerateNextBubble();
        
        return shotBubble;
    }
    
    private void ReplaceCurrentWithNext()
    {
        if (_nextBubble == null)
            return;

        _nextBubble.transform.position = _firePoint.position;
        _currentBubble = _nextBubble;
        _nextBubble = null;
    }
    
    private void GenerateNextBubble()
    {
        if (_shotsRemaining - 1 > 0)
        {
            _nextBubble = CreateRandomBubbleAtPoint(_nextBubblePoint.position);
        }
    }
    
    /// <summary>
    /// Принудительно обновить очередь (например, после того, как выстреленный пузырь был уничтожен логикой игры).
    /// </summary>
    public void ReloadNextBubble()
    {
        if (_nextBubble != null)
            Destroy(_nextBubble.gameObject);
        
        GenerateNextBubble();
    }
    
    /// <summary>
    /// Очистить лаунчер (при перезапуске уровня).
    /// </summary>
    public void Clear()
    {
        if (_currentBubble != null)
            Destroy(_currentBubble.gameObject);
        if (_nextBubble != null)
            Destroy(_nextBubble.gameObject);
        _currentBubble = null;
        _nextBubble = null;
        _shotsRemaining = _maxShots;
    }
}