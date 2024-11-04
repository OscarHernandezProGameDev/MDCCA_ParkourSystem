using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GatherInput gInput;
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minYAangle = -45f;
    [SerializeField] private float maxYAngle = 45f;
    [SerializeField] private Vector2 offset;

    private float rotationY, rotationX;

    private void Update()
    {
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
        var focusPosition = target.position + new Vector3(offset.x, offset.y);

        rotationX = gInput.mouseInput.y;
        rotationY = gInput.mouseInput.x;

        rotationX = Mathf.Clamp(rotationX, minYAangle, maxYAngle);

        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;
    }
}