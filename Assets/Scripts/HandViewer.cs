using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandViewer : MonoBehaviour
{

    public GameObject _handGameObject;

    private Meta.HandInput.Hand _hand;
    private Vector3 topPos, palmPos;

	void Start ()
    {
        _hand = _handGameObject.GetComponent<Meta.HandInput.Hand>();
	}
	
	void Update ()
    {
        palmPos = _hand.Palm.Position;
        topPos = _hand.Top.Position;
    }
}
