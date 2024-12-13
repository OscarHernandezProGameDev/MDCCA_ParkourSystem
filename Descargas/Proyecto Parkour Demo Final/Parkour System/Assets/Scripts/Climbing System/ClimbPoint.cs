using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Clase que representa un punto de escalada en un entorno 3D
public class ClimbPoint : MonoBehaviour
{
    // Booleana que indica si este ClimbPoint es 'un punto de montaje', usado para escalar hasta lo alto de un obstáculo y salir del estado 'Hanging'
    [SerializeField] private bool mountPoint;

    // Lista de vecinos que están conectados a este punto de escalada
    [SerializeField] private List<Neighbour> neighbours;

    private void Awake()
    {
        // Filtra los vecinos que tienen una conexión bidireccional (isTwoWay)
        var twoWayNeighbours = neighbours.Where(n => n.isTwoWay);
        
        // Para cada vecino bidireccional, si la conexión aún no existe en el punto vecino, se crea
        foreach (var neighbour in twoWayNeighbours)
        {
            // Verifica si el vecino ya tiene una conexión con este punto
            if (!neighbour.point.HasConnection(this))
            {  
                // Si no tiene conexión, crea una nueva conexión desde el vecino hacia este punto
                neighbour.point?.CreateConnection(this, -neighbour.direction, neighbour.connectionType, neighbour.isTwoWay);
            }
        }
    }

    // Crea una conexión entre este ClimbPoint y otro, con los detalles de la dirección, tipo de conexión y si es bidireccional
    public void CreateConnection(ClimbPoint point, Vector2 direction, 
                                ConnectionType connectionType, bool isTwoWay = true)
    {
        var neighbour = new Neighbour() // Crea un nuevo objeto Neighbour con los detalles de la conexión
        {
            point = point, // su clase ClimbPoint
            direction = direction, // la dirección en la que se encuentra respecto a este ClimbPoint
            connectionType = connectionType, // El tipo de conexión (salto o movimiento)
            isTwoWay = isTwoWay // ¿Es bidireccional?
        };

        // Añade el nuevo vecino a la lista de conexiones
        neighbours.Add(neighbour);
    }

    // Método para obtener el vecino en la dirección especificada
    public Neighbour GetNeighbour(Vector2 direction) 
    {
        Neighbour neighbour = null;

        // Primero busca un vecino que coincida con la dirección vertical (y)
        if (direction.y != 0)
            neighbour = neighbours.FirstOrDefault(n =>  n.direction.y == direction.y);
       
        // Si no se encontró uno en la dirección y, busca en la dirección horizontal (x)
        if (neighbour == null && direction.x != 0)
            neighbour = neighbours.FirstOrDefault(n => n.direction.x == direction.x);

        return neighbour;
    }

    // Comprueba si este ClimbPoint ya está conectado con otro punto dado
    public bool HasConnection(ClimbPoint point)
    {
        return neighbours.Any(n => n.point == point);
    }

    // Método que se ejecuta en el editor para dibujar Gizmos que representan las conexiones entre puntos
    private void OnDrawGizmos()
    {
        // Dibuja una línea azul hacia adelante desde la posición de este ClimbPoint (permite ver las normales de un vistazo)
        Debug.DrawRay(transform.position, transform.forward, Color.blue);
       
        if(neighbours != null)
        { 
            // Dibuja líneas entre este punto y cada uno de sus vecinos
            foreach (var neighbour in neighbours)
            {
                if (neighbour.point != null)// La línea es verde si es una conexión bidireccional, gris si es unidireccional
                    Debug.DrawLine(transform.position, neighbour.point.transform.position,
                                                      (neighbour.isTwoWay) ? Color.green : Color.gray);
            }
        }
       
    }

    // Propiedad que expone el valor de mountPoint
    public bool MountPoint => mountPoint;

}

[System.Serializable]
public class Neighbour // Clase serializable que representa la información de una conexión con un vecino (Usamos Serializable para poder verla en el inspector)
{
    public ClimbPoint point;  // El punto de escalada vecino
    public Vector2 direction;  // La dirección de la conexión (x para horizontal, y para vertical)
    public ConnectionType connectionType; // Tipo de conexión (salto o deslizamiento)
    public bool isTwoWay = true;// Indica si la conexión es bidireccional
}

// Enum para definir los diferentes tipos de conexiones entre puntos de escalada
public enum ConnectionType { Jump, Move}
