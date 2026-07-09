using UnityEngine;

public class WaveRippleEffect : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float startScale = 0.3f;
    [SerializeField] private float endScale = 2.5f;
    [SerializeField] private float duration = 0.35f;

    [Header("Alpha")]
    [SerializeField] private float startAlpha = 0.8f;
    [SerializeField] private float endAlpha = 0f;

    private SpriteRenderer spriteRenderer;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        transform.localScale = Vector3.one * startScale;
        SetAlpha(startAlpha);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float t = timer / duration;
        t = Mathf.Clamp01(t);

        float scale = Mathf.Lerp(startScale, endScale, t);
        transform.localScale = Vector3.one * scale;

        float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
        SetAlpha(alpha);

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderer == null)
            return;

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
