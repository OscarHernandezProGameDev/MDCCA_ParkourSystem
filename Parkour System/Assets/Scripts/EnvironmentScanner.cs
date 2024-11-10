using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] private Vector3 forwardOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private float forwardDistance = 0.8f;
    [SerializeField] private LayerMask obstacleLayer;

    public void ObstacleCkech()
    {
        Vector3 forwardOrigin = transform.position + forwardOffset;
        bool hitFound = Physics.Raycast(forwardOrigin, transform.forward, out RaycastHit hitInfo, forwardDistance,
            obstacleLayer);

        Debug.DrawRay(forwardOrigin, transform.forward * forwardDistance, hitFound ? Color.green : Color.red);
    }
}