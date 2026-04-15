using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private GridSettings _gridSettings;
    [SerializeField] private Transform _gridTransform;
    [SerializeField] private GameLoop _gameLoop;
    [SerializeField] private GameOverUI _gameOverUI;
    [SerializeField] private BubbleLauncher _launcher;
    [SerializeField] private TrajectoryRenderer _trajectoryRenderer;
    [SerializeField] private TrajectorySettings _trajectorySettings;
    [SerializeField] private BubbleAnimationSettings _bubbleAnimationSettings;

    private GridGenerator _grid;
    private IBubbleFactoryRandom _bubbleFactory;
    private IBubbleGridStorage _bubbleStorage;
    private IBubbleNeighborFinder _neighborFinder;
    private IGridPositionService _gridPositions;
    private IBubbleMatchFinder _matchFinder;
    private IStickyBubbleService _stickyBubbleService;
    private ITrajectoryPredictor _trajectoryPredictor;
    private ICollisionDetector _collisionDetector;
    private IFloatingBubbleRemover _floatingBubbleRemover;
    private IBubbleWaveAnimationService _bubbleWaveAnimationService;
    private IBubbleBoomAnimationService _bubbleBoomAnimationService;
    private IBubbleFlightAnimator _bubbleFlightAnimator;

    private void Awake()
    {
        CreateAssets();
    }

    private void CreateAssets()
    {        
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(5, 10, 0));
        bounds.min = new Vector3(bounds.min.x, bounds.min.y - 2, bounds.min.z);
        
        _bubbleFlightAnimator = new BubbleFlightAnimator();
        _bubbleBoomAnimationService = new BubbleBoomAnimationService();
        _gridPositions = new GridPositions(_gridSettings, _gridTransform);
        _bubbleStorage = new BubbleGridStorage(_gridPositions, _bubbleBoomAnimationService);
        _neighborFinder = new HexNeighborFinder(_gridSettings.rows, _gridSettings.columns, _bubbleStorage);
        _matchFinder = new BubbleMatchFinder(_bubbleStorage, _neighborFinder);
        _floatingBubbleRemover = new FloatingBubbleRemover(_bubbleStorage,_neighborFinder);
        _bubbleWaveAnimationService =
            new BubbleWaveAnimationService(_neighborFinder, _gridPositions, _bubbleStorage, _bubbleAnimationSettings);
        _stickyBubbleService = new StickyBubbleService(_matchFinder, _bubbleStorage, _floatingBubbleRemover, _bubbleWaveAnimationService);
        _bubbleFactory = new BubbleFactory(_gridSettings.Prefab, _gridTransform);
        _collisionDetector = new CollisionDetector(_bubbleStorage, _trajectorySettings.radiusBubble);
        
        _trajectoryPredictor = new TrajectoryPredictor(_bubbleStorage, _neighborFinder, _gridPositions, _trajectorySettings, _collisionDetector, bounds);
        _grid = new GridGenerator(_gridSettings, _bubbleFactory, _gridPositions, _bubbleStorage);
        _trajectoryRenderer.Initialize(_trajectoryPredictor, _trajectorySettings);
        _launcher.Constructor(_bubbleFactory, _trajectoryPredictor);
        
        //BubbleDebugger bubbleDebugger = new GameObject("BUBBLE DEBUGGER").AddComponent<BubbleDebugger>();
        //bubbleDebugger.Constructor(_bubbleStorage, _neighborFinder, _matchFinder);
    }

    private void Start()
    {
        _gameLoop.Constructor(_stickyBubbleService, _bubbleFlightAnimator, _grid, _gameOverUI, _launcher);
    }

    [ContextMenu("RegenerateGrid")]
    private void RegenerateGrid()
    {
        _grid.GenerateRandomBubbles();
    }
}
