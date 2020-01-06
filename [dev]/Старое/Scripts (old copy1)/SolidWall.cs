using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            StarController Player = coll.gameObject.GetComponent<StarController>();
            Rigidbody2D rb = coll.gameObject.GetComponent<Rigidbody2D>();
            ContactPoint2D[] contacts = new ContactPoint2D[1];

            coll.GetContacts(contacts);
            rb.velocity = Vector2.Reflect(rb.velocity * coef_Bounce, contacts[0].normal);
            /*Physics2D.defaultContactOffset;*/
            Vector2 s = 0.78f * contacts[0].normal - (contacts[0].point - rb.position);
            rb.position -= s;

            Player._LockControls = 2;
			Debug.Log("ENTER solid "+Time.frameCount);

			/*
            foreach (ContactPoint2D contact in coll.contacts)
            {
                Debug.DrawRay(contact.point, new Vector3( 0.01f, 0, 0), Color.red);
                Debug.DrawRay(contact.point, new Vector3(-0.01f, 0, 0), Color.red);
                Debug.DrawRay(contact.point, new Vector3( 0,-0.01f, 0), Color.red);
                Debug.DrawRay(contact.point, new Vector3( 0, 0.01f, 0), Color.red);
            }*/
        }
    }
    void OnCollisionStay2D(Collision2D coll)
    {        
        if (coll.gameObject.tag == "Player")
        {
			Debug.Log("STAY solid!!!!!! "+Time.frameCount);
        }
    }
    void OnCollisionExit2D(Collision2D coll)
    {        
        if (coll.gameObject.tag == "Player")
        {
			Debug.Log("EXIT solid "+Time.frameCount);
        }
    }
}
