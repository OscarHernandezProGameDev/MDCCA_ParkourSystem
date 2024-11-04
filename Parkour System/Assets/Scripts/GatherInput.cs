using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GatherInput : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction lookAction;
    private InputAction moveAction;

    public Vector2 mouseInput;
    public Vector2 direction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions["Look"];
        moveAction = playerInput.actions["Move"];
    }

    private void OnEnable()
    {
        lookAction.performed += ReadMouse;
        moveAction.performed += ReadDirection;
        moveAction.canceled += ReadDirection;
    }

    private void OnDisable()
    {
        lookAction.performed -= ReadMouse;
        moveAction.performed -= ReadDirection;
        moveAction.canceled -= ReadDirection;
    }

    private void ReadMouse(InputAction.CallbackContext context)
    {
        mouseInput = context.ReadValue<Vector2>();
    }

    private void ReadDirection(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector2>();
    }
}