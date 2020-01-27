using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour 
{   
    public Vector2 speed = new Vector2(0.1f, 0.1f); //must be positive
    public int steps = 5; //за сколько шагов сдвигать камеру

    Transform tr;
    BoxCollider2D bc; //camera collider    
    public Camera cam { get; protected set; }

    GameObject target; //object to follow
    Transform target_tr;    


    void Start() 
    {
        tr = transform;
        bc = GetComponent<BoxCollider2D> ();        
        cam = GetComponent<Camera>();
        
		
		//camera visible area rectangle - update collider2d size
		Vector2 cam_size = new Vector2 (cam.orthographicSize * cam.aspect * 2, cam.orthographicSize * 2);
		bc.size = new Vector2 (cam_size.x * 0.07f, cam_size.y * 0.07f);        
    }

    public void SetTarget(GameObject t)
    {
        target = t;
        target_tr = target.transform;
        //make camera centered at player
        tr.position = target_tr.position;
    }

    Vector2 shift;
    Vector2 delta;

    void FixedUpdate() 
    {        
        if (target == null)
            return;

        shift = Vector2.zero;

        if (!bc.OverlapPoint(target_tr.position))
        {            
            delta = (Vector2)target_tr.position - bc.ClosestPoint(target_tr.position);

            /*
            //приближение к таргету фиксированными шагами (не подходит для этой игры)
            shift = new Vector2
            (
                Mathf.Min(Mathf.Abs(delta.x), speed.x) * Mathf.Sign(delta.x),
                Mathf.Min(Mathf.Abs(delta.y), speed.y) * Mathf.Sign(delta.y)
            );
            */

            if (Math.Abs(delta.x) < speed.x)             
                shift.x = delta.x;
            else
                shift.x = delta.x / steps;
            if (Math.Abs(delta.y) < speed.y)
                shift.y = delta.y;
            else
                shift.y = delta.y / steps;            
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        tr.position += (Vector3)shift * Time.timeScale;
    }
}
