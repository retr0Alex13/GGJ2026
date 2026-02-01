using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Lever : MonoBehaviour, IInteractable
{
    public bool IsInteractable { get; set; } = true;

    private Animator _animator;
    private AudioSource _audioSource;

    public event Action OnLeverPulled;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void Interact(GameObject player)
    {
        if (!IsInteractable) return;

        PullLever();
    }

    private void PullLever()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Pull");
        }
        _audioSource?.Play();

        OnLeverPulled?.Invoke();
    }

    public void PullLeverProgrammatically()
    {
        PullLever();
    }
}