using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Lever : MonoBehaviour, IInteractable
{
    public bool IsInteractable { get; set; } = true;

    //[SerializeField] private Animator _animator;

    public event Action OnLeverPulled;

    public void Interact(GameObject player)
    {
        if (!IsInteractable) return;

        PullLever();
    }

    private void PullLever()
    {
        //if (_animator != null)
        //{
        //    _animator.SetTrigger("Pull");
        //}

        OnLeverPulled?.Invoke();
        Debug.Log("Lever pulled!");
    }

    public void PullLeverProgrammatically()
    {
        PullLever();
    }
}