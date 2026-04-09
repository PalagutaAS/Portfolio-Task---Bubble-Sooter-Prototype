using System;
using UnityEngine;
using static UnityEngine.Color;

public class Bubble : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;

    public event Action<Bubble, Bubble> OnCollisionBubbles;
    public BubbleColor Color { get; private set; }

    private void Awake()
    {
        if (_sprite == null)
            return;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent(out Bubble bubble) && TryGetComponent(out BubbleProjectile _bubbleProjectile) && _bubbleProjectile.IsLaunch)
        {
            OnCollisionBubbles?.Invoke(this, bubble);
        }
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
