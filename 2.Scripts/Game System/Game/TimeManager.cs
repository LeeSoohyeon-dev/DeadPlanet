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

    // 진행 중인 슬로우모션 예약을 취소하기 위한 토큰
    private CancellationTokenSource slowMotionCts;

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
        CancelPendingSlowMotion();

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
        CancelPendingSlowMotion();

        slowMotionCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        SlowMotionForAsync(seconds, slowMotionCts.Token).Forget();
    }

    private async UniTask SlowMotionForAsync(float seconds, CancellationToken ct)
    {
        timeAdjustRate = resumeRate;
        targetTimeScale = .5f;
        await UniTask.WaitForSeconds(seconds, ignoreTimeScale: true, cancellationToken: ct);
        ResumeTime();
    }

    private void CancelPendingSlowMotion()
    {
        if (slowMotionCts != null)
        {
            slowMotionCts.Cancel();
            slowMotionCts.Dispose();
            slowMotionCts = null;
        }
    }

    private void OnDestroy()
    {
        CancelPendingSlowMotion();

        if (instance == this)
        {
            instance = null;
        }
    }
}
