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
    private bool _isCurrentlyActive = false;

    private void Awake()
    {
        if (_extinguisherParticles != null)
        {
            _extinguisherParticles.Stop();
        }

        if (_collider != null)
        {
            _collider.enabled = false;
        }

        if (_extinguisherSource != null && _extinguisherSource.isPlaying)
        {
            _extinguisherSource.Stop();
        }
    }

    public void SetRecording(FireExtinguisherRecording recording)
    {
        _recording = recording;
        Debug.Log("Extinguisher recording system set");
    }

    public void SetPlaybackMode(bool enabled)
    {
        _isPlaybackMode = enabled;

        if (enabled)
        {
            _isCurrentlyActive = false;
            ActivateExtinguisher(false);
        }

        Debug.Log($"Extinguisher playback mode set to: {enabled}");
    }

    public FireExtinguisherRecording Recording => _recording;

    private void Update()
    {
        if (_isPlaybackMode) return;

        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetActive(true);
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            SetActive(false);
        }
    }

    private void SetActive(bool active)
    {
        if (_isCurrentlyActive == active) return;

        _isCurrentlyActive = active;
        ActivateExtinguisher(active);

        if (_recording != null && !_isPlaybackMode && GameManager.Instance != null)
        {
            _recording.RecordExtinguisherState(
                GameManager.Instance.LevelTimer,
                active
            );
            Debug.Log($"Recorded extinguisher: {active} at {GameManager.Instance.LevelTimer:F2}s");
        }
    }

    public void ActivateExtinguisher(bool active)
    {
        _isCurrentlyActive = active;

        if (_extinguisherParticles == null)
        {
            Debug.LogError("Extinguisher particles not assigned!");
            return;
        }

        if (active)
        {
            if (_collider != null)
                _collider.enabled = true;

            _extinguisherParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _extinguisherParticles.Play();

            if (_extinguisherSource != null && !_extinguisherSource.isPlaying)
            {
                _extinguisherSource.Play();
            }

            Debug.Log("Extinguisher ACTIVATED");
        }
        else
        {
            if (_collider != null)
                _collider.enabled = false;

            _extinguisherParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (_extinguisherSource != null && _extinguisherSource.isPlaying)
            {
                _extinguisherSource.Stop();
            }

            Debug.Log("Extinguisher DEACTIVATED");
        }
    }
}