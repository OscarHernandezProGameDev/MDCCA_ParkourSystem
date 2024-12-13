using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Clase que representa un punto de escalada en un entorno 3D
public class ClimbPoint : MonoBehaviour
{
    // Booleana que indica si este ClimbPoint es 'un punto de montaje', usado para escalar hasta lo alto de un obst�culo y salir del estado 'Hanging'
    [SerializeField] private bool mountPoint;

    // Lista de vecinos que est�n conectados a este punto de escalada
    [SerializeField] private List<Neighbour> neighbours;

    private void Awake()
    {
        // Filtra los vecinos que tienen una conexi�n bidireccional (isTwoWay)
        var twoWayNeighbours = neighbours.Where(n => n.isTwoWay);
        
        // Para cada vecino bidireccional, si la conexi�n a�n no existe en el punto vecino, se crea
        foreach (var neighbour in twoWayNeighbours)
        {
            // Verifica si el vecino ya tiene una conexi�n con este punto
            if (!neighbour.point.HasConnection(this))
            {  
                // Si no tiene conexi�n, crea una nueva conexi�n desde el vecino hacia este punto
                neighbour.point?.CreateConnection(this, -neighbour.direction, neighbour.connectionType, neighbour.isTwoWay);
            }
        }
    }

    // Crea una conexi�n entre este ClimbPoint y otro, con los detalles de la direcci�n, tipo de conexi�n y si es bidireccional
    public void CreateConnection(ClimbPoint point, Vector2 direction, 
                                ConnectionType connectionType, bool isTwoWay = true)
    {
        var neighbour = new Neighbour() // Crea un nuevo objeto Neighbour con los detalles de la conexi�n
        {
            point = point, // su clase ClimbPoint
            direction = direction, // la direcci�n en la que se encuentra respecto a este ClimbPoint
            connectionType = connectionType, // El tipo de conexi�n (salto o movimiento)
            isTwoWay = isTwoWay // �Es bidireccional?
        };

        // A�ade el nuevo vecino a la lista de conexiones
        neighbours.Add(neighbour);
    }

    // M�todo para obtener el vecino en la direcci�n especificada
    public Neighbour GetNeighbour(Vector2 direction) 
    {
        Neighbour neighbour = null;

        // Primero busca un vecino que coincida con la direcci�n vertical (y)
        if (direction.y != 0)
            neighbour = neighbours.FirstOrDefault(n =>  n.direction.y == direction.y);
       
        // Si no se encontr� uno en la direcci�n y, busca en la direcci�n horizontal (x)
        if (neighbour == null && direction.x != 0)
            neighbour = neighbours.FirstOrDefault(n => n.direction.x == direction.x);

        return neighbour;
    }

    // Comprueba si este ClimbPoint ya est� conectado con otro punto dado
    public bool HasConnection(ClimbPoint point)
    {
        return neighbours.Any(n => n.point == point);
    }

    // M�todo que se ejecuta en el editor para dibujar Gizmos que representan las conexiones entre puntos
    private void OnDrawGizmos()
    {
        // Dibuja una l�nea azul hacia adelante desde la posici�n de este ClimbPoint (permite ver las normales de un vistazo)
        Debug.DrawRay(transform.position, transform.forward, Color.blue);
       
        if(neighbours != null)
        { 
            // Dibuja l�neas entre este punto y cada uno de sus vecinos
            foreach (var neighbour in neighbours)
            {
                if (neighbour.point != null)// La l�nea es verde si es una conexi�n bidireccional, gris si es unidireccional
                    Debug.DrawLine(transform.position, neighbour.point.transform.position,
                                                      (neighbour.isTwoWay) ? Color.green : Color.gray);
            }
        }
       
    }

    // Propiedad que expone el valor de mountPoint
    public bool MountPoint => mountPoint;

}

[System.Serializable]
public class Neighbour // Clase serializable que representa la informaci�n de una conexi�n con un vecino (Usamos Serializable para poder verla en el inspector)
{
    public ClimbPoint point;  // El punto de escalada vecino
    public Vector2 direction;  // La direcci�n de la conexi�n (x para horizontal, y para vertical)
    public ConnectionType connectionType; // Tipo de conexi�n (salto o deslizamiento)
    public bool isTwoWay = true;// Indica si la conexi�n es bidireccional
}

// Enum para definir los diferentes tipos de conexiones entre puntos de escalada
public enum ConnectionType { Jump, Move}
