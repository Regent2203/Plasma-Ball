using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishArea : MonoBehaviour 
{
	void Start () 
	{
		GetComponent<SpriteRenderer>().enabled = false;

	}

	void OnTriggerEnter2D(Collider2D coll)
	{        
		if (coll.gameObject.tag == "Player")
		{			
			/* add VERY beautiful firework animation*/

			//Debug.Log("Finish "+Time.frameCount);
		}
	}
}
