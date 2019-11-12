using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartArea : MonoBehaviour 
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
			/*Player.CheckPoint = this.gameObject;*/
			/* add beautiful firework animation*/

			//Debug.Log("New Save Point "+Time.frameCount);
		}
	}
}
