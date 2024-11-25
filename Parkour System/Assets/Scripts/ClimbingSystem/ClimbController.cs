using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    [SerializeField] private GatherInput gatherInput;

    private EnvironmentScanner envScanner;

    private void Awake()
    {
        envScanner = GetComponent<EnvironmentScanner>();
    }

    void Update()
    {
        if (gatherInput.tryToJump)
        {
            if (envScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit))
            {
                Debug.Log("Climb Ledge Found");
            }
        }
    }
}
