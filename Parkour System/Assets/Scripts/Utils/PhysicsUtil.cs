using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsUtil
{
    // Este método 'ThreeRaycasts' lanza tres raycasts verticales desde el origen especificado, 
    // hacia abajo y separados por una distancia determinada a la izquierda y a la derecha.
    // Devuelve 'true' si alguno de los raycasts impacta con una superficie dentro de la distancia proporcionada.
    // Los impactos se almacenan en la lista 'hits' y opcionalmente se dibujan líneas de depuración.
    // dir = Vector3.down
    public static bool ThreeRayCast(Vector3 origin, Vector3 dir, float spacing, Transform transform,
        out List<RaycastHit> hits, float distance, LayerMask layer, bool debugDraw = false)
    {
        bool centerHitFound = Physics.Raycast(origin, dir, out RaycastHit centerHit, distance, layer);
        bool leftHitFound = Physics.Raycast(origin - transform.right * spacing, dir, out RaycastHit leftHit, distance,
            layer);
        bool rightHitFound = Physics.Raycast(origin + transform.right * spacing, dir, out RaycastHit rightHit, distance,
            layer);

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