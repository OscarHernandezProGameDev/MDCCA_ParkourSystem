using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


// *Borrado el m�todo de Target Matching ya que despu�s de refactorizar, es Playercontroller quien se encarga de ello*
// *Borrado tambi�n referencias al animator ( se usaban en el m�todo de TargetMatching, ahora en PlayerController)*

//Clase con la l�gica y m�todos para ejecutar las acciones de Parkour
public class ParkourController : MonoBehaviour
{
    //Referencia a script de controles (PlayerInput)
    [SerializeField] private GatherInput gatherInput;

    //Referencias a acciones
    [SerializeField] private List<ParkourAction> parkourActions;// Acciones de parkour comunes
    [SerializeField] private List<ParkourAction> longParkourActions;// *A�adido* Acciones de parkour que necesitan espacio por encima/a lo largo del obst�culo
    [SerializeField] private ParkourAction[] vaultFenceActions;// *A�adido*  Acciones de pakour de salto de valla
    [SerializeField] private ParkourAction jumpDownAction,autoJumpDownAction;// *A�adido*  Acciones de parkour de salto abajo
    [SerializeField] private ParkourAction finalAction;

    // *A�adido* Umbral m�ximo para realizar un auto-salto hacia abajo
    [SerializeField] private float autoJumpHeightLimit = 1f;

    // *A�adido* �ngulo m�nimo para realizar los saltos hacia abajo
    [SerializeField] private float angleThreshold = 50f;

    private EnvironmentScanner scanner;
    private PlayerController playerController;

    // *A�adido* Booleana de seguridad para que no realice el autoJump 2 veces seguidas si seguimos pulsando una direcci�n (suced�a a pesar de quitarle el control), se reinicia al acabar la acci�n
    public bool AbleToAutoJump { get; set; }

    private void Awake() 
    {
        scanner = GetComponent<EnvironmentScanner>();
        playerController = GetComponent<PlayerController>();
        AbleToAutoJump = true;
    }

    private void Update()
    {
        //Comprobamos si tenemos un obst�culo delante y de tenerlo, lo almacenamos en hitData
        var hitData = scanner.ObstacleCheck();

        //Si no estamos realizando una acci�n, ni estamos agarrados y pulsamos saltar...
        if (gatherInput.tryToJump && !playerController.InAction && !playerController.IsHanging)
        {
            if (hitData.forwardHitFound)//Si ha encontrado un obst�culo...
            {
                //*A�adido* ...Primero comprobamos si tiene la etiqueta de fence, si la tiene...
                if (hitData.forwardHit.transform.CompareTag("Fence"))
                {
                    ChooseVaultFenceAction(hitData);

                } //*A�adido* Tambien comprobramos si tiene la etiqueta Final Action
                else if (hitData.forwardHit.transform.CompareTag("FinalAction"))
                {
                    if (finalAction.CheckIfPossible(hitData, transform))//*A�adido* en cuyo caso, ejecutamos directamente final Action Parkour
                    {
                        StartCoroutine(DoParkourAction(finalAction));
                        gatherInput.tryToJump = false;
                    }
                }
                else // si no tiene la etiqueta de Fence o de FinalAction...
                {
                    //...Comprobamos si el raycast secundario confirma que hay espacio para acciones "largas"...
                    if (hitData.secondForwardHitFound)
                    {
                        //Si no hay espacio, usamos las acciones "normales"
                        foreach (var action in parkourActions)//...Recorremos las acciones y ejecutamos seg�n par�metros
                        {
                            if (action.CheckIfPossible(hitData, transform))
                            {
                                StartCoroutine(DoParkourAction(action));
                                gatherInput.tryToJump = false;
                                break;
                            }
                        }
                    }
                    else //Si hay espacio, usamos las acciones largas
                    {
                        foreach (var action in longParkourActions)//...Recorremos las acciones largas y ejecutamos seg�n par�metros
                        {
                            if (action.CheckIfPossible(hitData, transform))
                            {
                                StartCoroutine(DoParkourAction(action));
                                gatherInput.tryToJump = false;
                                break;
                            }
                        }
                    }

                   
                }
            }
        }

        // *Actualizado* Saltos bajos hacia abajo =>
           // Si el jugador est� en un saliente, no est� realizando ninguna acci�n y no detecta ning�n obst�culo delante suyo...
        if (playerController.IsOnLedge && !playerController.InAction && !hitData.forwardHitFound)
        {
            // Comprobamos si la altura es menor que el umbral para que realice el salto (hacia abajo) autom�tico,si el �ngulo entre el jugador y el "vac�o" es menor que el umbral proporcionado y AutoJump es verdadero...
            if (playerController.LedgeData.height <= autoJumpHeightLimit && playerController.LedgeData.angle <= angleThreshold && AbleToAutoJump)                //...realizamos Auto-salto hacia abajo
            {
                AbleToAutoJump = false; //Medida de seguridad para que no vuelva a saltar antes de empezar a caer, lo reiniciamos desde Player Controller al terminar DoAction
                playerController.IsOnLedge = false;
                StartCoroutine(DoParkourAction(autoJumpDownAction));

            }    // Si la altura es mayor que el umbral para el autosalto,el �ngulo entre el jugador y el "vac�o" es menor que el umbral proporcionado y presionamos saltar, realizamos salto hacia abajo
            else if(playerController.LedgeData.height > autoJumpHeightLimit && playerController.LedgeData.angle <= angleThreshold && gatherInput.tryToJump)
            {
                playerController.IsOnLedge = false;
                StartCoroutine(DoParkourAction(jumpDownAction));

                // Reiniciamos valor de salto para obligar al jugador a soltar espacio antes de usar salto otra vez
                if (gatherInput.tryToJump)
                    gatherInput.tryToJump = false;
            }
        }

        //Viejo m�todo para autosaltar
        /* if(playerController.IsOnLedge && !playerController.InAction && !hitData.forwardHitFound)
         {
             bool shouldJump = true;

             if(playerController.LedgeData.height > autoJumpHeightLimit && !gatherInput.tryToJump) 
                 shouldJump = false;

             if(shouldJump && playerController.LedgeData.angle <= 50 )
             {
                 playerController.IsOnLedge = false;
                 StartCoroutine(DoParkourAction(autoJumpDownAction));

                 if(gatherInput.tryToJump)
                     gatherInput.tryToJump = false;
             }
         }*/
    }


    // *A�adido* M�todo para elegir Vault Fence
    private void ChooseVaultFenceAction(EnvironmentScanner.ObstacleHitData hitData)
    {
        //...Comprobamos en que parte de ese fence estamos chocando
        var hitPoint = hitData.forwardHit.transform.InverseTransformPoint(hitData.forwardHit.point);

        // Creamos una accion vac�a antes de seleccionar una aleatoriamente
        ParkourAction selectedAction = null;

        // Si el jugador est� en el centro del obst�culo, usaremos VaultFenceAlternative2
        if (hitPoint.x > -0.2f && hitPoint.x < 0.2f)
        {
            selectedAction = vaultFenceActions[2]; // VaultFenceAlternative2
        }
        else // Si est� en alguno de los extremos, elegir entre VaultFence o VaultFenceAlternative1
        {
            bool useAlternative1 = Random.value > 0.5f;//Valor aleatorio
            selectedAction = useAlternative1 ? vaultFenceActions[0] : vaultFenceActions[1];//Elecci�n
        }

        //Ejecuta la acci�n de Vault Fence elegida
        if (selectedAction != null && selectedAction.CheckIfPossible(hitData, transform))
        {
            StartCoroutine(DoParkourAction(selectedAction));
            gatherInput.tryToJump = false;
        }
    }


    // Corrutina que toma los Parkour y Custom Actions que le pasemos para ejecutarlos
    private IEnumerator DoParkourAction(ParkourAction action)
    {
        // Le quitamos el control al jugador
        playerController.SetControl(false);

        // Creamos variable para los par�metros del target matching
        MatchTargetParams matchParams = null;

        // Si target Matching est� activado desde su Parkour/Custom Action...
        if(action.EnableTargetMatching)
        {
            matchParams = new MatchTargetParams() // Rellenamos los valores con los proporcionados en el Parkour/Custom Action
            {
                pos = action.MatchPosition,
                bodyPart = action.MatchBodyPart,
                posWeight = action.MatchPosWeight,
                startTime = action.MatchStartTime,
                targetTime = action.MatchTargetTime
            };
        }

        //Llamamos a DoAction para que realice todo el proceso de animaci�n + target Matching + extras
        yield return playerController.DoAction(action.AnimName, matchParams, action.TargetRotation,
            action.RotateToObstacle, action.PostActionDelay, action.Mirror);

        // Cuando haya acabado la acci�n, le devolvemos el control a player
        playerController.SetControl(true);
      
    }

}
