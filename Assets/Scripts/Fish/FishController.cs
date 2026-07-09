using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FishController : MonoBehaviour
{
    public enum FishState
    {
        Normal,
        Scared,
        Attracted,
        Eaten
    }

    [Header("Score")]
    [SerializeField] private int scoreValue = 10;

    public int ScoreValue => scoreValue;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float attractSpeed = 5f;

    [Header("Life Time")]
    [SerializeField] private float lifeTime = 15f;

    [Header("Random Scale")]
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;

    [Header("Scale Animation")]
    [SerializeField] private bool enableScaleAnimation = true;
    [SerializeField] private float scaleAmountX = 0.03f;
    [SerializeField] private float scaleAmountY = 0.05f;
    [SerializeField] private float scaleDuration = 2f;

    [Header("Swimming Wave")]
    [SerializeField] private bool enableSwimWave = true;
    [SerializeField] private float swimAmplitude = 0.25f;
    [SerializeField] private float swimFrequency = 2f;

    [Header("Eaten Animation")]
    [SerializeField] private float eatenDuration = 0.15f;
    [SerializeField] private float eatenRotateSpeed = 720f;

    [Header("Camera Shake")]
    [SerializeField] private bool enableCameraShake = true;
    [SerializeField] private float shakeDuration = 0.08f;
    [SerializeField] private float shakeMagnitude = 0.04f;

    [Header("Avoid Black Hole")]
    [SerializeField] private bool enableAvoidBlackHole = true;
    [SerializeField] private LayerMask blackHoleLayer;
    [SerializeField] private float avoidRadius = 2.5f;
    [SerializeField] private float avoidStrength = 2.5f;

    [Header("Scared State")]
    [SerializeField] private float scaredSpeedMultiplier = 1.6f;
    [SerializeField] private float scaredSwimAmplitudeMultiplier = 1.8f;

    [Header("External Push")]
    [SerializeField] private float pushDuration = 0.4f;

    private Vector3 externalPushVelocity;
    private float pushTimer;

    private FishState currentState = FishState.Normal;

    private Vector3 moveDirection = Vector3.left;
    private Vector3 spawnPosition;
    private Vector3 baseScale;

    private float moveDistance = 0f;
    private float randomOffset = 0f;
    private float facingSign = 1f;

    private Vector3 attractTarget;
    private Vector3 avoidVelocity;

    private void Start()
    {
        InitializeScale();

        randomOffset = Random.Range(0f, 100f);
        spawnPosition = transform.position;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        if (currentState == FishState.Eaten)
            return;

        if (currentState == FishState.Attracted)
        {
            MoveToAttractTarget();
        }
        else
        {
            MoveForward();
        }

        UpdateScale();

        currentState = FishState.Normal;
    }

    private void InitializeScale()
    {
        float randomScale = Random.Range(minScale, maxScale);

        baseScale = new Vector3(
            randomScale,
            randomScale,
            1f
        );

        ApplyScale(baseScale.x, baseScale.y);
    }

    private void MoveForward()
    {
        Vector3 finalDirection = moveDirection;

        bool isScared = false;

        if (enableAvoidBlackHole)
        {
            Vector3 avoidDirection = GetBlackHoleAvoidDirection();

            if (avoidDirection != Vector3.zero)
            {
                isScared = true;
                currentState = FishState.Scared;
                finalDirection = (moveDirection + avoidDirection * avoidStrength).normalized;
            }
        }

        float currentMoveSpeed = moveSpeed;

        if (isScared)
        {
            currentMoveSpeed *= scaredSpeedMultiplier;
        }

        Vector3 movement = finalDirection * currentMoveSpeed * Time.deltaTime;

        if (enableSwimWave)
        {
            Vector3 perpendicular = new Vector3(
                -moveDirection.y,
                moveDirection.x,
                0f
            );

            float currentSwimAmplitude = swimAmplitude;

            if (isScared)
            {
                currentSwimAmplitude *= scaredSwimAmplitudeMultiplier;
            }

            float wave = Mathf.Sin(
                (Time.time + randomOffset) * swimFrequency
            ) * currentSwimAmplitude;

            movement += perpendicular * wave * Time.deltaTime;
        }

        transform.position += movement;
        if (pushTimer > 0f)
        {
            pushTimer -= Time.deltaTime;
            movement += externalPushVelocity * Time.deltaTime;
        }
    }

    private void MoveToAttractTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            attractTarget,
            attractSpeed * Time.deltaTime
        );
    }

    private void UpdateScale()
    {
        if (!enableScaleAnimation)
        {
            ApplyScale(baseScale.x, baseScale.y);
            return;
        }

        float t = Mathf.Sin(
            (Time.time + randomOffset) *
            Mathf.PI * 2f /
            scaleDuration
        );

        float scaleX = baseScale.x + t * scaleAmountX;
        float scaleY = baseScale.y - t * scaleAmountY;

        ApplyScale(scaleX, scaleY);
    }

    private void ApplyScale(float x, float y)
    {
        transform.localScale = new Vector3(
            Mathf.Abs(x) * facingSign,
            y,
            baseScale.z
        );
    }

    public void SetMoveDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;

        // 素材預設面朝左；往右游時水平翻轉
        facingSign = moveDirection.x > 0 ? -1f : 1f;
    }

    public void AttractTo(Vector3 target)
    {
        if (currentState == FishState.Eaten)
            return;

        currentState = FishState.Attracted;
        attractTarget = target;
    }

    public void DestroyByBlackHole(Vector3 blackHoleCenter)
    {
        if (currentState == FishState.Eaten)
            return;

        currentState = FishState.Eaten;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        StartCoroutine(EatenAnimation(blackHoleCenter));
    }

    public void PushAway(Vector3 sourcePosition, float force)
    {
        Vector3 awayDirection = transform.position - sourcePosition;

        if (awayDirection.sqrMagnitude <= 0.001f)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            awayDirection = new Vector3(randomDirection.x, randomDirection.y, 0f);
        }

        externalPushVelocity = awayDirection.normalized * force;
        pushTimer = pushDuration;

        // 讓魚被推走後朝逃離方向游
        SetMoveDirection(awayDirection.normalized);
    }

    public bool BeEaten(Vector3 eatCenter)
    {
        if (currentState == FishState.Eaten)
            return false;

        currentState = FishState.Eaten;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowScorePopup(scoreValue, transform.position);
        }

        if (enableCameraShake && CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);
        }

        StartCoroutine(EatenAnimation(eatCenter));

        return true;
    }

    private Vector3 GetBlackHoleAvoidOffset()
    {
        if (!enableAvoidBlackHole)
            return Vector3.zero;

        Collider2D blackHole = Physics2D.OverlapCircle(
            transform.position,
            avoidRadius,
            blackHoleLayer
        );

        if (blackHole == null)
            return Vector3.zero;

        Vector3 awayDirection =
            transform.position - blackHole.transform.position;

        if (awayDirection.sqrMagnitude <= 0.001f)
        {
            awayDirection = Random.insideUnitCircle.normalized;
        }

        float distance = awayDirection.magnitude;

        float strengthMultiplier =
            1f - Mathf.Clamp01(distance / avoidRadius);

        Vector3 avoidOffset =
            awayDirection.normalized *
            avoidStrength *
            strengthMultiplier *
            Time.deltaTime;

        return avoidOffset;
    }

    private Vector3 GetBlackHoleAvoidDirection()
    {
        Collider2D blackHole = Physics2D.OverlapCircle(
        transform.position,
        avoidRadius,
        blackHoleLayer
    );

        if (blackHole == null)
            return Vector3.zero;

        Vector3 awayDirection =
            transform.position - blackHole.transform.position;

        if (awayDirection.sqrMagnitude <= 0.001f)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            return new Vector3(randomDirection.x, randomDirection.y, 0f);
        }

        float distance = awayDirection.magnitude;

        float dangerRate = 1f - Mathf.Clamp01(distance / avoidRadius);

        return awayDirection.normalized * dangerRate;
    }

    private IEnumerator EatenAnimation(Vector3 eatCenter)
    {
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;

        float timer = 0f;

        while (timer < eatenDuration)
        {
            timer += Time.deltaTime;

            float t = timer / eatenDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(
                startPosition,
                eatCenter,
                smoothT
            );

            transform.localScale = Vector3.Lerp(
                startScale,
                Vector3.zero,
                smoothT
            );

            transform.Rotate(
                0f,
                0f,
                eatenRotateSpeed * Time.deltaTime
            );

            yield return null;
        }

        Destroy(gameObject);
    }
}
