using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private ElectricalTask[] engineerTasks;
    [SerializeField]
    private DoctorTask[] doctorTasks;
    [SerializeField]
    private FirefighterTask[] firefighterTasks;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
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
