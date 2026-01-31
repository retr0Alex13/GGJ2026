using System.Collections.Generic;
using UnityEngine;

public class EchoPlayback : MonoBehaviour
{
    [Header("Playback Settings")]
    [SerializeField] private bool useInterpolation = true;
    [SerializeField] private float interpolationSpeed = 10f;

    private List<CharacterFrame> _frames;
    private float _playbackTime;
    private int _currentIndex;
    private int _nextIndex;

    private Transform _transform;
    private CharacterFrame _currentFrame;
    private CharacterFrame _nextFrame;

    private void Awake()
    {
        _transform = transform;
    }

    public void Initialize(List<CharacterFrame> frames)
    {
        _frames = frames;
        _playbackTime = 0;
        _currentIndex = 0;
        _nextIndex = Mathf.Min(1, frames.Count - 1);

        if (_frames != null && _frames.Count > 0)
        {
            _currentFrame = _frames[0];
            _nextFrame = _frames[_nextIndex];

            _transform.position = _currentFrame.position;
            _transform.rotation = _currentFrame.rotation;
        }

        Debug.Log($"Initialized playback with {frames.Count} frames");
    }

    private void Update()
    {
        if (_frames == null || _frames.Count == 0) return;
        if (_currentIndex >= _frames.Count - 1) return;

        _playbackTime += Time.deltaTime;

        UpdateFrameIndices();

        if (useInterpolation)
        {
            ApplyInterpolatedTransform();
        }
        else
        {
            ApplyDirectTransform();
        }
    }

    private void UpdateFrameIndices()
    {
        while (_nextIndex < _frames.Count - 1 && _frames[_nextIndex].time < _playbackTime)
        {
            _currentIndex++;
            _nextIndex++;
            _currentFrame = _frames[_currentIndex];
            _nextFrame = _frames[_nextIndex];
        }
    }

    private void ApplyInterpolatedTransform()
    {
        float t = 0f;
        float timeDelta = _nextFrame.time - _currentFrame.time;

        if (timeDelta > 0)
        {
            t = (_playbackTime - _currentFrame.time) / timeDelta;
            t = Mathf.Clamp01(t);
        }

        Vector3 targetPos = Vector3.Lerp(_currentFrame.position, _nextFrame.position, t);
        Quaternion targetRot = Quaternion.Slerp(_currentFrame.rotation, _nextFrame.rotation, t);

        _transform.position = Vector3.Lerp(_transform.position, targetPos, Time.deltaTime * interpolationSpeed);
        _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRot, Time.deltaTime * interpolationSpeed);
    }

    private void ApplyDirectTransform()
    {
        _transform.position = _currentFrame.position;
        _transform.rotation = _currentFrame.rotation;
    }

    public void ResetPlayback()
    {
        _playbackTime = 0;
        _currentIndex = 0;
        _nextIndex = Mathf.Min(1, _frames.Count - 1);

        if (_frames != null && _frames.Count > 0)
        {
            _transform.position = _frames[0].position;
            _transform.rotation = _frames[0].rotation;
        }
    }

    public float GetPlaybackProgress()
    {
        if (_frames == null || _frames.Count == 0) return 0;
        return _playbackTime / _frames[_frames.Count - 1].time;
    }

    public bool IsPlaybackFinished()
    {
        return _currentIndex >= _frames.Count - 1;
    }
}
