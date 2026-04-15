using System;

public interface IScore
{
    int CurrentScore { get; }
    event Action<int> OnChangeScore;
    void AddScore(int count);
    void ResetScore();
}
public class Score : IScore
{
    private int _currentScore = 0;
    
    public event Action<int> OnChangeScore;
    
    public void AddScore(int count)
    {
        CurrentScore += count;
    }

    public void ResetScore()
    {
        CurrentScore = 0;
    }

    public int CurrentScore
    {
        get => _currentScore;
        private set
        {
            if (value < 0)
                return;
            
            _currentScore = value;
            OnChangeScore?.Invoke(_currentScore);
        }
    }
}