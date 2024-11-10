using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] private Vector3 forwardOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private float forwardDistance = 0.8f;
    [SerializeField] private LayerMask obstacleLayer;

    public ObstacleHitData ObstacleCkech()
    {
        var hitData = new ObstacleHitData();
        Vector3 forwardOrigin = transform.position + forwardOffset;

        hitData.fowardHitFound = Physics.Raycast(forwardOrigin, transform.forward, out hitData.fowardHit,
            forwardDistance,
            obstacleLayer);

        Debug.DrawRay(forwardOrigin, transform.forward * forwardDistance,
            hitData.fowardHitFound ? Color.green : Color.red);

        return hitData;
    }

    public struct ObstacleHitData
    {
        public bool fowardHitFound;
        public RaycastHit fowardHit;
    }
}