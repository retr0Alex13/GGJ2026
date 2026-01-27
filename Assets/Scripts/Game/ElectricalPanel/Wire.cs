using UnityEngine;

public class Wire : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void ToggleCabelPhysics(bool enable)
    {
        _rigidbody.useGravity = enable;
        _rigidbody.isKinematic = !enable;
    }
}
