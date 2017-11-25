using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float timeToDie = 2.0f;

    private float _startTime;

	void Start ()
    {
        _startTime = Time.time;
	}
	
	void Update ()
    {
		if(Time.time - _startTime > timeToDie)
        {
            Destroy(gameObject);
        }
	}
}
