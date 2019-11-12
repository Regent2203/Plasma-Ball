using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using My_SolidCollisionCorrection;

public class SolidWall : MonoBehaviour 
{
    public float coef_Bounce = 1;

	void Start () 
    {
		
	}
	
    void Update ()
    {
    }
	
	void FixedUpdate ()
    {
        
    }

    void OnCollisionEnter2D(Collision2D coll)
    {        
        if (coll.gameObject.tag == "Player")
        {
			//Debug.Log("ENTER solid " + rb.position + "_" + Time.frameCount);
			//CorrectPosition(coll);           
        }
    }

    void OnCollisionStay2D(Collision2D coll)
    {        
        if (coll.gameObject.tag == "Player")
        {
			//Debug.Log("STAY solid!!!!!! "+Time.frameCount);
			//CorrectPosition(coll);
        }
    }

    void OnCollisionExit2D(Collision2D coll)
    {        
        if (coll.gameObject.tag == "Player")
        {
			//Debug.Log("EXIT solid "+Time.frameCount);
        }
    }


	//--------------------------------------------------------------------------------------------------------------------------------------------


	void CorrectPosition(Collision2D coll)
	{
		StarController Player = coll.gameObject.GetComponent<StarController>();
		CircleCollider2D cc = coll.gameObject.GetComponent<CircleCollider2D>();
		Rigidbody2D rb = coll.gameObject.GetComponent<Rigidbody2D>();
		ContactPoint2D[] contact = new ContactPoint2D[1];
		coll.GetContacts(contact);

		Player.CR.NeedCorr = true;
		float r1 = cc.radius + Physics2D.defaultContactOffset * 3 * 0;
		float r2 = Vector2.SqrMagnitude(contact[0].point - rb.position);
		float ratio = r2 / r1;

		if ( float.IsNaN(Player.CR.MinRatio) )
		{
			Player.CR.MinRatio = ratio;
			Player.CR.Delta = rb.position - Player.PrevPosition;
		}
		else
			Player.CR.MinRatio = Mathf.Min (ratio, Player.CR.MinRatio);

		Player.CR.CollCount++;

		rb.velocity = Vector2.Reflect(rb.velocity * coef_Bounce, contact[0].normal);
		//rb.velocity = Vector2.zero;
		//Player._LockControls = 2;


		//debug
		//Debug.Log("normal = " + contact[0].normal);
		foreach (ContactPoint2D contacts in coll.contacts)
		{
			Debug.DrawRay(contacts.point, new Vector3( 0.01f, 0, 0), Color.red);
			Debug.DrawRay(contacts.point, new Vector3(-0.01f, 0, 0), Color.red);
			Debug.DrawRay(contacts.point, new Vector3( 0,-0.01f, 0), Color.red);
			Debug.DrawRay(contacts.point, new Vector3( 0, 0.01f, 0), Color.red);
		}
	}
}
