using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] private GatherInput gatherInput;
    [SerializeField] private List<ParkourAction> parkourActions;
    [SerializeField] private ParkourAction jumpDownAction;
    [SerializeField] private float autoJumpHeightLimit = 1f;

    private EnvironmentScanner scanner;
    private Animator animator;
    private PlayerController playerController;

    private bool inAction;

    private void Awake()
    {
        scanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        var hitData = scanner.ObstacleCheck();

        if (gatherInput.tryToJump && !inAction)
        {
            if (hitData.forwardHitFound)
            {
                foreach (var action in parkourActions)
                {
                    if(action.CheckIfPossible(hitData,transform))
                    {
                        StartCoroutine(DoParkourAction(action));
                        break;
                    }
                }
                gatherInput.tryToJump = false;
            }
        }
        
        if(playerController.IsOnLedge && !inAction && !hitData.forwardHitFound )
        {
            bool shouldJump = true;

            if(playerController.LedgeData.height > autoJumpHeightLimit && !gatherInput.tryToJump) 
                shouldJump = false;

            if(shouldJump && playerController.LedgeData.angle <= 50 )
            {
                playerController.IsOnLedge = false;
                StartCoroutine(DoParkourAction(jumpDownAction));

                if(gatherInput.tryToJump)
                    gatherInput.tryToJump = false;
            }
        }
    }

    private IEnumerator DoParkourAction(ParkourAction action)
    {
        inAction = true;
        playerController.SetControl(false);

        animator.SetBool("mirrorAction", action.Mirror);
        animator.CrossFade(action.AnimName, 0.2f);

        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);

        if(!animState.IsName(action.AnimName))
        {
            Debug.LogError("The Parkour animation is Wrong!");
        }

        float timer = 0f;
        while(timer < animState.length)
        {
            timer += Time.deltaTime;

            if (action.RotateToObstacle)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.TargetRotation, 
                                                                playerController.RotationSpeed * Time.deltaTime);

            if (action.EnableTargetMatching)
                MatchTarget(action);

            if (animator.IsInTransition(0) && timer > 0.5f)
                break;

            yield return null;
        }


        yield return new WaitForSeconds(action.PostActionDelay);

        playerController.SetControl(true);
        inAction = false;
    }

    private void MatchTarget(ParkourAction action)
    {
        if (animator.isMatchingTarget || animator.IsInTransition(0)) return;

        animator.MatchTarget(action.MatchPosition, transform.rotation, action.MatchBodyPart,
            new MatchTargetWeightMask(action.MatchPosWeight, 0), action.MatchStartTime, action.MatchTargetTime);
    }


}
