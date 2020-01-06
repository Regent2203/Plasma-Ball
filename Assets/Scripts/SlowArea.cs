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
            Player pl = coll.GetComponentInParent<Player>();
            pl.k_Slow *= SlowRate;            
        }     
    }
    
	void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.tag == "PlayerCenterPoint")
        {
            Player pl = coll.GetComponentInParent<Player>();
            pl.k_Slow /= SlowRate;
        }
    }
}
