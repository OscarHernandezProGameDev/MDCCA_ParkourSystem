using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GatherInput : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction lookAction;
    private InputAction moveAction;
    private Vector2 direction;

    [SerializeField] private float smoothTime = 4f;

    public Vector2 mouseInput;
    public Vector2 smoothedDirection;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions["Look"];
        moveAction = playerInput.actions["Move"];
    }

    private void OnEnable()
    {
        lookAction.performed += ReadMouse;

        // En el Input cuando los composive los valores o es 0 o 1, no interpola en el tiempo. Nos interesa esta funcionalidad para usuar en el Animator para pasa de los estados:
        // Idle, Walk, Run. Usaremos el método Update

        // moveAction.performed += ReadDirection;
        // moveAction.canceled += ReadDirection;
    }

    private void OnDisable()
    {
        lookAction.performed -= ReadMouse;

        // En el Input cuando los composive los valores o es 0 o 1 o -1, no interpola en el tiempo. Nos interesa esta funcionalidad para usuar en el Animator para pasa de los estados:
        // Idle, Walk, Run. Usaremos el método Update

        // moveAction.performed -= ReadDirection;
        // moveAction.canceled -= ReadDirection;
    }

    private void Update()
    {
        direction = moveAction.ReadValue<Vector2>();
        // Para interpolar en el tiempo, no usamos Mathf.Lerp porque los a veces cuesta dar los valores iniciales o finales
        smoothedDirection = new Vector2
        (
            Mathf.MoveTowards(smoothedDirection.x, direction.x, smoothTime * Time.deltaTime),
            Mathf.MoveTowards(smoothedDirection.y, direction.y, smoothTime * Time.deltaTime)
        );
    }

    private void ReadMouse(InputAction.CallbackContext context)
    {
        mouseInput = context.ReadValue<Vector2>();
    }

    // En el Input cuando los composive los valores o es 0 o 1, no interpola en el tiempo. Nos interesa esta funcionalidad para usuar en el Animator para pasa de los estados:
    // Idle, Walk, Run. Usaremos el método Update

    /*
private void ReadDirection(InputAction.CallbackContext context)
{
    direction = context.ReadValue<Vector2>();
}
*/
}