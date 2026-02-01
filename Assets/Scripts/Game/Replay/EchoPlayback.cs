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
    private Animator _animator;
    private CharacterFrame _currentFrame;
    private CharacterFrame _nextFrame;

    private void Awake()
    {
        _transform = transform;
        _animator = GetComponentInChildren<Animator>();

        if (_animator == null)
        {
            Debug.LogError($"[EchoPlayback] No Animator found on {gameObject.name}!");
        }
        else
        {
            Debug.Log($"[EchoPlayback] Animator found on {gameObject.name}, enabled: {_animator.enabled}");
        }
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

            // Make sure animator is enabled
            if (_animator != null)
            {
                _animator.enabled = true;
                _animator.SetFloat("MoveX", _currentFrame.moveX);
                _animator.SetFloat("MoveY", _currentFrame.moveY);
                Debug.Log($"[EchoPlayback] Set initial animation: MoveX={_currentFrame.moveX}, MoveY={_currentFrame.moveY}");
            }
            else
            {
                Debug.LogWarning($"[EchoPlayback] Animator is null during Initialize!");
            }
        }

        Debug.Log($"[EchoPlayback] Initialized playback with {frames.Count} frames");
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

        // Interpolate and apply animation parameters
        if (_animator != null && _animator.enabled)
        {
            float targetMoveX = Mathf.Lerp(_currentFrame.moveX, _nextFrame.moveX, t);
            float targetMoveY = Mathf.Lerp(_currentFrame.moveY, _nextFrame.moveY, t);

            _animator.SetFloat("MoveX", targetMoveX);
            _animator.SetFloat("MoveY", targetMoveY);
        }
    }

    private void ApplyDirectTransform()
    {
        _transform.position = _currentFrame.position;
        _transform.rotation = _currentFrame.rotation;

        // Apply animation parameters directly
        if (_animator != null && _animator.enabled)
        {
            _animator.SetFloat("MoveX", _currentFrame.moveX);
            _animator.SetFloat("MoveY", _currentFrame.moveY);
        }
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

            if (_animator != null)
            {
                _animator.SetFloat("MoveX", _frames[0].moveX);
                _animator.SetFloat("MoveY", _frames[0].moveY);
            }
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