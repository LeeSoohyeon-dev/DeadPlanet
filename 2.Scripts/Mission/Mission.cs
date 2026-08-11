
public abstract class Mission
{
    public string missionName;
    public string missionDescription;

    protected bool isCompleted = false;
    private bool hasVictoryTriggered = false;

    public bool IsCompleted => isCompleted;

    public virtual void StartMission()
    {

    }

    public abstract bool CheckMissionComplete();

    public virtual void UpdateMission()
    {

    }

    protected void TriggerVictory()
    {
        if (hasVictoryTriggered)
            return;

        hasVictoryTriggered = true;
        GameEvents.RaiseGameVictory();
    }

    public virtual void ResetMission()
    {
        isCompleted = false;
        hasVictoryTriggered = false;
    }
}
