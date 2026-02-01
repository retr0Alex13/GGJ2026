using UnityEditor.Animations;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator _animator;
    private AudioSource _audioSource;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void Open()
    {
        _animator.SetTrigger("Open");
        _audioSource.Play();
    }
}
