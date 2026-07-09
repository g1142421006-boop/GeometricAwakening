using UnityEngine;

[System.Serializable]
public class FishSpawnInfo
{
    [Header("Fish Prefab")]
    public GameObject prefab;

    [Header("Spawn Weight")]
    [Min(0)]
    public int weight = 10;
}