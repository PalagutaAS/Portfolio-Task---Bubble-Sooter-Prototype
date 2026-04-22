using ScriptableObjects;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BubbleShooter : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private BubbleLauncher _bubbleLauncher;
    [SerializeField] private TrajectoryRenderer _trajectoryRenderer;
    [SerializeField] private Camera _mainCamera;

    [SerializeField] private BubbleShooterSettings _shooterSettings;
    
    private bool _isDragging;
    private Vector3 _firePointPos;
    private Vector3 _dragCurrentPos;
    
    private Vector2 _lastPreviewDirection;
    private float _lastPreviewSpeed;
    private int _frameSkipCounter;
    private const int FRAMES_TO_SKIP = 3;

    public void Constructor(BubbleShooterSettings shooterSettings)
    {
        _shooterSettings = shooterSettings;
        _bubbleLauncher ??= GetComponent<BubbleLauncher>();
        _mainCamera ??= Camera.main;
        _firePointPos = _bubbleLauncher.FirePoint.position;
    }
    
    private void Update()
    {
        if (!_bubbleLauncher.CurrentBubble) 
            return;
        
        if (Input.GetMouseButtonDown(0) && !_isDragging)
        {
            StartDrag();
        } 
        else if (Input.GetMouseButton(0) && _isDragging)
        {
            UpdateDrag();
            UpdateTrajectoryPreview();
        }
        else if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            ShootFromDrag();
            Reset();
        }
        
        if (Input.GetMouseButtonDown(1) && _isDragging)
        {
            Reset();
        }
    }

    private void Reset()
    {
        _trajectoryRenderer.HideTrajectory();
        _lastPreviewDirection = Vector2.zero;
        _lastPreviewSpeed = 0;
        _isDragging = false;
        _bubbleLauncher.ResetCurrentBubblePosition();
    }

    private void UpdateTrajectoryPreview()
    {
        _frameSkipCounter++;
        if (_frameSkipCounter < FRAMES_TO_SKIP)
            return;
        _frameSkipCounter = 0;
        
        Vector3 direction = (_firePointPos - _dragCurrentPos).normalized;
        float speed = CalculateSpeed();
        
        if (direction == (Vector3)_lastPreviewDirection && Mathf.Abs(speed - _lastPreviewSpeed) < 0.01)
            return;
        
        _lastPreviewDirection = direction;
        _lastPreviewSpeed = speed;
        _trajectoryRenderer.ShowTrajectory(direction, speed);
    }

    private void StartDrag()
    {
        _isDragging = true;
        _bubbleLauncher.CurrentBubble.transform.position = _firePointPos;
        UpdateDrag();
    }


    private void UpdateDrag()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 directionToMouse = mouseWorldPos - _firePointPos;
        float distance = directionToMouse.magnitude;

        if (distance > _shooterSettings.maxDragRadius)
        {
            directionToMouse = directionToMouse.normalized * _shooterSettings.maxDragRadius;
        }

        _dragCurrentPos = _firePointPos + directionToMouse;

        _bubbleLauncher.CurrentBubble.transform.position = _dragCurrentPos;
    }


    private void ShootFromDrag()
    {
        Vector3 direction = (_firePointPos - _dragCurrentPos).normalized;
        _bubbleLauncher.Shoot(direction, CalculateSpeed());
    }

    private float CalculateSpeed()
    {
        float dragDistance = (_dragCurrentPos - _firePointPos).magnitude;
    
        float t = Mathf.Clamp01(dragDistance / _shooterSettings.maxDragRadius);
        return Mathf.Lerp(_shooterSettings.minSpeed, _shooterSettings.maxSpeed, t);
    }


    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(_mainCamera.transform.position.z);
        return _mainCamera.ScreenToWorldPoint(mouseScreen);
    }
}