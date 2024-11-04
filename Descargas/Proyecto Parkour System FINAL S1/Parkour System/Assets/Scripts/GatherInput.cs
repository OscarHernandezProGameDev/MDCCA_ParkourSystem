using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GatherInput : MonoBehaviour
{

    private PlayerInput playerInput;

    private InputAction lookAction;
    private InputAction moveAction;

    [SerializeField] private float smoothTime = 4f;

    public Vector2 lookInput;
    public Vector2 smoothedDirection;
    private Vector2 direction;

    public bool usingGamepad;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions["Look"];
        moveAction = playerInput.actions["Move"];
    }

    private void OnEnable()
    {
        lookAction.performed += ReadLookInput;
        lookAction.canceled += OnLookCanceled;
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

    private void OnDisable()
    {
        lookAction.performed -= ReadLookInput;
        lookAction.canceled -= OnLookCanceled;
    }


}
