using UnityEngine;
using System.Collections.Generic;

public class TrajectoryRenderer : MonoBehaviour
{
    [SerializeField] private GameObject _dotPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private int _maxPoints = 50;
    [SerializeField, Range(1, 3)] private int _step = 1;
    [SerializeField] private float _minDistanceBetweenDots = 0.4f;
    [SerializeField] private float _scaleLastDot = 0.4f;

    private TrajectoryPredictor _predictor;
    private TrajectorySettings _settings;
    private bool _isActive = true;

    private List<GameObject> _activeDots = new List<GameObject>();
    private Queue<GameObject> _dotPool = new Queue<GameObject>();
    private GameObject _lastDot;
    private Vector2 _lastPoint;


    public void Initialize(TrajectoryPredictor predictor, TrajectorySettings trajectorySettings)
    {
        _predictor = predictor;
        _settings = trajectorySettings;

        for (int i = 0; i < _maxPoints; i++)
        {
            var dot = Instantiate(_dotPrefab, transform);
            dot.SetActive(false);
            _dotPool.Enqueue(dot);
        }
        
        _lastDot = Instantiate(_dotPrefab, transform);
        _lastDot.transform.localScale = Vector3.one * _scaleLastDot;
        _lastDot.SetActive(false);
    }

    public void ShowTrajectory(Vector2 direction, float shotSpeed)
    {
        if (!_isActive || _predictor == null || _firePoint == null) return;

        ShotResult result = _predictor.Predict(
            _firePoint.position, 
            direction,
            shotSpeed
            );

        List<Vector2> points = result.trajectory;
        if (points.Count < 2) return;
        
        HideTrajectory();
        
        _lastPoint = points[^1];
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 p0 = points[i];
            Vector2 p1 = points[i + 1];
            
            for (int j = 0; j < _step; j++)
            {
                float t = j / (float)_step;
                Vector2 interpolated = Vector2.Lerp(p0, p1, t);
                TryAddDot(interpolated);
            }
        }

        _lastDot.transform.position = points[^1];
        _lastDot.SetActive(result.hit);
    }
    
    private void TryAddDot(Vector2 position)
    {
        if (Vector2.Distance(position, _lastPoint) < _minDistanceBetweenDots)
            return;
        
        GameObject dot = GetDotFromPool();
        dot.transform.position = position;
        dot.SetActive(true);
        _activeDots.Add(dot);
    }

    private GameObject GetDotFromPool() => (_dotPool.Count > 0) ? _dotPool.Dequeue() :  Instantiate(_dotPrefab, transform);
    
    public void HideTrajectory()
    {
        foreach (var dot in _activeDots)
        {
            dot.SetActive(false);
            _dotPool.Enqueue(dot);
        }
        _lastDot.SetActive(false);
        _activeDots.Clear();
    }
    
    public void SetActive(bool active)
    {
        _isActive = active;
        if (!active) HideTrajectory();
    }
}