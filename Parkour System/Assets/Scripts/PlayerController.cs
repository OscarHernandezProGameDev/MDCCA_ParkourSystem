using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GatherInput gInput;
    [SerializeField] private float moveSpeed = 5f, rotationSpeed = 500f;

    private CameraController cameraController;
    private Animator animator;
    private Vector3 moveInput;
    private Quaternion tarjetRotation;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Vector2 direction = gInput.smoothedDirection;
        moveInput = new Vector3(direction.x, 0, direction.y);

        float moveAmount = Mathf.Clamp01(Mathf.Abs(direction.x) + Mathf.Abs(direction.y));
        var moveDir = cameraController.GetYRotation() * moveInput;

        if (moveDir.sqrMagnitude > 0f)
        {
            transform.position += moveDir * moveSpeed * Time.deltaTime;
            // para que mire en la direccion que mira el input
            tarjetRotation = Quaternion.LookRotation(moveDir);
            // Para hacer una rotacion suave
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, tarjetRotation, rotationSpeed * Time.deltaTime);
        }

        // dampTime: 0.1f
        animator.SetFloat("MoveAmount", moveAmount,0.15f,Time.deltaTime);
    }
}