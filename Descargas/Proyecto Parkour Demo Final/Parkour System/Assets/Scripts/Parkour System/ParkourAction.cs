using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnvironmentScanner;

// ScriptableObject que define las acciones de parkour, incluyendo informaci�n sobre la animaci�n, alturas de obst�culos,
// rotaci�n del jugador hacia el obst�culo, y par�metros de Target Matching para hacer coincidir el movimiento y la posici�n.

[CreateAssetMenu(menuName = "Parkour System/New Parkour Action")]
public class ParkourAction : ScriptableObject
{
    [Header("Action Info")]
    [SerializeField] private string animName; // Nombre de la animaci�n => Debe ser exacta a la del animator
    [SerializeField] private string obstacleTag; // Etiqueta de obst�culo, si tiene
    [SerializeField] private float minHeight; // Altura m�nima para ejecutar la acci�n
    [SerializeField] private float maxHeight; //Altura m�xima para ejecutar la acci�n
    [SerializeField] private bool rotateToObstacle; // �Debe rotar hacia el obst�culo?
    [SerializeField] private float postActionDelay; // Para animaciones combinadas, a�adir un delay antes de devolverle el control (sumar longitud de la siguiente animaci�n)


    [Header("Target Matching")]
    [SerializeField] private bool enableTargetMatching = true; // �Queremos que ejecute el target matching? por defecto decimos que si
    [SerializeField] private bool isLongAction = false; // *A�adido* �Se trata de una acci�n "larga"? (Necesita espacio para ejecutarla)
    [SerializeField] private protected AvatarTarget matchBodyPart; // Parte del cuerpo a usar en el Target Matching
    [SerializeField] private float matchStartTime; // Valor start del Target Matching
    [SerializeField] private float matchTargetTime;// Valor Target del Target Matching
    [SerializeField] private Vector3 matchPosWeight = new Vector3(0, 1, 0); // Vector 3 para a�adir pesos deseados del target MAtching, por defecto solo en altura
    [SerializeField] private Vector3 extraOffset;

    //Rotaci�n a hacer coincidir antes de realizar el target matching
    public Quaternion TargetRotation { get; set; } 

    //Posici�n a hacer coincidir en el target matching
    public Vector3 MatchPosition { get; set; }

    //Booleana para especificarle si debemos usar Mirror en el animator (la usamos en la Custom Action de VaultAction)
    public bool Mirror {  get; set; }



    public virtual bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        // Comprobamos que si la acci�n no tiene ninguna etiqueta en obstacleTag y si el objeto tiene una etiqueta diferente a la proporcionada en obstacleTag, si ambas son negativas, negamos la acci�n
        if(!string.IsNullOrEmpty(obstacleTag) && hitData.forwardHit.transform.tag != obstacleTag)                                                                   
            return false;

        // Comprobamos la altura del obst�culo con el que hemos chocado restando su punto m�s alto con nuestra posici�n Y
        float height = hitData.heightHit.point.y - player.position.y;

        //Si la altura del obst�culo no coincide con los valores m�nimos y m�ximos de nuestra acci�n de parkour, negamos la acci�n
        if (height < minHeight || height > maxHeight)
        {
            return false;
        }

        // �Obligamos al jugador a rotar hacia el obst�culo? Si es as�, le pasamos la direcci�n contraria a la normal del obst�culo (es decir,la direcci�n hacia el obst�culo)
        if (rotateToObstacle)
            TargetRotation = Quaternion.LookRotation(-hitData.forwardHit.normal);

        // �Est� habilitado el target matching? si es as�, tomaremos la posici�n a hacer coincidir en el m�todo target matching, le sumamos el offset proporcionado (normalmente 0)
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
