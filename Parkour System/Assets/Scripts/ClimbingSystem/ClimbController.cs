using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    [SerializeField] private GatherInput gatherInput;

    private EnvironmentScanner envScanner;
    private PlayerController playerController;

    private ClimbPoint currentPoint;

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
                    currentPoint = ledgeHit.transform.GetComponent<ClimbPoint>();
                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("IdleToHang", ledgeHit.transform, 0.41f, 0.54f));
                    gatherInput.tryToJump = false;
                }
            }
        }
        else
        {
            // Ledge to ledge

            var h = Mathf.Round(gatherInput.Direction.x);
            var v = Mathf.Round(gatherInput.Direction.y);
            var inputDir = new Vector2(h, v);

            if (playerController.InAction || inputDir == Vector2.zero)
                return;

            var neighbour = currentPoint.GetNeighbour(inputDir);

            if (neighbour == null)
                return;

            if (neighbour.type == ConnectionType.Jump && gatherInput.tryToJump)
            {
                currentPoint = neighbour.point;

                if (neighbour.direction.y == 1)
                    StartCoroutine(JumpToLedge("HangHopUp", currentPoint.transform, 0.34f, 0.65f));
                else if (neighbour.direction.y == -1)
                    StartCoroutine(JumpToLedge("HangDropDown", currentPoint.transform, 0.31f, 0.65f));
                else if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("HangHopRight", currentPoint.transform, 0.20f, 0.50f));
                else if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("HangHopLeft", currentPoint.transform, 0.20f, 0.50f));
            }
            else if (neighbour.type == ConnectionType.Move)
            {
                currentPoint = neighbour.point;

                if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform, 0.0f, 0.38f));
                else if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, 0.0f, 0.38f, AvatarTarget.LeftHand));
            }
        }
    }

    IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime, AvatarTarget hand = AvatarTarget.RightHand)
    {
        var matchParams = new MatchTargetParams
        {
            pos = GetHandPosition(ledge, hand),
            bodyPart = hand,
            startTime = matchStartTime,
            targetTime = matchTargetTime,
            posWeight = Vector3.one
        };
        // rota para mirar justo al saliente
        var targetRotation = Quaternion.LookRotation(-ledge.forward);

        yield return playerController.DoAction(anim, matchParams, targetRotation, true);

        playerController.IsHanging = true;
    }

    private Vector3 GetHandPosition(Transform ledge, AvatarTarget hand)
    {
        var horizontalDir = hand == AvatarTarget.RightHand ? ledge.right : -ledge.right;

        return ledge.position + ledge.forward * 0.05f + Vector3.up * 0.05f - horizontalDir * 0.3f;
    }
}
