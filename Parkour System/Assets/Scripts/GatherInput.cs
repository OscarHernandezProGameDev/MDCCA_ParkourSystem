using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GatherInput : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dropAction;
    private Vector2 _direction;

    [SerializeField] private float smoothTime = 4f;

    public Vector2 lookInput;
    public Vector2 smoothedDirection;
    public bool tryToJump;
    public bool tryToDrop;

    public bool usingGamePad;

    public Vector2 Direction => _direction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions["Look"];
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        dropAction = playerInput.actions["Drop"];
    }

    private void OnEnable()
    {
        lookAction.performed += ReadLookInput;
        lookAction.canceled += CancelLookInput;

        // En el Input cuando los composive los valores o es 0 o 1, no interpola en el tiempo. Nos interesa esta funcionalidad para usuar en el Animator para pasa de los estados:
        // Idle, Walk, Run. Usaremos el método Update

        // moveAction.performed += ReadDirection;
        // moveAction.canceled += ReadDirection;

        jumpAction.performed += Jump;
        jumpAction.canceled += OnJumpCanceled;

        dropAction.performed += Drop;
        dropAction.canceled += OnDropCanceled;
    }

    private void OnDisable()
    {
        lookAction.performed -= ReadLookInput;
        lookAction.canceled -= CancelLookInput;

        // En el Input cuando los composive los valores o es 0 o 1 o -1, no interpola en el tiempo. Nos interesa esta funcionalidad para usuar en el Animator para pasa de los estados:
        // Idle, Walk, Run. Usaremos el método Update

        // moveAction.performed -= ReadDirection;
        // moveAction.canceled -= ReadDirection;

        jumpAction.performed -= Jump;
        jumpAction.canceled -= OnJumpCanceled;

        dropAction.performed -= Drop;
        dropAction.canceled -= OnDropCanceled;
    }

    private void Update()
    {
        _direction = moveAction.ReadValue<Vector2>();
        // Para interpolar en el tiempo, no usamos Mathf.Lerp porque los a veces cuesta dar los valores iniciales o finales
        smoothedDirection = new Vector2
        (
            Mathf.MoveTowards(smoothedDirection.x, _direction.x, smoothTime * Time.deltaTime),
            Mathf.MoveTowards(smoothedDirection.y, _direction.y, smoothTime * Time.deltaTime)
        );
    }

    private void ReadLookInput(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
        usingGamePad = context.control.device is Gamepad;
    }

    private void CancelLookInput(InputAction.CallbackContext context)
    {
        // En el gamepad hay que cancelar la action porque si leemos la posición del ratón ya lo hace
        // Si no leemos la posición del ratón sino su delta no cancelamos la acción y por tanto hay que comentar la siguiente linea
        //if (usingGamePad)
        lookInput = Vector2.zero;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        tryToJump = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        tryToJump = false;
    }

    private void Drop(InputAction.CallbackContext context)
    {
        tryToDrop = true;
    }

    private void OnDropCanceled(InputAction.CallbackContext context)
    {
        tryToDrop = false;
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