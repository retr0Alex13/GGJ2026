using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ElectricalPanel : MonoBehaviour, IInteractable
{
    public bool IsInteractable { get; private set; } = true;

    [SerializeField] private string _taskID;
    [SerializeField] private Connector[] _connectors;
    [SerializeField] private Wire[] _wires;
    [SerializeField] private GameObject[] _lights;
    [SerializeField] private Door _doorToOpen;
    [SerializeField] private Lever _lever;
    [SerializeField] private Transform _cameraPositionForTask;

    private Vector3 _cameraSavedPosition;
    private Quaternion _cameraSavedRotation;
    private PlayerLook _playerLook;
    private PlayerMovement _playerMove;
    private PlayerTask _assignedTask;
    private Wire _currentWire;
    private Connector _lastConnector;
    private bool _isInteracting;

    private ElectricalPanelRecording _recording;
    private AudioSource _audioSource;
    private bool _isPlaybackMode = false;

    private void Start()
    {
        _lever.OnLeverPulled += OnLeverPressed;
        _assignedTask = GameManager.Instance.GetTask(_taskID);
        _audioSource = GetComponent<AudioSource>();

        InitializeWireColors();
    }

    public void SetRecording(ElectricalPanelRecording recording)
    {
        _recording = recording;
    }

    public string GetTaskID()
    {
        return _taskID;
    }

    public void SetPlaybackMode(bool enabled)
    {
        _isPlaybackMode = enabled;
        IsInteractable = !enabled;
    }

    private void InitializeWireColors()
    {
        int panelSeed = PlaybackData.MasterRandomSeed + _taskID.GetHashCode();
        System.Random rng = new System.Random(panelSeed);

        WireColor[] allColors = (WireColor[])Enum.GetValues(typeof(WireColor));
        WireColor[] shuffledColors = allColors.OrderBy(x => rng.Next()).ToArray();

        for (int i = 0; i < shuffledColors.Length; i++)
        {
            _connectors[i].SetConnectorColor(shuffledColors[i]);
        }

        WireColor[] wireColors;
        do
        {
            wireColors = allColors.OrderBy(x => rng.Next()).ToArray();
        }
        while (Enumerable.Range(0, wireColors.Length).Any(i => wireColors[i] == _connectors[i].ConnectorColor));

        for (int i = 0; i < wireColors.Length; i++)
        {
            _wires[i].SetWireColor(wireColors[i]);
        }

        Debug.Log($"Panel {_taskID} initialized with seed {panelSeed}");
    }

    public void OnLeverPressed()
    {
        if (!_isPlaybackMode)
        {
            foreach (Connector connector in _connectors)
            {
                Wire connectedWire = connector.GetWire();
                if (connectedWire == null) return;
                if (connectedWire.WireColor != connector.ConnectorColor) return;
            }

            if (_recording != null)
            {
                _recording.RecordLeverPull(GameManager.Instance.LevelTimer);
            }
        }

        _assignedTask?.IncrementProgress(1);
        IsInteractable = false;
        _lever.IsInteractable = false;

        Debug.Log("Electrical panel task completed!");
        _audioSource.Play();
        _doorToOpen.Open();
    }


    private void Update()
    {
        if (_isInteracting && !_isPlaybackMode)
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        if (Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            ExitInteraction();
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                if (hitInfo.collider.gameObject.TryGetComponent(out Connector connector))
                {
                    HandleConnectorClick(connector);
                }
            }
        }

        if (_currentWire != null)
        {
            UpdateWirePosition();
        }
    }

    private void HandleConnectorClick(Connector connector)
    {
        Wire wireInConnector = connector.GetWire();
        int connectorIndex = Array.IndexOf(_connectors, connector);
        float timestamp = GameManager.Instance.LevelTimer;

        if (_currentWire == null && wireInConnector != null)
        {
            int wireIndex = Array.IndexOf(_wires, wireInConnector);

            _currentWire = wireInConnector;
            _lastConnector = connector;
            connector.ReleaseWire();
            _currentWire.SetIsBeingHeld(true);
            _currentWire.ToggleCabelPhysics(false);

            if (_recording != null)
            {
                _recording.RecordWirePickup(timestamp, wireIndex, connectorIndex);
            }

            Debug.Log("Took wire from connector");
        }
        else if (_currentWire != null && wireInConnector == null)
        {
            int wireIndex = Array.IndexOf(_wires, _currentWire);

            _currentWire.SetIsBeingHeld(false);
            connector.ConnectWire(_currentWire);

            if (_recording != null)
            {
                _recording.RecordWireConnection(timestamp, wireIndex, connectorIndex);
            }

            _lastConnector = null;
            _currentWire = null;
            Debug.Log("Plugged wire into empty connector");
        }
        else if (_currentWire != null && wireInConnector != null)
        {
            int wire1Index = Array.IndexOf(_wires, _currentWire);
            int wire2Index = Array.IndexOf(_wires, wireInConnector);
            int lastConnectorIndex = _lastConnector != null ? Array.IndexOf(_connectors, _lastConnector) : -1;

            Wire tempWire = wireInConnector;
            connector.ReleaseWire();
            _currentWire.SetIsBeingHeld(false);
            connector.ConnectWire(_currentWire);

            if (_recording != null)
            {
                _recording.RecordWireSwap(timestamp, wire1Index, wire2Index, lastConnectorIndex, connectorIndex);
            }

            _lastConnector = connector;
            _currentWire = tempWire;
            _currentWire.SetIsBeingHeld(true);
            _currentWire.ToggleCabelPhysics(false);

            Debug.Log("Swapped wires!");
        }
    }

    private void UpdateWirePosition()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(mousePosition.x, mousePosition.y,
                       Camera.main.WorldToScreenPoint(_currentWire.transform.position).z)
        );
        worldPosition.z = _currentWire.transform.position.z;
        _currentWire.transform.position = worldPosition;
    }

    private void ExitInteraction()
    {
        _playerLook.TogglePlayerLook(true);
        _playerMove.ToggleMovement(true);
        Cursor.lockState = CursorLockMode.Locked;

        Camera.main.transform.position = _cameraSavedPosition;
        Camera.main.transform.rotation = _cameraSavedRotation;

        if (_currentWire != null)
        {
            ReturnWireToConnector();
        }

        _isInteracting = false;
    }

    private void ReturnWireToConnector()
    {
        _currentWire.SetIsBeingHeld(false);

        if (_lastConnector != null && _lastConnector.GetWire() == null)
        {
            _lastConnector.ConnectWire(_currentWire);
        }
        else
        {
            Connector freeConnector = _connectors.FirstOrDefault(c => c.GetWire() == null);
            if (freeConnector != null)
            {
                freeConnector.ConnectWire(_currentWire);
            }
            else
            {
                _currentWire.ToggleCabelPhysics(true);
            }
        }

        _currentWire = null;
        _lastConnector = null;
    }

    public void Interact(GameObject player)
    {
        if (!IsInteractable || _isInteracting || _isPlaybackMode) return;
        if (!player.TryGetComponent(out PlayerMovement playerMovement)) return;
        if (!player.TryGetComponent(out PlayerLook playerLook)) return;

        Cursor.lockState = CursorLockMode.Confined;

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

    public void ReplayWireConnection(WireConnectionEvent evt)
    {
        if (!_isPlaybackMode) return;

        switch (evt.eventType)
        {
            case WireEventType.Pickup:
                if (evt.wireIndex >= 0 && evt.wireIndex < _wires.Length &&
                    evt.connectorIndex >= 0 && evt.connectorIndex < _connectors.Length)
                {
                    _connectors[evt.connectorIndex].ReleaseWire();
                    Debug.Log($"[Playback] Released wire {evt.wireIndex} from connector {evt.connectorIndex}");
                }
                break;

            case WireEventType.Connect:
                if (evt.wireIndex >= 0 && evt.wireIndex < _wires.Length &&
                    evt.connectorIndex >= 0 && evt.connectorIndex < _connectors.Length)
                {
                    Wire wire = _wires[evt.wireIndex];
                    Connector connector = _connectors[evt.connectorIndex];
                    foreach (var conn in _connectors)
                    {
                        if (conn.GetWire() == wire)
                        {
                            conn.ReleaseWire();
                        }
                    }

                    connector.ConnectWire(wire);
                    Debug.Log($"[Playback] Connected wire {evt.wireIndex} to connector {evt.connectorIndex}");
                }
                break;

            case WireEventType.Swap:
                if (evt.wireIndex >= 0 && evt.wireIndex < _wires.Length &&
                    evt.targetWireIndex >= 0 && evt.targetWireIndex < _wires.Length &&
                    evt.connectorIndex >= 0 && evt.connectorIndex < _connectors.Length &&
                    evt.targetConnectorIndex >= 0 && evt.targetConnectorIndex < _connectors.Length)
                {
                    Wire wire1 = _wires[evt.wireIndex];
                    Wire wire2 = _wires[evt.targetWireIndex];
                    Connector connector1 = _connectors[evt.connectorIndex];
                    Connector connector2 = _connectors[evt.targetConnectorIndex];

                    connector1.ReleaseWire();
                    connector2.ReleaseWire();

                    connector1.ConnectWire(wire2);
                    connector2.ConnectWire(wire1);

                    Debug.Log($"[Playback] Swapped wires {evt.wireIndex} and {evt.targetWireIndex}");
                }
                break;

            case WireEventType.LeverPull:
                if (_lever != null)
                {
                    _lever.PullLeverProgrammatically();
                    Debug.Log("[Playback] Lever pulled");
                }
                break;
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
        }

        bool allCorrect = _connectors.All(connector =>
        {
            var wire = connector.GetWire();
            return wire != null && wire.WireColor == connector.ConnectorColor;
        });

        if (allCorrect && _isInteracting)
        {
            ExitInteraction();
            IsInteractable = false;
        }
    }
}