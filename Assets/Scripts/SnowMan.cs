using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowMan : MonoBehaviour
{
    private int _piecesLeft = 3;
    public float _speed = 1.0f;



	void Start ()
    {
	
	}
	
    public void RemovePiece()
    {
        _piecesLeft--;
        if(_piecesLeft==0)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, Camera.main.transform.position, Time.fixedDeltaTime * _speed);
    }

}
