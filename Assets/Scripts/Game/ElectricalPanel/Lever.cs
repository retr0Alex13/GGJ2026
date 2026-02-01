using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Lever : MonoBehaviour, IInteractable
{
    [SerializeField] private int leverIndex = -1;

    private bool _isInteractable = false;
    public bool IsInteractable
    {
        get => _isInteractable;
        set
        {
            if (_isInteractable == value) return;
            _isInteractable = value;
            Debug.Log($"[Lever] IsInteractable set to {_isInteractable} on {gameObject.name}\nStackTrace:\n{Environment.StackTrace}");
        }
    }
    private Animator _animator;
    private AudioSource _audioSource;
    private LeverDoorRecording _recording;
    private bool _isPlaybackMode = false;
    private static int _nextLeverIndex = 0;

    public event Action OnLeverPulled;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        if (leverIndex < 0)
        {
            leverIndex = _nextLeverIndex++;
        }
        else if (leverIndex >= _nextLeverIndex)
        {
            _nextLeverIndex = leverIndex + 1;
        }

        Debug.Log($"Lever {gameObject.name} initialized with index {leverIndex}");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetLeverIndexCounter()
    {
        _nextLeverIndex = 0;
    }

    public int GetLeverIndex()
    {
        return leverIndex;
    }

    public void SetRecording(LeverDoorRecording recording)
    {
        _recording = recording;
        _recording.RegisterLever(leverIndex, this);
    }

    public void SetPlaybackMode(bool enabled)
    {
        _isPlaybackMode = enabled;
        //IsInteractable = !enabled;
    }

    public void Interact(GameObject player)
    {
        if (!IsInteractable || _isPlaybackMode) return;

        PullLever();

        if (_recording != null && GameManager.Instance != null)
        {
            _recording.RecordLeverPull(GameManager.Instance.LevelTimer, leverIndex);
        }
    }

    private void PullLever()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Pull");
        }
        if (_audioSource != null)
        {
            _audioSource?.Play();
        }
        OnLeverPulled?.Invoke();
    }

    public void PullLeverProgrammatically()
    {
        PullLever();
    }
}