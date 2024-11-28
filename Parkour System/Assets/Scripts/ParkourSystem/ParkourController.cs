using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnvironmentScanner))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerController))]
public class ParkourController : MonoBehaviour
{
    [SerializeField] private GatherInput gatherInput;
    [SerializeField] private List<ParkourAction> parkourActions;
    [SerializeField] private ParkourAction jumpingDownAction;
    [SerializeField] private float autoJumpHeightLimit = 1f;

    private EnvironmentScanner scanner;
    private Animator animator;
    private PlayerController playerController;

    private void Awake()
    {
        scanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        var data = scanner.ObstacleCkech();

        if (gatherInput.tryToJump && !playerController.InAction && !playerController.IsHanging)
        {
            if (data.forwardHitFound)
            {
                foreach (var action in parkourActions)
                    if (action.CheckIfPossible(data, transform))
                    {
                        StartCoroutine(DoParkourAction(action));
                        gatherInput.tryToJump = false;
                        break;
                    }
            }
        }

        // También comprobamos que no haya un obstaculo de frente
        // solo saltamos si pulsamos la accion de salto
        if (playerController.IsOnLedge && !playerController.InAction && !data.forwardHitFound)
        {
            bool shouldJump = true;

            if (playerController.LedgeData.height > autoJumpHeightLimit && !gatherInput.tryToJump)
                shouldJump = false;

            // Si esl anguilo entre el player y saliente es muy grande quiere decid que no salte 
            if (shouldJump && playerController.LedgeData.angle <= 50)
            {
                playerController.IsOnLedge = false;
                StartCoroutine(DoParkourAction(jumpingDownAction));
                if (gatherInput.tryToJump)
                    gatherInput.tryToJump = false;
            }
        }
    }

    private IEnumerator DoParkourAction(ParkourAction action)
    {
        playerController.SetControl(false);

        MatchTargetParams matchParams = null;

        if (action.EnableTargetMatching)
        {
            matchParams = new MatchTargetParams
            {
                pos = action.MatchPosition,
                bodyPart = action.MatchBodyPart,
                posWeight = action.MatchPoseWeight,
                startTime = action.MatchStartTime,
                targetTime = action.MatchTargetTime
            };
        }

        yield return playerController.DoAction(action.AnimName, matchParams, action.TargetRotation, action.RotateToObstacle, action.PostActionDelay, action.Mirror);

        playerController.SetControl(true);
    }
}