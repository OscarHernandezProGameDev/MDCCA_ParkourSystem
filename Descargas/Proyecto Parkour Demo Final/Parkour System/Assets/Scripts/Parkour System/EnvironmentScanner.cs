using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Esta clase se encarga de escanear el entorno alrededor del jugador, detectando obstáculos y salientes
public class EnvironmentScanner : MonoBehaviour
{
    // Desplazamiento para rayo delantero, ligeramente elevado 
    [SerializeField] private Vector3 forwardRayOffset = new Vector3(0,0.5f,0);

    // Longitud de raycast delantero (forward), usado para detectar obstáculos frente al jugador
    [SerializeField] private float forwardRayLength = 0.8f;

    // *Añadido* Desplazamiento adicional para el segundo raycast delantero (usado para acciones "largas", que requieren cierto espacio)
    [SerializeField] private float secondForwardRayOffsetY = 1.0f;

    // *Añadido* Longitud adicional para del segundo raycast delantero
    [SerializeField] private float secondForwardRayLength = 1.5f;

    // Longitud del raycast vertical(hacia abajo) para medir la altura de un obstáculo
    [SerializeField] private float heightRayLength = 5f;

    // Longitud de rayo para detectar salientes (por debajo nuestro)
    [SerializeField] private float ledgeRayLength = 10f;

    // Longitud de rayo para detectar salientes escalables (por encima nuestro)
    [SerializeField] private float climbLedgeRayLength = 1.5f;

    // Layer de obstáculos
    [SerializeField] private LayerMask obstacleLayer;

    // Layer de salientes de escalada
    [SerializeField] private LayerMask climbLedgeLayer;

    // Umbral de altura para considerar un borde como un saliente 
    [SerializeField] private float ledgeHeightThreshold = 0.75f;

    // *Añadido* Usamos esto en PlayerController para restringir saltos normales en casos de haber obstáculos, permitiendo así realizar acciones de parkour
    public bool InFrontOfObstacle { get; private set; }


   
    // Función para detectar si hay obstáculos enfrente de player. Adicionalmente, si los hay, comprobamos si hay más obstáculos en la misma dirección pero mas lejanos (Para saber si podemos realizar un salto grande)
    public ObstacleHitData ObstacleCheck()
    {
        //Nueva instancia de ObstacleHitData
        var hitData = new ObstacleHitData();
         
        // Definimos origen del raycast delantero
        var forwardOrigin = transform.position + forwardRayOffset;
        
        //Lanzamos raycast hacia adelante y lo almacenamos en forwardHitFound
        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward,
                                              out hitData.forwardHit, forwardRayLength, obstacleLayer);
        
        //Dibujado del rayo delantero
        Debug.DrawRay(forwardOrigin, transform.forward * forwardRayLength, (hitData.forwardHitFound) ? Color.green : Color.red);


        if (hitData.forwardHitFound)// Si golpeamos con algo en la parte delantera...
        {
            //...*Añadido* Confirmamos que estamos enfrente de un obstáculo (lo usamos para el salto normal en PlayerController)
            InFrontOfObstacle = true;

            // *Añadido* Añadimos un desplazamiento minúsculo hacia adelante para asegurarnos 100% de que el rayo en altura choque con el obstáculo 
            Vector3 forwardOffset = transform.forward * 0.001f; 

            // Definimos origen de raycast en altura
            var heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLength + forwardOffset;

            // Lanzamos raycast en altura y lo almacenamos en heightHitFound
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down,
                                                    out hitData.heightHit, heightRayLength, obstacleLayer);
           
            // Dibujado del rayo en altura
            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength, (hitData.heightHitFound) ? Color.green : Color.red);

            //...*Añadido* también lanzamos otro rayo más lejano desde una posición más alta para confirmar si podemos usar acciones de "larga distancia", que requieren espacio por encima del obstáculo

            // *Añadido* Definimos origen de raycast secundario delantero
            var secondForwardOrigin = transform.position + forwardRayOffset + Vector3.up * secondForwardRayOffsetY;

            // *Añadido* Lanzamos raycast secundario delantero
            hitData.secondForwardHitFound = Physics.Raycast(secondForwardOrigin, transform.forward,
                                                            out hitData.secondForwardHit, secondForwardRayLength, obstacleLayer);

            // Lo dibujamos
            Debug.DrawRay(secondForwardOrigin, transform.forward * secondForwardRayLength, (hitData.secondForwardHitFound) ? Color.yellow : Color.blue);


            // *Añadido* Lanzamos un rayo secundario en altura, para definir el point de donde vamos a caer, y así pasarle el valor a targetTime del TargetMatching

            if (!hitData.secondForwardHitFound)
            {
                // Definir el origen del raycast en altura desde el punto de impacto del segundo rayo delantero
                var secondaryHeightOrigin = secondForwardOrigin + (transform.forward * secondForwardRayLength);
                
                // Lanzamos el rayo desde esta nueva altura hacia abajo
                hitData.secondHeightHitFound = Physics.Raycast(secondaryHeightOrigin, Vector3.down,
                                                         out hitData.secondHeightHit, heightRayLength, obstacleLayer);
               
                Debug.DrawRay(secondaryHeightOrigin, Vector3.down * heightRayLength,
                              (hitData.secondHeightHitFound) ? Color.cyan : Color.magenta);
            }
        }
        else // *Añadido* Si no chocamos con nada, configuramos booleanas a falso, ni hay obstáculo (lo usamos para el salto normal en PlayerController)
        {
            InFrontOfObstacle = false;
        }

        return hitData;
    }

    // Método para detectar si hay salientes para escalar
    public bool ClimbLedgeCheck(Vector3 direction, out RaycastHit ledgeHit)
    {
        // Inicializamos la data del raycast
        ledgeHit = new RaycastHit();

        // Si la dirección es cero, no realizamos el chequeo
        if (direction == Vector3.zero)
            return false;

        // Definimos el origen del raycast, ligeramente por encima del jugador
        var origin = transform.position + Vector3.up * 1.5f;

        // Definimos un pequeño desplazamiento vertical para lanzar los múltiples raycasts
        var offset = new Vector3(0,0.18f,0);

        // Lanzamos 10 raycasts desde diferentes posiciones verticales para detectar un saliente
        for (int i = 0; i < 10; i++)
        {
            // Dibujamos el raycast en la escena para depuración
            Debug.DrawRay(origin + offset * i, direction * climbLedgeRayLength);

            // Si alguno de los raycasts golpea un saliente escalable, confirmamos que tenemos un saliente para escalar
            if (Physics.Raycast(origin + offset * i,direction, out RaycastHit hit, climbLedgeRayLength, climbLedgeLayer))
            {
                ledgeHit = hit;
                return true;
            }
        }
        // Si no se detecta ningún saliente, devolver false
        return false;
    }

    // Método para detectar si hay un saliente al que el jugador puede descolgarse
    public bool DropLedgeCheck(out RaycastHit ledgeHit)
    {
        //Inicializamos la data del raycast
        ledgeHit = new RaycastHit();

        // Definimos el origen del raycast, un poco debajo y delante del jugador
        var origin = transform.position + Vector3.down * 0.1f + transform.forward * 0.2f;
       
        // Dibujamos el raycast en la escena para depuración
        Debug.DrawRay(origin, transform.forward, Color.red);

        // Si el raycast golpea un saliente, confirmamos que hay un saliente
        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, 0.8f, climbLedgeLayer))
        {
            ledgeHit = hit; 
            return true;
        }
        // Si no se detecta un saliente, devolvemos false
        return false;
    }

    // Método para detectar si el jugador está sobre un saliente
    public bool ObstacleLedgeCheck(Vector3 moveDirection, out LedgeData ledgeData)
    {  
        // Inicializamos los datos del saliente
        ledgeData = new LedgeData();

        // Si la dirección es cero, no realizamos el chequeo
        if (moveDirection == Vector3.zero) 
            return false;

        // Definimos el origen del raycast, desplazado desde la posición del jugador en la dirección del movimiento
        var originOffset = 0.65f;
        var origin = transform.position + moveDirection * originOffset + Vector3.up;

        // Usamos el método ThreeRaycasts de PhysicsUtil para lanzar múltiples raycasts hacia abajo
        if (PhysicsUtil.ThreeRaycasts(origin, Vector3.down,0.25f,transform,
                                        out List <RaycastHit> hits, ledgeRayLength, obstacleLayer,true))
        {
            // Filtramos los impactos que cumplen con el umbral de altura para ser considerados un saliente
            var validHits = hits.Where(h => transform.position.y - h.point.y > ledgeHeightThreshold).ToList();

            // Si se detecta un saliente válido, almacenamos los datos y devolvemos true
            if (validHits.Count > 0)
            { 
                // Definimos el origen de la superficie, ajustando su altura
                var surfaceOrigin = validHits[0].point;
                surfaceOrigin.y = transform.position.y - 0.1f;

                // Lanzamos un raycast desde el origen de la superficie hacia la posición del jugador para verificar la normal de la superficie
                if (Physics.Raycast(surfaceOrigin, transform.position - surfaceOrigin, out RaycastHit surfaceHit, 2, obstacleLayer))
                {
                    // Dibujamos una línea de depuración desde el origen de la superficie hasta la posición del jugador
                    Debug.DrawLine(surfaceOrigin, transform.position, Color.cyan);
                   
                    // Calculamos la altura entre la posición del jugador y el punto de impacto
                    float height = transform.position.y - validHits[0].point.y;

                    // Almacenamos los datos del ángulo entre la dirección del jugador y la normal de la superficie
                    ledgeData.angle = Vector3.Angle(transform.forward, surfaceHit.normal);
                   
                    // Almacenamos la altura calculada
                    ledgeData.height = height;
                    
                    // Almacenamos el impacto de la superficie
                    ledgeData.surfaceHit = surfaceHit;

                    return true;
                }
            }
        }
        // Si no se detecta un saliente, devolvemos false
        return false;
    }

    // Estructura para almacenar los resultados de los raycasts de detección de obstáculos
    public struct ObstacleHitData
    {
        public bool forwardHitFound; // Usado para confirmar si el rayo golpea con algo en su sentido forward ( hacia adelante)
        public bool heightHitFound;  // Usado para confirmar si el rayo golpea con algo en altura

        public RaycastHit forwardHit; // Almacenamos la información del Raycast en caso de golpeo
        public RaycastHit heightHit;  // Almacenamos la información del Raycast en caso de golpeo

        //Extras para acciones con más distancia (acciones "largas")
        public bool secondForwardHitFound;
        public bool secondHeightHitFound;
        public RaycastHit secondForwardHit;
        public RaycastHit secondHeightHit;
    }

    // Estructura para almacenar los datos de un saliente
    public struct LedgeData
    {
        public float height; // Altura
        public float angle;  // Ángulo
        public RaycastHit surfaceHit; //Almacenamos la información del Raycast en caso de golpeo
    }


}
