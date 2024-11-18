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

    public bool LedgeCheck(Vector3 moveDirection, out LedgeData ledgeData)
    {
        ledgeData = new LedgeData();

        if (moveDirection == Vector3.zero)
            return false;

        var originOffset = 0.5f;
        // Como el rayo se lanza en los pies sumamos una unidad para arriba para no tener problemas con el rayo que atraviese el suelo
        var origin = transform.position + moveDirection * originOffset + Vector3.up;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, ledgeRayLength, obstacleLayer))
        {
            Debug.DrawRay(origin, Vector3.down * ledgeRayLength, Color.magenta);

            var surfaceOriginal = transform.position + moveDirection - (new Vector3(0, 1, 0));

            if (Physics.Raycast(surfaceOriginal, -moveDirection, out RaycastHit surfaceHit, 2, obstacleLayer))
            {
                float height = transform.position.y - hit.point.y;

                // Esta en un saliente
                if (height > ledgeHeightThreshold)
                {
                    ledgeData.angle = Vector3.Angle(transform.position, surfaceHit.point.normalized);
                    ledgeData.height = height;
                    ledgeData.surfaceHit = surfaceHit; // Salient

                    return true;
                }
            }
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

    public struct LedgeData // Salient
    {
        public float height;
        public float angle;
        public RaycastHit surfaceHit; // Saliente
    }
}