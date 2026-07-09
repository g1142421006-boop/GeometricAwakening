using UnityEngine;
using System.Collections;

public class BlackHoleController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Patrol Area")]
    [SerializeField] private Vector2 clampMin = new Vector2(-8.5f, -4.5f);
    [SerializeField] private Vector2 clampMax = new Vector2(8.5f, 4.5f);

    [Header("Patrol")]
    [SerializeField] private float arriveDistance = 0.2f;

    [Header("Local Patrol")]
    [SerializeField] private bool useLocalPatrol = true;

    [SerializeField] private float patrolRadius = 3f;

    [SerializeField] private int maxFindPositionAttempts = 20;


    [Header("Fish Search")]
    [SerializeField] private LayerMask fishLayer;
    [SerializeField] private float searchRadius = 5f;
    [SerializeField] private float searchInterval = 0.5f;

    [Header("Eat Fish")]
    [SerializeField] private float eatRadius = 0.6f;

    [Header("Score Penalty")]
    [Range(0f, 1f)]
    [SerializeField] private float scorePenaltyMultiplier = 0.5f;

    [Header("Eat Cooldown")]
    [SerializeField] private float eatCooldown = 1f;

    [Header("Chase Limit")]
    [SerializeField] private float maxChaseTime = 3f;
    [SerializeField] private float maxChaseDistance = 7f;
    [SerializeField] private float lostFishDelay = 0.5f;

    [Header("Far Patrol")]
    [SerializeField] private float farPatrolMinDistance = 3f;
    [SerializeField] private float farPatrolMaxDistance = 6f;

    [Header("Patrol Preference")]
    [SerializeField] private bool enablePatrolPreference = true;
    [SerializeField] private int centerWeight = 70;
    [SerializeField] private int edgeWeight = 30;
    [SerializeField] private Vector2 centerAreaSize = new Vector2(8f, 4f);

    [Header("Transform")]
    [SerializeField] private float blackHoleScale = 0.5f;
    [SerializeField] private float scaleTransitionDuration = 0.3f;

    private Vector3 patrolTarget;

    private FishController targetFish;

    private float searchTimer;

    private bool isChasing;

    private float cooldownTimer = 0f;

    private float chaseTimer = 0f;
    private float lostFishTimer = 0f;

    private void OnEnable()
    {
        transform.localScale = Vector3.one;

        StartCoroutine(ScaleAnimation());

        PickNewTarget(false);
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            !GameManager.Instance.IsPlaying)
            return;

        cooldownTimer -= Time.deltaTime;
        cooldownTimer = Mathf.Max(cooldownTimer, 0f);

        bool canChase = cooldownTimer <= 0f;

        if (canChase)
        {
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchInterval)
            {
                searchTimer = 0f;
                SearchFish();
            }
        }
        else
        {
            targetFish = null;
            isChasing = false;
        }

        if (isChasing)
        {
            ChaseFish();
        }
        else
        {
            Patrol();
        }

        EatNearbyFish();
    }

    private void Patrol()
    {
        Vector3 nextPosition = Vector3.MoveTowards(
       transform.position,
       patrolTarget,
       patrolSpeed * Time.deltaTime
   );

        nextPosition.x = Mathf.Clamp(nextPosition.x, clampMin.x, clampMax.x);
        nextPosition.y = Mathf.Clamp(nextPosition.y, clampMin.y, clampMax.y);

        transform.position = nextPosition;

        if (Vector3.Distance(transform.position, patrolTarget) <= arriveDistance)
        {
            PickNewTarget(false);
        }
    }

    private void PickNewTarget(bool farPatrol)
    {
        bool preferCenter = ShouldPreferCenter();

        if (preferCenter)
        {
            patrolTarget = PickCenterTarget();
            return;
        }

        float minDistance = 0.5f;
        float maxDistance = patrolRadius;

        if (farPatrol)
        {
            minDistance = farPatrolMinDistance;
            maxDistance = farPatrolMaxDistance;
        }

        for (int i = 0; i < maxFindPositionAttempts; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;

            float distance = Random.Range(
                minDistance,
                maxDistance
            );

            Vector3 candidate =
                transform.position +
                new Vector3(direction.x, direction.y, 0f) * distance;

            candidate.x = Mathf.Clamp(candidate.x, clampMin.x, clampMax.x);
            candidate.y = Mathf.Clamp(candidate.y, clampMin.y, clampMax.y);

            float actualDistance = Vector3.Distance(
                transform.position,
                candidate
            );

            if (actualDistance >= minDistance)
            {
                patrolTarget = candidate;
                return;
            }
        }

        patrolTarget = PickCenterTarget();
    }

    private void SearchFish()
    {
        Collider2D[] fishes =
            Physics2D.OverlapCircleAll(
                transform.position,
                searchRadius,
                fishLayer
            );

        if (fishes.Length == 0)
        {
            targetFish = null;
            isChasing = false;
            return;
        }

        float nearestDistance = float.MaxValue;
        FishController nearestFish = null;

        foreach (Collider2D fish in fishes)
        {
            FishController fishController =
                fish.GetComponent<FishController>();

            if (fishController == null)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    fish.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestFish = fishController;
            }
        }

        targetFish = nearestFish;

        isChasing = targetFish != null;

        if (isChasing)
        {
            chaseTimer = 0f;
            lostFishTimer = 0f;
        }
    }

    private void ChaseFish()
    {
        if (targetFish == null)
        {
            lostFishTimer += Time.deltaTime;

            if (lostFishTimer >= lostFishDelay)
            {
                StopChase();
            }

            return;
        }

        lostFishTimer = 0f;
        chaseTimer += Time.deltaTime;

        float distanceToFish = Vector3.Distance(
            transform.position,
            targetFish.transform.position
        );

        if (chaseTimer >= maxChaseTime || distanceToFish >= maxChaseDistance)
        {
            StopChase();
            PickNewTarget(false);
            return;
        }

        Vector3 targetPosition = targetFish.transform.position;

        targetPosition.x = Mathf.Clamp(targetPosition.x, clampMin.x, clampMax.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, clampMin.y, clampMax.y);

        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            chaseSpeed * Time.deltaTime
        );

        nextPosition.x = Mathf.Clamp(nextPosition.x, clampMin.x, clampMax.x);
        nextPosition.y = Mathf.Clamp(nextPosition.y, clampMin.y, clampMax.y);

        transform.position = nextPosition;
    }

    private void EatNearbyFish()
    {
        Collider2D[] fishes = Physics2D.OverlapCircleAll(
        transform.position,
        eatRadius,
        fishLayer
    );

        foreach (Collider2D fishCollider in fishes)
        {
            FishController fish = fishCollider.GetComponent<FishController>();

            if (fish == null)
                continue;

            // 黑洞吞掉魚
            fish.DestroyByBlackHole(transform.position);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance?.PlayBlackHoleEat();
            }

            // 如果之後要扣分，可在 Inspector 設定負數
            if (ScoreManager.Instance != null && scorePenaltyMultiplier > 0f)
            {
                int penalty = Mathf.RoundToInt(
                    fish.ScoreValue * scorePenaltyMultiplier
                );

                penalty = Mathf.Clamp(
                    penalty,
                    0,
                    fish.ScoreValue
                );

                if (penalty > 0)
                {
                    ScoreManager.Instance.AddScore(-penalty);
                    UIManager.Instance.ShowScorePopup(
                        -penalty,
                        fish.transform.position,
                        true
                    );
                }
            }

            // ===== 吃到魚後進入冷卻 =====
            cooldownTimer = eatCooldown;

            // ===== 停止追擊，回到巡邏 =====
            StopChase();

            // ===== 巡邏重新找一個目的地 =====
            PickNewTarget(true);

            // 一次只吃一隻，避免同一幀吃掉整群
            break;
        }
    }

    private void StopChase()
    {
        targetFish = null;
        isChasing = false;
        chaseTimer = 0f;
        lostFishTimer = 0f;
    }

    private bool ShouldPreferCenter()
    {
        if (!enablePatrolPreference)
            return false;

        int totalWeight = Mathf.Max(0, centerWeight) + Mathf.Max(0, edgeWeight);

        if (totalWeight <= 0)
            return false;

        int randomValue = Random.Range(0, totalWeight);

        return randomValue < centerWeight;
    }

    private Vector3 PickCenterTarget()
    {
        Vector3 center = new Vector3(
            (clampMin.x + clampMax.x) * 0.5f,
            (clampMin.y + clampMax.y) * 0.5f,
            transform.position.z
        );

        float halfWidth = centerAreaSize.x * 0.5f;
        float halfHeight = centerAreaSize.y * 0.5f;

        Vector3 target = new Vector3(
            Random.Range(center.x - halfWidth, center.x + halfWidth),
            Random.Range(center.y - halfHeight, center.y + halfHeight),
            transform.position.z
        );

        target.x = Mathf.Clamp(target.x, clampMin.x, clampMax.x);
        target.y = Mathf.Clamp(target.y, clampMin.y, clampMax.y);

        return target;
    }

    private IEnumerator ScaleAnimation()
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = Vector3.one * blackHoleScale;

        float timer = 0f;

        while (timer < scaleTransitionDuration)
        {
            timer += Time.deltaTime;

            float t = timer / scaleTransitionDuration;

            // SmoothStep
            t = t * t * (3f - 2f * t);

            transform.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                t
            );

            yield return null;
        }

        transform.localScale = targetScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 center =
            (Vector3)(clampMin + clampMax) * 0.5f;

        Vector3 size =
            new Vector3(
                clampMax.x - clampMin.x,
                clampMax.y - clampMin.y,
                0f
            );

        Gizmos.DrawWireCube(center, size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(patrolTarget, 0.12f);

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            patrolRadius
        );

        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            searchRadius
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, eatRadius);
    }
}
