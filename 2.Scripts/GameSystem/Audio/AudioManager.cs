using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using VInspector;

public enum SoundType
{
    BGM,
    Click,
    WeaponSwitch,
    Shoot,
    Reload,
    Loot,
    Restart,
    PlayerHurt,
    Victory,
    GameOver,
    EnemyShoot,
    EnemySlash,
    EnemyFlameThrower,
    EnemyJumpImpact,
    EnemyGrenadeImpact,
    EnemyAxeImpact,
    EnemyHurt,
    PlayerBulletHurt,
    BuffPickup,
    ShieldBreak,
}

[System.Serializable]
public class SoundSettings
{
    [Tooltip("중복 재생 방지를 위한 최소 재생 간격")]
    public float minPlayInterval = 0.1f;

    [Tooltip("우선순위 (0: BGM과 함께 재생 가능, 0보다 크면 BGM 중단)")]
    public int priority = 0;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public SerializedDictionary<SoundType, AudioSource> audioDictionary = new SerializedDictionary<SoundType, AudioSource>();

    public SoundSettings defaultSettings = new SoundSettings { minPlayInterval = 0.1f, priority = 0 };

    [Header("우선순위 설정")]
    public SerializedDictionary<SoundType, SoundSettings> exceptionSettings = new SerializedDictionary<SoundType, SoundSettings>();

    private Dictionary<SoundType, float> lastPlayTime = new Dictionary<SoundType, float>();

    private HashSet<SoundType> currentlyPlaying = new HashSet<SoundType>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPlaySound += PlayAudio;
        GameEvents.OnStopSound += StopAudio;
        GameEvents.OnStopBGM += StopBGM;
    }

    private void OnDisable()
    {
        GameEvents.OnPlaySound -= PlayAudio;
        GameEvents.OnStopSound -= StopAudio;
        GameEvents.OnStopBGM -= StopBGM;
    }

    private void Update()
    {
        UpdateCurrentlyPlaying();
    }

    public void PlayAudio(SoundType soundType)
    {
        if (!audioDictionary.TryGetValue(soundType, out AudioSource audioSource) || audioSource == null)
        {
            return;
        }

        SoundSettings settings = GetSoundSettings(soundType);

        if (!CanPlayAfterInterval(soundType, settings.minPlayInterval))
            return;

        if (soundType == SoundType.BGM)
        {
            PlaySoundInternal(audioSource, soundType);
            return;
        }

        if (!CheckAndHandlePriority(soundType, settings.priority))
            return;

        PlaySoundInternal(audioSource, soundType);
    }

    private void PlaySoundInternal(AudioSource audioSource, SoundType soundType)
    {
        audioSource.Play();
        lastPlayTime[soundType] = Time.time;
        currentlyPlaying.Add(soundType);
    }

    private bool CanPlayAfterInterval(SoundType soundType, float minPlayInterval)
    {
        if (!lastPlayTime.ContainsKey(soundType))
            return true;

        float timeSinceLastPlay = Time.time - lastPlayTime[soundType];
        return timeSinceLastPlay >= minPlayInterval;
    }

    private bool CheckAndHandlePriority(SoundType soundType, int priority)
    {
        int maxPriority = GetMaxPriorityOfPlayingSounds();
        if (maxPriority > priority)
            return false;

        StopLowerPrioritySounds(priority);

        if (priority > 0 && currentlyPlaying.Contains(SoundType.BGM))
        {
            StopBGM();
        }

        return true;
    }

    public void StopAudio(SoundType soundType)
    {
        if (audioDictionary.TryGetValue(soundType, out AudioSource audioSource) && audioSource != null)
        {
            audioSource.Stop();
            currentlyPlaying.Remove(soundType);
        }
    }

    public void StopBGM()
    {
        StopAudio(SoundType.BGM);
    }

    private SoundSettings GetSoundSettings(SoundType soundType)
    {
        if (exceptionSettings != null && exceptionSettings.TryGetValue(soundType, out SoundSettings settings) && settings != null)
        {
            return settings;
        }

        return defaultSettings;
    }

    private int GetMaxPriorityOfPlayingSounds()
    {
        int maxPriority = int.MinValue;

        foreach (SoundType soundType in currentlyPlaying)
        {
            if (soundType == SoundType.BGM)
                continue;

            SoundSettings settings = GetSoundSettings(soundType);
            if (settings.priority > maxPriority)
            {
                maxPriority = settings.priority;
            }
        }

        return maxPriority == int.MinValue ? 0 : maxPriority;
    }

    private void StopLowerPrioritySounds(int priority)
    {
        List<SoundType> toStop = new List<SoundType>();

        foreach (SoundType soundType in currentlyPlaying)
        {
            if (soundType == SoundType.BGM)
                continue;

            SoundSettings settings = GetSoundSettings(soundType);

            if (settings.priority < priority)
            {
                toStop.Add(soundType);
            }
        }

        foreach (SoundType soundType in toStop)
        {
            StopAudio(soundType);
        }
    }

    private void UpdateCurrentlyPlaying()
    {
        if (currentlyPlaying.Count == 0)
            return;

        List<SoundType> toRemove = new List<SoundType>();

        foreach (SoundType soundType in currentlyPlaying)
        {
            if (!audioDictionary.TryGetValue(soundType, out AudioSource audioSource) || 
                audioSource == null || 
                !audioSource.isPlaying)
            {
                toRemove.Add(soundType);
            }
        }

        foreach (SoundType soundType in toRemove)
        {
            currentlyPlaying.Remove(soundType);
        }
    }
}
