using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] private Vector3 forwardOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private float footToNeckHeight = 1f;
    [SerializeField] private float forwardRayLength = 0.8f;
    [SerializeField] private float heightRayLength = 5f;
    [SerializeField] private float ledgeRayLength = 10f;
    [SerializeField] private float climbLedgeRayLength = 1.5f;
    // distancia para que no salga del saliente
    [SerializeField] private float originOffset = 0.65f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask climbLedgeLayer;
    // Threshold = límite
    [SerializeField] private float ledgeHeightThreshold = 0.75f;
    [SerializeField] private float ledgeSpacing = 0.25f;

    public ObstacleHitData ObstacleCkech()
    {
        var hitData = new ObstacleHitData();
        Vector3 forwardOrigin = transform.position + forwardOffset;

        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward, out hitData.forwardHit, forwardRayLength, obstacleLayer);

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

    public bool ClimbLedgeCheck(Vector3 direction, out RaycastHit ledgeHit)
    {
        ledgeHit = new RaycastHit(); // Por defaul

        if (direction == Vector3.zero)
            return false;

        // el valor origin tiene que estar en la altura del cuello
        var origin = transform.position + footToNeckHeight * Vector3.up;
        // el saliente es 0,2, para que no se nos escape ningun saliente
        var offset = new Vector3(0, 0.18f, 0);

        for (int i = 0; i < 10; i++)
        {
            Debug.DrawRay(origin + offset * i, direction);
            if (Physics.Raycast(origin + offset * i, direction, out RaycastHit hit, climbLedgeRayLength, climbLedgeLayer))
            {
                ledgeHit = hit;

                return true;
            }
        }

        return false;
    }

    public bool DropLedgeCheck(out RaycastHit ledgeHit)
    {
        ledgeHit = new RaycastHit(); // Por defaul

        var origin = transform.position + Vector3.down * 0.1f + transform.forward * 2;

        Debug.DrawRay(origin, -transform.forward * 3, Color.green);
        if (Physics.Raycast(origin, -transform.forward, out RaycastHit hit, 3, climbLedgeLayer))
        {
            ledgeHit = hit;

            return true;
        }

        return false;
    }

    public bool ObstacleLedgeCheck(Vector3 moveDirection, out LedgeData ledgeData)
    {
        ledgeData = new LedgeData();

        if (moveDirection == Vector3.zero)
            return false;

        // distancia para que no salga del saliente
        // lo ajusrtaremos desde el editor
        //var originOffset = 0.65f;
        // Como el rayo se lanza en los pies sumamos una unidad para arriba para no tener problemas con el rayo que atraviese el suelo
        var origin = transform.position + moveDirection * originOffset + Vector3.up;

        if (PhysicsUtil.ThreeRayCast(origin, Vector3.down, ledgeSpacing, transform, out List<RaycastHit> hits, ledgeRayLength, obstacleLayer, true))
        {
            RaycastHit hitMin = default;
            float heightMin = int.MaxValue;
            bool found = false;

            foreach (var hit in hits)
            {
                var height = transform.position.y - hit.point.y; // Sal

                // Esta en un saliente
                if (height > ledgeHeightThreshold)
                {
                    // es menor que el anterior
                    if (height < heightMin)
                    {
                        heightMin = height;
                        Debug.Log($"height2: {height}");
                        hitMin = hit;
                        if (!found)
                            found = true; // Encontrad
                    }
                }
            }
            if (found) // Encontrad)
            {
                //var surfaceOriginal = transform.position + moveDirection - (new Vector3(0, 1, 0));
                var surfaceOrigin = hitMin.point;
                // para que este por dejado del pie del jugador
                surfaceOrigin.y = transform.position.y - 0.1f; // Salie

                if (Physics.Raycast(surfaceOrigin, transform.position - surfaceOrigin, out RaycastHit surfaceHit, 2, obstacleLayer))
                {
                    Debug.DrawLine(surfaceOrigin, transform.position, Color.cyan);
                    float height = heightMin;

                    Debug.Log($"height: {height}");

                    ledgeData.angle = Vector3.Angle(transform.forward, surfaceHit.normal);
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