using UnityEngine;

public class CameraBoundsFitter : MonoBehaviour
{
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private float _horizontalPadding = 0f;
    [SerializeField] private float _verticalPadding = 0f;

    private Bounds _bounds;
    
    public void Constructor(Bounds bounds)
    {
        if (_targetCamera == null)
        {
            _targetCamera = GetComponent<Camera>();
            if (_targetCamera == null)
                _targetCamera = Camera.main;
        }
        
        _bounds = bounds;
        FitToBounds();
    }

    [ContextMenu("Reset")]
    public void FitToBoundsApply()
    {
        FitToBounds();
    }
    
    private void FitToBounds()
    {
        if (_targetCamera == null)
        {
            Debug.LogError("CameraBoundsFitter: нет назначенной камеры", this);
            return;
        }
        
        if (!_targetCamera.orthographic)
        {
            Debug.LogWarning("CameraBoundsFitter: камера переведена в ортографический режим", this);
            _targetCamera.orthographic = true;
        }

        float aspect = _targetCamera.aspect;
        
        float requiredWidth = _bounds.size.x + _horizontalPadding * 2f;
        float requiredHeight = _bounds.size.y + _verticalPadding * 2f;
        
        float orthoSizeForHeight = requiredHeight * 0.5f;
        float orthoSizeForWidth = (requiredWidth * 0.5f) / aspect;
        
        float finalOrthoSize;
        Vector3 finalPos;
        
        if (orthoSizeForWidth > orthoSizeForHeight)
        {
            finalOrthoSize = orthoSizeForWidth;
            float camY = _bounds.max.y - finalOrthoSize;
            finalPos = new Vector3(_bounds.center.x, camY, _targetCamera.transform.position.z);
        }
        else
        {
            finalOrthoSize = orthoSizeForHeight;
            finalPos = new Vector3(_bounds.center.x, _bounds.center.y, _targetCamera.transform.position.z);
        }
        
        _targetCamera.orthographicSize = finalOrthoSize;
        _targetCamera.transform.position = finalPos;
    }
}