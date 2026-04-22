using ScriptableObjects;

public interface IScoreService
{
    void CalculateScore(int matchCount, int floatingCount = 0);
}

public class ScoreService : IScoreService
{
    private readonly IScore _score;
    private readonly BubbleDataSettings _bubbleSettings;


    public ScoreService(IScore score, BubbleDataSettings bubbleSettings)
    {
        _score = score;
        _bubbleSettings = bubbleSettings;
    }

    public void CalculateScore(int matchCount, int floatingCount = 0)
    {
        int scoreByPop = _bubbleSettings.ScoreByPop;
        float rate = _bubbleSettings.Rate;

        int scoreByBubble = scoreByPop + (int) (scoreByPop * (matchCount - 3) * rate);
        int totalAddScore = scoreByBubble * matchCount + scoreByPop * floatingCount;
        
        _score.AddScore(totalAddScore);
    }
}

