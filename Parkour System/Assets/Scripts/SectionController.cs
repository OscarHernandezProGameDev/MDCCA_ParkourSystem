using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionController : MonoBehaviour
{
    public Collider[] sectorOutterCollides;

    private void OnTriggerEnter(Collider other)
    {
        foreach (var collider in sectorOutterCollides)
            collider.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        foreach (var collider in sectorOutterCollides)
            collider.enabled = false;
    }
}
