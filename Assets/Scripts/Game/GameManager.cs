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

    private MovementRecorder currentRecorder;
    private float levelTimer = 0;
    private GameObject _replayExtinguisherInstance;

    public CharañterType CurrentCharacter { get; private set; }
    public static GameManager Instance { get; private set; }
    public float LevelTimer => levelTimer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CurrentCharacter = (CharañterType)PlaybackData.activePlayerIndex;
    }

    private void Start()
    {
        ApplyCharacterSettings();
        InitializeTaskRecordings();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        levelTimer += Time.deltaTime;

        PlaybackPreviousCharacters();
        PlaybackTaskEvents();
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
    }

    public void CheckProgress()
    {
        bool allTasksDone = false;

        switch (CurrentCharacter)
        {
            case CharañterType.Engineer:
                allTasksDone = engineerTasks.All(t => t.IsCompleted);
                break;
            case CharañterType.Firefighter:
                allTasksDone = firefighterTasks.All(t => t.IsCompleted);
                break;
        }

        if (allTasksDone)
        {
            Invoke(nameof(SwitchToNextCharacter), 2f);
        }
    }

    public PlayerTask GetTask(string id)
    {
        return engineerTasks.Cast<PlayerTask>()
            .Concat(firefighterTasks)
            .FirstOrDefault(t => t.TaskID == id);
    }

    public void RestartCurrentCharacter()
    {
        PlaybackData.WipeCurrentCharacter();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGame()
    {
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
    Doctor,
}