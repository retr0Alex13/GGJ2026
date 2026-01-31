using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optimized movement recorder that reduces redundant frames
/// </summary>
public class MovementRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private bool recordEveryFrame = false;
    [SerializeField] private float minTimeBetweenFrames = 0.05f; // 20 FPS
    [SerializeField] private float minPositionChange = 0.01f;
    [SerializeField] private float minRotationChange = 0.5f; // degrees
    
    private List<CharacterFrame> _recording = new();
    private float _levelTimer = 0;
    private float _lastRecordTime = 0;
    
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;
    
    private Transform _transform;
    
    private void Awake()
    {
        _transform = transform;
        _lastPosition = _transform.position;
        _lastRotation = _transform.rotation;
    }
    
    private void Update()
    {
        _levelTimer += Time.deltaTime;
        
        if (recordEveryFrame)
        {
            RecordFrameUnconditional();
        }
        else
        {
            RecordFrameOptimized();
        }
    }
    
    /// <summary>
    /// Record every frame (original behavior, memory intensive)
    /// </summary>
    private void RecordFrameUnconditional()
    {
        _recording.Add(new CharacterFrame(
            _transform.position,
            _transform.rotation,
            _levelTimer
        ));
    }
    
    /// <summary>
    /// Record only when significant movement occurs or time threshold met
    /// </summary>
    private void RecordFrameOptimized()
    {
        bool shouldRecord = false;
        
        // Always record first frame
        if (_recording.Count == 0)
        {
            shouldRecord = true;
        }
        // Check time threshold
        else if (_levelTimer - _lastRecordTime >= minTimeBetweenFrames)
        {
            // Check if position or rotation changed significantly
            float positionDelta = Vector3.Distance(_transform.position, _lastPosition);
            float rotationDelta = Quaternion.Angle(_transform.rotation, _lastRotation);
            
            if (positionDelta >= minPositionChange || rotationDelta >= minRotationChange)
            {
                shouldRecord = true;
            }
        }
        
        if (shouldRecord)
        {
            _recording.Add(new CharacterFrame(
                _transform.position,
                _transform.rotation,
                _levelTimer
            ));
            
            _lastRecordTime = _levelTimer;
            _lastPosition = _transform.position;
            _lastRotation = _transform.rotation;
        }
    }
    
    /// <summary>
    /// Get the recorded frames
    /// </summary>
    public List<CharacterFrame> GetRecording()
    {
        return new List<CharacterFrame>(_recording);
    }
    
    /// <summary>
    /// Get recording statistics
    /// </summary>
    public void LogRecordingStats()
    {
        if (_recording.Count == 0) return;
        
        float duration = _recording[_recording.Count - 1].time;
        float avgFrameRate = _recording.Count / duration;
        float memorySize = _recording.Count * System.Runtime.InteropServices.Marshal.SizeOf(typeof(CharacterFrame)) / 1024f;
        
        Debug.Log($"Recording Stats:\n" +
                  $"- Duration: {duration:F2}s\n" +
                  $"- Frames: {_recording.Count}\n" +
                  $"- Avg FPS: {avgFrameRate:F1}\n" +
                  $"- Memory: ~{memorySize:F2} KB");
    }
    
    /// <summary>
    /// Clear recording
    /// </summary>
    public void Clear()
    {
        _recording.Clear();
        _levelTimer = 0;
        _lastRecordTime = 0;
    }
}
