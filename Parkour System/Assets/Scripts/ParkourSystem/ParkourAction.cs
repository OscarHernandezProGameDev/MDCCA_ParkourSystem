using static EnvironmentScanner;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Parkour System/New Parkour Action")]
public class ParkourAction : ScriptableObject
{
    [SerializeField] private string animName;

    // Como mínimo tiene que ser como el valor del character controller step offset (si es menor de este valor el character controler lo entiende como una escalera)
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;

    public string AnimName => animName;

    public bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        float height = hitData.heightHit.point.y - player.position.y;

        return height >= minHeight && height <= maxHeight;
    }
}