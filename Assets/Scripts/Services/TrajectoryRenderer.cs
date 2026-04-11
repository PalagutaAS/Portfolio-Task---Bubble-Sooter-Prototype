using UnityEngine;
using System.Collections.Generic;

public class TrajectoryRenderer : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private int _maxPoints = 50;
    [SerializeField] private float _gravity = 20f;

    private TrajectoryPredictor _predictor;
    private TrajectorySettings _settings;
    private bool _isActive = true;

    public void Initialize(TrajectoryPredictor predictor, TrajectorySettings trajectorySettings)
    {
        _predictor = predictor;
        _settings = trajectorySettings;
        if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
    }

    public void ShowTrajectory(Vector2 direction, float shotSpeed)
    {
        if (!_isActive || _predictor == null || _firePoint == null) return;

        ShotResult result = _predictor.Predict(
            _firePoint.position, 
            direction,
            shotSpeed,
            _gravity,
            isAimLine: true
        );

        List<Vector2> points = result.trajectory;
        if (points.Count < 2) return;

        // Выбираем равномерные точки для LineRenderer
        List<Vector3> displayPoints = new List<Vector3>();
        if (points.Count <= _maxPoints)
        {
            foreach (var p in points) displayPoints.Add(p);
        }
        else
        {
            int step = points.Count / _maxPoints;
            for (int i = 0; i < points.Count; i += step)
            {
                displayPoints.Add(points[i]);
            }
            // Обязательно добавляем последнюю точку
            if (!displayPoints[displayPoints.Count - 1].Equals(points[points.Count - 1]))
                displayPoints.Add(points[points.Count - 1]);
        }

        _lineRenderer.positionCount = displayPoints.Count;
        _lineRenderer.SetPositions(displayPoints.ToArray());

        // Опционально: цвет по умолчанию (белый)
        _lineRenderer.startColor = Color.white;
        _lineRenderer.endColor = Color.white;
        
        // Можно покрасить последний сегмент в красный при попадании (но не обязательно)
        if (result.hit && displayPoints.Count >= 2)
        {
            // Просто пример – меняем цвет всей линии, если хотите
            _lineRenderer.startColor = Color.yellow;
            _lineRenderer.endColor = Color.red;
        }
    }

    public void HideTrajectory()
    {
        _lineRenderer.positionCount = 0;
    }

    public void SetActive(bool active)
    {
        _isActive = active;
        if (!active) HideTrajectory();
    }
}