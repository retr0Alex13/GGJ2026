using System.Collections.Generic;
using UnityEngine;

public class MovementRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private float recordInterval = 0.05f;
    [SerializeField] private bool optimizeRecording = true;

    private List<CharacterFrame> _frames = new List<CharacterFrame>();
    private float _nextRecordTime = 0f;
    private Transform _transform;
    private Animator _animator;
    private float _startTime;
    private int _frameCount = 0;

    private void Awake()
    {
        _transform = transform;
        _animator = GetComponentInChildren<Animator>();
        _startTime = Time.time;
    }

    private void Update()
    {
        if (Time.time >= _nextRecordTime)
        {
            RecordFrame();
            _nextRecordTime = Time.time + recordInterval;
        }
    }

    private void RecordFrame()
    {
        float currentTime = Time.time - _startTime;

        float moveX = 0f;
        float moveY = 0f;

        if (_animator != null)
        {
            moveX = _animator.GetFloat("MoveX");
            moveY = _animator.GetFloat("MoveY");
        }

        CharacterFrame frame = new CharacterFrame(
            _transform.position,
            _transform.rotation,
            currentTime,
            moveX,
            moveY
        );

        _frameCount++;

        if (optimizeRecording && _frames.Count > 0)
        {
            CharacterFrame lastFrame = _frames[_frames.Count - 1];

            bool posChanged = Vector3.Distance(frame.position, lastFrame.position) > 0.01f;
            bool rotChanged = Quaternion.Angle(frame.rotation, lastFrame.rotation) > 1f;
            bool animChanged = Mathf.Abs(frame.moveX - lastFrame.moveX) > 0.05f ||
                              Mathf.Abs(frame.moveY - lastFrame.moveY) > 0.05f;

            if (!posChanged && !rotChanged && !animChanged)
            {
                return;
            }
        }

        _frames.Add(frame);
    }

    public List<CharacterFrame> GetRecording()
    {
        return _frames;
    }
}