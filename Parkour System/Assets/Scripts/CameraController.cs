using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GatherInput gInput;
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;

    private float rotationY;

    private void Update()
    {
        rotationY = gInput.mouseInput.x;
        transform.position = target.position - Quaternion.Euler(0, rotationY, 0) * new Vector3(0, 0, distance);
    }
}