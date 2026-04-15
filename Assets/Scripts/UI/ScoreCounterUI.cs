using TMPro;
using UnityEngine;

public class ScoreCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _tmpText;
    private IScore _score;

    public void Constructor(IScore score)
    {
        _score = score;
        _score.OnChangeScore += ChangeText;
    }

    private void ChangeText(int count)
    {
        _tmpText.text = count.ToString();
    }

    private void OnDestroy()
    {
        _score.OnChangeScore -= ChangeText;
    }
}
