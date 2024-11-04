using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GatherInput gInput;
    [SerializeField] private float moveSpeed = 5f;

    private Vector3 moveInput;

    void Update()
    {
        moveInput = new Vector3(gInput.direction.x, 0, gInput.direction.y);

        transform.position += moveInput * moveSpeed * Time.deltaTime;
    }
}