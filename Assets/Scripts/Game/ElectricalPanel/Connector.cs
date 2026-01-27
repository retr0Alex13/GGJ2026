using System;
using UnityEngine;

public class Connector : MonoBehaviour
{
    [SerializeField]
    private Transform _connectionPoint;

    private Wire _wireSlot;

    public static event Action<Wire> OnWireConnected;

    private void OnTriggerEnter(Collider other)
    {
        if (_wireSlot != null) return;

        if (other.gameObject.TryGetComponent(out Wire wire))
        {
            wire.transform.position = _connectionPoint.position;
            wire.ToggleCabelPhysics(false);

            _wireSlot = wire;
            OnWireConnected?.Invoke(wire);
        }
    }

    public void ReleaseWire()
    {
        _wireSlot = null;
    }

    public Wire GetWire()
    {
        return _wireSlot;
    }

    public void ConnectWire(Wire wire)
    {

    }
}
