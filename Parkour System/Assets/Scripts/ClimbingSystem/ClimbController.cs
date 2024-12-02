using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    [SerializeField] private GatherInput gatherInput;
    [Header("Offsets")]
    [SerializeField] private Vector3 handOffsetIdleToHang = new Vector3(0.3f, 0.05f, 0.1f);
    // 0.3, 0.07, 0.1
    [SerializeField] private Vector3 handOffsetJumpUp = new Vector3(0.3f, 0.06f, 0.09f);
    [SerializeField] private Vector3 handOffsetJumpDown = new Vector3(0.25f, 0.07f, 0.08f);
    [SerializeField] private Vector3 handOffsetJumpLeft = new Vector3(0.37f, 0.06f, 0.01f);
    [SerializeField] private Vector3 handOffsetJumpRight = new Vector3(0.34f, 0.06f, 0.05f);
    [SerializeField] private Vector3 handOffsetShimmyLeft = new Vector3(0.3f, 0.02f, 0.1f);
    [SerializeField] private Vector3 handOffsetShimmyRight = new Vector3(0.3f, 0.02f, 0.1f);

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
                    StartCoroutine(JumpToLedge("IdleToHang", ledgeHit.transform, 0.41f, 0.54f, handOffset: handOffsetIdleToHang));
                    gatherInput.tryToJump = false;
                }
            }
        }
        else
        {
            // Ledge to ledge
            if (gatherInput.tryToDrop && !playerController.InAction)
            {
                StartCoroutine(JumpFroHang());
                gatherInput.tryToDrop = false;

                return;
            }

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
                    StartCoroutine(JumpToLedge("HangHopUp", currentPoint.transform, 0.34f, 0.65f, handOffset: handOffsetJumpUp));
                else if (neighbour.direction.y == -1)
                    StartCoroutine(JumpToLedge("HangDropDown", currentPoint.transform, 0.31f, 0.65f, handOffset: handOffsetJumpDown));
                else if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("HangHopRight", currentPoint.transform, 0.20f, 0.42f, handOffset: handOffsetJumpRight));
                else if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("HangHopLeft", currentPoint.transform, 0.20f, 0.42f, handOffset: handOffsetJumpLeft));
                // No resetamos para que el jugador pueda saltar de saliente en saliente sin soltar la tecla de salto
                //gatherInput.tryToJump = false;
            }
            else if (neighbour.type == ConnectionType.Move)
            {
                currentPoint = neighbour.point;

                // 0.3, 0, 0.1
                if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform, 0.0f, 0.38f, handOffset: handOffsetShimmyRight));
                else if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, 0.0f, 0.38f, AvatarTarget.LeftHand, handOffset: handOffsetShimmyLeft));
            }
        }
    }

    IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime, AvatarTarget hand = AvatarTarget.RightHand, Vector3? handOffset = null)
    {
        var matchParams = new MatchTargetParams
        {
            pos = GetHandPosition(ledge, hand, handOffset),
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

    private IEnumerator JumpFroHang()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("JumpFromHang");
        playerController.ResetTargetRotation();
        playerController.SetControl(true);
    }

    private Vector3 GetHandPosition(Transform ledge, AvatarTarget hand, Vector3? handOffset)
    {
        // 0.3, 0.07, 0.1
        var offsetValue = (handOffset.HasValue) ? handOffset.Value : new Vector3(0.3f, 0.05f, 0.1f);
        var horizontalDir = hand == AvatarTarget.RightHand ? ledge.right : -ledge.right;

        //return ledge.position + ledge.forward * 0.05f + Vector3.up * 0.05f - horizontalDir * 0.3f;
        return ledge.position + ledge.forward * offsetValue.z + Vector3.up * offsetValue.y - horizontalDir * offsetValue.x;
    }
}
