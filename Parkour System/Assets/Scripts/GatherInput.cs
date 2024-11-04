using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GatherInput : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction lookAction;

    public Vector2 mouseInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions["Look"];
    }

    private void OnEnable()
    {
        lookAction.performed += ReadMouse;
    }

    private void OnDisable()
    {
        lookAction.performed -= ReadMouse;
    }

    private void ReadMouse(InputAction.CallbackContext context)
    {
        mouseInput = context.ReadValue<Vector2>();
    }
}