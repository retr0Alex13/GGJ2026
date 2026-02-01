using System.Collections.Generic;
using UnityEngine;

public class LeverDoorRecording : ITaskEventRecording
{
    private CharañterType _ownerCharacter;
    private List<LeverPullEvent> _events = new();
    private Dictionary<int, Lever> _leversByIndex = new();
    private float _lastPlaybackTime = 0;

    public LeverDoorRecording(CharañterType owner)
    {
        _ownerCharacter = owner;
    }

    public void RegisterLever(int leverIndex, Lever lever)
    {
        if (_leversByIndex.ContainsKey(leverIndex))
        {
            _leversByIndex[leverIndex] = lever;
        }
        else
        {
            _leversByIndex.Add(leverIndex, lever);
        }

        Debug.Log($"Registered/Updated lever {leverIndex} for recording");
    }

    public void Clear()
    {
        _events.Clear();
        _lastPlaybackTime = 0;
        Debug.Log($"Cleared lever/door events for {_ownerCharacter}");
    }

    public void RecordEvent(float timestamp, object eventData)
    {
        if (eventData is LeverPullEvent evt)
        {
            _events.Add(evt);
            Debug.Log($"Recorded lever pull: Index={evt.leverIndex} at time={evt.timestamp:F2}");
        }
    }

    public void RecordLeverPull(float time, int leverIndex)
    {
        var evt = new LeverPullEvent
        {
            timestamp = time,
            leverIndex = leverIndex
        };
        RecordEvent(time, evt);
    }

    public void Playback(float currentTime)
    {
        foreach (var evt in _events)
        {
            if (evt.timestamp > _lastPlaybackTime && evt.timestamp <= currentTime)
            {
                if (_leversByIndex.TryGetValue(evt.leverIndex, out Lever lever))
                {
                    if (lever != null)
                    {
                        lever.PullLeverProgrammatically();
                        Debug.Log($"Replaying lever pull: Index={evt.leverIndex} at time={currentTime:F2}");
                    }
                    else
                    {
                        Debug.LogWarning($"Lever object with index {evt.leverIndex} is null (destroyed) during playback!");
                    }
                }
                else
                {
                    Debug.LogWarning($"Lever with index {evt.leverIndex} not found in dictionary for playback");
                }
            }
        }

        _lastPlaybackTime = currentTime;
    }

    public bool BelongsToCharacter(CharañterType character)
    {
        return _ownerCharacter == character;
    }
}

[System.Serializable]
public struct LeverPullEvent
{
    public float timestamp;
    public int leverIndex;
}