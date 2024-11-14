using static EnvironmentScanner;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Parkour System/New Parkour Action")]
public class ParkourAction : ScriptableObject
{
    [SerializeField] private string animName;
    [SerializeField] private string obstacleTag;

    // Como mínimo tiene que ser como el valor del character controller step offset (si es menor de este valor el character controler lo entiende como una escalera)
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;
    [SerializeField] private bool rotateToObstacle;
    [SerializeField] private float postActionDelay;

    [Header("Target Matching")]
    [SerializeField]
    private bool enableTargetMatching = true;

    [SerializeField] private protected AvatarTarget matchBodyPart;

    // Cuando en la animación empieza a despergar del suelo
    [SerializeField] private float matchStartTime;

    // Cuando en la animación pone el pie en la plataforma
    [SerializeField] private float matchTargetTime;
    [SerializeField] private Vector3 matchPoseWeight = new Vector3(0, 1, 0);
    [SerializeField] private Vector3 extraOffset;

    public Quaternion TargetRotation { get; set; }
    public Vector3 MatchPosition { get; set; }

    public string AnimName => animName;
    public bool RotateToObstacle => rotateToObstacle;
    public float PostActionDelay => postActionDelay;
    public Vector3 MatchPoseWeight => matchPoseWeight;
    public bool EnableTargetMatching => enableTargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;

    public bool Mirror { get; set; }

    public virtual bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        // Check tag
        if (!string.IsNullOrEmpty(obstacleTag) && !hitData.forwardHit.transform.CompareTag(obstacleTag))
            return false;

        // Check height
        float height = hitData.heightHit.point.y - player.position.y;

        if (height < minHeight || height > maxHeight)
            return false;

        if (rotateToObstacle)
            TargetRotation = Quaternion.LookRotation(-hitData.forwardHit.normal);

        if (enableTargetMatching)
            MatchPosition = hitData.heightHit.point + extraOffset;

        return true;
    }
}