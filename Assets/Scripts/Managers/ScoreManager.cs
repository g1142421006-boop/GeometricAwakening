using UnityEngine;


public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Limit")]
    [SerializeField] private bool allowNegativeScore = false;

    private int score = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UIManager.Instance.UpdateScore(score);
    }

    public void AddScore(int amount)
    {
        score += amount;

        if (!allowNegativeScore)
        {
            score = Mathf.Max(score, 0);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
        }
    }

    public int GetScore()
    {
        return score;
    }
}
