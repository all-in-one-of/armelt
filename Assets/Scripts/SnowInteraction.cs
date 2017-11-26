using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowInteraction : MonoBehaviour
{
    private SnowMan snowman;

    void Start()
    {
        snowman = GetComponentInParent<SnowMan>();
    }

    private void OnTriggerEnter(Collider collider)
    {

    }

    private void OnTriggerStay(Collider other)
    {
        transform.localScale *= 0.97f;
        if (transform.localScale.x < 0.1f)
        {
            snowman.RemovePiece();
            Destroy(gameObject);
        }
    }
}
