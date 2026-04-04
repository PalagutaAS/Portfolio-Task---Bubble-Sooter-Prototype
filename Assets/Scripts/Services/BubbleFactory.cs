using UnityEngine;

public class BubbleFactory
{
    /// <summary>
    /// Создаёт пузырь указанного цвета в заданной позиции и родителе.
    /// </summary>
    /// <param name="prefab">Префаб с компонентом Bubble.</param>
    /// <param name="position">Локальная позиция.</param>
    /// <param name="parent">Родительский трансформ.</param>
    /// <param name="color">Цвет пузыря.</param>
    /// <returns>Компонент Bubble созданного объекта.</returns>
    public Bubble CreateBubble(Bubble prefab, Vector2 position, Transform parent, BubbleColor color)
    {
        Bubble bubble = Object.Instantiate(prefab, parent);
        bubble.transform.localPosition = position;

        bubble.SetColor(color);
        return bubble;
    }
    
    /// <summary>
    /// Создаёт пузырь случайного цвета в заданной позиции и родителе.
    /// </summary>
    /// <param name="prefab">Префаб с компонентом Bubble.</param>
    /// <param name="position">Локальная позиция.</param>
    /// <param name="parent">Родительский трансформ.</param>
    /// <returns>Компонент Bubble созданного объекта.</returns>
    public Bubble CreateRandomBubble(Bubble prefab, Vector2 position, Transform parent)
    {
        BubbleColor[] colors = (BubbleColor[])System.Enum.GetValues(typeof(BubbleColor));
        
        Bubble bubble = Object.Instantiate(prefab, parent);
        bubble.transform.localPosition = position;

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