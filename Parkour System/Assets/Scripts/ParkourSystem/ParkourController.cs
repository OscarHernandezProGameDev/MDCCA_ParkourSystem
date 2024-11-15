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

    void Update()
    {
        if (gatherInput.tryToJump && !inAction)
        {
            var data = scanner.ObstacleCkech();

            if (data.forwardHitFound)
            {
                foreach (var action in parkourActions)
                    if (action.CheckIfPossible(data, transform))
                    {
                        StartCoroutine(DoParkourAction(action));
                        break;
                    }
            }

            gatherInput.tryToJump = false;
        }
    }

    private IEnumerator DoParkourAction(ParkourAction action)
    {
        inAction = true;
        playerController.SetControl(false);

        animator.SetBool("MirrorAction", action.Mirror);
        // No hacemos un play porque queremos hacer una transicion de la animacion actual y la de stepUp
        //animator.CrossFade(action.AnimName, 0.2f);
        animator.CrossFadeInFixedTime(action.AnimName, 0.2f);

        // no se ejecutar hasta llegar a fin del frame
        yield return null;

        // vamos a obtener la duracion de la animacion stepUp. Para ello usaremos GetNextAnimatorStateInfo y no
        // GetCurrentAnimatorStateInfo porque esta la transcion de la animacion actual y la de stepUp

        var animState = animator.GetNextAnimatorStateInfo(0);

        if (!animState.IsName(action.AnimName))
            Debug.Log("The Parkour animation is Wrong!");

        float timer = 0f;
        while (timer < animState.length)
        {
            timer += Time.deltaTime;

            if (action.RotateToObstacle)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.TargetRotation,
                    playerController.RotationSpeed * Time.deltaTime);

            if (action.EnableTargetMatching)
                MatchTarget(action);

            // Esta condición es para en el caso del VaultFence para que cuando vaya saltado la valla
            // tome el control del characterController Para que actue la gravedad y no quede flotando en el aire 
            // al final de la animación
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
        // solo hay que ejecutarlo una vez
        if (animator.isMatchingTarget || animator.IsInTransition(0))
            return;

        animator.MatchTarget(action.MatchPosition, transform.rotation, action.MatchBodyPart,
            new MatchTargetWeightMask(action.MatchPoseWeight, 0f), action.MatchStartTime, action.MatchTargetTime);
    }
}