using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public Vector2 LookInput => _lookInput;
    public Vector2 MoveInput => _moveInput;

    public event Action JumpEvent;

    private Vector2 _lookInput;
    private Vector2 _moveInput;

    private InputActions _inputs;

    private void Awake()
    {
        _inputs = new InputActions();
        _inputs.Enable();

        _inputs.Player.Jump.performed += OnJumpButtonPressed;
    }

    private void Update()
    {
        _lookInput = _inputs.Player.Look.ReadValue<Vector2>();
        _moveInput = _inputs.Player.Move.ReadValue<Vector2>();
    }

    private void OnJumpButtonPressed(InputAction.CallbackContext context)
    {
        JumpEvent?.Invoke();
    }

    private void OnDisable()
    {
        _inputs.Player.Jump.performed -= OnJumpButtonPressed;

        _inputs.Disable();
    }

    private void OnDestroy()
    {
        _inputs.Dispose();
    }
}