using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    [SerializeField] private GatherInput gatherInput;
    [Header("Offsets")]
    [SerializeField] private Vector3 handOffsetIdleToHang = new Vector3(0.3f, 0.05f, 0.1f);
    // 0.3, 0.07, 0.1
    [SerializeField] private Vector3 handOffsetJumpUp = new Vector3(0.3f, 0.06f, 0.09f);
    [SerializeField] private Vector3 handOffsetJumpDown = new Vector3(0.25f, 0.07f, 0.08f);
    [SerializeField] private Vector3 handOffsetJumpLeft = new Vector3(0.37f, 0.06f, 0.01f);
    [SerializeField] private Vector3 handOffsetJumpRight = new Vector3(0.34f, 0.06f, 0.05f);
    [SerializeField] private Vector3 handOffsetShimmyLeft = new Vector3(0.3f, 0.02f, 0.1f);
    [SerializeField] private Vector3 handOffsetShimmyRight = new Vector3(0.3f, 0.02f, 0.1f);
    [SerializeField] private Vector3 handOffsetDropToHang = new Vector3(0.3f, 0.02f, 0.1f);

    private EnvironmentScanner envScanner;
    private PlayerController playerController;

    private ClimbPoint currentPoint;

    private void Awake()
    {
        envScanner = GetComponent<EnvironmentScanner>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        // Si no estamos en estado "agarrado"...
        if (!playerController.IsHanging)
        {
            // ...y pulsamos saltar y no estamos realizando otra acción...
            if (gatherInput.tryToJump && !playerController.InAction && playerController.IsGrounded)
            {
                // Mandamos a escanear en busca de salientes de escalada delante nuestro
                if (envScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit))
                {
                    currentPoint = GetNearestPoint(ledgeHit.transform, ledgeHit.point);
                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("IdleToHang", currentPoint.transform, 0.41f, 0.54f, handOffset: handOffsetIdleToHang));
                    gatherInput.tryToJump = false;
                }
            }

            // Si no estamos en estado "agarrado" y pulsamos soltar (drop) y no estamos realizando otra acción...
            if (gatherInput.tryToDrop && !playerController.InAction && playerController.IsGrounded)
            {
                // Escáneamos por busca de salientes por delante y debajo nuestro con DropLedgeCheck
                if (envScanner.DropLedgeCheck(out RaycastHit ledgeHit))
                {
                    currentPoint = GetNearestPoint(ledgeHit.transform, ledgeHit.point);
                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("DropToHang", currentPoint.transform, 0.30f, 0.45f, handOffset: handOffsetDropToHang));
                    gatherInput.tryToDrop = false;
                }
            }
        }
        else // Si estamos en estado "agarrado"...
        {
            // Ledge to ledge
            // ...y pulsamos drop y no estamos realizando otra acción...
            if (gatherInput.tryToDrop && !playerController.InAction)
            {
                StartCoroutine(JumpFromHang());
                gatherInput.tryToDrop = false;

                return;
            }

            // Obtenemos la dirección (en ejes separados) a la que el jugador pretende ir
            var h = Mathf.Round(gatherInput.Direction.x);
            var v = Mathf.Round(gatherInput.Direction.y);
            // La almacenamos en un Vector2
            var inputDir = new Vector2(h, v);

            // Si player está realizando una acción o inputDir es nulo(no pretende ir hacia ninguna dirección), devolvemos el método
            if (playerController.InAction || inputDir == Vector2.zero)
                return;

            // Si el ClimbPoint actual es un MountPoint y pulsamos dirección arriba, nos montamos encima del obstáculo
            if (currentPoint.MountPoint && inputDir.y == 1)
            {
                StartCoroutine(MountFromHang());

                return;
            }

            // Obtenemos el ClimbPOint más cercano hacia la dirección que pretendemos movernos y la almacenamos como "vecino"
            var neighbour = currentPoint.GetNeighbour(inputDir);

            // Si no tenemos "vecinos" en esa dirección, devolvemos el método y no hacemos nada
            if (neighbour == null)
                return;

            // Si tenemos un vecino cuya conexión es 'Jump' y pulsamos saltar...
            if (neighbour.type == ConnectionType.Jump && gatherInput.tryToJump)
            {
                //...Cambiamos el current Point con el ClimbPoint al que estamos a punto de cambiar
                currentPoint = neighbour.point;

                // Manejamos las direcciones disponibles... (en nuestro caso, 4 => Si contamos con las animaciones, podemos añadir
                //direcciones extras aquí)
                // ...y ejecutamos los saltos correspondientes según dirección
                if (neighbour.direction.y == 1)
                    StartCoroutine(JumpToLedge("HangHopUp", currentPoint.transform, 0.34f, 0.65f, handOffset: handOffsetJumpUp));
                else if (neighbour.direction.y == -1)
                    StartCoroutine(JumpToLedge("HangDropDown", currentPoint.transform, 0.31f, 0.65f, handOffset: handOffsetJumpDown));
                else if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("HangHopRight", currentPoint.transform, 0.20f, 0.42f, handOffset: handOffsetJumpRight));
                else if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("HangHopLeft", currentPoint.transform, 0.20f, 0.42f, handOffset: handOffsetJumpLeft));
                // No resetamos para que el jugador pueda saltar de saliente en saliente sin soltar la tecla de salto
                //gatherInput.tryToJump = false;
            }
            else if (neighbour.type == ConnectionType.Move) // Si tenemos un vecino cuya conexión es 'Move'...
            {
                currentPoint = neighbour.point;

                // ...y ejecutamos los movimientos correspondientes según dirección
                // 0.3, 0, 0.1
                if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform, 0.0f, 0.38f, handOffset: handOffsetShimmyRight));
                else if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, 0.0f, 0.38f, AvatarTarget.LeftHand, handOffset: handOffsetShimmyLeft));
            }
        }
    }

    IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime, AvatarTarget hand = AvatarTarget.RightHand, Vector3? handOffset = null)
    {
        var matchParams = new MatchTargetParams
        {
            pos = GetHandPosition(ledge, hand, handOffset),
            bodyPart = hand,
            startTime = matchStartTime,
            targetTime = matchTargetTime,
            posWeight = Vector3.one
        };
        // rota para mirar justo al saliente
        var targetRotation = Quaternion.LookRotation(-ledge.forward);

        yield return playerController.DoAction(anim, matchParams, targetRotation, true);

        playerController.IsHanging = true;
    }

    private IEnumerator JumpFromHang()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("JumpFromHang");
        playerController.ResetTargetRotation();
        playerController.SetControl(true);
    }

    private IEnumerator MountFromHang()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("MountFromHang");

        // Activamos el collide para que el jugador no se hunda en la plataforma
        playerController.EnabledCharacterController(true);

        yield return new WaitForSeconds(0.5f);
        playerController.ResetTargetRotation();
        playerController.SetControl(true);
    }

    private Vector3 GetHandPosition(Transform ledge, AvatarTarget hand, Vector3? handOffset)
    {
        // 0.3, 0.07, 0.1
        var offsetValue = (handOffset.HasValue) ? handOffset.Value : new Vector3(0.3f, 0.05f, 0.1f);
        var horizontalDir = hand == AvatarTarget.RightHand ? ledge.right : -ledge.right;

        //return ledge.position + ledge.forward * 0.05f + Vector3.up * 0.05f - horizontalDir * 0.3f;
        return ledge.position + ledge.forward * offsetValue.z + Vector3.up * offsetValue.y - horizontalDir * offsetValue.x;
    }

    private ClimbPoint GetNearestPoint(Transform ledge, Vector3 hitPoint)
    {
        var points = ledge.GetComponentsInChildren<ClimbPoint>();

        ClimbPoint nearestPoint = null;
        float nearestPointDistance = Mathf.Infinity;

        foreach (var point in points)
        {
            var distance = Vector3.Distance(point.transform.position, hitPoint);

            if (distance < nearestPointDistance)
            {
                nearestPoint = point;
                nearestPointDistance = distance;
            }
        }

        return nearestPoint;
    }
}
