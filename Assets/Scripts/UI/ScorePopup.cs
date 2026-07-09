using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScorePopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text scoreText;

    [Header("Animation")]
    [SerializeField] private float moveUpDistance = 60f;
    [SerializeField] private float duration = 0.6f;

    [Header("Colors")]
    [SerializeField] private Color positiveColor = new Color(1f, 0.9f, 0.2f);
    [SerializeField] private Color negativeColor = new Color(1f, 0.35f, 0.35f);

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void SetScore(int score, bool isPenalty)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString("+0;-0;0");
            scoreText.color = isPenalty ? negativeColor : positiveColor;
        }

        StartCoroutine(PlayAnimation(isPenalty));
    }

    private IEnumerator PlayAnimation(bool isPenalty)
    {
        Vector3 startPosition = transform.position;
        Vector3 direction = isPenalty
            ? Vector3.down
            : Vector3.up;

        Vector3 endPosition =
            startPosition +
            direction * moveUpDistance;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            transform.position = Vector3.Lerp(
                startPosition,
                endPosition,
                t
            );

            canvasGroup.alpha = 1f - t;

            yield return null;
        }

        Destroy(gameObject);
    }
}
