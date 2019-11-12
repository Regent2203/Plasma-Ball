using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour 
{
    private bool _NeedShift = false;
	BoxCollider2D bc;
	Transform bgi;
	//private Vector2 bg_shift;

    public GameObject target = null; //set to "PlayerStar" manually in Unity

    void Start () 
    {
		bc = this.GetComponent<BoxCollider2D> ();
		bgi = transform.Find ("Background_Image");
		
		//camera visible rectangle - update collider2d size
		Vector2 cam_size = new Vector2 (Camera.main.orthographicSize * Camera.main.aspect * 2, Camera.main.orthographicSize * 2);
		bc.size = new Vector2 (cam_size.x * 0.07f, cam_size.y * 0.07f);
    }

    void FixedUpdate () 
    {
		if (_NeedShift)
        {
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
			bgi.localPosition -= delta / 20;

			if ( Mathf.Abs(bgi.localPosition.x) > 10.24f)
				bgi.Translate(10.24f * -Mathf.Sign(bgi.localPosition.x), 0, 0);
			if ( Mathf.Abs(bgi.localPosition.y) > 7.68f)
				bgi.Translate(0,  7.68f * -Mathf.Sign(bgi.localPosition.y), 0);
        }

		if (_NeedShift == false)
			_NeedShift = true;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
		
    }

    void OnTriggerStay2D(Collider2D coll)
    {
		if (coll.tag == "PlayerCenterPoint") 
			_NeedShift = false;
    }

    void OnTriggerExit2D(Collider2D coll)
    {
		if (coll.tag == "PlayerCenterPoint") 
			_NeedShift = true;
    }

 
}
