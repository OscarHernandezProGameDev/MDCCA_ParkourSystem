using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnvironmentScanner;

// ScriptableObject que define las acciones de parkour, incluyendo información sobre la animación, alturas de obstáculos,
// rotación del jugador hacia el obstáculo, y parámetros de Target Matching para hacer coincidir el movimiento y la posición.

[CreateAssetMenu(menuName = "Parkour System/New Parkour Action")]
public class ParkourAction : ScriptableObject
{
    [Header("Action Info")]
    [SerializeField] private string animName; // Nombre de la animación => Debe ser exacta a la del animator
    [SerializeField] private string obstacleTag; // Etiqueta de obstáculo, si tiene
    [SerializeField] private float minHeight; // Altura mínima para ejecutar la acción
    [SerializeField] private float maxHeight; //Altura máxima para ejecutar la acción
    [SerializeField] private bool rotateToObstacle; // ¿Debe rotar hacia el obstáculo?
    [SerializeField] private float postActionDelay; // Para animaciones combinadas, añadir un delay antes de devolverle el control (sumar longitud de la siguiente animación)


    [Header("Target Matching")]
    [SerializeField] private bool enableTargetMatching = true; // ¿Queremos que ejecute el target matching? por defecto decimos que si
    [SerializeField] private bool isLongAction = false; // *Añadido* ¿Se trata de una acción "larga"? (Necesita espacio para ejecutarla)
    [SerializeField] private protected AvatarTarget matchBodyPart; // Parte del cuerpo a usar en el Target Matching
    [SerializeField] private float matchStartTime; // Valor start del Target Matching
    [SerializeField] private float matchTargetTime;// Valor Target del Target Matching
    [SerializeField] private Vector3 matchPosWeight = new Vector3(0, 1, 0); // Vector 3 para añadir pesos deseados del target MAtching, por defecto solo en altura
    [SerializeField] private Vector3 extraOffset;

    //Rotación a hacer coincidir antes de realizar el target matching
    public Quaternion TargetRotation { get; set; } 

    //Posición a hacer coincidir en el target matching
    public Vector3 MatchPosition { get; set; }

    //Booleana para especificarle si debemos usar Mirror en el animator (la usamos en la Custom Action de VaultAction)
    public bool Mirror {  get; set; }



    public virtual bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        // Comprobamos que si la acción no tiene ninguna etiqueta en obstacleTag y si el objeto tiene una etiqueta diferente a la proporcionada en obstacleTag, si ambas son negativas, negamos la acción
        if(!string.IsNullOrEmpty(obstacleTag) && hitData.forwardHit.transform.tag != obstacleTag)                                                                   
            return false;

        // Comprobamos la altura del obstáculo con el que hemos chocado restando su punto más alto con nuestra posición Y
        float height = hitData.heightHit.point.y - player.position.y;

        //Si la altura del obstáculo no coincide con los valores mínimos y máximos de nuestra acción de parkour, negamos la acción
        if (height < minHeight || height > maxHeight)
        {
            return false;
        }

        // ¿Obligamos al jugador a rotar hacia el obstáculo? Si es así, le pasamos la dirección contraria a la normal del obstáculo (es decir,la dirección hacia el obstáculo)
        if (rotateToObstacle)
            TargetRotation = Quaternion.LookRotation(-hitData.forwardHit.normal);

        // ¿Está habilitado el target matching? si es así, tomaremos la posición a hacer coincidir en el método target matching, le sumamos el offset proporcionado (normalmente 0)
        if (enableTargetMatching)
        {
            if(!isLongAction) //Si no es long action, tomamos el height Hit principal..
                MatchPosition = hitData.heightHit.point + extraOffset;
            else              //Si lo es, tomamos el secundario
                MatchPosition = hitData.secondHeightHit.point + extraOffset;    
            
        }
        //Confirmamos que es posible
        return true;

    }

    //Propiedades para exponer las variables de Parkour Action
    public string AnimName => animName;
    public bool RotateToObstacle => rotateToObstacle;
    public float PostActionDelay => postActionDelay;
    public Vector3 MatchPosWeight => matchPosWeight;
    public bool EnableTargetMatching => enableTargetMatching ;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;

}
