using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public void HideGameOver()
    {
        gameObject.SetActive(false);
    }

    public void ShowGameOver()
    {
        gameObject.SetActive(true);
    }
}
