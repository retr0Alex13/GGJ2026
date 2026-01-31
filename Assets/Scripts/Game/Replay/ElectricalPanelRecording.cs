using System.Collections.Generic;
using UnityEngine;

public class ElectricalPanelRecording : ITaskEventRecording
{
    private CharañterType _ownerCharacter;
    private List<WireConnectionEvent> _events = new();
    private float _lastPlaybackTime = 0;

    private ElectricalPanel _panel;

    public ElectricalPanelRecording(CharañterType owner, ElectricalPanel panel)
    {
        _ownerCharacter = owner;
        _panel = panel;
    }

    public void Clear()
    {
        _events.Clear();
        _lastPlaybackTime = 0;
        Debug.Log($"Cleared electrical panel events for {_ownerCharacter}");
    }

    public void RecordEvent(float timestamp, object eventData)
    {
        if (eventData is WireConnectionEvent wireEvent)
        {
            _events.Add(wireEvent);
            Debug.Log($"Recorded wire event at {timestamp:F2}s: Type={wireEvent.eventType}");
        }
    }

    public void Playback(float currentTime)
    {
        foreach (var evt in _events)
        {
            if (evt.timestamp > _lastPlaybackTime && evt.timestamp <= currentTime)
            {
                _panel?.ReplayWireConnection(evt);
            }
        }

        _lastPlaybackTime = currentTime;
    }

    public bool BelongsToCharacter(CharañterType character)
    {
        return _ownerCharacter == character;
    }

    public void RecordWirePickup(float time, int wireIndex, int fromConnectorIndex)
    {
        var evt = new WireConnectionEvent
        {
            timestamp = time,
            eventType = WireEventType.Pickup,
            wireIndex = wireIndex,
            connectorIndex = fromConnectorIndex,
            targetConnectorIndex = -1
        };
        RecordEvent(time, evt);
    }

    public void RecordWireConnection(float time, int wireIndex, int toConnectorIndex)
    {
        var evt = new WireConnectionEvent
        {
            timestamp = time,
            eventType = WireEventType.Connect,
            wireIndex = wireIndex,
            connectorIndex = toConnectorIndex,
            targetConnectorIndex = -1
        };
        RecordEvent(time, evt);
    }

    public void RecordWireSwap(float time, int wire1Index, int wire2Index, int connector1Index, int connector2Index)
    {
        var evt = new WireConnectionEvent
        {
            timestamp = time,
            eventType = WireEventType.Swap,
            wireIndex = wire1Index,
            connectorIndex = connector1Index,
            targetConnectorIndex = connector2Index,
            targetWireIndex = wire2Index
        };
        RecordEvent(time, evt);
    }

    public void RecordLeverPull(float time)
    {
        var evt = new WireConnectionEvent
        {
            timestamp = time,
            eventType = WireEventType.LeverPull,
            wireIndex = -1,
            connectorIndex = -1
        };
        RecordEvent(time, evt);
        Debug.Log($"Recorded lever pull at {time:F2}s (door will open during playback)");
    }

    public void SetPanel(ElectricalPanel newPanel)
    {
        _panel = newPanel;
    }
}

[System.Serializable]
public struct WireConnectionEvent
{
    public float timestamp;
    public WireEventType eventType;
    public int wireIndex;
    public int connectorIndex;
    public int targetConnectorIndex;
    public int targetWireIndex;
}

public enum WireEventType
{
    Pickup,
    Connect,
    Swap,
    LeverPull
}