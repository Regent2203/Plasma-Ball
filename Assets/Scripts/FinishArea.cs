﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishArea : MonoBehaviour 
{
	//GameObject Player;

	void Start () 
	{
		this.GetComponent<SpriteRenderer> ().enabled = false;

	}

	void OnTriggerEnter2D(Collider2D coll)
	{        
		if (coll.gameObject.tag == "Player")
		{
			//StarController Player = coll.gameObject.GetComponentInParent<StarController>();

			/* add VERY beautiful firework animation*/

			//Debug.Log("Finish "+Time.frameCount);
		}
	}
}