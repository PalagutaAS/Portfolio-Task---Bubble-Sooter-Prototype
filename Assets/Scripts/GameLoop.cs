using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Основной цикл игры Bubble Shooter.
/// Состояния: IDLE → SHOOT → STICK → MATCH → FALL → CHECK_GAMEOVER → SPAWN_NEXT → IDLE
/// </summary>
public class GameLoop : MonoBehaviour
{
    public enum GameState
    {
        IDLE,           // Ожидание ввода игрока
        SHOOT,          // Выстрел (пуля летит)
        STICK,          // Привязка пузыря к сетке
        MATCH,          // Поиск и удаление совпадений (3+ одинакового цвета)
        FALL,           // Удаление отсоединённых пузырей (не связанных с потолком)
        CHECK_GAMEOVER, // Проверка окончания игры
        SPAWN_NEXT      // Подготовка следующего пузыря
    }

    [Header("Настройки выстрела")]
    [SerializeField] private float _shootSpeed = 10f;
    [SerializeField] private int _maxBounces = 10;

    private GameState _currentState = GameState.IDLE;
    
    private IBubbleGridStorage _storage;
    private IBubbleNeighborFinder _neighborFinder;
    private IBubbleMatchFinder _matchFinder;
    
    private Shooter _shooter;
    private TrajectoryCalculator _trajectoryCalc;
    private SnapToGrid _snapToGrid;
    
    private Bubble _currentBubble;
    private BubbleColor _nextBubbleColor;
    private BubbleColor _currentBubbleColor;
    
    private int _bubblesRemaining = 10; // Количество шаров для выстрела
    private int _score = 0;

    public GameState CurrentState => _currentState;
    public int Score => _score;
    public int BubblesRemaining => _bubblesRemaining;
    public BubbleColor CurrentBubbleColor => _currentBubbleColor;
    public BubbleColor NextBubbleColor => _nextBubbleColor;

    private void Awake()
    {
        Debug.Log("GameLoop Awake");
    }

    public void Constructor(
        IBubbleGridStorage bubbleStorage, 
        IBubbleNeighborFinder neighborFinder, 
        IBubbleMatchFinder matchFinder,
        GridSettings gridSettings,
        BubbleFactory bubbleFactory,
        Transform bubbleParent)
    {
        _storage = bubbleStorage;
        _neighborFinder = neighborFinder;
        _matchFinder = matchFinder;

        _trajectoryCalc = new TrajectoryCalculator(gridSettings);
        _snapToGrid = new SnapToGrid(gridSettings, bubbleStorage, neighborFinder);
        _shooter = new Shooter(bubbleFactory, bubbleParent, gridSettings);

        // Инициализация цветов пузырей
        InitializeBubbleColors();
        
        Debug.Log("GameLoop Constructor initialized");
    }

    private void Start()
    {
        Debug.Log("GameLoop Start");
        _shooter.CreateShooterVisual();
        SpawnCurrentBubble();
        EnterState(GameState.IDLE);
    }

    private void Update()
    {
        switch (_currentState)
        {
            case GameState.IDLE:
                HandleIdleState();
                UpdateAimVisual();
                break;
            case GameState.SHOOT:
                HandleShootState();
                break;
        }
    }

    private void UpdateAimVisual()
    {
        if (_currentBubble != null)
        {
            Vector2 aimDirection = GetAimDirection();
            if (aimDirection != Vector2.zero)
            {
                _shooter.UpdateAimLineVisual(aimDirection);
            }
        }
    }

    private void HandleIdleState()
    {
        // Ожидание нажатия игрока для выстрела
        if (Input.GetMouseButtonDown(0) && _currentBubble != null)
        {
            Vector2 shootDirection = _shooter.GetAimDirection();
            if (shootDirection != Vector2.zero)
            {
                EnterState(GameState.SHOOT);
                _shooter.ShootBubble(_currentBubble, shootDirection, _shootSpeed, OnBubbleStopped);
            }
        }
    }

    private void HandleShootState()
    {
        // Логика выстрела обрабатывается в Shooter
        // State переключится в STICK через callback OnBubbleStopped
    }

    private void OnBubbleStopped(Vector2 position)
    {
        StartCoroutine(ProcessStickAndFollow(position));
    }

    private IEnumerator ProcessStickAndFollow(Vector2 hitPosition)
    {
        EnterState(GameState.STICK);
        
        // Привязка пузыря к ближайшей ячейке
        Vector2Int targetIndices = _snapToGrid.FindNearestEmptyCell(_currentBubble.transform.position);
        
        if (targetIndices.y >= 0)
        {
            _storage.AddBubble(targetIndices, _currentBubble);
            _shooter.UpdateCurrentBubblePosition(targetIndices);
        }
        else
        {
            Debug.LogWarning("Не удалось найти ячейку для пузыря!");
            _storage.AddBubble(targetIndices, _currentBubble);
        }

        yield return new WaitForSeconds(0.1f);

        // Переход к проверке совпадений
        EnterState(GameState.MATCH);
        yield return StartCoroutine(ProcessMatch());
    }

    private IEnumerator ProcessMatch()
    {
        // Поиск совпадений по цвету
        List<Bubble> matchedBubbles = _matchFinder.FindMatchingBubbles(_currentBubble);

        if (matchedBubbles.Count >= 3)
        {
            // Удаление совпавших пузырей
            _score += matchedBubbles.Count * 10;
            
            foreach (var bubble in matchedBubbles)
            {
                _storage.RemoveBubble(bubble);
                Destroy(bubble.gameObject);
            }
            
            Debug.Log($"MATCH: Удалено {matchedBubbles.Count} пузырей. Очки: {_score}");
            yield return new WaitForSeconds(0.3f);
        }

        // Переход к удалению отсоединённых пузырей
        EnterState(GameState.FALL);
        yield return StartCoroutine(ProcessFall());
    }

    private IEnumerator ProcessFall()
    {
        // Поиск и удаление пузырей, не связанных с потолком
        List<Bubble> disconnectedBubbles = FindDisconnectedBubbles();
        
        if (disconnectedBubbles.Count > 0)
        {
            _score += disconnectedBubbles.Count * 20; // Бонус за падение
            
            foreach (var bubble in disconnectedBubbles)
            {
                _storage.RemoveBubble(bubble);
                StartCoroutine(AnimateFall(bubble));
            }
            
            Debug.Log($"FALL: Упало {disconnectedBubbles.Count} пузырей. Очки: {_score}");
            yield return new WaitForSeconds(0.5f);
        }

        // Переход к проверке GameOver
        EnterState(GameState.CHECK_GAMEOVER);
        yield return StartCoroutine(ProcessGameOver());
    }

    private List<Bubble> FindDisconnectedBubbles()
    {
        List<Bubble> disconnected = new List<Bubble>();
        
        foreach (var bubble in _storage.GetAllBubbles())
        {
            if (_storage.TryGetIndices(bubble, out Vector2Int indices))
            {
                // Если пузырь не связан с верхним рядом - он падает
                if (!_neighborFinder.IsConnectedToTopRow(indices))
                {
                    disconnected.Add(bubble);
                }
            }
        }
        
        return disconnected;
    }

    private IEnumerator AnimateFall(Bubble bubble)
    {
        Vector2 startPos = bubble.transform.position;
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            bubble.transform.position = Vector2.Lerp(startPos, startPos + Vector2.down * 5f, t);
            yield return null;
        }
        
        Destroy(bubble.gameObject);
    }

    private IEnumerator ProcessGameOver()
    {
        bool isGameOver = CheckGameOver();
        
        if (isGameOver)
        {
            Debug.Log($"GAME OVER! Финальный счёт: {_score}");
            // Здесь можно вызвать UI Game Over экрана
            yield break;
        }

        // Переход к подготовке следующего пузыря
        EnterState(GameState.SPAWN_NEXT);
        yield return StartCoroutine(ProcessSpawnNext());
    }

    private bool CheckGameOver()
    {
        // Игра окончена если:
        // 1. Закончились шары для выстрела
        if (_bubblesRemaining <= 0)
            return true;
        
        // 2. Пузыри достигли критической линии (низ экрана)
        // Здесь можно добавить проверку
        foreach (var bubble in _storage.GetAllBubbles())
        {
            if (_storage.TryGetPosition(bubble, out Vector2 pos))
            {
                // Если пузырь слишком низко - игра окончена
                if (pos.y < -4f) // Порог нужно настроить
                    return true;
            }
        }
        
        return false;
    }

    private IEnumerator ProcessSpawnNext()
    {
        // Подготовка текущего и следующего пузыря
        _currentBubbleColor = _nextBubbleColor;
        _nextBubbleColor = GetRandomBubbleColor();
        
        _bubblesRemaining--;
        
        SpawnCurrentBubble();
        
        // Обновляем превью следующего пузыря
        _shooter.UpdateNextBubblePreview(_nextBubbleColor);
        
        Debug.Log($"SPAWN: Осталось шаров: {_bubblesRemaining}");
        
        yield return new WaitForSeconds(0.2f);
        
        // Возврат в IDLE
        EnterState(GameState.IDLE);
    }

    private void SpawnCurrentBubble()
    {
        if (_currentBubble == null)
        {
            _currentBubble = _shooter.SpawnBubbleAtLauncher(_currentBubbleColor);
        }
        else
        {
            _shooter.SetBubbleColor(_currentBubble, _currentBubbleColor);
            _shooter.ResetBubblePosition();
        }
    }

    private void EnterState(GameState newState)
    {
        Debug.Log($"GameLoop: {_currentState} → {newState}");
        _currentState = newState;
    }

    private void InitializeBubbleColors()
    {
        _currentBubbleColor = GetRandomBubbleColor();
        _nextBubbleColor = GetRandomBubbleColor();
    }

    private BubbleColor GetRandomBubbleColor()
    {
        BubbleColor[] colors = (BubbleColor[])System.Enum.GetValues(typeof(BubbleColor));
        return colors[Random.Range(0, colors.Length)];
    }

    // Метод для получения направления прицеливания (для визуализации)
    public Vector2 GetAimDirection()
    {
        return _shooter.GetAimDirection();
    }

    // Метод для получения точки выстрела
    public Vector3 GetShootOrigin()
    {
        return _shooter.GetShootOrigin();
    }
}
