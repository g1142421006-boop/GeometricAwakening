using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ResultUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text bestScoreText;

    [Header("Scene Names")]
    [SerializeField] private string retrySceneName = "OceanDemoScene";
    [SerializeField] private string backSceneName = "StartScene";

    private void Start()
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = "Score : " + GameResultData.FinalScore;
        }

        if (bestScoreText != null)
        {
            bestScoreText.text = "Best : " + GameResultData.BestScore;
        }
    }

    public void Retry()
    {
        SceneManager.LoadScene(retrySceneName);
    }

    public void BackToStart()
    {
        SceneManager.LoadScene(backSceneName);
    }

}
