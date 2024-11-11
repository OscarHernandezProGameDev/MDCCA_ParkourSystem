using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/New Parkour Action")]
public class ParkourAction : ScriptableObject
{
    [SerializeField] private string animName;

    // Como mínimo tiene que ser como el valor del character controller step offset (si es menor de este valor el character controler lo entiende como una escalera)
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;
}