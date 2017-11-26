using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int points = 0;

	void Start ()
    {
		
	}
	
    public void AddPoint()
    {
        points++;
    }

    public void GameOver()
    {
        points = 0;
    }
}
