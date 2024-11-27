using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClimbPoint : MonoBehaviour
{
    [SerializeField] private List<Neighbour> neighbours;

    private void Awake()
    {
        var twoWayNeighbours = neighbours.Where(n => n.isToWay);

        foreach (var neighbour in twoWayNeighbours)
        {
            if (neighbour.point != null)
                if (!neighbour.point.HasConnectionTo(this))
                    neighbour.point.CreateConnection(this, -neighbour.direction, neighbour.type, neighbour.isToWay);
        }
    }

    public void CreateConnection(ClimbPoint point, Vector2 direction, ConnectionType type, bool isToWay = true)
    {
        var neighbour = new Neighbour
        {
            point = point,
            direction = direction,
            type = type,
            isToWay = isToWay
        };

        neighbours.Add(neighbour);
    }

    public bool HasConnectionTo(ClimbPoint point) => neighbours.Any(n => n.point == point);

    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.blue);
        foreach (var neighbour in neighbours)
        {
            if (neighbour.point != null)
                Debug.DrawLine(transform.position, neighbour.point.transform.position, neighbour.isToWay ? Color.green : Color.gray);
        }
    }
}

[Serializable]
public class Neighbour
{
    public ClimbPoint point;
    public Vector2 direction;
    public ConnectionType type;
    public bool isToWay = true;
}

public enum ConnectionType { Jump, Move }
