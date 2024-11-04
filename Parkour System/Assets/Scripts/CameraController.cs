using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GatherInput gInput;
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minYAangle = -45f;
    [SerializeField] private float maxYAngle = 45f;
    [SerializeField] private Vector2 offset;
    [SerializeField] private bool invertX, invertY;

    private float rotationY, rotationX;
    private float invertValueX, invertValueY;

    public Quaternion GetYRotation() => Quaternion.Euler(0, rotationY, 0);

    private void Start()
    {
        Cursor.visible = false;
    }

    private void Update()
    {
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
        var focusPosition = target.position + new Vector3(offset.x, offset.y);

        invertValueX = invertX ? -1 : 1;
        invertValueY = invertY ? -1 : 1;

        rotationX = gInput.mouseInput.y * invertValueY * rotationSpeed;
        rotationY = gInput.mouseInput.x * invertValueX * rotationSpeed;

        rotationX = Mathf.Clamp(rotationX, minYAangle, maxYAngle);

        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;
    }
}