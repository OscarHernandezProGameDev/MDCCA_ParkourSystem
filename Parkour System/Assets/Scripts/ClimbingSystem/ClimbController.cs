using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    [SerializeField] private GatherInput gatherInput;

    private EnvironmentScanner envScanner;
    private PlayerController playerController;

    private void Awake()
    {
        envScanner = GetComponent<EnvironmentScanner>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!playerController.IsHanging)
        {
            if (gatherInput.tryToJump && !playerController.InAction)
            {
                if (envScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit))
                {
                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("IdleToHang", ledgeHit.transform, 0.41f, 0.54f));
                    gatherInput.tryToJump = false;
                }
            }
        }
        else
        {
            // Ledge to ledge
        }
    }

    IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime)
    {
        var matchParams = new MatchTargetParams
        {
            pos = GetHandPosition(ledge),
            bodyPart = AvatarTarget.RightHand,
            startTime = matchStartTime,
            targetTime = matchTargetTime,
            posWeight = Vector3.one
        };
        // rota para mirar justo al saliente
        var targetRotation = Quaternion.LookRotation(-ledge.forward);

        yield return playerController.DoAction(anim, matchParams, targetRotation, true);

        playerController.IsHanging = true;
    }

    private Vector3 GetHandPosition(Transform ledge)
    {
        return ledge.position + ledge.forward * 0.05f + Vector3.up * 0.17f - ledge.right * 0.3f;
    }
}
