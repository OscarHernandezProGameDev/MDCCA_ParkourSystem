using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnvironmentScanner))]
public class ParkourController : MonoBehaviour
{
    [SerializeField] private GatherInput gatherInput;

    private EnvironmentScanner scanner;

    private void Awake()
    {
        scanner = GetComponent<EnvironmentScanner>();
    }

    void Update()
    {
        if (gatherInput.tryToJump)
        {
            var data = scanner.ObstacleCkech();

            if (data.forwardHitFound)
                Debug.Log($"Obstacle found ''{data.forwardHit.transform.name}'' at {data.forwardHit.point}");

            gatherInput.tryToJump = false;
        }
    }
}