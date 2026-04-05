using UnityEngine;

/// <summary>
/// Компонент для ручного управления полётом пузыря без использования физики Unity.
/// Добавляется к выстреленному пузырю.
/// </summary>
public class BubbleProjectile : MonoBehaviour
{
    [Header("Настройки полёта (захардкожены в коде)")]
    private float _speed = 15f;
    private float _gravity = 20f;

    private Vector3 _velocity;

    /// <summary>
    /// Инициализировать полёт пузыря.
    /// </summary>
    /// <param name="direction">Нормализованное направление полёта</param>
    /// <param name="speed">Начальная скорость (опционально)</param>
    /// <param name="gravity">Сила гравитации (опционально)</param>
    public void Launch(Vector3 direction, float speed = -1f, float gravity = -1f)
    {
        if (speed > 0) _speed = speed;
        if (gravity > 0) _gravity = gravity;

        _velocity = direction * _speed;
    }

    private void Update()
    {
        // Применяем гравитацию (тянем вниз по оси Y)
        _velocity.y -= _gravity * Time.deltaTime;

        // Перемещаем пузырь
        transform.position += _velocity * Time.deltaTime;

        // Опционально: удаляем пузырь, если он улетел слишком далеко
        if (Mathf.Abs(transform.position.x) > 30f || transform.position.y < -10f || transform.position.y > 20f)
        {
            Destroy(gameObject);
        }
    }
}