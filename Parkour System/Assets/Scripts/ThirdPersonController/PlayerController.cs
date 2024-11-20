using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static EnvironmentScanner;

/// <summary>
/// https://docs.unity3d.com/es/2018.4/Manual/class-CharacterController.html. Para configurar los valores del CharacterController
/// El radio del character controller y de la esfera que usamos para el GroundCheck tiene que ser iguales o muy iguales
/// Skin width : Un buen ajuste es hacer este valor 10% del Radius.Dos Colliders pueden penetrarse entre sí tan profundo como el ancho de su piel (Skin Width).
/// Mayores Skin Widths van a reducir la fluctuación de fase.  Bajo Skin Width puede causar al personaje en quedarse atrapado. Un buen ajuste es hacer este valor 10% del Radius.
/// Center = Height / 2 + Skin Width
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GatherInput gatherInput;
    [SerializeField] private float moveSpeed = 5f, rotationSpeed = 500f;

    [Header("GroundCheck")]
    [SerializeField]
    private float groundCheckRadius = 0.2f;

    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private LayerMask groundLayer;

    private CameraController cameraController;
    private EnvironmentScanner environmentScanner;
    private CharacterController characterController;
    private Animator animator;
    private Vector3 moveInput;
    private Vector3 desiredMoveDir;
    private Vector3 moveDir;
    private Vector3 velocity;
    private Quaternion targetRotation;
    private float ySpeed;

    private bool hasControl = true;

    public float RotationSpeed => rotationSpeed;

    public bool IsOnLedge { get; set; }

    public LedgeData LedgeData { get; set; }

    public bool HasControl { get => hasControl; set => hasControl = value; }

    public void SetControl(bool hasControl)
    {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;

        if (!this.hasControl)
        {
            animator.SetFloat("MoveAmount", 0);
            targetRotation = transform.rotation;
        }
    }

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        // Para garantizar un fps constante
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        Vector2 direction = gatherInput.smoothedDirection;
        moveInput = new Vector3(direction.x, 0, direction.y);

        float moveAmount = Mathf.Clamp01(Mathf.Abs(direction.x) + Mathf.Abs(direction.y));
        desiredMoveDir = cameraController.GetYRotation() * moveInput;
        moveDir = desiredMoveDir;

        if (!hasControl)
            return;

        velocity = Vector3.zero;
        var isGrounded = CheckGround();

        animator.SetBool("IsGrounded", isGrounded);

        if (isGrounded)
        {
            // No lo podemos a cero para asegurarnos que el jugado siempre este en el suelo
            ySpeed = -1f;
            velocity = desiredMoveDir * moveSpeed;

            // desiredMoveDir lo normalizamos para suaviar la curba dampTime
            IsOnLedge = environmentScanner.LedgeCheck(desiredMoveDir.normalized, out LedgeData ledgeData);

            if (IsOnLedge)
            {
                LedgeData = ledgeData;
                LedgeMovement();
            }

            // dampTime: 0.1f
            // divimos por la moveSpeed(velocidad máxima del jugador) para normalizarlo
            // velocity.magnitude / moveSpeed o velocity.sqrMagnitude / (moveSpeed*moveSpeed), magnitude es un poco más costosa que sqlMagnitude
            animator.SetFloat("MoveAmount", velocity.sqrMagnitude / (moveSpeed * moveSpeed), 0.15f, Time.deltaTime);
        }
        else
        {
            velocity = transform.forward * moveSpeed / 2;
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        velocity.y = ySpeed;

        //transform.position += moveDir * moveSpeed * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
        // ya no usamos moveDir.sqrMagnitude porque tenemos la variable moveAmount
        if (moveAmount > 0f && moveDir.sqrMagnitude > 0.05f)
        {
            // para que mire en la direccion que mira el input
            targetRotation = Quaternion.LookRotation(moveDir);
            // Para hacer una rotacion suave
        }

        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

    private bool CheckGround()
    {
        bool isGrounded =
            Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);

        return isGrounded;
    }

    private void LedgeMovement()
    {
        float signedAngle = Vector3.SignedAngle(LedgeData.surfaceHit.normal, desiredMoveDir, Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        if (Vector3.Angle(desiredMoveDir, transform.forward) >= 80)
        {
            // Don't move but rotate
            velocity = Vector3.zero;

            return;
        }

        // no queremos que se mueva cuando se va fuera del saliente
        // en vez de 60 ponemos 50 para que el jugado se mueve y no se quede quieto
        if (angle < 50)
        {
            velocity = Vector3.zero;
            // que no rote
            moveDir = Vector3.zero;
        }
        else if (angle < 50) // Cambiamos el <90 para que no haya problemas con los joystick
        {
            // Angle is between 60 and 90, so limit the velocity to horizontal direction
            var left = Vector3.Cross(Vector3.up, LedgeData.surfaceHit.normal);
            var dir = left * Mathf.Sign(signedAngle);

            velocity = velocity.magnitude * dir;
            moveDir = dir;
        }
    }
}