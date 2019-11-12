using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowArea : MonoBehaviour 
{
	public float coef_Slow = 0.5f;

	void Start () 
    {
        
	}
	
	void OnTriggerEnter2D(Collider2D coll)
    {        
        if (coll.gameObject.tag == "PlayerCenterPoint")
        {
			StarController Player = coll.gameObject.GetComponentInParent<StarController>();
			Rigidbody2D rb = coll.transform.parent.GetComponent<Rigidbody2D>();          
            
			Player.k_Slow *= coef_Slow;
			rb.velocity *= coef_Slow;

			//Debug.Log("ENTER slow "+Time.frameCount);
        }
    }

	void OnTriggerStay2D(Collider2D coll)
    {        
		if (coll.gameObject.tag == "PlayerCenterPoint")
        {
            //Debug.Log("STAY slow ");
        }
    }

	void OnTriggerExit2D(Collider2D coll)
    {        
		if (coll.gameObject.tag == "PlayerCenterPoint")
        {
			StarController Player = coll.gameObject.GetComponentInParent<StarController>();
			Rigidbody2D rb = coll.transform.parent.GetComponent<Rigidbody2D>();

			Player.k_Slow /= coef_Slow;
			rb.velocity /= coef_Slow;

			//Debug.Log("EXIT slow "+Time.frameCount);
        }
    }
}
