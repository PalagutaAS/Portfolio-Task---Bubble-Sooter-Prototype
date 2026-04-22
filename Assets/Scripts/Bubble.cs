using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;

    public BubbleColor Color { get; private set; }
    public bool IsDestroying { get; set; }

    private void Awake()
    {
        IsDestroying = false;
        _sprite ??= GetComponent<SpriteRenderer>();
    }

    public void Constructor(ColorData colorData)
    {
        Color = colorData.ColorName;
        _sprite.color = colorData.ColorValue;
    }
}
