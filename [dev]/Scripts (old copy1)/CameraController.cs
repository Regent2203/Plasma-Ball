using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour 
{
    private bool _NeedShift = false;

    public GameObject target = null; //set to "PlayerStar" manually in Unity

    void Start () 
    {
        //CollShift = new Vector2(5,3); //calculate with colliders on camera

		//camera visible rectangle
		/*
		Camera.main.orthographicSize;
		Debug.Log(Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect);
		Debug.Log(Camera.main.transform.position.x + Camera.main.orthographicSize * Camera.main.aspect);
		*/


    }

    void FixedUpdate () 
    {        
        if (_NeedShift)
        {
            //StarController trg = target.GetComponent<StarController>();
            Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
            Vector2 d = target.transform.position - this.transform.position;
            Vector3 delta = Vector3.zero;

            if (Math.Sign(rb.velocity.x) == Math.Sign(d.x))
            {
                delta.x = rb.velocity.x/60;
            }
            if (Math.Sign(rb.velocity.y) == Math.Sign(d.y))
            {
                delta.y = rb.velocity.y/60;
            }
            this.transform.position += delta;
        }
    }

    void LateUpdate ()
    {
        //
    }

	void CenterOnTarget ()
	{
	}

    void OnTriggerEnter2D(Collider2D coll)
    {
    }

    void OnTriggerStay2D(Collider2D coll)
    {
        if (coll.tag == "Player") 
            _NeedShift = true;
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.tag == "Player") 
            _NeedShift = false;
        
        /*
        Vector2 d;
        d = target.transform.position - this.transform.position;
        Speed.x = Math.Sign(d.x)*Mathf.Max((Math.Abs(d.x) - Math.Abs(CollShift.x)),0);
        Speed.y = Math.Sign(d.y)*Mathf.Max((Math.Abs(d.y) - Math.Abs(CollShift.y)),0);
        */
    }

 
}
