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
            // 같은 GameObject에 EnemyManager가 함께 있어 gameObject를 파괴하면 안 됨
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
        // EnemyManager.Start()의 적 수 초기화가 끝난 뒤 목표치를 읽기 위한 지연
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
        currentMission?.StartMission();
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

    private void OnDestroy()
    {
        // Mission(비 MonoBehaviour)의 static 이벤트 구독을 대신 해제
        if (currentMission != null)
        {
            currentMission.ResetMission();
        }

        if (instance == this)
        {
            instance = null;
        }
    }
}
