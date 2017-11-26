using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int points = 0;
    public int hitPoints = 3;

	void Start ()
    {
		
	}
	
    public void AddPoint()
    {
        points++;
        Debug.Log("points: " + points);
    }

    public void AddHit()
    {
        hitPoints--;
        Debug.Log("hit point: " + hitPoints);
        if(hitPoints <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        points = 0;
        Debug.Log("GAME OVER");
    }
}
