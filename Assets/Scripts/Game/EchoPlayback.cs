using System.Collections.Generic;
using UnityEngine;

public class EchoPlayback : MonoBehaviour
{
    private List<CharacterFrame> _frames;
    private float _timer;
    private int _index;

    public void Initialize(List<CharacterFrame> frames)
    {
        _frames = frames;
        _timer = 0;
        _index = 0;
    }

    void Update()
    {
        if (_frames == null || _index >= _frames.Count) return;

        _timer += Time.deltaTime;

        while (_index < _frames.Count - 1 && _frames[_index].time < _timer)
        {
            _index++;
        }

        transform.position = _frames[_index].position;
        transform.rotation = _frames[_index].rotation;
    }
}