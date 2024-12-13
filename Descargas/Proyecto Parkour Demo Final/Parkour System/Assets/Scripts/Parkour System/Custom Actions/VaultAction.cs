using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/Custom Actions/New vault action")]

// Clase (hija) personalizada de ParkourAction
public class VaultAction : ParkourAction
{
    // Sobreescribimos el m�todo CheckIfPossible de ParkourAction
    public override bool CheckIfPossible(EnvironmentScanner.ObstacleHitData hitData, Transform player)
    {
        // Llam�mos al m�todo base y si es falso, devolvemos falso inmediatamente...
        if(!base.CheckIfPossible(hitData, player))
            return false;

        //... de lo contrario, lo ampliamos para saber si debemos aplicar un mirror a las acciones de VaultFence

        //Almacenamos el punto donde chocamos con el obst�culo convertido de espacio local
        var hitPoint = hitData.forwardHit.transform.InverseTransformPoint(hitData.forwardHit.point);

        //Comprobamos si chocamos el obst�culo por la izquierda o la derecha y teniendo en cuenta la direcci�n (con hitPoint.z) para 
        if(hitPoint.z < 0 && hitPoint.x < 0 || hitPoint.z > 0 && hitPoint.x > 0)                              // aplicar el Mirror o no
        {
            //Aplica el espejo y usa la mano derecha en el matchBodyPart del Target Matching
            Mirror = true;
            matchBodyPart = AvatarTarget.RightHand;
        }
        else
        {
            //No apliques el espejo y usa la mano izquierda en el matchBodyPart del Target Matching
            Mirror = false;
            matchBodyPart = AvatarTarget.LeftHand;
        }

        //Devolvemos true para que ejecute la acci�n con la data ampliada de Mirror y matchBodyPart
        return true;

    }


}
