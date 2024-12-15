using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GatherInput gatherInput;
    [SerializeField] private Transform target;

    [Header("Settings")]
    [SerializeField] private float rotationSpeedMouse = 5f;
    [SerializeField] private float rotationSpeedGamepad = 100f;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minYAangle = -45f;
    [SerializeField] private float maxYAngle = 45f;
    [SerializeField] private Vector2 offset;
    [SerializeField] private bool invertX, invertY;

    // para evitar clipping con la camara al acercanos a la mallas
    [Header("clipping")]
    [SerializeField] private bool preventingClippingCamera = true;
    [SerializeField] private float collisionRadius = 0.5f; // Radio para detectar colisiones
    [SerializeField] private LayerMask collisionLayers; // Capas con las que la cámara colisionara    

    private float rotationY, rotationX;

    private Vector3 currentCameraPosition;  // Posición actual de la cámara, lo usaremos para actualizar la posición de la cámara en caso de chocar con algo

    public Quaternion GetYRotation() => Quaternion.Euler(0, rotationY, 0);

    private void Start()
    {
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        // Invertimos los valores según las booleanas correspondientes
        float invertValueX = invertX ? -1 : 1;
        float invertValueY = invertY ? -1 : 1;

        // Usamos una velocidad u otra según si estamos usando gamePad.
        float rotationSpeed = gatherInput.usingGamePad ? rotationSpeedGamepad : rotationSpeedMouse;

        // Si leemos la posición del ratón tenemos que hacer un código diferente para el gamepad porque este da diferencias
        // Por tanto si leemos el delta del ratón tenemos un código único
        // if (gInput.usingGamePad)
        // {
        //     rotationX += gInput.lookInput.y * invertValueY * rotationSpeed * Time.deltaTime;
        //     rotationY += gInput.lookInput.x * invertValueX * rotationSpeed * Time.deltaTime;
        // }
        // else
        // {
        //     rotationX = gInput.lookInput.y * invertValueY * rotationSpeed;
        //     rotationY = gInput.lookInput.x * invertValueX * rotationSpeed;
        // }
        // Almacenamos los valores resultantes de cada eje teniendo en cuenta en input, los valores de invertir y la velocidad por el tiempo.
        rotationX += gatherInput.lookInput.y * invertValueY * rotationSpeed * Time.deltaTime;
        rotationY += gatherInput.lookInput.x * invertValueX * rotationSpeed * Time.deltaTime;

        // Aplicamos las restricciones en Y a la cámara
        rotationX = Mathf.Clamp(rotationX, minYAangle, maxYAngle);

        // Almacenamos la rotación actualizada en un Quaternion.Euler
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        // Almacenamos en focusPosition la posición del objetivo más los desplazamientos
        var focusPosition = target.position + new Vector3(offset.x, offset.y);

        // Calculamos la posición ideal de la cámara (posición + rotación + distancia)
        Vector3 desiredCameraPosition = focusPosition - targetRotation * new Vector3(0, 0, distance);

        if (preventingClippingCamera)
        {
            // Detectar colisiones entre el target y la cámara para evitar meternos dentro de mallas
            if (Physics.SphereCast(focusPosition, collisionRadius, (desiredCameraPosition - focusPosition).normalized, out var hit, distance, collisionLayers))
            {
                // Si hay una colisión, ajusta la posición de la cámara
                float adjustedDistance = hit.distance - collisionRadius;
                currentCameraPosition = focusPosition - targetRotation * new Vector3(0, 0, adjustedDistance);
            }
            else
            {
                // Si no hay colisión, usamos la posición deseada
                currentCameraPosition = desiredCameraPosition;
            }
        }
        else // si no activamos el comportamiento hacemos el código antiguo
            currentCameraPosition = desiredCameraPosition;

        // Finalmente aplicamos la posición y rotación actualizadas
        transform.SetPositionAndRotation(currentCameraPosition, targetRotation);

        // Vieja línea para aplicar posición/rotación
        //transform.transform.SetPositionAndRotation(focusPosition - targetRotation * new Vector3(0, 0, distance),
        //    targetRotation);
    }
}