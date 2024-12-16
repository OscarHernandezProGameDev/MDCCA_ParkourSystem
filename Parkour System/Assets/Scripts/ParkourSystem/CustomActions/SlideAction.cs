using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/Custom Actions/New Slide Action")]
public class SlideAction : ParkourAction
{
    public override bool CheckIfPossible(EnvironmentScanner.ObstacleHitData hitData, Transform player)
    {
        // Check tag
        if (!string.IsNullOrEmpty(obstacleTag) && !hitData.forwardHit.transform.CompareTag(obstacleTag))
            return false;

        // Se puede añadir cualquier lógica adicional

        return true;
    }
}
