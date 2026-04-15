using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FpsCounterTMP : MonoBehaviour
{
    [SerializeField] private float _updateInterval = 0.5f;
    private TextMeshProUGUI _text;
    private float _accumulatedFrames = 0f;
    private float _timeLeft;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        _timeLeft = _updateInterval;
    }

    private void Update()
    {
        _accumulatedFrames++;
        _timeLeft -= Time.unscaledDeltaTime;

        if (_timeLeft <= 0f)
        {
            float fps = _accumulatedFrames / _updateInterval;
            _text.text = $"FPS: {fps:F1}";
            _accumulatedFrames = 0f;
            _timeLeft = _updateInterval;
        }
    }
}