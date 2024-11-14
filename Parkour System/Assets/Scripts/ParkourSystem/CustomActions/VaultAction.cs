using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaultAction : ParkourAction
{
    public override bool CheckIfPossible(EnvironmentScanner.ObstacleHitData hitData, Transform player)
    {
        if (!base.CheckIfPossible(hitData, player))
            return false;

        // Vamos a usar el espacio local para saber si estamos a la derecha o izquierda del obstáculo
        var hitPoint = hitData.forwardHit.transform.InverseTransformPoint(hitData.forwardHit.point);

        // hitPoint.z => pasar que lado de la valla estamos encarando, solo miramos el signo

        if ((hitPoint.z < 0 && hitPoint.x < 0) || (hitPoint.z > 0 && hitPoint.x > 0))
        {
            // Mirror animation
        }
        else
        {
            // Don't Mirror
        }

        return true;
    }
}