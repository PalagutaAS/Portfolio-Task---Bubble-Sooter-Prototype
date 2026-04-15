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
    //private GameUI _gameUI;
    private BubbleLauncher _bubbleLauncher;
    private IBubbleGridStorage _bubbleStorage;
    private GameState _currentState;
    private IStickyBubbleService _stickyBubbleService;
    private IBubbleFlightAnimator _flightAnimator;

    private void Awake()
    {
        Debug.Log("Awake");
        
    }
    
    public void Constructor(
        IBubbleGridStorage bubbleStorage,
        IBubbleNeighborFinder neighborFinder,
        IBubbleMatchFinder matchFinder,
        IStickyBubbleService stickyBubbleService,
        IBubbleFlightAnimator flightAnimator,
        GridGenerator gridGenerator,
        BubbleLauncher launcher)
    {
        _bubbleStorage = bubbleStorage;
        _gridGenerator = gridGenerator;
        _bubbleLauncher = launcher;
        _stickyBubbleService = stickyBubbleService;
        _flightAnimator = flightAnimator;
        //_gameUI = gameUI;
        
        _bubbleLauncher.OnShotProcessed += HandleShotProcessed;
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
    
    private void StartNewGame()
    {
        _currentState = GameState.Playing;
        
        _gridGenerator.GenerateRandomBubbles();
        _bubbleLauncher.Clear();
        _bubbleLauncher.LoadInitialBubbles();
        
        //_gameUI.HideGameOver();
    }
    
    private IEnumerator ProcessShotCoroutine(Bubble shotBubble, ShotResult result)
    {
        yield return _flightAnimator.AnimateFlight(shotBubble, result);

        if (result.hit)
            _stickyBubbleService.AttachToCell(shotBubble, result.targetCell);
        else
            Destroy(shotBubble.gameObject);

        if (_bubbleLauncher.ShotsRemaining == 0)
        {
            GameOverState();
            yield break;
        }
        
        _bubbleLauncher.Reload();
        _currentState = GameState.Playing;
    }

    private void GameOverState()
    {
        if (_currentState != GameState.ProcessingShot)
            return;
        
        Debug.Log("GAME OVER");
        _currentState = GameState.GameOver;
        //_gameUI.ShowGameOver();
    }
}