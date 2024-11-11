using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnvironmentScanner))]
[RequireComponent(typeof(Animator))]
public class ParkourController : MonoBehaviour
{
    [SerializeField] private GatherInput gatherInput;

    private EnvironmentScanner scanner;
    private Animator animator;

    private bool inAction;

    private void Awake()
    {
        scanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (gatherInput.tryToJump && !inAction)
        {
            var data = scanner.ObstacleCkech();

            if (data.forwardHitFound)
            {
                StartCoroutine(DoParkourAction());
            }

            gatherInput.tryToJump = false;
        }
    }

    private IEnumerator DoParkourAction()
    {
        inAction = true;

        // No hacemos un play porque queremos hacer una transicion de la animacion actual y la de stepUp
        animator.CrossFade("StepUp", 0.2f);

        // no se ejecutar hasta llegar a fin del frame
        yield return null;

        // vamos a obtener la duracion de la animacion stepUp. Para ello usaremos GetNextAnimatorStateInfo y no
        // GetCurrentAnimatorStateInfo porque esta la transcion de la animacion actual y la de stepUp

        var animState = animator.GetNextAnimatorStateInfo(0);

        Debug.Log($"StepUp Duracion: {animState.length}");

        yield return new WaitForSeconds(animState.length);

        inAction = false;
    }
}