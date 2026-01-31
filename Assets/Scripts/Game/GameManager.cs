using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ElectricalTask[] engineerTasks;
    [SerializeField] private DoctorTask[] doctorTasks;
    [SerializeField] private FirefighterTask[] firefighterTasks;

    [Header("Character Objects")]
    [SerializeField] private GameObject engineerObject;
    [SerializeField] private GameObject firefighterObject;

    private List<CharacterFrame> currentRecording = new();
    private float levelTimer = 0;

    public CharaterType CurrentCharacter { get; private set; }
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CurrentCharacter = (CharaterType)PlaybackData.activePlayerIndex;
    }

    private void Start()
    {
        ApplyCharacterSettings();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        levelTimer += Time.deltaTime;
        GameObject activeObj = GetActiveCharacterObject();
        if (activeObj != null)
        {
            currentRecording.Add(new CharacterFrame(
                activeObj.transform.position,
                activeObj.transform.rotation,
                levelTimer
            ));
        }
    }

    private void ApplyCharacterSettings()
    {
        ConfigureCharacter(engineerObject, CharaterType.Engineer);
        ConfigureCharacter(firefighterObject, CharaterType.Firefighter);
    }

    private void ConfigureCharacter(GameObject obj, CharaterType type)
    {
        bool isCurrent = (CurrentCharacter == type);
        bool hasRecord = PlaybackData.Records.ContainsKey(type);

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

            var playback = obj.GetComponent<EchoPlayback>() ?? obj.AddComponent<EchoPlayback>();
            playback.Initialize(PlaybackData.Records[type]);
        }
    }

    private void SwitchToNextCharacter()
    {
        PlaybackData.SaveRecord(CurrentCharacter, currentRecording);

        int nextIndex = (int)CurrentCharacter + 1;
        if (nextIndex < System.Enum.GetValues(typeof(CharaterType)).Length)
        {
            PlaybackData.activePlayerIndex = nextIndex;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private GameObject GetActiveCharacterObject()
    {
        return CurrentCharacter switch
        {
            CharaterType.Engineer => engineerObject,
            CharaterType.Firefighter => firefighterObject,
            _ => null
        };
    }

    public void CheckProgress()
    {
        bool allTasksDone = false;

        switch (CurrentCharacter)
        {
            case CharaterType.Engineer:
                allTasksDone = engineerTasks.All(t => t.IsCompleted);
                break;
            case CharaterType.Firefighter:
                allTasksDone = firefighterTasks.All(t => t.IsCompleted);
                break;
            case CharaterType.Doctor:
                allTasksDone = doctorTasks.All(t => t.IsCompleted);
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
            .Concat(doctorTasks)
            .Concat(firefighterTasks)
            .FirstOrDefault(t => t.TaskID == id);
    }
}

public enum CharaterType
{
    Engineer,
    Firefighter,
    Doctor,
}