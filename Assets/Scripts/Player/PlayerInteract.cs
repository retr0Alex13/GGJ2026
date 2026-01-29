using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 rayOrigin = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Ray ray = Camera.main.ScreenPointToRay(rayOrigin);

            if (Physics.Raycast(ray, out RaycastHit hit, 50f))
            {
                if (!hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    Debug.Log("Hit " + hit.collider.name);
                    return;
                }
                interactable.Interact(gameObject);
            }
        }
    }
}
