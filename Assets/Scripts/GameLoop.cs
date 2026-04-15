using System.Collections;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    private enum GameState
    {
        Playing,
        ProcessingShot,
        GameOver
    }
    
    private GridGenerator _gridGenerator;
    private GameOverUI _gameOverUI;
    private BubbleLauncher _bubbleLauncher;
    private GameState _currentState;
    private IStickyBubbleService _stickyBubbleService;
    private IBubbleFlightAnimator _flightAnimator;
    private bool _gameOverPending;
    private IScore _score;

    public void Constructor(
        IStickyBubbleService stickyBubbleService,
        IBubbleFlightAnimator flightAnimator,
        IScore score,
        GridGenerator gridGenerator,
        GameOverUI gameOverUI,
        BubbleLauncher launcher)
    {
        _gridGenerator = gridGenerator;
        _bubbleLauncher = launcher;
        _stickyBubbleService = stickyBubbleService;
        _flightAnimator = flightAnimator;
        _score = score;
        _gameOverUI = gameOverUI;
        
        _bubbleLauncher.OnShotProcessed += HandleShotProcessed;
        _bubbleLauncher.OnOutOfShots += HandleShotsEmpty;
        gameObject.SetActive(true);
    }

    private void HandleShotsEmpty()
    {
        _gameOverPending = true;
    }

    private void HandleShotProcessed(Bubble shotBubble, ShotResult shotResult)
    {
        if (_currentState != GameState.Playing) return;
        _currentState = GameState.ProcessingShot;

        StartCoroutine(ProcessShotCoroutine(shotBubble, shotResult));
    }

    private void Start()
    {
        StartNewGame();
    }
    
    public void StartNewGame()
    {
        StopAllCoroutines();
        _gameOverPending = false;
        _score.ResetScore();
        
        _currentState = GameState.Playing;
        
        _gridGenerator.GenerateRandomBubbles();
        _bubbleLauncher.Clear();
        _bubbleLauncher.LoadInitialBubbles();
        
        _gameOverUI.HideGameOver();
    }
    
    private IEnumerator ProcessShotCoroutine(Bubble shotBubble, ShotResult result)
    {
        yield return _flightAnimator.AnimateFlight(shotBubble, result);

        if (result.hit)
            _stickyBubbleService.AttachToCell(shotBubble, result.targetCell);
        else
            Destroy(shotBubble.gameObject);

        if (_gameOverPending)
        {
            Invoke(nameof(GameOverState),0.5f);
            yield break;
        }
        
        _bubbleLauncher.Reload();
        _currentState = GameState.Playing;
    }

    private void GameOverState()
    {
        if (_currentState != GameState.ProcessingShot)
            return;
        
        _currentState = GameState.GameOver;
        _gameOverUI.ShowGameOver();
    }
    
    private void OnDestroy()
    {
        if (_bubbleLauncher != null)
        {
            _bubbleLauncher.OnShotProcessed -= HandleShotProcessed;
            _bubbleLauncher.OnOutOfShots -= HandleShotsEmpty;
        }
    }
}