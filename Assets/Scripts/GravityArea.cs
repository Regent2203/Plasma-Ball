using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityArea : MyTile 
{
	public Vector2 Force = new Vector2( 0.0f, 0.03f);

    
	void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.tag == "PlayerCenterPoint")
        {
            Player pl = coll.GetComponentInParent<Player>();
            pl.ex_Grav += Force;
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.tag == "PlayerCenterPoint")
        {
            Player pl = coll.GetComponentInParent<Player>();
            pl.ex_Grav -= Force;
        }
    }
}