using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    [SerializeField] private float resumeRate = 3;
    [SerializeField] private float pauseRate = 7;

    private float timeAdjustRate;
    private float targetTimeScale = 1;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnSlowMotion += SlowMotionFor;
        GameEvents.OnPauseTime += PauseTime;
        GameEvents.OnResumeTime += ResumeTime;
    }

    private void OnDisable()
    {
        GameEvents.OnSlowMotion -= SlowMotionFor;
        GameEvents.OnPauseTime -= PauseTime;
        GameEvents.OnResumeTime -= ResumeTime;
    }

    private void Update()
    {
        if (Mathf.Abs(Time.timeScale - targetTimeScale) > .05f)
        {
            float adjustRate = Time.unscaledDeltaTime * timeAdjustRate;
            Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, adjustRate);
        }
        else
            Time.timeScale = targetTimeScale;
    }

    public void PauseTime()
    {
        timeAdjustRate = pauseRate;
        targetTimeScale = 0;
    }

    public void ResumeTime()
    {
        timeAdjustRate = resumeRate;
        targetTimeScale = 1;
    }

    public void SlowMotionFor(float seconds)
    {
        SlowMotionForAsync(seconds, this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTask SlowMotionForAsync(float seconds, CancellationToken ct = default)
    {
        timeAdjustRate = resumeRate;
        targetTimeScale = .5f;
        await UniTask.WaitForSeconds(seconds, ignoreTimeScale: true, cancellationToken: ct);
        ResumeTime();
    }
}
