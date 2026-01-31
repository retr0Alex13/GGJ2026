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

    [Header("Recording Settings")]
    [SerializeField] private bool useOptimizedRecording = true;
    [SerializeField] private bool debugRecording = false;

    private MovementRecorder currentRecorder;
    private float levelTimer = 0;

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
            else
            {
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

            foreach (SkinnedMeshRenderer renderer in obj.GetComponent<PlayerMovement>().Renderers)
            {
                renderer.enabled = true;
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

            if (debugRecording)
            {
                currentRecorder.LogRecordingStats();
            }
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
            Debug.Log($"{CurrentCharacter} completed all tasks!");
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
}


public enum CharañterType
{
    Engineer,
    Firefighter,
    Doctor,
}