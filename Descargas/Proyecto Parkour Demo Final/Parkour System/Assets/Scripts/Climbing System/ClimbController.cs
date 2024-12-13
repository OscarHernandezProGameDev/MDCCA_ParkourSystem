using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Clase con la lógica y métodos para ejecutar acciones de escalada
public class ClimbController : MonoBehaviour
{
    // Referencias
    [SerializeField] private GatherInput gatherInput;
    private EnvironmentScanner envScanner;
    private PlayerController playerController;

    // El punto de escalada actual al que está agarrado el jugador
    private ClimbPoint currentPoint;

    [Header("Climb Offsets")] // Offsets para ajustar las manos en diferentes acciones de escalada
    [SerializeField] private Vector3 handOffsetIdleToHang = new Vector3(0.3f, -0.007f, 0.15f);
    [SerializeField] private Vector3 handOffsetJumpUp = new Vector3(0.3f, -0.007f, 0.15f);
    [SerializeField] private Vector3 handOffsetJumpDown = new Vector3(0.3f, -0.007f, 0.15f);
    [SerializeField] private Vector3 handOffsetJumpLeft = new Vector3(0.3f, -0.007f, 0.15f);
    [SerializeField] private Vector3 handOffsetJumpRight = new Vector3(0.3f, -0.007f, 0.15f);
    [SerializeField] private Vector3 handOffsetShimmyLeft = new Vector3(0.3f, -0.007f, 0.15f);
    [SerializeField] private Vector3 handOffsetShimmyRight = new Vector3(0.3f, -0.007f, 0.15f);
    [SerializeField] private Vector3 handOffsetDropToHang = new Vector3(0.3f, 0f, 0f);



    private void Awake()
    {
        envScanner = GetComponent<EnvironmentScanner>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        // Si no estamos en estado "agarrado"...
        if(!playerController.IsHanging)
        {       // ...y pulsamos saltar y no estamos realizando otra acción...
            if (gatherInput.tryToJump && !playerController.InAction)
            {
                    // Mandamos a escanear en busca de salientes de escalada delante nuestro
                if (envScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit))
                { 
                    // Obtenemos el ClimbPoint más cercano con el método GetNearestClimbPoint y lo almacenamos en currentPoint
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);

                    // Quitamos el control al jugador
                    playerController.SetControl(false);

                    // Mandamos ejecutar el salto para incorporarnos a un saliente desde Idle
                    StartCoroutine(JumpToLedge("IdleToHang", currentPoint.transform, 0.41f, 0.54f,handOffset : handOffsetIdleToHang));
                   
                    //Reiniciamos tryToJump por seguridad
                    gatherInput.tryToJump = false;
                }
            }

            // Si no estamos en estado "agarrado" y pulsamos soltar (drop) y no estamos realizando otra acción...
            if(gatherInput.tryToDrop && !playerController.InAction)
            {
                // Escáneamos por busca de salientes por delante y debajo nuestro con DropLedgeCheck
                if(envScanner.DropLedgeCheck(out RaycastHit ledgeHit))
                {
                    // Obtenemos el ClimbPoint más cercano con el método GetNearestClimbPoint y lo almacenamos en currentPoint
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);

                    // Quitamos el control al jugador
                    playerController.SetControl(false);
                    // Mandamos ejecutar la acción de descolgarse para incorporarnos a un saliente desde Idle
                    StartCoroutine(JumpToLedge("DropToHang", currentPoint.transform, 0.30f, 0.45f,handOffset: handOffsetDropToHang));

                    //Reiniciamos tryToJump por seguridad
                    gatherInput.tryToDrop = false;
                }
            }
        }
        else // Si estamos en estado "agarrado"...
        {
            // ...y pulsamos drop y no estamos realizando otra acción...
            if(gatherInput.tryToDrop && !playerController.InAction)
            {
                // Saltamos desde el saliente hasta el suelo
                StartCoroutine(JumpFromHang());

                // Reiniciamos tryToDrop por seguridad
                gatherInput.tryToDrop = false;

                return; // Devolvemos el método para que no ejecute nada más
            }

            // Obtenemos la dirección (en ejes separados) a la que el jugador pretende ir
            float h = Mathf.Round(gatherInput.Direction.x); 
            float v = Mathf.Round(gatherInput.Direction.y); 

            // La almacenamos en un Vector2
            var inputDir = new Vector2 (h, v);

            // Si player está realizando una acción o inputDir es nulo(no pretende ir hacia ninguna dirección), devolvemos el método
            if (playerController.InAction || inputDir == Vector2.zero) return;

            // Si el ClimbPoint actual es un MountPoint y pulsamos dirección arriba, nos montamos encima del obstáculo
            if(currentPoint.MountPoint && inputDir.y == 1) //            => Aquí podemos añadir que debemos pulsar saltar para subir
            {
                StartCoroutine(MountFromHang());
                return;
            }

            // Obtenemos el ClimbPOint más cercano hacia la dirección que pretendemos movernos y la almacenamos como "vecino"
            var neighbour = currentPoint.GetNeighbour(inputDir);
           
            // Si no tenemos "vecinos" en esa dirección, devolvemos el método y no hacemos nada
            if (neighbour == null) return; 

            // Si tenemos un vecino cuya conexión es 'Jump' y pulsamos saltar...
            if (neighbour.connectionType == ConnectionType.Jump && gatherInput.tryToJump) 
            {
                currentPoint = neighbour.point;//...Cambiamos el current Point con el ClimbPoint al que estamos a punto de cambiar

                // Manejamos las direcciones disponibles... (en nuestro caso, 4 => Si contamos con las animaciones, podemos añadir
                                                                                                             //direcciones extras aquí)
              // ...y ejecutamos los saltos correspondientes según dirección
                if (neighbour.direction.y == 1)
                   StartCoroutine(JumpToLedge("HangHopUp", currentPoint.transform, 0.34f, 0.65f,handOffset : handOffsetJumpUp));
                else if(neighbour.direction.y == -1)
                    StartCoroutine(JumpToLedge("HangHopDown", currentPoint.transform, 0.31f, 0.65f, handOffset: handOffsetJumpDown));
                else if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("HangHopRight", currentPoint.transform, 0.20f, 0.42f, handOffset: handOffsetJumpRight));
                else if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("HangHopLeft", currentPoint.transform, 0.20f, 0.42f, handOffset: handOffsetJumpLeft));
            }
            else if(neighbour.connectionType == ConnectionType.Move)// Si tenemos un vecino cuya conexión es 'Move'...
            {
                currentPoint = neighbour.point;//...Cambiamos el current Point con el ClimbPoint al que estamos a punto de cambiar

                // Manejamos las direcciones disponibles (izq. y der.)...

                // ...y ejecutamos los movimientos correspondientes según dirección
                if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform,0f,0.38f,handOffset: handOffsetShimmyLeft));
                else if( neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, 0f, 0.38f,AvatarTarget.LeftHand, 
                                                                                              handOffset: handOffsetShimmyRight));
            }
        }
    }

    //Método para ejecutar los movimientos de saliente a saliente correspondientes
    private IEnumerator JumpToLedge(string anim,Transform ledge,float matchStartTime, float matchTargetTime, 
                                   AvatarTarget hand = AvatarTarget.RightHand, Vector3? handOffset = null)
    {
        var matchParams = new MatchTargetParams()//Tomamos la data correspondiente del TargetMatching
        {
            pos = GetHandPosition(ledge,hand, handOffset), // Usamos GetHandPosition para elegir mano + ajuste de la misma
            bodyPart = hand,
            startTime = matchStartTime,
            targetTime = matchTargetTime,
            posWeight = Vector3.one
        };

        var targetRotation = Quaternion.LookRotation(-ledge.forward); // Rotamos el jugador hacia el saliente

        yield return playerController.DoAction(anim, matchParams, targetRotation, true);// Ejecutamos la animación

        playerController.IsHanging = true;// Cambiamos el estado a 'agarrado'
    }

    //Método para ejecutar un salto de un saliente al suelo
    private IEnumerator JumpFromHang()
    {
        playerController.IsHanging = false;// Salimos del estado 'agarrado'
        yield return playerController.DoAction("JumpFromHang");// Ejecutamos la acción y esperamos a que se complete con un yield return
        playerController.ResetTargetRotation();// Cuando se haya completado, reiniciamos la rotación con la rotación actual...
        playerController.SetControl(true); //... y le devolvemos el control a player
    }
   
    //Método para ejecutar la acción de montarnos encima del obstáculo
    private IEnumerator MountFromHang()
    {
        playerController.IsHanging = false;// Salimos del estado 'agarrado'
        yield return playerController.DoAction("MountFromHang");// Ejecutamos la acción y esperamos a que se complete con un yield return

        playerController.EnableCharacterController(true); // Activamos el collider antes de devolverle el control para evitar el 'bug'
                                                          // de meternos dentro de la malla ( el collider sujetará al player encima)  
        yield return new WaitForSeconds(0.5f); // Esperamos medio segundo...

        playerController.ResetTargetRotation();//. .. y reiniciamos rotación con la actual...
        playerController.SetControl(true);// ... y finalmente le devolvemos el control
    }

    // Método para seleccionar la mano adecuada según la dirección más aplicación de offsets
    private Vector3 GetHandPosition(Transform ledge, AvatarTarget hand, Vector3? handOffset)
     {
        // Si no le pasamos un valor a handOffset, usará este Vector3 por defecto
        var offsetValue = (handOffset != null) ? handOffset.Value : new Vector3(0.3f, 0.07f, 0.1f);
       
        // Según la dirección del ledge, usaremos la mano izquierda o derecha
        var horizontalDir = (hand == AvatarTarget.RightHand) ? ledge.right : -ledge.right;

        // Devolvemos la posición para hacer 'match' junto a los offsets de cada eje
        return ledge.position + ledge.forward * offsetValue.z + Vector3.up * offsetValue.y - horizontalDir * offsetValue.x;
     }

    // Método para obtener el climbPoint más cercano
    private ClimbPoint GetNearestClimbPoint(Transform ledge,Vector3 hitPoint)
    {
        // Recorremos los climb Points que tenga el saliente al que apuntamos
        var points = ledge.GetComponentsInChildren<ClimbPoint>();

        ClimbPoint nearestPoint = null; // Creamos Climb point vacío

        // Representación de un valor positivo muy alto (infinito)
        float nearestPointDistance = Mathf.Infinity;

        // Recorremos los climbPoints obtenidos...
        foreach (var point in points)
        {
            // ... y vamos comparandolos con el ledgeHit.Point que le pasamos del RaycastHit
            float distance = Vector3.Distance(point.transform.position, hitPoint);

            // Si esa distancia es menor que la guardada en 'punto de distancia + cercano'...
            if(distance < nearestPointDistance)
            {
                // Actualizamos Climb Point (+cercano) y 'nearestPointDistance' (para seguir comparando)
                nearestPoint = point;
                nearestPointDistance = distance;
            }
        }

        return nearestPoint; // Lo devolvemos como punto más cercano

    }

}
