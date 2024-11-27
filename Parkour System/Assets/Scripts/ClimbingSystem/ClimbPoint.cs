using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbPoint : MonoBehaviour
{
    [SerializeField] private List<Neighbour> neighbours;
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
