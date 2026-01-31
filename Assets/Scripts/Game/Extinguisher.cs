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

    private FireExtinguisherRecording _recording;
    private bool _isPlaybackMode = false;

    public void SetRecording(FireExtinguisherRecording recording)
    {
        _recording = recording;
        Debug.Log("Extinguisher recording system set");
    }

    public void SetPlaybackMode(bool enabled)
    {
        _isPlaybackMode = enabled;
    }

    public FireExtinguisherRecording Recording => _recording;

    private void Update()
    {
        if (_isPlaybackMode) return;

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

    public void ActivateExtinguisher(bool active)
    {
        if (active)
        {
            _collider.enabled = true;
            _extinguisherParticles.Play();
            _extinguisherSource.Play();
        }
        else
        {
            _collider.enabled = false;
            _extinguisherParticles.Stop();
            _extinguisherSource.Stop();
        }
    }
}
