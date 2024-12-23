using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionController : MonoBehaviour
{
    public Collider[] sectorOutterCollides;

    public void EnableColliders(bool enable = true)
    {
        if (sectorOutterCollides != null)
            foreach (var collider in sectorOutterCollides)
                collider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnableColliders();
        SectionsManager.Instance.CurrentSection = this;
    }

    private void OnTriggerExit(Collider other)
    {
        EnableColliders(false);
        SectionsManager.Instance.CurrentSection = null;
    }
}
