using UnityEngine;

public class BubbleIdleAnimation : MonoBehaviour
{
    [Header("Float")]
    [SerializeField] private bool enableFloat = true;
    [SerializeField] private float floatDistance = 0.08f;
    [SerializeField] private float floatDuration = 2.5f;

    [Header("Breath Scale")]
    [SerializeField] private bool enableBreath = true;
    [SerializeField] private float scaleAmountX = 0.03f;
    [SerializeField] private float scaleAmountY = 0.03f;
    [SerializeField] private float scaleDuration = 2f;

    private Vector3 startLocalPosition;
    private Vector3 startLocalScale;
    private float randomOffset;

    private void Start()
    {
        startLocalPosition = transform.localPosition;
        startLocalScale = transform.localScale;
        randomOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        UpdateFloat();
        UpdateBreath();
    }

    private void UpdateFloat()
    {
        if (!enableFloat)
            return;

        float t = Mathf.Sin(
            (Time.time + randomOffset) *
            Mathf.PI * 2f /
            floatDuration
        );

        transform.localPosition =
            startLocalPosition +
            Vector3.up * (t * floatDistance);
    }

    private void UpdateBreath()
    {
        if (!enableBreath)
            return;

        float t = Mathf.Sin(
            (Time.time + randomOffset) *
            Mathf.PI * 2f /
            scaleDuration
        );

        float scaleX = startLocalScale.x + t * scaleAmountX;
        float scaleY = startLocalScale.y - t * scaleAmountY;

        transform.localScale = new Vector3(
            scaleX,
            scaleY,
            startLocalScale.z
        );
    }
}
