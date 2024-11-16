using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GatherInput : MonoBehaviour
{

    private PlayerInput playerInput;

    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction jumpAction;

    [SerializeField] private float smoothTime = 4f;

    public Vector2 lookInput;
    public Vector2 smoothedDirection;
    private Vector2 direction;

    public bool tryToJump;

    public bool usingGamepad;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions["Look"];
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
    }

    private void OnEnable()
    {
        lookAction.performed += ReadLookInput;
        lookAction.canceled += OnLookCanceled;
        jumpAction.performed += Jump;
        jumpAction.canceled += OnJumpCanceled;
    }


    private void Update()
    {
        direction = moveAction.ReadValue<Vector2>();

        smoothedDirection = new Vector2(
            Mathf.MoveTowards(smoothedDirection.x, direction.x, smoothTime * Time.deltaTime),
            Mathf.MoveTowards(smoothedDirection.y, direction.y, smoothTime * Time.deltaTime)
            );
    }

    private void ReadLookInput(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
        usingGamepad = context.control.device is Gamepad;
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
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


    private void OnDisable()
    {
        lookAction.performed -= ReadLookInput;
        lookAction.canceled -= OnLookCanceled;
        jumpAction.performed -= Jump;
        jumpAction.canceled -= OnJumpCanceled;
    }


}
