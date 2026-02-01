using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Camera.main == null) return;

            Vector3 rayOrigin = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Ray ray = Camera.main.ScreenPointToRay(rayOrigin);
            float maxDistance = 10f;

            RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance);
            if (hits == null || hits.Length == 0) return;

            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;

                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    if (interactable.IsInteractable)
                    {
                        interactable.Interact(gameObject);
                        return;
                    }
                }
            }
        }
    }
}