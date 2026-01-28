using System;
using System.Linq;
using UnityEngine;

public class Wire : MonoBehaviour
{
    public WireColor WireColor => _wireColor;
    public bool IsBeingHeld => _isBeingHeld;
    private bool _isBeingHeld;

    private WireColor _wireColor;

    private Rigidbody _rigidbody;
    private Material _wireMaterial;
    private Material _endWireMaterial;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _endWireMaterial = GetComponent<MeshRenderer>().material;
        _wireMaterial = transform.parent.GetComponent<MeshRenderer>().material;
    }

    public void ToggleCabelPhysics(bool enable)
    {
        _rigidbody.useGravity = enable;
        _rigidbody.isKinematic = !enable;
    }

    public void SetIsBeingHeld(bool isBeingHeld)
    {
        _isBeingHeld = isBeingHeld;
    }

    public void SetWireColor(WireColor wireColor)
    {
        _wireColor = wireColor;

        switch (wireColor)
        {
            case WireColor.Red:
                _wireMaterial.color = Color.red;
                _endWireMaterial.color = Color.red;
                break;
            case WireColor.Blue:
                _wireMaterial.color = Color.blue;
                _endWireMaterial.color = Color.blue;
                break;
            case WireColor.Green:
                _wireMaterial.color = Color.green;
                _endWireMaterial.color = Color.green;
                break;
            case WireColor.Yellow:
                _wireMaterial.color = Color.yellow;
                _endWireMaterial.color = Color.yellow;
                break;
        }
    }
}

public enum WireColor
{
    Red,
    Blue,
    Green,
    Yellow
}
