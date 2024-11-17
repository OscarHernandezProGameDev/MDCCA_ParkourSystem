using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] private Vector3 forwardOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private float forwardRayLength = 0.8f;
    [SerializeField] private float heightRayLength = 5f;
    [SerializeField] private float ledgeRayLength = 10f;
    [SerializeField] private LayerMask obstacleLayer;
    // Threshold = límite
    [SerializeField] private float ledgeHeightThreshold = 0.75f;

    public ObstacleHitData ObstacleCkech()
    {
        var hitData = new ObstacleHitData();
        Vector3 forwardOrigin = transform.position + forwardOffset;

        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward, out hitData.forwardHit,
            forwardRayLength,
            obstacleLayer);

        Debug.DrawRay(forwardOrigin, transform.forward * forwardRayLength,
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

    public bool LedgeCheck(Vector3 moveDirection)
    {
        if (moveDirection == Vector3.zero)
            return false;

        var originOffset = 0.5f;
        var origin = transform.position + moveDirection * originOffset;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, ledgeRayLength, obstacleLayer))
        {
            float height = transform.position.y - hit.point.y;

            // Esta en un saliente
            if (height > ledgeHeightThreshold)
                return true;
        }

        return false;
    }

    public struct ObstacleHitData
    {
        public bool forwardHitFound;
        public bool heightHitFound;

        public RaycastHit forwardHit;
        public RaycastHit heightHit;
    }
}