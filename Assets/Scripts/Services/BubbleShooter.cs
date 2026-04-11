using UnityEngine;

public class BubbleShooter : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private BubbleLauncher _bubbleLauncher;
    [SerializeField] private TrajectoryRenderer _trajectoryRenderer;
    [SerializeField] private Camera _mainCamera;

    [Header("Настройки натягивания")]
    [SerializeField] private float _maxDragRadius = 3f;
    [SerializeField] private float _minSpeed = 5f;
    [SerializeField] private float _maxSpeed = 20f;

    private bool _isDragging;
    private Vector3 _firePointPos;
    private Vector3 _dragCurrentPos;

    private void Awake()
    {
        if (_bubbleLauncher == null)
            _bubbleLauncher = GetComponent<BubbleLauncher>();
        
        if (_mainCamera == null)
            _mainCamera = Camera.main;
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
            _trajectoryRenderer.HideTrajectory();
        }
    }

    private void UpdateTrajectoryPreview()
    {
        Vector3 direction = (_firePointPos - _dragCurrentPos).normalized;
        _trajectoryRenderer.ShowTrajectory(direction, CalculateSpeed());
    }

    private void StartDrag()
    {
        _isDragging = true;
        _firePointPos = _bubbleLauncher.FirePoint.position;
        _bubbleLauncher.CurrentBubble.transform.position = _firePointPos;
        UpdateDrag();
    }


    private void UpdateDrag()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 directionToMouse = mouseWorldPos - _firePointPos;
        float distance = directionToMouse.magnitude;

        if (distance > _maxDragRadius)
        {
            directionToMouse = directionToMouse.normalized * _maxDragRadius;
        }

        _dragCurrentPos = _firePointPos + directionToMouse;

        _bubbleLauncher.CurrentBubble.transform.position = _dragCurrentPos;
    }


    private void ShootFromDrag()
    {
        Vector3 direction = (_firePointPos - _dragCurrentPos).normalized;
        _bubbleLauncher.Shoot(direction, CalculateSpeed());

        _isDragging = false;
    }

    private float CalculateSpeed()
    {
        float dragDistance = (_dragCurrentPos - _firePointPos).magnitude;
    
        float t = Mathf.Clamp01(dragDistance / _maxDragRadius);
        return Mathf.Lerp(_minSpeed, _maxSpeed, t);
    }


    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(_mainCamera.transform.position.z);
        return _mainCamera.ScreenToWorldPoint(mouseScreen);
    }
}