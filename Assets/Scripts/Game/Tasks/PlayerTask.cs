using System;
using UnityEngine;

public abstract class PlayerTask
{
    [field: SerializeField]
    public string TaskName { get; protected set; }

    [field: SerializeField]
    public string TaskID { get; protected set; }

    [field: SerializeField]
    public int RequiredCount { get; protected set; }

    [field: SerializeField]
    public int CurrentCount { get; protected set; }

    public bool IsCompleted => CurrentCount >= RequiredCount;

    public PlayerTask(string taskName, string taskID, int requiredCount)
    {
        TaskName = taskName;
        RequiredCount = requiredCount;
        CurrentCount = 0;
    }

    public void IncrementProgress(int amount)
    {
        CurrentCount += amount;
        if (CurrentCount >= RequiredCount)
        {
            CurrentCount = RequiredCount;
        }
    }

}

[Serializable]
public class ElectricalTask : PlayerTask
{
    public ElectricalTask(string taskName, string taskID, int requiredCount) : base(taskName, taskID, requiredCount) { }
}

[Serializable]
public class FirefighterTask : PlayerTask
{
    public FirefighterTask(string taskName, string taskID, int requiredCount) : base(taskName, taskID, requiredCount) { }
}

[Serializable]
public class DoctorTask : PlayerTask
{
    public DoctorTask(string taskName, string taskID, int requiredCount) : base(taskName, taskID, requiredCount) { }
}
