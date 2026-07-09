using UnityEngine;

public class JellyfishFloat : MonoBehaviour
{
    [Header("Float")]
    [SerializeField] private float floatDistance = 0.5f;
    [SerializeField] private float floatDuration = 3f;

    [Header("Scale")]
    [SerializeField] private bool enableScaleAnimation = true;
    [SerializeField] private float scaleAmountX = 0.05f;
    [SerializeField] private float scaleAmountY = 0.08f;
    [SerializeField] private float scaleDuration = 2f;

    [Header("Random")]
    [SerializeField] private bool randomizeOffset = true;
    [SerializeField] private float randomOffset = 0f;

    private Vector3 startPosition;
    private Vector3 startScale;
    private Vector3 floatDirection;

    private void Start()
    {
        startPosition = transform.position;
        startScale = transform.localScale;

        // 使用物件目前的 Up 方向
        floatDirection = transform.up.normalized;
        if (randomizeOffset)
        {
            randomOffset = Random.Range(0f, 100f);
        }
    }

    private void Update()
    {
        UpdateFloat();
        UpdateScale();
    }

    private void UpdateFloat()
    {
        float t = Mathf.Sin(
            (Time.time + randomOffset) *
            Mathf.PI * 2f /
            floatDuration
        );

        transform.position =
            startPosition +
            floatDirection * (t * floatDistance);
    }

    private void UpdateScale()
    {
        if (!enableScaleAnimation)
            return;

        float t = Mathf.Sin(
            (Time.time + randomOffset) *
            Mathf.PI * 2f /
            scaleDuration
        );

        float scaleX =
            startScale.x +
            (t * scaleAmountX);

        float scaleY =
            startScale.y -
            (t * scaleAmountY);

        transform.localScale = new Vector3(
            scaleX,
            scaleY,
            startScale.z
        );
    }
}
