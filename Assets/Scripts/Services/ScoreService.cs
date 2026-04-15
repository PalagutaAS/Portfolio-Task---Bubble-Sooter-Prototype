public interface IScoreService
{
    void BubblePopped(int matchCount, int floatingCount = 0);
}

public class ScoreService : IScoreService
{
    private readonly IScore _score;
    private readonly int _scoreByPop;

    public ScoreService(IScore score)
    {
        _score = score;
        _scoreByPop = 10;
    }

    public void BubblePopped(int matchCount, int floatingCount = 0)
    {
        int scoreByBubble = _scoreByPop + (matchCount - 3) * 2;
        int totalAddScore = scoreByBubble * matchCount + _scoreByPop * floatingCount;
        _score.AddScore(totalAddScore);
    }
}

