using UnityEngine;

public interface IBubbleFactoryRandom
{
    Bubble CreateBubble(Vector2 position);
}
public interface IBubbleFactory
{
    Bubble CreateBubble(Vector2 position, BubbleColor color);
}

public class BubbleFactory : IBubbleFactoryRandom, IBubbleFactory
{
    private readonly Bubble _prefabBubble;
    private readonly StickyBubbleService _stickyBubbleService;
    private readonly Transform _parentForBubbles;

    public BubbleFactory(Bubble prefabBubble, StickyBubbleService stickyBubbleService, Transform parentForBubbles)
    {
        _prefabBubble = prefabBubble;
        _stickyBubbleService = stickyBubbleService;
        _parentForBubbles = parentForBubbles;
    }

    /// <summary>
    /// Создаёт пузырь указанного цвета в заданной позиции и родителе.
    /// </summary>
    /// <param name="position">World позиция.</param>
    /// <param name="color">Цвет пузыря.</param>
    /// <returns>Компонент Bubble созданного объекта.</returns>
    public Bubble CreateBubble(Vector2 position, BubbleColor color)
    {
        Bubble bubble = Object.Instantiate(_prefabBubble, _parentForBubbles);
        bubble.transform.position = position;

        bubble.SetColor(color);
        return bubble;
    }
    
    /// <summary>
    /// Создаёт пузырь случайного цвета в заданной позиции и родителе.
    /// </summary>
    /// <param name="position">World позиция.</param>
    /// <returns>Компонент Bubble созданного объекта.</returns>
    public Bubble CreateBubble(Vector2 position)
    {
        BubbleColor[] colors = (BubbleColor[])System.Enum.GetValues(typeof(BubbleColor));
        
        Bubble bubble = Object.Instantiate(_prefabBubble, _parentForBubbles);
        bubble.transform.position = position;

        bubble.SetColor(colors[Random.Range(0, colors.Length)]);
        return bubble;
    }
}

public enum BubbleColor
{
    Green,
    Blue,
    Yellow,
    Red,
    Purple
}