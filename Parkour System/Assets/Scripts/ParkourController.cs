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
        scanner.ObstacleCkech();
    }
}