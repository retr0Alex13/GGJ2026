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
    [SerializeField] private GameObject doctorObject;

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
    }

    private void ApplyCharacterSettings()
    {
        engineerObject.SetActive(CurrentCharacter == CharaterType.Engineer);
        firefighterObject.SetActive(CurrentCharacter == CharaterType.Firefighter);
        doctorObject.SetActive(CurrentCharacter == CharaterType.Doctor);
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
            SwitchToNextCharacter();
        }
    }

    private void SwitchToNextCharacter()
    {
        int nextIndex = (int)CurrentCharacter + 1;

        if (nextIndex < System.Enum.GetValues(typeof(CharaterType)).Length)
        {
            PlaybackData.activePlayerIndex = nextIndex;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Debug.Log("All characters finished their mission!");
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