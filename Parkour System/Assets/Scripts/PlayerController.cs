using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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

    [Header("GroundCheck")] [SerializeField]
    private float groundCheckRadius = 0.2f;

    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private LayerMask groundLayer;

    private CameraController cameraController;
    private CharacterController characterController;
    private Animator animator;
    private Vector3 moveInput;
    private Quaternion tarjetRotation;
    private float ySpeed;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
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
        var moveDir = cameraController.GetYRotation() * moveInput;

        if (CheckGround())
        {
            // No lo podemos a cero para asegurarnos que el jugado siempre este en el suelo
            ySpeed = -1f;
        }
        else
            ySpeed += Physics.gravity.y * Time.deltaTime;

        var velocity = moveDir * moveSpeed;

        velocity.y = ySpeed;

        //transform.position += moveDir * moveSpeed * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
        // ya no usamos moveDir.sqrMagnitude porque tenemos la variable moveAmount
        if (moveAmount > 0f)
        {
            // para que mire en la direccion que mira el input
            tarjetRotation = Quaternion.LookRotation(moveDir);
            // Para hacer una rotacion suave
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, tarjetRotation, rotationSpeed * Time.deltaTime);
        }

        // dampTime: 0.1f
        animator.SetFloat("MoveAmount", moveAmount, 0.15f, Time.deltaTime);
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
}