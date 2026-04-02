using UnityEngine;
using Random = UnityEngine.Random;

public class Bubble : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;

    private void Awake()
    {
        if (_sprite == null)
            return;

        _sprite.color = Random.ColorHSV();

    }
}
