using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnvironmentScanner))]
public class ParkourController : MonoBehaviour
{
    private EnvironmentScanner scanner;

    private void Awake()
    {
        scanner = GetComponent<EnvironmentScanner>();
    }

    void Update()
    {
        var data = scanner.ObstacleCkech();

        if (data.forwardHitFound)
            Debug.Log($"Obstacle found ''{data.forwardHit.transform.name}'' at {data.forwardHit.point}");
    }
}