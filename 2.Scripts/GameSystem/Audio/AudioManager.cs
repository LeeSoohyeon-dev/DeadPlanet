using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
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
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public SerializedDictionary<SoundType, AudioSource> audioDictionary = new SerializedDictionary<SoundType, AudioSource>();



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
        
        GameEvents.OnPlaySound += PlayAudio;
        GameEvents.OnStopBGM += StopBGM;
        GameEvents.OnStopAllAudio += StopAllAudio;
    }

    private void OnDestroy()
    {
        GameEvents.OnPlaySound -= PlayAudio;
        GameEvents.OnStopBGM -= StopBGM;
        GameEvents.OnStopAllAudio -= StopAllAudio;
    }

    public void PlaySFX(AudioSource sfx, bool randomPitch = false, float minPitch = .85f, float maxPitch = 1.1f)
    {
        if (sfx == null)
            return;

        float pitch = Random.Range(minPitch, maxPitch);

        sfx.pitch = pitch;
        sfx.Play();
    }

    public void PlayAudio(SoundType soundType)
    {
        if(soundType != SoundType.Restart){
            if (IsAudioPlaying(SoundType.Victory) || IsAudioPlaying(SoundType.GameOver))
            {
                return;
            }
        }

        if (audioDictionary == null)
        {
            Debug.LogWarning("오디오 딕셔너리가 초기화되지 않았습니다.");
            return;
        }

        if (audioDictionary.TryGetValue(soundType, out AudioSource audioSource) && audioSource != null)
        {
            PlaySFX(audioSource, true, 0.9f, 1.1f);
        }
        else
        {
            Debug.LogWarning($"오디오를 찾을 수 없습니다: {soundType}");
        }
    }

    public void StopAudio(SoundType soundType)
    {
        if (audioDictionary == null)
        {
            Debug.LogWarning("오디오 딕셔너리가 초기화되지 않았습니다.");
            return;
        }

        if (audioDictionary.TryGetValue(soundType, out AudioSource audioSource) && audioSource != null)
        {
            audioSource.Stop();
        }
        else
        {
            Debug.LogWarning($"오디오를 찾을 수 없습니다: {soundType}");
        }
    }

    public void StopAllAudio()
    {
        if (audioDictionary == null)
        {
            Debug.LogWarning("오디오 딕셔너리가 초기화되지 않았습니다.");
            return;
        }

        foreach (var audioSource in audioDictionary.Values)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
    }

    public void StopAllAudioExcept(SoundType excludeSound)
    {
        if (audioDictionary == null)
        {
            Debug.LogWarning("오디오 딕셔너리가 초기화되지 않았습니다.");
            return;
        }

        foreach (var kvp in audioDictionary)
        {
            if (kvp.Key != excludeSound && kvp.Value != null)
            {
                kvp.Value.Stop();
            }
        }
    }


    public void SFXDelayAndFade(AudioSource source, bool play, float targetVolume, float delay = 0, float fadeDuration = 1)
    {
        StartCoroutine(SFXDelayAndFadeCo(source, play, targetVolume, delay, fadeDuration));
    }

    public void PlayBGM()
    {
        StopBGM();

        if (audioDictionary.TryGetValue(SoundType.BGM, out AudioSource audioSource) && audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("BGM을 찾을 수 없습니다.");
        }
    }

    public void StopBGM()
    {
        if (audioDictionary.TryGetValue(SoundType.BGM, out AudioSource audioSource) && audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private bool IsBgmPlaying()
    {
        if (audioDictionary.TryGetValue(SoundType.BGM, out AudioSource audioSource) && audioSource != null)
        {
            return audioSource.isPlaying;
        }
        return false;
    }

    private bool IsAudioPlaying(SoundType soundType)
    {
        if (audioDictionary.TryGetValue(soundType, out AudioSource audioSource) && audioSource != null)
        {
            return audioSource.isPlaying;
        }
        return false;
    }

    private IEnumerator SFXDelayAndFadeCo(AudioSource source,bool play, float targetVolume, float delay = 0, float fadeDuration = 1)
    {
        yield return new WaitForSeconds(delay);

        float startVolume = play ? 0 : source.volume;
        float endVolume = play ? targetVolume : 0;
        float elapsed = 0;

        if (play)
        {
            source.volume = 0;
            source.Play();
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume,endVolume, elapsed/ fadeDuration);
            yield return null;
        }

        source.volume = endVolume;

        if (play == false)
            source.Stop();
    }
}
