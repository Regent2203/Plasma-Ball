using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LoopedController : MonoBehaviour 
{
	public int LoopTime  = 60;
	public int LoopShift =  0;
	public Vector2 dPos;
    public float dAngle;
    public bool LoopedAngle = false;
    /*enabler/disabler frames?*/

	private int t;

	//SpriteRenderer sr;
	Transform tr;

	void Start () 
	{
		//sr = this.GetComponent<SpriteRenderer> ();
		tr = this.GetComponent<Transform> ();

		t = LoopShift % LoopTime; //[0,1,2,3..59] when loop=60
	}
	

	void FixedUpdate () 
	{
		tr.Translate(dPos.x * Mathf.Cos(2 * Mathf.PI * t/LoopTime), dPos.y * Mathf.Sin(2 * Mathf.PI * t/LoopTime), 0);
        if (LoopedAngle)
            tr.Rotate(new Vector3(0,0,dAngle * Mathf.Cos(2 * Mathf.PI * t/LoopTime)));
        else
            tr.Rotate(new Vector3(0,0,dAngle));
		
        //add to timer
		t++;
		if (t == LoopTime)
			t = 0;
	}
}

