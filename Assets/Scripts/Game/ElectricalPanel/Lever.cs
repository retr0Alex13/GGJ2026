using System;
using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    public bool IsInteractable { get; set; } = true;
    public event Action OnLeverPulled;

    public void Interact(GameObject player)
    {
        OnLeverPulled?.Invoke();
    }
}
