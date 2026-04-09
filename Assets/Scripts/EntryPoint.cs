using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private GridSettings _gridSettings;
    [SerializeField] private Transform _gridTransform;
    [SerializeField] private BubbleLauncher _launcher;

    private GridGenerator _grid;
    private IBubbleFactoryRandom _bubbleFactory;
    private IBubbleGridStorage _bubbleStorage;
    private IBubbleNeighborFinder _neighborFinder;
    private IGridPositionService _gridPositions;
    private IBubbleMatchFinder _matchFinder;
    private StickyBubbleService _stickyBubble;

    private void Awake()
    {
        CreateAssets();
    }

    private void CreateAssets()
    {
        _gridPositions = new GridPositions(_gridSettings, _gridTransform);
        _bubbleStorage = new BubbleGridStorage(_gridPositions);
        _neighborFinder = new HexNeighborFinder(_gridSettings.rows, _gridSettings.columns, _bubbleStorage);
        _matchFinder = new BubbleMatchFinder(_bubbleStorage, _neighborFinder);
        _stickyBubble = new StickyBubbleService(_matchFinder, _neighborFinder, _bubbleStorage, _gridPositions);
        _bubbleFactory = new BubbleFactory(_gridSettings.Prefab, _stickyBubble, _gridTransform);

        _grid = new GridGenerator(_gridSettings, _bubbleFactory, _gridPositions, _bubbleStorage);
        _grid.GenerateRandomBubbles();
        
        _launcher.Constructor(_bubbleFactory);
        
        BubbleDebugger bubbleDebugger = new GameObject("BUBBLE DEBUGGER").AddComponent<BubbleDebugger>();
        bubbleDebugger.Constructor(_bubbleStorage, _neighborFinder, _matchFinder);
    }

    private void Start()
    {
        GameLoop gameLoop = new GameObject("GAME LOOP").AddComponent<GameLoop>();
        gameLoop.Constructor(_bubbleStorage, _neighborFinder, _matchFinder);
    }

    [ContextMenu("RegenerateGrid")]
    private void RegenerateGrid()
    {
        _grid.GenerateRandomBubbles();
    }
}
