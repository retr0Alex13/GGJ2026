using System;
using System.Linq;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;
using Random = UnityEngine.Random;

public class ElectricalPanel : MonoBehaviour, IInteractable
{
    public bool IsInteractable { get; private set; } = true;

    [SerializeField]
    private string _taskID;

    [SerializeField]
    private Connector[] _connectors;

    [SerializeField]
    private Wire[] _wires;

    [SerializeField]
    private GameObject[] _lights;

    [SerializeField]
    private Lever _lever;

    [SerializeField]
    private Transform _cameraPositionForTask;

    private Vector3 _cameraSavedPosition;
    private Quaternion _cameraSavedRotation;

    private PlayerLook _playerLook;
    private PlayerMovement _playerMove;

    private PlayerTask _assignedTask;

    private Wire _currentWire;
    private Connector _lastConnector;

    private bool _isInteracting;

    private void Start()
    {
        _lever.OnLeverPulled += OnLeverPressed;

        _assignedTask = GameManager.Instance.GetTask(_taskID);

        WireColor[] allColors = (WireColor[])Enum.GetValues(typeof(WireColor));
        WireColor[] shuffledColors = allColors.OrderBy(x => Random.value).ToArray();

        for (int i = 0; i < shuffledColors.Length; i++)
        {
            _connectors[i].SetConnectorColor(shuffledColors[i]);
        }

        WireColor[] wireColors;
        do
        {
            wireColors = allColors.OrderBy(x => Random.value).ToArray();
        }
        while (Enumerable.Range(0, wireColors.Length).Any(i => wireColors[i] == _connectors[i].ConnectorColor));

        for (int i = 0; i < wireColors.Length; i++)
        {
            _wires[i].SetWireColor(wireColors[i]);
        }
    }

    public void OnLeverPressed()
    {
        foreach (Connector connector in _connectors)
        {
            Wire connectedWire = connector.GetWire();
            if (connectedWire == null) return;
            if (connectedWire.WireColor != connector.ConnectorColor) return;
        }

        _assignedTask.IncrementProgress(1);
        IsInteractable = false;
        _lever.IsInteractable = false;
        foreach (GameObject light in _lights)
        {
            light.gameObject.SetActive(true);
        }

    }

    private void Update()
    {
        if (_isInteracting)
        {
            if (Keyboard.current.escapeKey.wasReleasedThisFrame)
            {
                _playerLook.TogglePlayerLook(true);
                _playerMove.ToggleMovement(true);

                Camera.main.transform.position = _cameraSavedPosition;
                Camera.main.transform.rotation = _cameraSavedRotation;

                if (_currentWire != null)
                {
                    ReturnWireToConnector();
                }

                _isInteracting = false;
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hitInfo))
                {
                    if (hitInfo.collider.gameObject.TryGetComponent(out Connector connector))
                    {
                        Wire wireInConnector = connector.GetWire();

                        if (_currentWire == null && wireInConnector != null)
                        {
                            _currentWire = wireInConnector;
                            _lastConnector = connector;
                            connector.ReleaseWire();

                            _currentWire.SetIsBeingHeld(true);
                            _currentWire.ToggleCabelPhysics(false);
                            Debug.Log("Took wire from connector");
                        }
                        else if (_currentWire != null && wireInConnector == null)
                        {
                            _currentWire.SetIsBeingHeld(false);
                            connector.ConnectWire(_currentWire);
                            _lastConnector = null;
                            _currentWire = null;
                            Debug.Log("Plugged wire into empty connector");
                        }
                        else if (_currentWire != null && wireInConnector != null)
                        {
                            Wire tempWire = wireInConnector;

                            connector.ReleaseWire();

                            _currentWire.SetIsBeingHeld(false);
                            connector.ConnectWire(_currentWire);

                            _lastConnector = connector;
                            _currentWire = tempWire;
                            _currentWire.SetIsBeingHeld(true);
                            _currentWire.ToggleCabelPhysics(false);

                            Debug.Log("Swapped wires!");
                        }
                    }
                }
            }

            if (_currentWire != null)
            {
                Vector3 mousePosition = Mouse.current.position.ReadValue();
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
                    new Vector3(mousePosition.x, mousePosition.y, Camera.main.WorldToScreenPoint(_currentWire.transform.position).z)
                );
                worldPosition.z = _currentWire.transform.position.z;
                _currentWire.transform.position = worldPosition;
                _currentWire.SetIsBeingHeld(false);
            }
        }
    }

    private void OnEnable()
    {
        Connector.OnWireConnected += HandleWireConnected;
    }
    private void OnDisable()
    {
        Connector.OnWireConnected -= HandleWireConnected;
        _lever.OnLeverPulled -= OnLeverPressed;
    }

    private void HandleWireConnected(Wire connectedWire)
    {
        if (_currentWire == connectedWire)
        {
            _currentWire = null;
            Debug.Log("Current wire locked into connector.");
        }
    }

    private void ReturnWireToConnector()
    {
        _currentWire.SetIsBeingHeld(false);

        if (_lastConnector != null && _lastConnector.GetWire() == null)
        {
            _lastConnector.ConnectWire(_currentWire);
            Debug.Log("Wire returned to last connector.");
        }
        else
        {
            Connector freeConnector = _connectors.FirstOrDefault(c => c.GetWire() == null);

            if (freeConnector != null)
            {
                freeConnector.ConnectWire(_currentWire);
                Debug.Log("Last connector was busy, found a new free slot.");
            }
            else
            {
                _currentWire.ToggleCabelPhysics(true);
                Debug.LogWarning("No free connectors found! Wire dropped.");
            }
        }

        _currentWire = null;
        _lastConnector = null;
    }

    public void Interact(GameObject player)
    {
        if (!IsInteractable) return;
        if (_isInteracting) return;
        if (!player.TryGetComponent(out PlayerMovement playerMovement)) return;
        if (!player.TryGetComponent(out PlayerLook playerLook)) return;

        _playerMove = playerMovement;
        _playerLook = playerLook;

        _cameraSavedPosition = Camera.main.transform.position;
        _cameraSavedRotation = Camera.main.transform.rotation;

        _playerLook.TogglePlayerLook(false);
        _playerMove.ToggleMovement(false);

        Camera.main.transform.position = _cameraPositionForTask.position;
        Camera.main.transform.rotation = _cameraPositionForTask.rotation;

        _isInteracting = true;
    }
}