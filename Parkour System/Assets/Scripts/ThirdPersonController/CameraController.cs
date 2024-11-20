using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GatherInput gatherInput;
    [SerializeField] private Transform target;

    [SerializeField] private float rotationSpeedMouse = 5f;
    [SerializeField] private float rotationSpeedGamepad = 100f;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minYAangle = -45f;
    [SerializeField] private float maxYAngle = 45f;
    [SerializeField] private Vector2 offset;
    [SerializeField] private bool invertX, invertY;

    private float rotationY, rotationX;

    public Quaternion GetYRotation() => Quaternion.Euler(0, rotationY, 0);

    private void Start()
    {
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
        var focusPosition = target.position + new Vector3(offset.x, offset.y);

        float invertValueX = invertX ? -1 : 1;
        float invertValueY = invertY ? -1 : 1;
        float rotationSpeed = gatherInput.usingGamePad ? rotationSpeedGamepad : rotationSpeedMouse;

        // Si leemos la posición del ratón tenemos que hacer un código diferente para el gamepad porque este da diferencias
        // Por tanto si leemos el delta del ratón tenemos un código único
        // if (gInput.usingGamePad)
        // {
        //     rotationX += gInput.lookInput.y * invertValueY * rotationSpeed * Time.deltaTime;
        //     rotationY += gInput.lookInput.x * invertValueX * rotationSpeed * Time.deltaTime;
        // }
        // else
        // {
        //     rotationX = gInput.lookInput.y * invertValueY * rotationSpeed;
        //     rotationY = gInput.lookInput.x * invertValueX * rotationSpeed;
        // }
        rotationX += gatherInput.lookInput.y * invertValueY * rotationSpeed * Time.deltaTime;
        rotationY += gatherInput.lookInput.x * invertValueX * rotationSpeed * Time.deltaTime;

        rotationX = Mathf.Clamp(rotationX, minYAangle, maxYAngle);

        transform.transform.SetPositionAndRotation(focusPosition - targetRotation * new Vector3(0, 0, distance),
            targetRotation);
    }
}