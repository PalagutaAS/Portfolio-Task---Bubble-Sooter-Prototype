using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private GridSettings _gridSettings;
    [SerializeField] private Transform _gridTransform;
    [SerializeField] private BubbleLauncher _launcher;

    private GridGenerator _grid;
    private BubbleFactory _bubbleFactory;
    private IBubbleGridStorage _bubbleStorage;
    private IBubbleNeighborFinder _neighborFinder;
    private GridPositions _gridPositions;
    private IBubbleMatchFinder _matchFinder;

    private void Awake()
    {
        CreateAssets();
    }

    private void CreateAssets()
    {
        _bubbleFactory = new BubbleFactory(_gridSettings.Prefab);
        _gridPositions = new GridPositions(_gridSettings);
        _bubbleStorage = new BubbleGridStorage(_gridPositions);

        _grid = new GridGenerator(_gridSettings, _bubbleFactory, _gridPositions, _gridTransform, _bubbleStorage);

        _grid.GenerateRandomBubbles();
        
        _neighborFinder = new HexNeighborFinder(_gridSettings.rows, _gridSettings.columns, _bubbleStorage);
        _matchFinder = new BubbleMatchFinder(_bubbleStorage, _neighborFinder);

        BubbleDebugger bubbleDebugger = new GameObject("BUBBLE DEBUGGER").AddComponent<BubbleDebugger>();
        bubbleDebugger.Constructor(_bubbleStorage, _neighborFinder, _matchFinder);
        _launcher.Constructor(_bubbleFactory, _gridTransform);
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
