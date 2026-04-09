using UnityEngine;

public class BubbleShooter : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private BubbleLauncher _bubbleLauncher;
    [SerializeField] private Camera _mainCamera;

    [Header("Настройки натягивания")]
    [SerializeField] private float _maxDragRadius = 3f;
    [SerializeField] private float _minSpeed = 5f;
    [SerializeField] private float _maxSpeed = 20f;
    [SerializeField] private float _gravity = 20f;

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
        if (_bubbleLauncher.CurrentBubble == null) 
            return;
        
        if (Input.GetMouseButtonDown(0) && !_isDragging)
        {
            StartDrag();
        } 
        else if (Input.GetMouseButton(0) && _isDragging)
        {
            UpdateDrag();
        }
        else if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            ShootFromDrag();
        }
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
    
        float dragDistance = (_dragCurrentPos - _firePointPos).magnitude;
    
        float t = Mathf.Clamp01(dragDistance / _maxDragRadius);
        float shootSpeed = Mathf.Lerp(_minSpeed, _maxSpeed, t);
    
        Bubble bubble = _bubbleLauncher.Shoot();
        if (bubble == null)
        {
            Debug.LogWarning("Не удалось выстрелить – нет доступных выстрелов.");
            _isDragging = false;
            return;
        }

        bubble.gameObject.AddComponent<BubbleProjectile>()
            .Launch(direction, shootSpeed, _gravity);
    
        _isDragging = false;
    }


    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(_mainCamera.transform.position.z);
        return _mainCamera.ScreenToWorldPoint(mouseScreen);
    }
}