using TMPro;
using UnityEngine;

public class BubbleCounterUI : MonoBehaviour
{
    [SerializeField] private BubbleLauncher _bubbleLauncher;
    [SerializeField] private TMP_Text _tmpText;

    private void Awake()
    {
        _bubbleLauncher.OnShotsCountChanged += ChangeText;
    }

    private void ChangeText(int count)
    {
        _tmpText.text = count.ToString();
    }
}
