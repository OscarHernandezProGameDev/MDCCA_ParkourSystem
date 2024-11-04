using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private GatherInput gatherInput;
    [SerializeField] private float moveSpeed = 5f, rotationSpeed = 500;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadious = 0.2f;
    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private LayerMask groundLayer;

    private CameraController cameraController;
    private CharacterController characterController;
    private Animator animator;

    private Vector3 moveInput;
    private Quaternion targetRotation;
    private float ySpeed;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        Application.targetFrameRate = 60;
    }
  
    void Update()
    {
        Vector2 direction = gatherInput.smoothedDirection;
        moveInput = new Vector3(direction.x, 0, direction.y);

        float moveAmount = Mathf.Clamp01(Mathf.Abs(direction.x) + Mathf.Abs(direction.y));
        var moveDir = cameraController.GetYRotation * moveInput;

        if (GroundCheck())
        {
            ySpeed = -1;
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }
   
        var velocity = moveDir * moveSpeed;
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0f)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        animator.SetFloat("moveAmount", moveAmount,0.1f,Time.deltaTime);
    }

    private bool GroundCheck()
    {
       bool isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadious, groundLayer);
       return isGrounded;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadious);
    }

}
