using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized playback data storage with support for both movement and task events
/// </summary>
public static class PlaybackData
{
    public static int MasterRandomSeed { get; private set; }

    public static int activePlayerIndex = 0;

    public static Dictionary<CharañterType, List<CharacterFrame>> movementRecords = new();
    public static Dictionary<string, ITaskEventRecording> taskEventRecords = new();


    public static void SaveMovementRecord(CharañterType type, List<CharacterFrame> frames)
    {
        if (!movementRecords.ContainsKey(type))
            movementRecords[type] = new List<CharacterFrame>();

        movementRecords[type] = new List<CharacterFrame>(frames);
        Debug.Log($"Saved {frames.Count} movement frames for {type}");
    }


    public static void RegisterTaskEventRecording(string taskID, ITaskEventRecording recording)
    {
        if (!taskEventRecords.ContainsKey(taskID))
        {
            taskEventRecords[taskID] = recording;
            Debug.Log($"Registered task event recording for: {taskID}");
        }
    }

    public static ITaskEventRecording GetTaskEventRecording(string taskID)
    {
        return taskEventRecords.ContainsKey(taskID) ? taskEventRecords[taskID] : null;
    }


    public static void WipeAll()
    {
        Debug.Log("Wiping all playback data");
        movementRecords.Clear();

        foreach (var recording in taskEventRecords.Values)
        {
            recording.Clear();
        }

        activePlayerIndex = 0;

        MasterRandomSeed = (int)System.DateTime.Now.Ticks;
        Debug.Log($"Generated new Game Seed: {MasterRandomSeed}");
    }

    public static void WipeCurrentCharacter()
    {
        CharañterType currentType = (CharañterType)activePlayerIndex;

        if (movementRecords.ContainsKey(currentType))
        {
            movementRecords.Remove(currentType);
            Debug.Log($"Wiped movement data for {currentType}");
        }

        foreach (var recording in taskEventRecords.Values)
        {
            if (recording.BelongsToCharacter(currentType))
            {
                recording.Clear();
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        WipeAll();
    }
}

public interface ITaskEventRecording
{
    void Clear();
    void RecordEvent(float timestamp, object eventData);
    void Playback(float currentTime);
    bool BelongsToCharacter(CharañterType character);
}


[System.Serializable]
public struct CharacterFrame
{
    public Vector3 position;
    public Quaternion rotation;
    public float time;

    public CharacterFrame(Vector3 pos, Quaternion rot, float t)
    {
        position = pos;
        rotation = rot;
        time = t;
    }

    public static CharacterFrame Lerp(CharacterFrame a, CharacterFrame b, float t)
    {
        return new CharacterFrame(
            Vector3.Lerp(a.position, b.position, t),
            Quaternion.Slerp(a.rotation, b.rotation, t),
            Mathf.Lerp(a.time, b.time, t)
        );
    }
}
