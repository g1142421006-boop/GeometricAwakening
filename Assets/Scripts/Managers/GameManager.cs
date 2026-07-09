using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Ready,
        Playing,
        GameOver,
        Pause
    }

    [Header("Game Time")]
    [SerializeField] private float gameTime = 60f;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;

    [Header("Scene")]
    [SerializeField] private string resultSceneName = "ResultScene";
    [SerializeField] private float resultDelay = 2f;

    private float currentTime;
    private GameState currentState = GameState.Ready;

    public bool IsPlaying => currentState == GameState.Playing;

    public float CurrentTime => currentTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        StartGame();
    }

    private void Update()
    {
        if (currentState != GameState.Playing)
            return;

        UpdateTimer();
    }

    public void StartGame()
    {
        currentState = GameState.Playing;

        currentTime = gameTime;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTimer(currentTime);
        }
    }

    private void UpdateTimer()
    {
        currentTime -= Time.deltaTime;

        currentTime = Mathf.Max(currentTime, 0f);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTimer(currentTime);
        }

        if (currentTime <= 0f)
        {
            EndGame();
        }
    }

    public void EndGame()
    {
        if (currentState == GameState.GameOver)
            return;

        currentState = GameState.GameOver;

        ShowGameOverUI();

        StartCoroutine(LoadResultScene());
    }

    private void ShowGameOverUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null &&
            ScoreManager.Instance != null)
        {
            finalScoreText.text =
                "Score : " + ScoreManager.Instance.GetScore();
        }

        if (ScoreManager.Instance != null)
        {
            int finalScore = ScoreManager.Instance.GetScore();

            GameResultData.FinalScore = finalScore;

            if (finalScore > GameResultData.BestScore)
            {
                GameResultData.BestScore = finalScore;
            }
        }
    }

    private IEnumerator LoadResultScene()
    {
        yield return new WaitForSeconds(resultDelay);

        SceneManager.LoadScene(resultSceneName);
    }

    public GameState GetGameState()
    {
        return currentState;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public float GetGameTime()
    {
        return gameTime;
    }
}
