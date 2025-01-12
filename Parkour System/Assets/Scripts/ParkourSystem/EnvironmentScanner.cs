using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnvironmentScanner : MonoBehaviour
{
    [Header("Obstacules")]
    // Desplazamiento para rayo delantero, ligeramente elevado 
    [SerializeField, Tooltip("Desplazamiento para rayo delantero, ligeramente elevado")] private Vector3 forwardOffset = new Vector3(0, 0.5f, 0);
    // altura del cuello
    [SerializeField, Tooltip("altura del cuello")] private float footToNeckHeight = 1f;
    // Longitud de raycast delantero (forward), usado para detectar obstáculos frente al jugador
    [SerializeField, Tooltip("Longitud de raycast delantero (forward), usado para detectar obstáculos frente al jugador")] private float forwardRayLength = 0.8f;
    // Longitud del raycast vertical(hacia abajo) para medir la altura de un obstáculo
    [SerializeField, Tooltip("Longitud del raycast vertical(hacia abajo) para medir la altura de un obstáculo")] private float heightRayLength = 5f;
    // Layer de obstáculos
    [SerializeField, Tooltip("Layer de obstáculos")] private LayerMask obstacleLayer;
    // Detecta si hay espacio suficiente para desplazarse
    [SerializeField, Tooltip("Detecta si hay espacio suficiente para desplazarse")] private float slideHeightRayOffsetY = 1.0f;
    // Longitud del raycast superior
    [SerializeField, Tooltip("Longitud del raycast superior")] private float slideHeightRayLength = 1.0f;

    [Header("Ledge")]
    // Longitud de rayo para detectar salientes (por debajo nuestro)
    [SerializeField, Tooltip("Longitud de rayo para detectar salientes (por debajo nuestro)")] private float ledgeRayLength = 10f;
    // Longitud de rayo para detectar salientes escalables (por encima nuestro)
    [SerializeField, Tooltip("Longitud de rayo para detectar salientes escalables (por encima nuestro)")] private float climbLedgeRayLength = 1.5f;
    // distancia para que no salga del saliente
    [SerializeField, Tooltip("distancia para que no salga del saliente")] private float originOffset = 0.65f;
    // Threshold = límite
    // Umbral de altura para considerar un borde como un saliente 
    [SerializeField, Tooltip("Umbral de altura para considerar un borde como un saliente ")] private float ledgeHeightThreshold = 0.75f;
    // Espacios entre rayos para saber si el obstaculo es escalable
    [SerializeField, Tooltip("Espacios entre rayos para saber si el obstaculo es escalable")] private float ledgeSpacing = 0.25f;
    // Layer de salientes de escalada
    [SerializeField, Tooltip("Layer de salientes de escalada")] private LayerMask climbLedgeLayer;

    // Usamos esto en PlayerController para restringir saltos normales en casos de haber obstáculos, permitiendo así realizar acciones de parkour
    [field: SerializeField, Header("Debug")] public bool InFrontOfObstacle { get; private set; }

    public ObstacleHitData ObstacleCkech()
    {
        var hitData = new ObstacleHitData();
        Vector3 forwardOrigin = transform.position + forwardOffset;

        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward, out hitData.forwardHit, forwardRayLength, obstacleLayer);

        Debug.DrawRay(forwardOrigin, transform.forward * forwardRayLength, hitData.forwardHitFound ? Color.green : Color.red);

        // origen del nuevo rayo más elevado para la acción de slide
        var upperOrigin = transform.position + forwardOffset + Vector3.up * slideHeightRayOffsetY;
        bool lowerHit = hitData.forwardHitFound;
        // lanzamos el rayo superior
        bool upperHit = Physics.Raycast(upperOrigin, transform.forward, out hitData.slideHit, slideHeightRayLength, obstacleLayer);

        Debug.DrawRay(upperOrigin, transform.forward * slideHeightRayLength, upperHit ? Color.green : Color.red);

        // Si no hay nada por debajo y arriba
        hitData.canSlide = !lowerHit && upperHit;

        // Si no golpeamos con algo en la parte delantera
        if (!hitData.forwardHitFound)
        {
            // Rango de angulo`para rayos adicionales
            float angleStep = 10f; // angulo entre cada rayo
            float maxAngle = 40f; // angulo maximo a la izquierda y a la derecha
            int raysPerSide = (int)(maxAngle / angleStep); // Cantidad de rayos por lado

            for (int i = -raysPerSide; i <= raysPerSide; i++)
            {
                // rayo centro no lo comprobamos porque es la primera comprobacion que se sido más arriba
                if (i == 0)
                    continue;

                Vector3 direction = Quaternion.Euler(0, i * angleStep, 0) * transform.forward; // Rayo hacia la derecha o izquierda

                Debug.DrawRay(forwardOrigin, direction * forwardRayLength, Color.yellow);

                if (Physics.Raycast(forwardOrigin, direction, out var hit, forwardRayLength, obstacleLayer))
                {
                    hitData.forwardHitFound = true; // Si golpeamos con algo en la parte delantera, lo guardamos en hitDa
                    hitData.forwardHit = hit;
                    Debug.DrawRay(forwardOrigin, direction * forwardRayLength, Color.green);

                    break;
                }
            }
        }

        if (hitData.forwardHitFound)
        {
            // Confirmamos que estamos enfrente de un obstáculo (lo usamos para el salto normal en PlayerController)
            InFrontOfObstacle = true;

            // Añadimos un desplazamiento minúsculo hacia adelante para asegurarnos 100% de que el rayo en altura choque con el obstáculo 
            Vector3 forwardOffset = transform.forward * 0.001f;

            // Definimos origen de raycast en altura
            var heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLength + forwardOffset;

            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down, out hitData.heightHit, heightRayLength,
                obstacleLayer);

            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength,
                hitData.heightHitFound ? Color.green : Color.red);
        }
        else // *Añadido* Si no chocamos con nada, configuramos booleanas a falso, ni hay obstáculo (lo usamos para el salto normal en PlayerController)
        {
            InFrontOfObstacle = false;
        }

        return hitData;
    }

    public bool ClimbLedgeCheck(Vector3 direction, out RaycastHit ledgeHit)
    {
        ledgeHit = new RaycastHit(); // Por defaul

        if (direction == Vector3.zero)
            return false;

        // el valor origin tiene que estar en la altura del cuello
        var origin = transform.position + footToNeckHeight * Vector3.up;
        // el saliente es 0,2, para que no se nos escape ningun saliente
        var offset = new Vector3(0, 0.18f, 0);

        for (int i = 0; i < 10; i++)
        {
            Debug.DrawRay(origin + offset * i, direction);
            if (Physics.Raycast(origin + offset * i, direction, out RaycastHit hit, climbLedgeRayLength, climbLedgeLayer))
            {
                ledgeHit = hit;

                return true;
            }
        }

        return false;
    }

    public bool DropLedgeCheck(out RaycastHit ledgeHit)
    {
        ledgeHit = new RaycastHit(); // Por defaul

        var origin = transform.position + Vector3.down * 0.1f + transform.forward * 0.2f;

        Debug.DrawRay(origin, transform.forward, Color.red);
        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, 0.8f, climbLedgeLayer))
        {
            ledgeHit = hit;

            return true;
        }

        return false;
    }

    public bool ObstacleLedgeCheck(Vector3 moveDirection, out LedgeData ledgeData)
    {
        ledgeData = new LedgeData();

        if (moveDirection == Vector3.zero)
            return false;

        // distancia para que no salga del saliente
        // lo ajusrtaremos desde el editor
        //var originOffset = 0.65f;
        // Como el rayo se lanza en los pies sumamos una unidad para arriba para no tener problemas con el rayo que atraviese el suelo
        var origin = transform.position + moveDirection * originOffset + Vector3.up;

        if (PhysicsUtil.ThreeRayCast(origin, Vector3.down, ledgeSpacing, transform, out List<RaycastHit> hits, ledgeRayLength, obstacleLayer, true))
        {
            RaycastHit hitMin = default;
            float heightMin = int.MaxValue;
            bool found = false;

            foreach (var hit in hits)
            {
                var height = transform.position.y - hit.point.y; // Sal

                // Esta en un saliente
                if (height > ledgeHeightThreshold)
                {
                    // es menor que el anterior
                    if (height < heightMin)
                    {
                        heightMin = height;
                        Debug.Log($"height2: {height}");
                        hitMin = hit;
                        if (!found)
                            found = true; // Encontrad
                    }
                }
            }
            if (found) // Encontrad)
            {
                //var surfaceOriginal = transform.position + moveDirection - (new Vector3(0, 1, 0));
                var surfaceOrigin = hitMin.point;
                // para que este por dejado del pie del jugador
                surfaceOrigin.y = transform.position.y - 0.1f; // Salie

                if (Physics.Raycast(surfaceOrigin, transform.position - surfaceOrigin, out RaycastHit surfaceHit, 2, obstacleLayer))
                {
                    Debug.DrawLine(surfaceOrigin, transform.position, Color.cyan);
                    float height = heightMin;

                    Debug.Log($"height: {height}");

                    ledgeData.angle = Vector3.Angle(transform.forward, surfaceHit.normal);
                    ledgeData.height = height;
                    ledgeData.surfaceHit = surfaceHit; // Salient

                    return true;
                }
            }

        }

        return false;
    }

    public struct ObstacleHitData
    {
        // Usado para confirmar si el rayo golpea con algo en su sentido forward ( hacia adelante)
        public bool forwardHitFound;
        // Usado para confirmar si el rayo golpea con algo en altura
        public bool heightHitFound;

        // Almacenamos la información del Raycast en caso de golpeo
        public RaycastHit forwardHit;
        // Almacenamos la información del Raycast en caso de golpeo
        public RaycastHit heightHit;

        // Indica si es posible deslizarse
        public bool canSlide;
        // Almacenamos la información del Raycast para el deslizamiento
        public RaycastHit slideHit;
    }

    public struct LedgeData // Salient
    {
        public float height;
        public float angle;
        public RaycastHit surfaceHit; // Saliente
    }
}