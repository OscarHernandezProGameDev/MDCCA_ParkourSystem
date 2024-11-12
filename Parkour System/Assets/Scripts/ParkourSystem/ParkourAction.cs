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
    [SerializeField] private bool rotateToObstacle;

    [Header("Target Matching")] [SerializeField]
    private bool enabletargetMatching;

    [SerializeField] private AvatarTarget matchBodyPart;

    // Cuando en la animación empieza a despergar del suelo
    [SerializeField] private float matchStartTime;

    // Cuando en la animación pone el pie en la plataforma
    [SerializeField] private float matchTargetTime;

    public Quaternion TargetRotation { get; set; }
    public Vector3 MatchingPosition { get; set; }
    
    public string AnimName => animName;
    public bool RotateToObstacle => rotateToObstacle;
    public bool EnableTargetMatching => enabletargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;

    public bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        float height = hitData.heightHit.point.y - player.position.y;

        if (height < minHeight || height > maxHeight)
            return false;

        if (rotateToObstacle)
            TargetRotation = Quaternion.LookRotation(-hitData.forwardHit.normal);
        
        if(enabletargetMatching)
            MatchingPosition= hitData.heightHit.point;

        return true;
    }
}