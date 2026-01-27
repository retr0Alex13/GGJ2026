using UnityEngine;
using UnityEngine.InputSystem;

public class ElectricalPanel : MonoBehaviour
{
    [SerializeField]
    private Transform _cameraPositionForTask;

    private Vector3 _cameraSavedPosition;
    private Quaternion _cameraSavedRotation;

    private PlayerLook _playerLook;
    private PlayerMovement _playerMove;

    private Wire _currentWireEnd;

    private bool _isInteracting;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerLook playerLook))
        {
            _playerLook = playerLook;
            _playerMove = other.gameObject.GetComponent<PlayerMovement>();

            _cameraSavedPosition = Camera.main.transform.position;
            _cameraSavedRotation = Camera.main.transform.rotation;

            _playerLook.TogglePlayerLook(false);
            _playerMove.ToggleMovement(false);

            Camera.main.transform.position = _cameraPositionForTask.position;
            Camera.main.transform.rotation = _cameraPositionForTask.rotation;

            _isInteracting = true;
        }
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            _playerLook.TogglePlayerLook(true);
            _playerMove.ToggleMovement(true);

            Camera.main.transform.position = _cameraSavedPosition;
            Camera.main.transform.rotation = _cameraSavedRotation;

            _isInteracting = false;
        }

        if (_isInteracting)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hitInfo))
                {
                    if (hitInfo.collider.gameObject.TryGetComponent(out Connector connector) && connector.GetWire() != null)
                    {
                        _currentWireEnd = connector.GetWire();
                        connector.ReleaseWire();
                    }
                }
            }
            if (_currentWireEnd != null)
            {
                Vector3 mousePosition = Mouse.current.position.ReadValue();
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
                    new Vector3(mousePosition.x, mousePosition.y, Camera.main.WorldToScreenPoint(_currentWireEnd.transform.position).z)
                );
                worldPosition.z = _currentWireEnd.transform.position.z;
                _currentWireEnd.transform.position = worldPosition;
            }
        }
        else
        {
            if (_currentWireEnd == null) return;

            _currentWireEnd.ToggleCabelPhysics(true);
            _currentWireEnd = null;
            Debug.Log("Dropped the wire.");
        }
    }

    private void OnEnable() => Connector.OnWireConnected += HandleWireConnected;
    private void OnDisable() => Connector.OnWireConnected -= HandleWireConnected;

    private void HandleWireConnected(Wire connectedWire)
    {
        if (_currentWireEnd == connectedWire)
        {
            _currentWireEnd = null;
            Debug.Log("Current wire locked into connector.");
        }
    }
}