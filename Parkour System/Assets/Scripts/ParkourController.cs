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

        if (data.fowardHitFound)
            Debug.Log($"Obstacle found ''{data.fowardHit.transform.name}'' at {data.fowardHit.point}");
    }
}