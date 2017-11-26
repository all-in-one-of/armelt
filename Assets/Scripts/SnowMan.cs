using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowMan : MonoBehaviour
{
    private int _piecesLeft = 3;
    private GameManager _GM;
    public float _speed = 1.0f;
    public GameObject _gameManager;

	void Start ()
    {
        _GM = _gameManager.GetComponent<GameManager>();
	}
	
    public void RemovePiece()
    {
        _piecesLeft--;
        if(_piecesLeft==0)
        {
            _GM.AddPoint();            
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, Camera.main.transform.position, Time.fixedDeltaTime * _speed);
        if( (transform.position - Camera.main.transform.position).magnitude < 0.1f )
        {
            _GM.AddHit();
            Destroy(gameObject);
        }
    }

}
