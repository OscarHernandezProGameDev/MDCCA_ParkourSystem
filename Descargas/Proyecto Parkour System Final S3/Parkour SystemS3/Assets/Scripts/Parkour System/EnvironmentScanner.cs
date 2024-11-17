using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] private Vector3 forwardRayOffset = new Vector3(0,0.5f,0);
    [SerializeField] private float forwardRayLenght = 0.8f;
    [SerializeField] private float heightRayLenght = 5f;
    [SerializeField] private float ledgeRayLenght = 10f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float ledgeHeightThreshold = 0.75f;


    public ObstacleHitData ObstacleCheck()
    {
       var hitData = new ObstacleHitData();

       var forwardOrigin = transform.position + forwardRayOffset;
       hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward,
                                             out hitData.forwardHit, forwardRayLenght, obstacleLayer);

     //  Debug.DrawRay(forwardOrigin, transform.forward * forwardRayLenght, (hitData.forwardHitFound) ? Color.green : Color.red);


        if (hitData.forwardHitFound)
        {
            var heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLenght;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down,
                                                    out hitData.heightHit, heightRayLenght, obstacleLayer);

            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLenght, (hitData.heightHitFound) ? Color.green : Color.red);

        }

        return hitData;

    }


    public bool LedgeCheck(Vector3 moveDirection, out LedgeData ledgeData)
    {
        ledgeData = new LedgeData();

        if(moveDirection == Vector3.zero) 
            return false;

        var originOffset = 0.65f;
        var origin = transform.position + moveDirection * originOffset + Vector3.up;

        if(PhysicsUtil.ThreeRaycasts(origin, Vector3.down,0.25f,transform,
                                        out List <RaycastHit> hits, ledgeRayLenght, obstacleLayer,true))
        {

            var validHits = hits.Where(h => transform.position.y - h.point.y > ledgeHeightThreshold).ToList();

            if(validHits.Count > 0)
            {
                var surfaceOrigin = validHits[0].point;
                surfaceOrigin.y = transform.position.y - 0.1f;

                if (Physics.Raycast(surfaceOrigin, transform.position - surfaceOrigin, out RaycastHit surfaceHit, 2, obstacleLayer))
                {
                    Debug.DrawLine(surfaceOrigin, transform.position, Color.cyan);

                    float height = transform.position.y - validHits[0].point.y;
                   
                    ledgeData.angle = Vector3.Angle(transform.forward, surfaceHit.normal);
                    ledgeData.height = height;
                    ledgeData.surfaceHit = surfaceHit;

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

    public struct LedgeData
    {
        public float height;
        public float angle;
        public RaycastHit surfaceHit;
    }


}
