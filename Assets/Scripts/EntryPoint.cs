using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private GridSettings _gridSettings;
    [SerializeField] private Transform _gridTransform;

    private GridGenerator _generator;
    private BubbleFactory _bubbleFactory;
    private IBubbleGridStorage _bubbleStorage;
    private IBubbleNeighborFinder _neighborFinder;
    private PositionCalculator _positionsCalculator;
    private IBubbleMatchFinder _matchFinder;

    private void Awake()
    {
        CreateAssets();
    }
    
    private void CreateAssets()
    {
        _bubbleFactory = new BubbleFactory();
        _positionsCalculator = new PositionCalculator(_gridSettings);
        _bubbleStorage = new BubbleGridStorage(_positionsCalculator);


        _generator = new GridGenerator(_gridSettings, _bubbleFactory, _positionsCalculator, _gridTransform, _bubbleStorage);
        _neighborFinder = new HexNeighborFinder(_gridSettings.rows, _gridSettings.columns, _bubbleStorage);
        _matchFinder = new BubbleMatchFinder(_bubbleStorage, _neighborFinder);

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
        _generator.GenerateGrid();
    }
}
