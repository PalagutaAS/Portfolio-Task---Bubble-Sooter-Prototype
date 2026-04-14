using UnityEngine;
using static UnityEngine.Color;

public class Bubble : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;

    public BubbleColor Color { get; private set; }
    public bool IsDestroying { get; set; }

    private void Awake()
    {
        IsDestroying = false;
        if (_sprite == null)
            return;
    }

    public void SetColor(BubbleColor color)
    {
        Color newColor = color switch
        {
            BubbleColor.Green => green,
            BubbleColor.Blue => blue,
            BubbleColor.Yellow => yellow,
            BubbleColor.Red => red,
            BubbleColor.Purple => new Color(0.5f, 0, 0.5f),
            _ => white
        };
        Color = color;
        _sprite.color = newColor;
    }
}
