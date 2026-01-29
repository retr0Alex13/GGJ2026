using UnityEngine;

[RequireComponent(typeof(InputReader))]
public class PlayerLook : MonoBehaviour
{
    [SerializeField]
    private float _mouseSensitivity = 20f;

    [SerializeField]
    private float maxLookAngle = 90f;

    [SerializeField]
    private float minLookAngle = -90f;

    [SerializeField]
    private Transform _cameraRoot;

    private InputReader _input;

    private float _xRotation;

    private bool _canLook = true;

    private void Awake()
    {
        _input = GetComponent<InputReader>();
    }

    private void Update()
    {
        if (_canLook)
        {
            Look();
        }
    }

    private void Look()
    {
        float mouseX = _input.LookInput.x * _mouseSensitivity * Time.deltaTime;
        float mouseY = _input.LookInput.y * _mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, minLookAngle, maxLookAngle);

        _cameraRoot.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    public void TogglePlayerLook(bool canLook)
    {
        _canLook = canLook;
    }

    public void ToggleCameraRoot(bool isActive)
    {
        _cameraRoot.gameObject.SetActive(isActive);
    }
}
