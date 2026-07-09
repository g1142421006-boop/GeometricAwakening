using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public enum SpawnDirection
    {
        LeftRight,
        TopBottom
    }

    [Header("Fish List")]
    [SerializeField] private FishSpawnInfo[] fishList;

    [Header("Spawn Area")]
    [SerializeField] private float spawnXRight = 10f;
    [SerializeField] private float spawnXLeft = -10f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;

    [Header("Spawn Interval")]
    [SerializeField] private float minSpawnInterval = 0.8f;
    [SerializeField] private float maxSpawnInterval = 2.0f;

    [Header("Spawn Count")]
    [SerializeField] private int minSpawnCount = 1;
    [SerializeField] private int maxSpawnCount = 3;

    [Header("Spawn Limit")]
    [SerializeField] private int maxFishCount = 30;

    [Header("Multi Spawn Offset")]
    [SerializeField] private float minYSpacing = 0.4f;
    [SerializeField] private float xSpacing = 0.4f;

    [Header("Side Balance")]
    [SerializeField] private int maxSideDifference = 2;

    [Header("Spawn Direction")]
    [SerializeField] private SpawnDirection spawnDirection = SpawnDirection.LeftRight;

    private float timer;
    private float nextSpawnTime;

    private int leftSpawnCount = 0;
    private int rightSpawnCount = 0;

    private void Start()
    {
        SetNextSpawnTime();
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            timer = 0f;
            SpawnFishGroup();
            SetNextSpawnTime();
        }
    }

    private void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private void SpawnFishGroup()
    {
        int currentFishCount = GameObject.FindGameObjectsWithTag("Fish").Length;

        if (currentFishCount >= maxFishCount)
            return;

        int spawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);

        int availableCount = maxFishCount - currentFishCount;
        spawnCount = Mathf.Min(spawnCount, availableCount);

        bool spawnRight = GetBalancedSpawnSide();

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab = GetRandomFishPrefab();

            if (prefab == null)
                continue;

            SpawnFish(prefab, spawnRight, i, spawnCount);
        }
    }

    private bool GetBalancedSpawnSide()
    {
        int difference = rightSpawnCount - leftSpawnCount;

        if (difference >= maxSideDifference)
        {
            leftSpawnCount++;
            return false;
        }

        if (difference <= -maxSideDifference)
        {
            rightSpawnCount++;
            return true;
        }

        bool spawnRight = Random.value > 0.5f;

        if (spawnRight)
            rightSpawnCount++;
        else
            leftSpawnCount++;

        return spawnRight;
    }

    private void SpawnFish(GameObject prefab, bool spawnRight, int index, int totalCount)
    {
        Vector3 spawnPos;
        Vector3 moveDirection;

        if (spawnDirection == SpawnDirection.LeftRight)
        {
            float baseX = spawnRight ? spawnXRight : spawnXLeft;

            float xOffset = index * xSpacing;
            float x = spawnRight ? baseX + xOffset : baseX - xOffset;

            float y = GetRandomY(index, totalCount);

            spawnPos = new Vector3(x, y, 0f);
            moveDirection = spawnRight ? Vector3.left : Vector3.right;
        }
        else
        {
            bool spawnTop = Random.value > 0.5f;

            float y = spawnTop ? maxY : minY;
            float x = Random.Range(spawnXLeft, spawnXRight);

            float yOffset = index * xSpacing;
            y = spawnTop ? y + yOffset : y - yOffset;

            spawnPos = new Vector3(x, y, 0f);
            moveDirection = spawnTop ? Vector3.down : Vector3.up;
        }

        GameObject fishObj = Instantiate(
            prefab,
            spawnPos,
            Quaternion.identity
        );

        FishController fish = fishObj.GetComponent<FishController>();

        if (fish != null)
        {
            fish.SetMoveDirection(moveDirection);
        }
    }

    private float GetRandomY(int index, int totalCount)
    {
        if (totalCount <= 1)
            return Random.Range(minY, maxY);

        float range = maxY - minY;
        float segmentHeight = range / totalCount;

        float segmentMin = minY + segmentHeight * index;
        float segmentMax = segmentMin + segmentHeight;

        float y = Random.Range(segmentMin, segmentMax);

        if (index > 0)
        {
            y += Random.Range(-minYSpacing, minYSpacing);
        }

        return Mathf.Clamp(y, minY, maxY);
    }

    private GameObject GetRandomFishPrefab()
    {
        int totalWeight = 0;

        foreach (FishSpawnInfo fish in fishList)
        {
            if (fish.prefab == null)
                continue;

            if (fish.weight <= 0)
                continue;

            totalWeight += fish.weight;
        }

        if (totalWeight <= 0)
            return null;

        int random = Random.Range(0, totalWeight);
        int current = 0;

        foreach (FishSpawnInfo fish in fishList)
        {
            if (fish.prefab == null)
                continue;

            if (fish.weight <= 0)
                continue;

            current += fish.weight;

            if (random < current)
                return fish.prefab;
        }

        return null;
    }
}
