using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] private Vector3 forwardOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private float forwardRayLength = 0.8f;
    [SerializeField] private float heightRayLength = 5f;
    [SerializeField] private LayerMask obstacleLayer;

    public ObstacleHitData ObstacleCkech()
    {
        var hitData = new ObstacleHitData();
        Vector3 forwardOrigin = transform.position + forwardOffset;

        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, Vector3.forward, out hitData.forwardHit,
            forwardRayLength,
            obstacleLayer);

        Debug.DrawRay(forwardOrigin, Vector3.forward * forwardRayLength,
            hitData.forwardHitFound ? Color.green : Color.red);

        if (hitData.forwardHitFound)
        {
            var heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLength;

            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down, out hitData.heightHit, heightRayLength,
                obstacleLayer);

            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength,
                hitData.heightHitFound ? Color.green : Color.red);
        }

        return hitData;
    }

    public struct ObstacleHitData
    {
        public bool forwardHitFound;
        public bool heightHitFound;

        public RaycastHit forwardHit;
        public RaycastHit heightHit;
    }
}