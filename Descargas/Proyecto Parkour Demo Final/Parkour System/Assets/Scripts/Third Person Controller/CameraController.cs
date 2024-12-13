using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Referencia a nuestro recolector de inputs
    [SerializeField] private GatherInput gatherInput;

    // Referencia de objetivo a seguir
    [SerializeField] private Transform target;

    // Velocidad de rotaci�n con rat�n
    [SerializeField] private float rotationSpeedMouse = 12f;

    // Velocidad de rotaci�n con gamepad
    [SerializeField] private float rotationSpeedGamepad = 120f;

    // Distancia entre el jugador y la c�mara
    [SerializeField] private float distance = 5f;

    // L�mite inferior del �ngulo Y
    [SerializeField] private float minYAngle = -20f;

    // L�mite superior del �ngulo Y
    [SerializeField] private float maxYAngle = 45f;

    // Desplazamiento de c�mara respector al jugador
    [SerializeField] private Vector2 offset;

    // Booleanas para invertir ejes X o Y
    [SerializeField] private bool invertX,invertY;

    // Rotaciones separadas X e Y
    private float rotationY,rotationX;

    // *A�adido* para evitar clipping con la c�mara al acercarnos a mallas.
    [SerializeField] private float collisionRadius = 0.5f; // Radio para detectar colisiones
    [SerializeField] private LayerMask collisionLayers;   // Capas con las que la c�mara colisionar�
    private Vector3 currentCameraPosition;               // Posici�n actual de la c�mara, lo usaremos para actualizar la posici�n de la c�mara en caso de "chocar" con algo


    private void Start()
    {
        // Hacemos al cursor invisible
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        // Invertimos los valores seg�n las booleanas correspondientes
        float invertValueX = invertX ? -1 : 1;
        float invertValueY = invertY ? -1 : 1;

        // Usamos una velocidad u otra seg�n si estamos usando gamePad.
        float rotationSpeed = gatherInput.usingGamepad ? rotationSpeedGamepad : rotationSpeedMouse;
     
        // Almacenamos los valores resultantes de cada eje teniendo en cuenta en input, los valores de invertir y la velocidad por el tiempo.
        rotationX += gatherInput.lookInput.y * invertValueY * rotationSpeed * Time.deltaTime;
        rotationY += gatherInput.lookInput.x * invertValueX * rotationSpeed * Time.deltaTime;

        // Aplicamos las restricciones en Y a la c�mara
        rotationX = Mathf.Clamp(rotationX, minYAngle, maxYAngle);

        // Almacenamos la rotaci�n actualizada en un Quaternion.Euler
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        // Almacenamos en focusPosition la posici�n del objetivo m�s los desplazamientos
        var focusPosition = target.position + new Vector3(offset.x, offset.y);

        // Calculamos la posici�n ideal de la c�mara (posici�n + rotaci�n + distancia)
        Vector3 desiredCameraPosition = focusPosition - targetRotation * new Vector3(0, 0, distance);

        // *A�adido* => Detectar colisiones entre el target y la c�mara para evitar meternos dentro de mallas
     
                
        if (Physics.SphereCast(focusPosition, collisionRadius, (desiredCameraPosition - focusPosition).normalized, out var hit, distance, collisionLayers))
        {
            // Si hay una colisi�n, ajusta la posici�n de la c�mara
            float adjustedDistance = hit.distance - collisionRadius; 
            currentCameraPosition = focusPosition - targetRotation * new Vector3(0, 0, adjustedDistance);
        }
        else
        {
            // Si no hay colisi�n, usamos la posici�n deseada
            currentCameraPosition = desiredCameraPosition;
        }

        // Finalmente aplicamos la posici�n y rotaci�n actualizadas
        transform.SetPositionAndRotation(currentCameraPosition, targetRotation);

        // Vieja l�nea para aplicar posici�n/rotaci�n
        // transform.SetPositionAndRotation(focusPosition - targetRotation * new Vector3(0, 0, distance), targetRotation);
    }

    // Propiedad p�blica para exponer el valor de la rotacion en Y
    public Quaternion GetYRotation => Quaternion.Euler(0,rotationY,0);


}
