using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Extinguisher : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _extinguisherParticles;

    [SerializeField]
    private AudioSource _extinguisherSource;

    [SerializeField]
    private Collider _collider;

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _collider.enabled = true;
            _extinguisherParticles.Play();
            _extinguisherSource.Play();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            _collider.enabled = false;
            _extinguisherParticles.Stop();
            _extinguisherSource.Stop();
        }
    }
}
