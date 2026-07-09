using UnityEngine;
using System.Collections;

public class PhaseManager : MonoBehaviour
{
    public static PhaseManager Instance { get; private set; }

    public enum GamePhase
    {
        Bubble,
        BlackHole,
        Result
    }

    [Header("Current Phase")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Bubble;

    [Header("Score Trigger")]
    [SerializeField] private bool enableScoreTrigger = true;
    [SerializeField] private int requiredScore = 300;

    [Header("Time Trigger")]
    [SerializeField] private bool enableTimeTrigger = true;
    [SerializeField] private float remainingTimeToTransform = 30f;

    [Header("Transition")]
    [SerializeField] private float transitionDelay = 1f;

    [Header("Visual Objects")]
    [SerializeField] private GameObject bubbleVisual;
    [SerializeField] private GameObject blackHoleVisual;

    [Header("Controllers")]
    [SerializeField] private PlayerBubbleController playerBubbleController;
    [SerializeField] private BlackHoleController blackHoleController;

    [Header("Layer Switch")]
    [SerializeField] private GameObject phaseObject;
    [SerializeField] private string bubbleLayerName = "Player";
    [SerializeField] private string blackHoleLayerName = "BlackHole";

    [Header("Fish Spawners")]
    [SerializeField] private FishSpawner leftRightSpawner;
    [SerializeField] private FishSpawner topBottomSpawner;

    private bool isTransitioning = false;

    public GamePhase CurrentPhase => currentPhase;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetBubblePhase();
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        if (currentPhase != GamePhase.Bubble)
            return;

        if (isTransitioning)
            return;

        CheckPhaseTrigger();
    }

    private void CheckPhaseTrigger()
    {
        bool scoreTriggered = false;
        bool timeTriggered = false;

        if (enableScoreTrigger && ScoreManager.Instance != null)
        {
            scoreTriggered = ScoreManager.Instance.GetScore() >= requiredScore;
        }

        if (enableTimeTrigger && GameManager.Instance != null)
        {
            timeTriggered = GameManager.Instance.GetCurrentTime() <= remainingTimeToTransform;
        }

        if (scoreTriggered || timeTriggered)
        {
            StartCoroutine(TransformToBlackHole());
        }
    }

    private IEnumerator TransformToBlackHole()
    {
        isTransitioning = true;

        if (playerBubbleController != null)
        {
            playerBubbleController.enabled = false;
        }

        yield return new WaitForSeconds(transitionDelay);

        SetBlackHolePhase();

        isTransitioning = false;
    }

    private void SetBubblePhase()
    {
        currentPhase = GamePhase.Bubble;

        SetLayer(phaseObject, bubbleLayerName);

        if (bubbleVisual != null)
            bubbleVisual.SetActive(true);

        if (blackHoleVisual != null)
            blackHoleVisual.SetActive(false);

        if (playerBubbleController != null)
            playerBubbleController.enabled = true;

        if (blackHoleController != null)
            blackHoleController.enabled = false;

        if (leftRightSpawner != null)
            leftRightSpawner.enabled = true;

        if (topBottomSpawner != null)
            topBottomSpawner.enabled = false;
    }

    private void SetBlackHolePhase()
    {
        currentPhase = GamePhase.BlackHole;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBlackHoleOpen();
        }

        SetLayer(phaseObject, blackHoleLayerName);

        if (bubbleVisual != null)
            bubbleVisual.SetActive(false);

        if (blackHoleVisual != null)
            blackHoleVisual.SetActive(true);

        if (playerBubbleController != null)
            playerBubbleController.enabled = false;

        if (blackHoleController != null)
            blackHoleController.enabled = true;

        if (leftRightSpawner != null)
            leftRightSpawner.enabled = true;

        if (topBottomSpawner != null)
            topBottomSpawner.enabled = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.MusicType.BlackHole);
        }

        Debug.Log("Phase Changed: BlackHole");
    }

    private void SetLayer(GameObject targetObject, string layerName)
    {
        if (targetObject == null)
            return;

        int layer = LayerMask.NameToLayer(layerName);

        if (layer == -1)
        {
            Debug.LogWarning("Layer not found: " + layerName);
            return;
        }

        targetObject.layer = layer;
    }
}
