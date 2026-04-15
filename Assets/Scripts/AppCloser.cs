using UnityEngine;

public class AppCloser : MonoBehaviour
{
    public void CloseApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}