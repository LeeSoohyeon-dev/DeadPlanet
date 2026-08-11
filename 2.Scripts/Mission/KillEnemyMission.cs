using UnityEngine;

public class KillEnemyMission : Mission
{
    private int amountToKill;
    private int fallbackKillCount = 0;
    private int lastKnownTargetCount = -1;

    public KillEnemyMission(string missionName, string missionDescription, int amountToKill)
    {
        this.missionName = missionName;
        this.missionDescription = missionDescription;
        this.amountToKill = amountToKill;
    }

    // 목표치 = 현재까지 등록된 전체 적 수 (웨이브 스폰에 따라 증가)
    private int TotalTargetCount =>
        EnemyManager.Instance != null ? EnemyManager.Instance.TotalEnemyCount : amountToKill;

    // 진행도는 EnemyManager의 집계를 단일 소스로 사용 (미션 생성 전에 죽은 적도 포함)
    private int CurrentKillCount =>
        EnemyManager.Instance != null ? EnemyManager.Instance.KilledEnemyCount : fallbackKillCount;

    public override void StartMission()
    {
        fallbackKillCount = 0;
        lastKnownTargetCount = TotalTargetCount;

        GameEvents.OnAnyEnemyDied += OnAnyEnemyDied;

        UpdateMissionUI();
    }

    public override bool CheckMissionComplete()
    {
        if (WaveManager.instance != null && !WaveManager.instance.AllWavesCompleted)
            return false;

        return CurrentKillCount >= TotalTargetCount;
    }

    public override void UpdateMission()
    {
        base.UpdateMission();

        if (TotalTargetCount != lastKnownTargetCount)
        {
            lastKnownTargetCount = TotalTargetCount;
            UpdateMissionUI();
        }

        if (CheckMissionComplete() && !isCompleted)
        {
            OnMissionComplete();
        }
    }

    private void OnAnyEnemyDied(Enemy enemy)
    {
        fallbackKillCount++;
        UpdateMissionUI();
    }

    private void OnMissionComplete()
    {
        isCompleted = true;

        GameEvents.OnAnyEnemyDied -= OnAnyEnemyDied;

        TriggerVictory();
    }

    private void UpdateMissionUI()
    {
        int killCount = CurrentKillCount;
        int remainingEnemies = Mathf.Max(0, TotalTargetCount - killCount);
        GameEvents.RaiseMissionUIUpdate(remainingEnemies, killCount);
    }

    public override void ResetMission()
    {
        base.ResetMission();
        fallbackKillCount = 0;

        GameEvents.OnAnyEnemyDied -= OnAnyEnemyDied;
    }

}
