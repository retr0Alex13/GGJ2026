using UnityEngine;

public interface IInteractable
{
    public bool IsInteractable { get; }
    public void Interact(GameObject player);
}
