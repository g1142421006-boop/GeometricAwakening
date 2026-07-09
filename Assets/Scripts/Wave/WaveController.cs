using UnityEngine;

public class WaveController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float triggerSpeed = 12f;
    [SerializeField] private float cooldown = 0.25f;

    [Header("Wave")]
    [SerializeField] private LayerMask fishLayer;
    [SerializeField] private float waveRadius = 2.5f;
    [SerializeField] private float waveForce = 5f;

    [Header("Ripple Effect")]
    [SerializeField] private GameObject ripplePrefab;
    [SerializeField] private float rippleRandomScaleMin = 0.8f;
    [SerializeField] private float rippleRandomScaleMax = 1.2f;

    private Vector3 lastMouseWorldPosition;
    private float cooldownTimer;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Start()
    {
        lastMouseWorldPosition = GetMouseWorldPosition();
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        if (PhaseManager.Instance == null ||
            PhaseManager.Instance.CurrentPhase != PhaseManager.GamePhase.BlackHole)
            return;

        cooldownTimer -= Time.deltaTime;
        cooldownTimer = Mathf.Max(cooldownTimer, 0f);

        Vector3 currentMouseWorldPosition = GetMouseWorldPosition();
        float mouseSpeed = Vector3.Distance(
            currentMouseWorldPosition,
            lastMouseWorldPosition
        ) / Time.deltaTime;

        if (mouseSpeed >= triggerSpeed && cooldownTimer <= 0f)
        {
            CreateWave(currentMouseWorldPosition);
            cooldownTimer = cooldown;
        }

        lastMouseWorldPosition = currentMouseWorldPosition;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0f;

        return worldPosition;
    }

    private void CreateWave(Vector3 center)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWaveRipple();
        }

        SpawnRipple(center);
        if (ripplePrefab != null)
        {
            Instantiate(
                ripplePrefab,
                center,
                Quaternion.identity
            );
        }

        Collider2D[] fishes = Physics2D.OverlapCircleAll(
            center,
            waveRadius,
            fishLayer
        );

        foreach (Collider2D fishCollider in fishes)
        {
            FishController fish = fishCollider.GetComponent<FishController>();

            if (fish != null)
            {
                fish.PushAway(center, waveForce);
            }
        }
    }

    private void SpawnRipple(Vector3 position)
    {
        if (ripplePrefab == null)
            return;

        GameObject ripple = Instantiate(
            ripplePrefab,
            position,
            Quaternion.Euler(0f, 0f, Random.Range(0f, 360f))
        );

        float randomScale = Random.Range(
            rippleRandomScaleMin,
            rippleRandomScaleMax
        );

        ripple.transform.localScale *= randomScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            Application.isPlaying ? GetMouseWorldPosition() : transform.position,
            waveRadius
        );
    }
}
