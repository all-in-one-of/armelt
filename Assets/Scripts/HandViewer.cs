using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandViewer : MonoBehaviour
{
    public GameObject _handGameObject;
    public float _palmSmooth = 0.8f;
    public float _topSmooth = 0.7f;


    private Meta.HandInput.Hand _hand;
    private Vector3 _topPos, _palmPos;
    private Vector3 _topPosPrev, _palmPosPrev;
    private Meta.HandInput.PalmState _palmState;
    public GameObject _particleShooting;
    public GameObject _particleIdle;
    public GameObject _ShootTrigger;

    void Start()
    {
        _hand = _handGameObject.GetComponent<Meta.HandInput.Hand>();
    }

    void Update()
    {
        _palmPos = Vector3.Lerp(_palmPosPrev, _hand.Palm.Position, _palmSmooth);
        _topPos = Vector3.Lerp(_topPosPrev, _hand.Top.Position, _topSmooth);

        _palmPosPrev = _palmPos;
        _topPosPrev = _topPos;

        Quaternion rot = Quaternion.identity;
        rot.SetLookRotation(_topPos - _palmPos);

        transform.rotation = rot;
        transform.position = _palmPos;
        
        _palmState = _hand.Palm.PalmState;
        //if (_palmState == Meta.HandInput.PalmState.Grabbing)
        if (_hand.IsGrabbing)
        {
                _particleIdle.SetActive(true);
            _particleShooting.SetActive(false);
        }
        else
        {
            _particleIdle.SetActive(false);
            _particleShooting.SetActive(true);
        }
    }
}
