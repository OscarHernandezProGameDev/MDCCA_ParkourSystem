using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnvironmentScanner;

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
    private EnvironmentScanner environmentScanner;
    private Animator animator;

    private Vector3 moveInput;
    private Quaternion targetRotation;
    private float ySpeed;

    private bool hasControl = true;

    private Vector3 desiredMoveDir;
    private Vector3 moveDir;
    private Vector3 velocity;
    public bool IsOnLedge { get; set; }
    public LedgeData LedgeData { get; set; }

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        characterController = GetComponent<CharacterController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        Application.targetFrameRate = 60;
    }
  
    void Update()
    {
        Vector2 direction = gatherInput.smoothedDirection;
        moveInput = new Vector3(direction.x, 0, direction.y);

        float moveAmount = Mathf.Clamp01(Mathf.Abs(direction.x) + Mathf.Abs(direction.y));

        desiredMoveDir = cameraController.GetYRotation * moveInput;
        moveDir = desiredMoveDir;
        if (!hasControl)
            return;

        velocity = Vector3.zero;
        animator.SetBool("isGrounded", GroundCheck());

        if (GroundCheck())
        {
            ySpeed = -1;
            velocity = desiredMoveDir * moveSpeed;

            IsOnLedge = environmentScanner.LedgeCheck(desiredMoveDir.normalized, out LedgeData ledgeData); 
            if(IsOnLedge) 
            {
                LedgeData = ledgeData;
                LedgeMovement();
            } 
            animator.SetFloat("moveAmount", velocity.magnitude/moveSpeed, 0.1f, Time.deltaTime);
        }
        else
        {
            velocity = transform.forward * moveSpeed / 2;
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }
   
        
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0f && moveDir.sqrMagnitude > 0.05f)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
        }
       
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void LedgeMovement()
    {
        float signedAngle = Vector3.SignedAngle(LedgeData.surfaceHit.normal, desiredMoveDir,Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        if(Vector3.Angle(desiredMoveDir,transform.forward) >= 80)
        {
            //No permitimos el movimiento, pero si rotar hacia la cornisa
            velocity = Vector3.zero;
            return;
        }

        if(angle < 50)
        {
             velocity = Vector3.zero;
             // Comenta la siguiente línea para permitir que el player rote al llegar a una cornisa =>
             moveDir = Vector3.zero;                          // Rotar previene que si el player hace "el tonto" en una esquina, nos caigamos eventualmente
                                                              // Como contra, el "feeling" de rotar sin el movimiento, puede ser peor que sin la rotación
        }
        else if(angle < 90)
        {
            //Si el ángulo está entre 60 y 90, limitamos velocity a solo la dirección horizontal del saliente en el que estemos

            var left = Vector3.Cross(Vector3.up, LedgeData.surfaceHit.normal);
            var dir = left * Mathf.Sign(signedAngle);

            velocity = velocity.magnitude * dir;
            moveDir = dir;
        }
    }

    public void SetControl(bool hasControl)
    {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;

        if (!hasControl)
        {
            animator.SetFloat("moveAmount", 0);
            targetRotation = transform.rotation;
        }
    }

    public bool HasControl
    {
        get => hasControl;
        set => hasControl = value;
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

    public float RotationSpeed => rotationSpeed;

}
