using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityArea : MonoBehaviour 
{
	public Vector2 grav_force = new Vector2( 5.0f, 0.0f);
	public AffSideType GravSide; //if selected smth other than "none", force is more powerful if closer to chosen wall
	private Vector2 aff_side;

	BoxCollider2D bc;

	public enum AffSideType
	{
		None, Left, Top, Right, Bottom
	}

	void Start () 
    {
		bc = this.GetComponent<BoxCollider2D>();

		switch (GravSide) 
		{
			case AffSideType.Left:
				aff_side = new Vector2 ( bc.bounds.min.x, float.NaN);
			break;
			case AffSideType.Top:
				aff_side = new Vector2 ( float.NaN, bc.bounds.max.y);
			break;
			case AffSideType.Right:
				aff_side = new Vector2 ( bc.bounds.max.x, float.NaN);
			break;
			case AffSideType.Bottom:
				aff_side = new Vector2 ( float.NaN, bc.bounds.min.y);
			break;
			case AffSideType.None:
				aff_side = new Vector2 ( float.NaN, float.NaN);
			break;
		}

		//Debug.Log ("Gravity side = " + aff_side);
	}
	
	void OnTriggerEnter2D(Collider2D coll)
    {
		/*
        if (coll.gameObject.tag == "PlayerCenterPoint")
        {
			//Debug.Log("ENTER gravarea "+Time.frameCount);
        }
		*/
    }
	
	void OnTriggerStay2D(Collider2D coll)
    {
		if (coll.gameObject.tag == "PlayerCenterPoint")
        {
			Rigidbody2D rb = coll.transform.parent.GetComponent<Rigidbody2D>();

			if (GravSide == AffSideType.None)
				rb.AddForce (grav_force * 60);
			else 
			{
				float rate = 1;

				switch (GravSide) 
				{
				case AffSideType.Left:
				case AffSideType.Right:
					rate = 1 - Mathf.Abs( (coll.transform.position.x - aff_side.x) / (bc.size.x * transform.localScale.x) );
					break;
				case AffSideType.Top:
				case AffSideType.Bottom:
					rate = 1 - Mathf.Abs( (coll.transform.position.y - aff_side.y) / (bc.size.y * transform.localScale.y) );
					break;
				}


				rate = Mathf.Clamp(rate, 0.0f, 1.0f);
				rb.AddForce (grav_force * 60 * rate);

				//Debug.Log("rate = "+rate);
			}

			//Debug.Log("STAY gravarea "+Time.frameCount);
        }
    }

	void OnTriggerExit2D(Collider2D coll)
    {        
		/*
		if (coll.gameObject.tag == "PlayerCenterPoint")
        {
			//Debug.Log("EXIT gravarea "+Time.frameCount);
        }
		*/
    }
}