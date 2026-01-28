using System;
using UnityEngine;

public class Connector : MonoBehaviour
{
    public WireColor ConnectorColor => _connectorColor;
    public Wire WireSlot => _wireSlot;

    [SerializeField]
    private Transform _connectionPoint;

    private WireColor _connectorColor;
    private Wire _wireSlot;

    private Material _material;

    public static event Action<Wire> OnWireConnected;

    private void Awake()
    {
        _material = GetComponent<MeshRenderer>().material;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_wireSlot != null) return;

        if (other.gameObject.TryGetComponent(out Wire wire))
        {
            if (!wire.IsBeingHeld)
            {
                ConnectWire(wire);
            }
        }
    }

    public void ReleaseWire()
    {
        if (_wireSlot != null)
        {
            _wireSlot = null;
        }
    }

    public Wire GetWire()
    {
        return _wireSlot;
    }

    public void ConnectWire(Wire wire)
    {
        wire.transform.position = _connectionPoint.position;
        wire.ToggleCabelPhysics(false);

        _wireSlot = wire;
        OnWireConnected?.Invoke(wire);
    }

    public void SetConnectorColor(WireColor color)
    {
        _connectorColor = color;

        switch (color)
        {
            case WireColor.Red:
                _material.color = Color.red;
                break;
            case WireColor.Blue:
                _material.color = Color.blue;
                break;
            case WireColor.Green:
                _material.color = Color.green;
                break;
            case WireColor.Yellow:
                _material.color = Color.yellow;
                break;
        }
    }
}