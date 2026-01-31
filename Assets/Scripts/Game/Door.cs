using UnityEditor.Animations;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Open()
    {
        _animator.SetTrigger("Open");
    }
}
