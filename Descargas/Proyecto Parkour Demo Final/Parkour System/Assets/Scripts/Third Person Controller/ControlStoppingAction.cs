using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Clase StateMachineBehaviour para adjuntar a ciertas animaciones y tener mayor control sobre ellas
public class ControlStoppingAction : StateMachineBehaviour
{
    private PlayerController player;

    // Sobreescribimos OnStateEnter, estado que empieza cuando empieza la animación
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) // Obtenemos acceso a la clase del Player...
            player = animator.GetComponent<PlayerController>();

        player.HasControl = false; // ... y le quitamos el control al empezar la animación
    }

    // Sobreescribimos OnStateExit, estado que empieza cuando termina la animación
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) // Obtenemos acceso a la clase del Player...
            player = animator.GetComponent<PlayerController>();

        player.HasControl = true;// ... y le devolvemos el control al terminar la animación
    }


}
