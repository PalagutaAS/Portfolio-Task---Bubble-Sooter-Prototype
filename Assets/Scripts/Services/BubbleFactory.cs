using UnityEngine;

public interface IBubbleFactoryRandom
{
    Bubble CreateBubble(Vector2 position);
}
public interface IBubbleFactory
{
    Bubble CreateBubble(Vector2 position, BubbleColor colorType);
}

public class BubbleFactory : IBubbleFactoryRandom, IBubbleFactory
{
    private readonly Bubble _prefabBubble;
    private readonly Transform _parentForBubbles;
    private readonly BubbleDataSettings _dataSettings;

    public BubbleFactory(Bubble prefabBubble, Transform parentForBubbles, BubbleDataSettings dataSettings)
    {
        _prefabBubble = prefabBubble;
        _parentForBubbles = parentForBubbles;
        _dataSettings = dataSettings;
    }

    /// <summary>
    /// Создаёт пузырь указанного цвета в заданной позиции и родителе.
    /// </summary>
    /// <param name="position">World позиция.</param>
    /// <param name="colorType">Цвет пузыря.</param>
    /// <returns>Компонент Bubble созданного объекта.</returns>
    public Bubble CreateBubble(Vector2 position, BubbleColor colorType)
    {
        Bubble bubble = Object.Instantiate(_prefabBubble, _parentForBubbles);
        bubble.transform.position = position;
        bubble.Constructor(_dataSettings.GetData(colorType));
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
        var colorData = _dataSettings.GetData(colors[Random.Range(0, colors.Length)]);
        bubble.Constructor(colorData);
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