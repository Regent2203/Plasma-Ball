using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowArea : MonoBehaviour 
{
    public float coef_Slow = 1;

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
            //StarController Player = coll.gameObject.GetComponent<StarController>();
            Rigidbody2D rb = coll.gameObject.GetComponent<Rigidbody2D>();
            //ContactPoint2D[] contacts = new ContactPoint2D[1];

            //coll.GetContacts(contacts);
            rb.velocity = rb.velocity * coef_Slow;
                        
            //Player._LockControls = 2;
            Debug.Log("ENTER slow");
			/*
            foreach (ContactPoint2D contact in coll.contacts)
            {
                Debug.DrawRay(contact.point, new Vector3( 0.01f, 0, 0), Color.red);
                Debug.DrawRay(contact.point, new Vector3(-0.01f, 0, 0), Color.red);
                Debug.DrawRay(contact.point, new Vector3( 0,-0.01f, 0), Color.red);
                Debug.DrawRay(contact.point, new Vector3( 0, 0.01f, 0), Color.red);
                //Debug.DrawRay(contact.point, contact.normal, Color.white);
                //Debug.Log(contact.normal);
            }*/
        }
    }
    void OnCollisionStay2D(Collision2D coll)
    {        
        if (coll.gameObject.tag == "Player")
        {
            //Debug.Log("STAY slow");
        }
    }
    void OnCollisionExit2D(Collision2D coll)
    {        
        if (coll.gameObject.tag == "Player")
        {
			Rigidbody2D rb = coll.gameObject.GetComponent<Rigidbody2D>();
			rb.velocity = rb.velocity / coef_Slow;
			Debug.Log("EXIT slow");
        }
    }
}
