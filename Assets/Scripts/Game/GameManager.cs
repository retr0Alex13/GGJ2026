using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Tasks")]
    [SerializeField] private ElectricalTask[] engineerTasks;
    [SerializeField] private FirefighterTask[] firefighterTasks;

    [Header("Character Objects")]
    [SerializeField] private GameObject engineerObject;
    [SerializeField] private GameObject firefighterObject;

    [Header("Replay Extinguisher")]
    [SerializeField] private GameObject extinguisherPrefab;
    [SerializeField] private Transform extinguisherReplayPosition;

    [Header("Recording Settings")]
    [SerializeField] private bool useOptimizedRecording = true;
    [SerializeField] private bool debugRecording = false;

    [Header("Replay Settings")]
    [SerializeField] private GameObject replayCameraPrefab;
    [SerializeField] private bool autoStartReplay = true;


    private bool _isInReplayMode = false;
    private ReplayCamera _replayCamera;

    private MovementRecorder currentRecorder;
    private float levelTimer = 0;
    private bool _levelEnded;
    private GameObject _replayExtinguisherInstance;
    private bool _allTasksCompleted = false;

    public CharañterType CurrentCharacter { get; private set; }
    public static GameManager Instance { get; private set; }
    public float LevelTimer => levelTimer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (PlaybackData.activePlayerIndex == -1)
        {
            _isInReplayMode = true;
            CurrentCharacter = CharañterType.Engineer;
        }
        else
        {
            CurrentCharacter = (CharañterType)PlaybackData.activePlayerIndex;
        }
    }

    private void Start()
    {
        if (_isInReplayMode)
        {
            SetupReplayMode();
        }
        else
        {
            ApplyCharacterSettings();
            InitializeTaskRecordings();
            UpdateTaskUI();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private string GetCurrentTaskText()
    {
        PlayerTask[] currentTasks = null;
        string characterName = "";

        switch (CurrentCharacter)
        {
            case CharañterType.Engineer:
                currentTasks = engineerTasks.Cast<PlayerTask>().ToArray();
                characterName = "Engineer";
                break;
            case CharañterType.Firefighter:
                currentTasks = firefighterTasks.Cast<PlayerTask>().ToArray();
                characterName = "Firefighter";
                break;
        }

        if (currentTasks == null || currentTasks.Length == 0)
            return "No tasks assigned";

        // Build task list
        string taskList = $"<b>{characterName} Tasks:</b>\n\n";

        foreach (var task in currentTasks)
        {
            string checkmark = task.IsCompleted ? "v" : "o";
            string colorStart = task.IsCompleted ? "<color=#00FF00>" : "<color=#FFFFFF>";
            string colorEnd = "</color>";
            taskList += $"{colorStart}{checkmark} {task.TaskName} ({task.CurrentCount}/{task.RequiredCount}){colorEnd}\n";
        }
        string tutorialHint = "";
        if (CurrentCharacter == CharañterType.Engineer)
        {
            tutorialHint = "Press left button to interact";
        }
        else
        {
            tutorialHint = "Hold left button to spray extinguisher";
        }
        taskList += $"\n\n{tutorialHint}";
        return taskList;
    }


    private void UpdateTaskUI()
    {
        // Don't update task UI in replay mode
        if (_isInReplayMode) return;

        if (CanvasUI.Instance == null) return;

        string taskText = GetCurrentTaskText();
        CanvasUI.Instance.UpdateTaskText(taskText);
    }

    private void SetupReplayMode()
    {
        Debug.Log("Setting up replay mode...");

        // Unlock cursor for free camera view
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hide task UI during replay
        if (CanvasUI.Instance != null)
        {
            CanvasUI.Instance.UpdateTaskText("<b>REPLAY MODE</b>\nWatching recorded gameplay...");
        }

        // Disable all player controls and set both to playback mode
        ConfigureCharacterForReplay(engineerObject, CharañterType.Engineer);
        ConfigureCharacterForReplay(firefighterObject, CharañterType.Firefighter);

        // Setup task playbacks
        InitializeTaskRecordings();

        // Create and setup replay camera
        SetupReplayCamera();
    }

    private void ConfigureCharacterForReplay(GameObject obj, CharañterType type)
    {
        if (!PlaybackData.movementRecords.ContainsKey(type))
        {
            obj.SetActive(false);
            return;
        }

        obj.SetActive(true);

        // Disable all player input components
        obj.GetComponent<PlayerMovement>().enabled = false;
        obj.GetComponent<PlayerLook>().ToggleCameraRoot(false);
        obj.GetComponent<PlayerLook>().enabled = false;
        if (obj.GetComponent<PlayerInteract>() != null)
        {
            obj.GetComponent<PlayerInteract>().enabled = false;
        }
        obj.GetComponent<CharacterController>().enabled = false;

        // Enable renderers
        PlayerMovement pm = obj.GetComponent<PlayerMovement>();
        if (pm != null && pm.Renderers != null)
        {
            foreach (SkinnedMeshRenderer renderer in pm.Renderers)
            {
                renderer.enabled = true;
            }
        }

        // Setup animator
        Animator animator = obj.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            animator.applyRootMotion = false;
        }

        // Setup playback
        var playback = obj.GetComponent<EchoPlayback>();
        if (playback == null)
            playback = obj.AddComponent<EchoPlayback>();

        playback.Initialize(PlaybackData.movementRecords[type]);
    }

    private void SetupReplayCamera()
    {
        // Create replay camera
        if (replayCameraPrefab != null)
        {
            GameObject cameraObj = Instantiate(replayCameraPrefab);
            _replayCamera = cameraObj.GetComponent<ReplayCamera>();

            if (_replayCamera != null)
            {
                Transform engineerTransform = engineerObject.transform;
                Transform firefighterTransform = firefighterObject.transform;

                _replayCamera.Initialize(engineerTransform, firefighterTransform);
                Debug.Log("Replay camera initialized successfully");
            }
            else
            {
                Debug.LogError("ReplayCamera component not found on prefab!");
            }
        }
        else
        {
            Debug.LogWarning("No replay camera prefab assigned!");
        }
    }

    private void Update()
    {
        if (_isInReplayMode)
        {
            UpdateReplayMode();
        }
        else
        {
            levelTimer += Time.deltaTime;
            PlaybackPreviousCharacters();
            PlaybackTaskEvents();
        }
    }

    private void UpdateReplayMode()
    {
        levelTimer += Time.deltaTime;

        // Update replay UI with current time
        if (CanvasUI.Instance != null)
        {
            CanvasUI.Instance.UpdateTaskText($"");
            CanvasUI.Instance.SetActiveVictoryText();
        }

        // Play back all task events
        foreach (var kvp in PlaybackData.taskEventRecords)
        {
            kvp.Value.Playback(levelTimer);
        }

        // Check if all playbacks are finished
        bool allFinished = true;

        var engineerPlayback = engineerObject.GetComponent<EchoPlayback>();
        var firefighterPlayback = firefighterObject.GetComponent<EchoPlayback>();

        if (engineerPlayback != null && !engineerPlayback.IsPlaybackFinished())
            allFinished = false;
        if (firefighterPlayback != null && !firefighterPlayback.IsPlaybackFinished())
            allFinished = false;

        if (allFinished)
        {
            Debug.Log("Replay finished!");
            if (CanvasUI.Instance != null)
            {
                CanvasUI.Instance.UpdateTaskText("<b>REPLAY COMPLETE</b>\n\nPress R to restart");
            }
        }
    }

    public void OnPlayerDied()
    {
        if (_levelEnded) return;

        _levelEnded = true;
        Debug.Log("Player died! Restarting level...");

        RestartCurrentCharacter();
    }

    private void InitializeTaskRecordings()
    {
        ElectricalPanel[] electricalPanels = FindObjectsByType<ElectricalPanel>(FindObjectsSortMode.None);
        foreach (var panel in electricalPanels)
        {
            string taskID = panel.GetTaskID();
            if (!string.IsNullOrEmpty(taskID))
            {
                var existingRecording = PlaybackData.GetTaskEventRecording(taskID);

                if (existingRecording != null && existingRecording is ElectricalPanelRecording electricRecording)
                {
                    electricRecording.SetPanel(panel);

                    if (!electricRecording.BelongsToCharacter(CurrentCharacter))
                    {
                        panel.SetPlaybackMode(true);
                    }
                    else
                    {
                        panel.SetRecording(electricRecording);
                    }
                }
                else
                {
                    var recording = new ElectricalPanelRecording(CharañterType.Engineer, panel);
                    PlaybackData.RegisterTaskEventRecording(taskID, recording);

                    if (CurrentCharacter == CharañterType.Engineer)
                    {
                        panel.SetRecording(recording);
                    }
                }
            }
        }

        Fire[] fires = FindObjectsByType<Fire>(FindObjectsSortMode.None);

        if (fires.Length > 0)
        {
            string fireTaskID = "firefighter_extinguisher_recording";
            var existingRecording = PlaybackData.GetTaskEventRecording(fireTaskID);

            FireExtinguisherRecording recording;

            if (existingRecording != null && existingRecording is FireExtinguisherRecording fireRecording)
            {
                fireRecording.ReinitializeFires(fires);
                recording = fireRecording;
            }
            else
            {
                recording = new FireExtinguisherRecording(CharañterType.Firefighter, fires);
                PlaybackData.RegisterTaskEventRecording(fireTaskID, recording);
            }

            if (CurrentCharacter == CharañterType.Firefighter)
            {
                Extinguisher extinguisher = firefighterObject.GetComponentInChildren<Extinguisher>();
                if (extinguisher != null)
                {
                    recording.SetExtinguisher(extinguisher);
                    extinguisher.SetRecording(recording);
                    extinguisher.SetPlaybackMode(false);
                }
            }
            else if (PlaybackData.movementRecords.ContainsKey(CharañterType.Firefighter))
            {
                if (extinguisherPrefab != null)
                {
                    _replayExtinguisherInstance = Instantiate(extinguisherPrefab, extinguisherReplayPosition);
                    _replayExtinguisherInstance.name = "ReplayExtinguisher";

                    Extinguisher replayExtinguisher = _replayExtinguisherInstance.GetComponent<Extinguisher>();
                    if (replayExtinguisher != null)
                    {
                        recording.SetExtinguisher(replayExtinguisher);
                        replayExtinguisher.SetPlaybackMode(true);
                    }
                }
            }
        }

        Lever[] levers = FindObjectsByType<Lever>(FindObjectsSortMode.None);
        if (levers.Length > 0)
        {
            string leverTaskID = "lever_door_recording";
            var existingRecording = PlaybackData.GetTaskEventRecording(leverTaskID);

            LeverDoorRecording recording;

            if (existingRecording != null && existingRecording is LeverDoorRecording leverRecording)
            {
                foreach (var lever in levers)
                {
                    leverRecording.RegisterLever(lever.GetLeverIndex(), lever);
                    lever.SetRecording(leverRecording);
                    lever.SetPlaybackMode(!leverRecording.BelongsToCharacter(CurrentCharacter));
                }
                recording = leverRecording;
            }
            else
            {
                recording = new LeverDoorRecording(CharañterType.Engineer);
                PlaybackData.RegisterTaskEventRecording(leverTaskID, recording);

                foreach (var lever in levers)
                {
                    recording.RegisterLever(lever.GetLeverIndex(), lever);
                    lever.SetRecording(recording);
                    lever.SetPlaybackMode(CurrentCharacter != CharañterType.Engineer);
                }
            }

            Debug.Log($"Initialized lever/door recording with {levers.Length} levers");
        }
    }

    private void PlaybackPreviousCharacters()
    {
        if (CurrentCharacter > CharañterType.Engineer && PlaybackData.movementRecords.ContainsKey(CharañterType.Engineer))
        {
        }

        if (CurrentCharacter > CharañterType.Firefighter && PlaybackData.movementRecords.ContainsKey(CharañterType.Firefighter))
        {
        }
    }

    private void PlaybackTaskEvents()
    {
        foreach (var kvp in PlaybackData.taskEventRecords)
        {
            if (ShouldPlaybackTaskEvents(kvp.Value))
            {
                kvp.Value.Playback(levelTimer);
            }
        }
    }

    private bool ShouldPlaybackTaskEvents(ITaskEventRecording recording)
    {
        if (recording.BelongsToCharacter(CharañterType.Engineer) && CurrentCharacter > CharañterType.Engineer)
            return true;

        if (recording.BelongsToCharacter(CharañterType.Firefighter) && CurrentCharacter > CharañterType.Firefighter)
            return true;

        return false;
    }

    private void ApplyCharacterSettings()
    {
        ConfigureCharacter(engineerObject, CharañterType.Engineer);
        ConfigureCharacter(firefighterObject, CharañterType.Firefighter);
    }

    private void ConfigureCharacter(GameObject obj, CharañterType type)
    {
        bool isCurrent = (CurrentCharacter == type);
        bool hasRecord = PlaybackData.movementRecords.ContainsKey(type);

        obj.SetActive(isCurrent || hasRecord);

        if (isCurrent)
        {
            obj.GetComponent<PlayerMovement>().enabled = true;
            obj.GetComponent<PlayerLook>().ToggleCameraRoot(true);
            obj.GetComponent<PlayerLook>().enabled = true;
            if (obj.GetComponent<PlayerInteract>() != null)
            {
                obj.GetComponent<PlayerInteract>().enabled = true;
            }
            obj.GetComponent<CharacterController>().enabled = true;

            if (useOptimizedRecording)
            {
                currentRecorder = obj.AddComponent<MovementRecorder>();
            }
        }
        else if (hasRecord)
        {
            obj.GetComponent<PlayerMovement>().enabled = false;
            obj.GetComponent<PlayerLook>().ToggleCameraRoot(false);
            obj.GetComponent<PlayerLook>().enabled = false;
            if (obj.GetComponent<PlayerInteract>() != null)
            {
                obj.GetComponent<PlayerInteract>().enabled = false;
            }
            obj.GetComponent<CharacterController>().enabled = false;

            PlayerMovement pm = obj.GetComponent<PlayerMovement>();
            if (pm != null && pm.Renderers != null)
            {
                foreach (SkinnedMeshRenderer renderer in pm.Renderers)
                {
                    renderer.enabled = true;
                }
            }

            Animator animator = obj.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
                animator.applyRootMotion = false;
            }

            var playback = obj.GetComponent<EchoPlayback>();
            if (playback == null)
                playback = obj.AddComponent<EchoPlayback>();

            playback.Initialize(PlaybackData.movementRecords[type]);
        }
    }

    private void SwitchToNextCharacter()
    {
        if (useOptimizedRecording && currentRecorder != null)
        {
            List<CharacterFrame> recording = currentRecorder.GetRecording();
            PlaybackData.SaveMovementRecord(CurrentCharacter, recording);
        }

        int nextIndex = (int)CurrentCharacter + 1;
        if (nextIndex < System.Enum.GetValues(typeof(CharañterType)).Length)
        {
            PlaybackData.activePlayerIndex = nextIndex;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Debug.Log("All characters completed their tasks!");
        }
    }

    public void CheckProgress()
    {
        if (_allTasksCompleted) return;

        // Don't check progress in replay mode
        if (!_isInReplayMode)
        {
            UpdateTaskUI(); // Only update UI if not in replay mode
        }

        bool allTasksDone = false;

        switch (CurrentCharacter)
        {
            case CharañterType.Engineer:
                allTasksDone = engineerTasks.All(t => t.IsCompleted);
                break;
            case CharañterType.Firefighter:
                allTasksDone = firefighterTasks.All(t => t.IsCompleted);
                int totalCharacters = System.Enum.GetValues(typeof(CharañterType)).Length;
                if ((int)CurrentCharacter == totalCharacters - 1)
                {
                    bool allPreviousTasksDone = engineerTasks.All(t => t.IsCompleted) &&
                                               firefighterTasks.All(t => t.IsCompleted);

                    if (allPreviousTasksDone)
                    {
                        _allTasksCompleted = true;
                        OnAllTasksCompleted();
                        return;
                    }
                }
                break;
        }

        if (allTasksDone && !_allTasksCompleted)
        {
            Debug.Log($"{CurrentCharacter} completed all tasks!");
            Invoke(nameof(SwitchToNextCharacter), 2f);
        }
    }

    private void OnAllTasksCompleted()
    {
        Debug.Log("All tasks completed! Starting replay mode...");
        _allTasksCompleted = true;

        if (autoStartReplay)
        {
            Invoke(nameof(StartReplayMode), 2f);
        }
    }

    private void StartReplayMode()
    {
        _isInReplayMode = true;

        // Save current character's recording if not already saved
        if (useOptimizedRecording && currentRecorder != null)
        {
            List<CharacterFrame> recording = currentRecorder.GetRecording();
            PlaybackData.SaveMovementRecord(CurrentCharacter, recording);
        }

        // Set a special replay index to indicate replay mode
        PlaybackData.activePlayerIndex = -1; // Special value for replay mode

        // Reload the scene to reset everything
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private GameObject GetCurrentCharacterObject()
    {
        switch (CurrentCharacter)
        {
            case CharañterType.Engineer:
                return engineerObject;
            case CharañterType.Firefighter:
                return firefighterObject;
            default:
                return null;
        }
    }

    private void EndGame()
    {
        Debug.Log("Game Complete! Returning to main menu or restarting...");
        // You can add:
        // - Show victory screen UI
        // - Load main menu scene
        // - Restart the game
        // - Show statistics

        // For now, just restart the entire game
        RestartGame();
    }

    public PlayerTask GetTask(string id)
    {
        return engineerTasks.Cast<PlayerTask>()
            .Concat(firefighterTasks)
            .FirstOrDefault(t => t.TaskID == id);
    }

    public void RestartCurrentCharacter()
    {
        _levelEnded = false;
        _allTasksCompleted = false;
        PlaybackData.WipeCurrentCharacter();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGame()
    {
        _allTasksCompleted = false;
        PlaybackData.WipeAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (_replayExtinguisherInstance != null)
        {
            Destroy(_replayExtinguisherInstance);
        }
    }
}

public enum CharañterType
{
    Engineer,
    Firefighter,
}