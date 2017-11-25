using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowInteraction : MonoBehaviour
{

    void Start()
    {

    }

    private void OnTriggerEnter(Collider collider)
    {

    }

    private void OnTriggerStay(Collider other)
    {
        transform.localScale *= 0.97f;
        if (transform.localScale.x < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}
