using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Text UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timerText;

    [Header("Score Popup")]
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private Transform popupParent;
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        Instance = this;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score : " + score;
        }
    }

    public void UpdateTimer(float time)
    {
        if (timerText == null)
            return;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public void ShowScorePopup(int score, Vector3 worldPosition, bool isPenalty = false)
    {
        if (scorePopupPrefab == null || popupParent == null || mainCamera == null)
            return;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        GameObject popup = Instantiate(
            scorePopupPrefab,
            popupParent
        );

        popup.transform.position = screenPosition;

        ScorePopup scorePopup = popup.GetComponent<ScorePopup>();

        if (scorePopup != null)
        {
            scorePopup.SetScore(score, isPenalty);
        }
    }
}
