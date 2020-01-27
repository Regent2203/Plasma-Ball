using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowArea : MyTile 
{
	public float SlowRate = 0.5f;
	

	void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.tag == "PlayerCenterPoint")
        {
            //PlayerController pl = 
            coll.transform.parent.GetComponent<PlayerController>().k_Slow *= SlowRate;            
        }     
    }
    
	void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.tag == "PlayerCenterPoint")
        {
            //PlayerController pl = 
            coll.transform.parent.GetComponent<PlayerController>().k_Slow /= SlowRate;
        }
    }
}
