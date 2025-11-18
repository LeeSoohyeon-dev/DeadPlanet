using UnityEngine;

public abstract class Mission
{
    public string missionName;
    public string missionDescription;
    protected bool isCompleted = false;

    public virtual void StartMission()
    {

    }

    private bool hasVictoryTriggered = false;

    public virtual bool MissionCompleted()
    {
        if (isCompleted && !hasVictoryTriggered)
        {
            hasVictoryTriggered = true;
            GameEvents.OnGameVictory?.Invoke();
            return true;
        }
        return isCompleted;
    }

    public abstract bool CheckMissionComplete();
    public virtual void UpdateMission()
    {

    }

    public bool IsCompleted => isCompleted;

    public virtual void ResetMission()
    {
        isCompleted = false;
        hasVictoryTriggered = false;
    }
}
