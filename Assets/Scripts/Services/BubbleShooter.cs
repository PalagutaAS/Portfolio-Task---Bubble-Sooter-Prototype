using UnityEngine;

/// <summary>
/// Управляет стрельбой пузырями по нажатию мыши.
/// </summary>
public class BubbleShooter : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private BubbleLauncher _bubbleLauncher;
    [SerializeField] private Camera _mainCamera;      // камера для преобразования координат мыши

    [Header("Настройки выстрела (захардкожены)")]
    private float _shootSpeed = 15f;       // скорость выстрела
    private float _gravity = 20f;           // гравитация для пузыря

    private Transform _firePoint;

    private void Awake()
    {
        if (_bubbleLauncher == null)
            _bubbleLauncher = GetComponent<BubbleLauncher>();
        
        if (_mainCamera == null)
            _mainCamera = Camera.main;


        _firePoint = _bubbleLauncher.FirePoint;
    }

    private void Update()
    {
        // Стреляем по левой кнопке мыши
        if (Input.GetMouseButtonDown(0))
        {
            ShootAtMouseCursor();
        }
    }

    private void ShootAtMouseCursor()
    {
        if (_bubbleLauncher == null)
        {
            Debug.LogError("BubbleShooter: нет ссылки на BubbleLauncher!");
            return;
        }

        // Получаем пузырь от лаунчера
        Bubble bubble = _bubbleLauncher.Shoot();
        if (bubble == null)
        {
            // Выстрелов больше нет или нет текущего пузыря
            Debug.Log("Невозможно выстрелить: лимит исчерпан или пузырь отсутствует.");
            return;
        }

        // Определяем направление от точки выстрела к курсору мыши
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 direction = (mouseWorldPos - _firePoint.position).normalized;

        // Добавляем компонент полёта к пузырю и запускаем
        BubbleProjectile projectile = bubble.gameObject.GetComponent<BubbleProjectile>();
        if (projectile == null)
            projectile = bubble.gameObject.AddComponent<BubbleProjectile>();
        
        projectile.Launch(direction, _shootSpeed, _gravity);

        // Убедимся, что пузырь находится в точке выстрела (на всякий случай)
        bubble.transform.position = _firePoint.position;
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Получаем координаты мыши на экране
        Vector3 mouseScreen = Input.mousePosition;
        // Задаём глубину на расстоянии камеры (обычно 10-20 единиц от камеры)
        mouseScreen.z = Mathf.Abs(_mainCamera.transform.position.z); 
        return _mainCamera.ScreenToWorldPoint(mouseScreen);
    }
}