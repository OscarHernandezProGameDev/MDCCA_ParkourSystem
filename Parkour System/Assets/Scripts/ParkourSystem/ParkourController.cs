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

        // No hacemos un play porque queremos hacer una transicion de la animacion actual y la de stepUp
        animator.CrossFade(action.AnimName, 0.2f);

        // no se ejecutar hasta llegar a fin del frame
        yield return null;

        // vamos a obtener la duracion de la animacion stepUp. Para ello usaremos GetNextAnimatorStateInfo y no
        // GetCurrentAnimatorStateInfo porque esta la transcion de la animacion actual y la de stepUp

        var animState = animator.GetNextAnimatorStateInfo(0);

        if (!animState.IsName(action.AnimName))
            Debug.Log("The Parkour animation is Wrong!");

        //Debug.Log($"StepUp Duracion: {animState.length}");

        //yield return new WaitForSeconds(animState.length);

        float time = 0f;
        while (time < animState.length)
        {
            time += Time.deltaTime;
            yield return null;
        }

        playerController.SetControl(true);
        inAction = false;
    }
}