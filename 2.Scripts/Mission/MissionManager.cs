using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager instance;

    private Mission currentMission;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Invoke(nameof(CreateDefaultKillEnemyMission), 0.1f);
    }

    private void Update()
    {
        if (currentMission != null && !currentMission.IsCompleted)
        {
            currentMission.UpdateMission();
        }
    }

    public void SetCurrentMission(Mission newMission)
    {
        if (currentMission != null)
        {
            currentMission.ResetMission();
        }

        currentMission = newMission;
    }

    public void StartMission()
    {
        currentMission.StartMission();
    }

    public void CreateAndStartKillEnemyMission(string missionName, string missionDescription, int amountToKill)
    {
        KillEnemyMission newMission = new KillEnemyMission(missionName, missionDescription, amountToKill);
        SetCurrentMission(newMission);
        StartMission();
    }

    public void CreateDefaultKillEnemyMission()
    {
        int totalEnemies = EnemyManager.Instance != null ? EnemyManager.Instance.TotalEnemyCount : 10;
        CreateAndStartKillEnemyMission("모든 적 처치", "맵의 모든 적을 처치하세요!", totalEnemies);
    }
}
