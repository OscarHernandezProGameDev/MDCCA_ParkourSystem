using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Referencia a nuestro recolector de inputs
    [SerializeField] private GatherInput gatherInput;

    // Referencia de objetivo a seguir
    [SerializeField] private Transform target;

    // Velocidad de rotación con ratón
    [SerializeField] private float rotationSpeedMouse = 12f;

    // Velocidad de rotación con gamepad
    [SerializeField] private float rotationSpeedGamepad = 120f;

    // Distancia entre el jugador y la cámara
    [SerializeField] private float distance = 5f;

    // Límite inferior del ángulo Y
    [SerializeField] private float minYAngle = -20f;

    // Límite superior del ángulo Y
    [SerializeField] private float maxYAngle = 45f;

    // Desplazamiento de cámara respector al jugador
    [SerializeField] private Vector2 offset;

    // Booleanas para invertir ejes X o Y
    [SerializeField] private bool invertX,invertY;

    // Rotaciones separadas X e Y
    private float rotationY,rotationX;

    // *Añadido* para evitar clipping con la cámara al acercarnos a mallas.
    [SerializeField] private float collisionRadius = 0.5f; // Radio para detectar colisiones
    [SerializeField] private LayerMask collisionLayers;   // Capas con las que la cámara colisionará
    private Vector3 currentCameraPosition;               // Posición actual de la cámara, lo usaremos para actualizar la posición de la cámara en caso de "chocar" con algo


    private void Start()
    {
        // Hacemos al cursor invisible
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        // Invertimos los valores según las booleanas correspondientes
        float invertValueX = invertX ? -1 : 1;
        float invertValueY = invertY ? -1 : 1;

        // Usamos una velocidad u otra según si estamos usando gamePad.
        float rotationSpeed = gatherInput.usingGamepad ? rotationSpeedGamepad : rotationSpeedMouse;
     
        // Almacenamos los valores resultantes de cada eje teniendo en cuenta en input, los valores de invertir y la velocidad por el tiempo.
        rotationX += gatherInput.lookInput.y * invertValueY * rotationSpeed * Time.deltaTime;
        rotationY += gatherInput.lookInput.x * invertValueX * rotationSpeed * Time.deltaTime;

        // Aplicamos las restricciones en Y a la cámara
        rotationX = Mathf.Clamp(rotationX, minYAngle, maxYAngle);

        // Almacenamos la rotación actualizada en un Quaternion.Euler
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        // Almacenamos en focusPosition la posición del objetivo más los desplazamientos
        var focusPosition = target.position + new Vector3(offset.x, offset.y);

        // Calculamos la posición ideal de la cámara (posición + rotación + distancia)
        Vector3 desiredCameraPosition = focusPosition - targetRotation * new Vector3(0, 0, distance);

        // *Añadido* => Detectar colisiones entre el target y la cámara para evitar meternos dentro de mallas
     
                
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

        // Finalmente aplicamos la posición y rotación actualizadas
        transform.SetPositionAndRotation(currentCameraPosition, targetRotation);

        // Vieja línea para aplicar posición/rotación
        // transform.SetPositionAndRotation(focusPosition - targetRotation * new Vector3(0, 0, distance), targetRotation);
    }

    // Propiedad pública para exponer el valor de la rotacion en Y
    public Quaternion GetYRotation => Quaternion.Euler(0,rotationY,0);


}
