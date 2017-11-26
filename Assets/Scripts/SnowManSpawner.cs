using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowManSpawner : MonoBehaviour {

    public GameObject _snowMan;
    public GameObject _gameManager;
    public float _spawnInterval = 3.0f;

    private float _lastSpawned;

	void Start ()
    {
        _lastSpawned = Time.time + 10.0f;
    }

    void Update ()
    {
        float elapsed = Time.time - _lastSpawned;
        if(elapsed > _spawnInterval)   // &&   Meta.SlamUI.CalibrationStage.Completed)
        {
            SpawnSnowMan();
            _lastSpawned = Time.time;
        }
	}

    public void SpawnSnowMan()
    {
        Vector3 pos = new Vector3(3.0f * (Random.value - 0.5f), -1.0f, 2.5f + 2.0f * Random.value);
        GameObject snowmanInstance = Instantiate(_snowMan, pos, Quaternion.identity);
        SnowMan snowman = snowmanInstance.GetComponent<SnowMan>();
        snowman._speed = 0.05f + 0.125f * Random.value;
        snowman._gameManager = _gameManager;
    }
}
