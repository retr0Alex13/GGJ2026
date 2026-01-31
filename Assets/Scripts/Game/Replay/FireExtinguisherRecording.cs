using System.Collections.Generic;
using UnityEngine;

public class FireExtinguisherRecording : ITaskEventRecording
{
    private CharañterType _ownerCharacter;
    private List<ExtinguisherEvent> _events = new();
    private float _lastPlaybackTime = 0;

    private Extinguisher _extinguisher;
    private Dictionary<int, Fire> _firesByIndex = new();

    public FireExtinguisherRecording(CharañterType owner, Extinguisher extinguisher, Fire[] fires)
    {
        _ownerCharacter = owner;
        _extinguisher = extinguisher;

        foreach (var fire in fires)
        {
            int fireIndex = fire.GetFireIndex();
            _firesByIndex[fireIndex] = fire;
        }

        Debug.Log($"Fire extinguisher recording initialized with {fires.Length} fires");
    }

    public void Clear()
    {
        _events.Clear();
        _lastPlaybackTime = 0;
        Debug.Log($"Cleared fire extinguisher events for {_ownerCharacter}");
    }

    public void RecordEvent(float timestamp, object eventData)
    {
        if (eventData is ExtinguisherEvent evt)
        {
            _events.Add(evt);
        }
    }

    public void Playback(float currentTime)
    {
        foreach (var evt in _events)
        {
            if (evt.timestamp > _lastPlaybackTime && evt.timestamp <= currentTime)
            {
                if (_firesByIndex.ContainsKey(evt.fireIndex))
                {
                    _firesByIndex[evt.fireIndex].Extinguish(evt.damageAmount);
                }
                else
                {
                    Debug.LogWarning($"Fire with index {evt.fireIndex} not found for playback");
                }
            }
        }

        _lastPlaybackTime = currentTime;
    }

    public bool BelongsToCharacter(CharañterType character)
    {
        return _ownerCharacter == character;
    }

    public void RecordFireDamage(float time, int fireIndex, float damage)
    {
        var evt = new ExtinguisherEvent
        {
            timestamp = time,
            fireIndex = fireIndex,
            damageAmount = damage
        };
        RecordEvent(time, evt);
    }
}

[System.Serializable]
public struct ExtinguisherEvent
{
    public float timestamp;
    public int fireIndex;
    public float damageAmount;
}
