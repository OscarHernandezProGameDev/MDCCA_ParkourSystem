using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsUtil
{
    // dir = Vector3.down
    public static bool ThreeRayCast(Vector3 origin, Vector3 dir, float spacing, Transform transform,
        out List<RaycastHit> hits, float distance, LayerMask layerMask, bool debugDraw = false)
    {
        bool centerHitFound = Physics.Raycast(origin, dir, out RaycastHit centerHit, distance, layerMask);
        bool leftHitFound = Physics.Raycast(origin - transform.right * spacing, dir, out RaycastHit leftHit, distance,
            layerMask);
        bool rightHitFound = Physics.Raycast(origin + transform.right * spacing, dir, out RaycastHit rightHit, distance,
            layerMask);

        hits = new List<RaycastHit>() { centerHit, leftHit, rightHit };

        bool hitFound = centerHitFound || leftHitFound || rightHitFound;

        if (hitFound && debugDraw)
        {
            Debug.DrawLine(origin, centerHit.point, Color.magenta);
            Debug.DrawLine(origin - transform.right * spacing, leftHit.point, Color.magenta);
            Debug.DrawLine(origin + transform.right * spacing, rightHit.point, Color.magenta);
        }

        return hitFound;
    }
}