using UnityEditor.Animations;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator _animator;
    private string _openTrigger = "Open";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _openTrigger = Animator.StringToHash("Open").ToString();
    }

    public void Open()
    {
        Debug.Log("Door opening");
        _animator.SetTrigger("Open");
    }
}
