using UnityEngine;
using System.Collections;

public class PlayerBubbleController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float followSpeed = 10f;

    [Header("Fish Detection")]
    [SerializeField] private LayerMask fishLayer;

    [Header("Radius")]
    [SerializeField] private float baseAttractRadius = 2.5f;
    [SerializeField] private float baseEatRadius = 0.45f;

    [Header("Radius Center Offset")]
    [SerializeField] private Vector2 radiusCenterOffset = Vector2.zero;

    [Header("Growth")]
    [SerializeField] private bool enableGrowth = true;
    [SerializeField] private float growthPerFish = 0.02f;
    [SerializeField] private float maxScale = 2.5f;

    [Header("Pop Animation")]
    [SerializeField] private bool enablePopAnimation = true;
    [SerializeField] private float popScaleAmount = 0.08f;
    [SerializeField] private float popDuration = 0.1f;

    [Header("Camera Clamp")]
    [SerializeField] private bool enableClamp = true;
    [SerializeField] private Vector2 clampMin = new Vector2(-8.5f, -4.5f);
    [SerializeField] private Vector2 clampMax = new Vector2(8.5f, 4.5f);

    private Vector3 baseScale;
    private float currentAttractRadius;
    private float currentEatRadius;

    private Coroutine popCoroutine;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Start()
    {
        baseScale = transform.localScale;
        UpdateRadius();
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        FollowMouse();
        AttractFish();
        EatNearbyFish();
    }

    private void FollowMouse()
    {
        if (mainCamera == null)
            return;

        Vector3 mousePosition = Input.mousePosition;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0f;

        // 限制 Bubble 不超出遊戲範圍
        if (enableClamp)
        {
            worldPosition.x = Mathf.Clamp(worldPosition.x, clampMin.x, clampMax.x);
            worldPosition.y = Mathf.Clamp(worldPosition.y, clampMin.y, clampMax.y);
        }

        transform.position = Vector3.Lerp(
            transform.position,
            worldPosition,
            followSpeed * Time.deltaTime
        );
    }

    private void AttractFish()
    {
        Vector3 center = GetRadiusCenter();

        Collider2D[] fishColliders = Physics2D.OverlapCircleAll(
            center,
            currentAttractRadius,
            fishLayer
        );

        foreach (Collider2D fishCollider in fishColliders)
        {
            FishController fish = fishCollider.GetComponent<FishController>();

            if (fish != null)
            {
                fish.AttractTo(center);  
            }
        }
    }

    private void EatNearbyFish()
    {
        Vector3 center = GetRadiusCenter();

        Collider2D[] fishColliders = Physics2D.OverlapCircleAll(
            center,
            currentEatRadius,
            fishLayer
        );
        

        foreach (Collider2D fishCollider in fishColliders)
        {
            FishController fish = fishCollider.GetComponent<FishController>();

            if (fish != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayBubbleEat();
                }

                bool eatenSuccess = fish.BeEaten(center);
                if (!eatenSuccess)
                    continue;

                if (enableGrowth)
                {
                    Grow();
                }

                if (enablePopAnimation)
                {
                    PlayPopAnimation();
                }
            }
        }
    }

    private void Grow()
    {
        float currentScale = transform.localScale.x;
        float newScale = Mathf.Min(
            currentScale + growthPerFish,
            maxScale
        );

        transform.localScale = new Vector3(
            newScale,
            newScale,
            transform.localScale.z
        );

        UpdateRadius();
    }

    private void UpdateRadius()
    {
        if (baseScale == Vector3.zero)
        {
            baseScale = transform.localScale;
        }

        float scaleMultiplier = transform.localScale.x / baseScale.x;

        currentAttractRadius = baseAttractRadius * scaleMultiplier;
        currentEatRadius = baseEatRadius * scaleMultiplier;
    }

    private Vector3 GetRadiusCenter()
    {
        return transform.position + (Vector3)radiusCenterOffset;
    }

    private void PlayPopAnimation()
    {
        if (popCoroutine != null)
        {
            StopCoroutine(popCoroutine);
        }

        popCoroutine = StartCoroutine(PopAnimation());
    }

    private IEnumerator PopAnimation()
    {
        Vector3 normalScale = transform.localScale;
        Vector3 popScale = normalScale * (1f + popScaleAmount);

        float halfDuration = popDuration * 0.5f;
        float timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;

            transform.localScale = Vector3.Lerp(
                normalScale,
                popScale,
                t
            );

            UpdateRadius();

            yield return null;
        }

        timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;

            transform.localScale = Vector3.Lerp(
                popScale,
                normalScale,
                t
            );

            UpdateRadius();

            yield return null;
        }

        transform.localScale = normalScale;
        UpdateRadius();

        popCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (baseScale == Vector3.zero)
        {
            baseScale = transform.localScale;
        }

        UpdateRadius();

        Vector3 center = GetRadiusCenter();

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            center,
            currentAttractRadius
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            center,
            currentEatRadius
        );

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(
            center,
            0.05f
        );
    }
}
