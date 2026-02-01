using TMPro;
using UnityEngine;

public class CanvasUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _taskText;
    [SerializeField] private TextMeshProUGUI _victoryText;

    public static CanvasUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    public void UpdateTaskText(string newText)
    {
        _taskText.text = newText;
    }

    public void SetActiveVictoryText()
    {
        _victoryText.gameObject.SetActive(true);
    }
}
