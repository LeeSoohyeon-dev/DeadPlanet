using UnityEngine;

public class KillEnemyMission : Mission
{
    private int amountToKill;
    private int currentKillCount = 0;
    private int totalEnemyCount = 0;

    public KillEnemyMission(string missionName, string missionDescription, int amountToKill)
    {
        this.missionName = missionName;
        this.missionDescription = missionDescription;
        this.amountToKill = amountToKill;
    }

    public override void StartMission()
    {
        currentKillCount = 0;
        totalEnemyCount = EnemyManager.Instance != null ? EnemyManager.Instance.TotalEnemyCount : amountToKill;

        if (amountToKill > totalEnemyCount)
        {
            amountToKill = totalEnemyCount;
        }

        GameEvents.OnAnyEnemyDied += OnAnyEnemyDied;

        UpdateMissionUI();
    }

    public override bool CheckMissionComplete()
    {
        return currentKillCount >= amountToKill;
    }

    public override void UpdateMission()
    {
        base.UpdateMission();

        if (CheckMissionComplete() && !isCompleted)
        {
            OnMissionComplete();
        }
    }

    private void OnAnyEnemyDied(Enemy enemy)
    {
        currentKillCount++;
        UpdateMissionUI();

    }

    private void OnMissionComplete()
    {
        isCompleted = true;

        GameEvents.OnAnyEnemyDied -= OnAnyEnemyDied;

        MissionCompleted();
    }

    private void UpdateMissionUI()
    {
        int remainingEnemies = amountToKill - currentKillCount;
        GameEvents.OnMissionUIUpdate?.Invoke(remainingEnemies, currentKillCount);
    }

    public override void ResetMission()
    {
        base.ResetMission();
        currentKillCount = 0;

        GameEvents.OnAnyEnemyDied -= OnAnyEnemyDied;
    }

    public int CurrentKillCount => currentKillCount;

    public int AmountToKill => amountToKill;

    public int RemainingEnemies => amountToKill - currentKillCount;

    private void OnDestroy()
    {
        GameEvents.OnAnyEnemyDied -= OnAnyEnemyDied;
    }
}