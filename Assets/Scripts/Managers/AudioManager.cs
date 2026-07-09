using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public enum MusicType
    {
        Bubble,
        BlackHole
    }

    [Header("BGM Clips")]
    [SerializeField] private AudioClip bubbleBGM;
    [SerializeField] private AudioClip blackHoleBGM;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sourceA;
    [SerializeField] private AudioSource sourceB;

    [Header("Settings")]
    [SerializeField] private float fadeTime = 1.2f;

    [Header("SFX Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("SFX Settings")]
    [SerializeField] private float sfxVolume = 1f;

    [Header("Master Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxMasterVolume = 1f;

    [Header("UI SFX")]

    [SerializeField] private AudioClip uiButtonClickSFX;

    [Range(0f, 1f)]
    [SerializeField] private float uiButtonVolume = 0.7f;

    [SerializeField] private float uiButtonCooldown = 0.05f;

    [Header("Bubble SFX")]

    [SerializeField] private AudioClip bubbleEatSFX;

    [Range(0f, 1f)]
    [SerializeField] private float bubbleEatVolume = 0.65f;

    [SerializeField] private float bubbleEatCooldown = 0.05f;

    [Header("Wave SFX")]

    [SerializeField] private AudioClip waveRippleSFX;

    [Range(0f, 1f)]
    [SerializeField] private float waveRippleVolume = 0.8f;

    [SerializeField] private float waveRippleCooldown = 0.2f;

    [Header("Black Hole Open")]

    [SerializeField] private AudioClip blackHoleOpenSFX;

    [Range(0f, 1f)]
    [SerializeField] private float blackHoleOpenVolume = 1f;

    [SerializeField] private float blackHoleOpenCooldown = 0.5f;

    [Header("Black Hole Eat")]

    [SerializeField] private AudioClip blackHoleEatSFX;

    [Range(0f, 1f)]
    [SerializeField] private float blackHoleEatVolume = 0.75f;

    [SerializeField] private float blackHoleEatCooldown = 0.12f;

    private float lastUIButtonTime = -999f;
    private float lastBubbleEatTime = -999f;
    private float lastWaveRippleTime = -999f;
    private float lastBlackHoleOpenTime = -999f;
    private float lastBlackHoleEatTime = -999f;

    private AudioSource currentSource;
    private AudioSource nextSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentSource = sourceA;
        nextSource = sourceB;
    }

    private void Start()
    {
        PlayMusic(MusicType.Bubble, true);
        Debug.Log("AudioManager Start");
    }

    public void PlayMusic(MusicType musicType, bool instant = false)
    {
        Debug.Log("PlayMusic : " + musicType);
        AudioClip targetClip = GetClip(musicType);

        if (targetClip == null)
            return;

        if (currentSource.clip == targetClip)
            return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (instant)
        {
            currentSource.clip = targetClip;
            currentSource.loop = true;
            currentSource.volume = bgmVolume;
            currentSource.Play();
            return;
        }

        fadeCoroutine = StartCoroutine(CrossFade(targetClip));
    }

    public void PlayButtonClick()
    {
        PlaySFXWithCooldown(
            uiButtonClickSFX,
            uiButtonVolume,
            uiButtonCooldown,
            ref lastUIButtonTime
        );
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayBubbleEat()
    {
        PlaySFXWithCooldown(
            bubbleEatSFX,
            bubbleEatVolume,
            bubbleEatCooldown,
            ref lastBubbleEatTime
        );
    }

    public void PlayWaveRipple()
    {
        PlaySFXWithCooldown(
            waveRippleSFX,
            waveRippleVolume,
            waveRippleCooldown,
            ref lastWaveRippleTime
        );
    }

    public void PlayBlackHoleOpen()
    {
        PlaySFXWithCooldown(
            blackHoleOpenSFX,
            blackHoleOpenVolume,
            blackHoleOpenCooldown,
            ref lastBlackHoleOpenTime
        );
    }

    public void PlayBlackHoleEat()
    {
        PlaySFXWithCooldown(
            blackHoleEatSFX,
            blackHoleEatVolume,
            blackHoleEatCooldown,
            ref lastBlackHoleEatTime
        );
    }

    private void PlaySFXWithCooldown(
     AudioClip clip,
    float volume,
    float cooldown,
    ref float lastPlayTime
)
    {
        if (clip == null || sfxSource == null)
            return;

        if (Time.time - lastPlayTime < cooldown)
            return;

        lastPlayTime = Time.time;

        sfxSource.PlayOneShot(
            clip,
            volume * sfxMasterVolume
        );
    }

    private AudioClip GetClip(MusicType musicType)
    {
        switch (musicType)
        {
            case MusicType.Bubble:
                return bubbleBGM;

            case MusicType.BlackHole:
                return blackHoleBGM;

            default:
                return null;
        }
    }

    private IEnumerator CrossFade(AudioClip newClip)
    {
        nextSource.clip = newClip;
        nextSource.loop = true;
        nextSource.volume = 0f;
        nextSource.Play();

        float timer = 0f;
        float startVolume = currentSource.volume;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeTime;

            currentSource.volume = Mathf.Lerp(startVolume, 0f, t);
            nextSource.volume = Mathf.Lerp(0f, bgmVolume, t);

            yield return null;
        }

        currentSource.Stop();
        currentSource.clip = null;
        currentSource.volume = 0f;

        AudioSource temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;

        fadeCoroutine = null;
    }
}
