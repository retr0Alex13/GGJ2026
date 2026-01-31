using System.Collections.Generic;
using UnityEngine;

public class FireExtinguisherRecording : ITaskEventRecording
{
    private CharañterType _ownerCharacter;
    private List<FireStateEvent> _fireEvents = new();
    private List<ExtinguisherActivationEvent> _activationEvents = new();
    private Dictionary<int, Fire> _firesByIndex = new();
    private Dictionary<int, float> _lastAppliedHealth = new();
    private float _lastPlaybackTime = 0;
    private Extinguisher _extinguisher;

    public FireExtinguisherRecording(CharañterType owner, Fire[] fires)
    {
        _ownerCharacter = owner;
        ReinitializeFires(fires);
    }

    public void SetExtinguisher(Extinguisher extinguisher)
    {
        _extinguisher = extinguisher;
    }

    public void ReinitializeFires(Fire[] fires)
    {
        _firesByIndex.Clear();
        _lastAppliedHealth.Clear();

        foreach (var fire in fires)
        {
            int fireIndex = fire.GetFireIndex();
            _firesByIndex[fireIndex] = fire;
            _lastAppliedHealth[fireIndex] = fire.HealthPoints;

            fire.SetPlaybackMode(!BelongsToCharacter(_ownerCharacter));
        }

        Debug.Log($"Initialized fire extinguisher recording with {fires.Length} fires");
    }

    public void Clear()
    {
        _fireEvents.Clear();
        _activationEvents.Clear();
        _lastPlaybackTime = 0;
        _lastAppliedHealth.Clear();
        Debug.Log($"Cleared fire extinguisher events for {_ownerCharacter}");
    }

    public void RecordEvent(float timestamp, object eventData)
    {
        if (eventData is FireStateEvent fireEvt)
        {
            _fireEvents.Add(fireEvt);
        }
        else if (eventData is ExtinguisherActivationEvent activationEvt)
        {
            _activationEvents.Add(activationEvt);
        }
    }

    public void RecordFireState(float time, int fireIndex, float health)
    {
        if (_lastAppliedHealth.TryGetValue(fireIndex, out float lastHealth))
        {
            if (Mathf.Abs(lastHealth - health) < 0.1f) return;
        }

        var evt = new FireStateEvent
        {
            timestamp = time,
            fireIndex = fireIndex,
            health = health
        };

        RecordEvent(time, evt);
        _lastAppliedHealth[fireIndex] = health;
    }

    public void RecordExtinguisherState(float time, bool isActive)
    {
        var evt = new ExtinguisherActivationEvent
        {
            timestamp = time,
            isActive = isActive
        };

        RecordEvent(time, evt);
    }

    public void Playback(float currentTime)
    {
        foreach (var evt in _activationEvents)
        {
            if (evt.timestamp > _lastPlaybackTime && evt.timestamp <= currentTime)
            {
                if (_extinguisher != null)
                {
                    _extinguisher.ActivateExtinguisher(evt.isActive);
                }
            }
        }

        var assignedRuntimeIndices = new HashSet<int>();

        foreach (var evt in _fireEvents)
        {
            if (evt.timestamp > _lastPlaybackTime && evt.timestamp <= currentTime)
            {
                if (_firesByIndex.TryGetValue(evt.fireIndex, out Fire fire) && !assignedRuntimeIndices.Contains(evt.fireIndex))
                {
                    fire.SetHealthForPlayback(evt.health);
                    assignedRuntimeIndices.Add(evt.fireIndex);
                }
                else
                {
                    Fire bestFire = null;
                    int bestKey = -1;
                    float bestDiff = float.MaxValue;

                    foreach (var kvp in _firesByIndex)
                    {
                        if (assignedRuntimeIndices.Contains(kvp.Key)) continue;

                        float diff = Mathf.Abs(kvp.Value.HealthPoints - evt.health);
                        if (diff < bestDiff)
                        {
                            bestDiff = diff;
                            bestFire = kvp.Value;
                            bestKey = kvp.Key;
                        }
                    }

                    const float remapThreshold = 20f;

                    if (bestFire != null && bestDiff <= remapThreshold)
                    {
                        Debug.Log($"Remapped missing recorded fire index {evt.fireIndex} -> runtime index {bestKey} (health diff {bestDiff:F1})");
                        bestFire.SetHealthForPlayback(evt.health);
                        assignedRuntimeIndices.Add(bestKey);

                        _firesByIndex[evt.fireIndex] = bestFire;
                    }
                    else
                    {
                        string available = _firesByIndex.Count > 0 ? string.Join(", ", _firesByIndex.Keys) : "<none>";
                        Debug.LogWarning($"Fire with index {evt.fireIndex} not found for playback. Available indices: {available}");
                    }
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
public struct FireStateEvent
{
    public float timestamp;
    public int fireIndex;
    public float health;
}

[System.Serializable]
public struct ExtinguisherActivationEvent
{
    public float timestamp;
    public bool isActive;
}