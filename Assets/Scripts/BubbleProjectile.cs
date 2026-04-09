using UnityEngine;

public class BubbleProjectile : MonoBehaviour
{
    private float _speed = 15f;
    private float _gravity = 20f;

    public bool IsLaunch => (_speed > 0);
    
    private Vector3 _velocity;
    
    public void Launch(Vector3 direction, float speed = -1f, float gravity = -1f)
    {
        _speed = speed;
        _gravity = gravity;
        if (!IsLaunch)
            return;

        _velocity = direction * _speed;
    }

    private void Update()
    {
        if (!IsLaunch)
            return;
        
        _velocity.y -= _gravity * Time.deltaTime;

        transform.position += _velocity * Time.deltaTime;

        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }
}