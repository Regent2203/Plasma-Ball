using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour 
{
    Rigidbody2D rb;

	//auxiliary variables
	[NonSerialized] public int _LockControls = 0;
	[NonSerialized] private Vector3 PrevPosition = Vector3.zero;
	[NonSerialized] public Vector2 CollDeltaPos = Vector2.zero; //positiveInfinity ADD BOOLEAN
	[NonSerialized] public float k_Slow = 1; //for slow areas
	[NonSerialized] public bool _IsGravitated = false;

	//characteristics
	[NonSerialized] public int health = 100;
	[NonSerialized] public float BaseSpeed = 0.09f*60;
	[NonSerialized] public Vector2 GravitySpeed = new Vector2 ( 0f*60f, -0.003f*60f);
	[NonSerialized] private float MaxRBSpeed = 5.0f*60;
	[NonSerialized] public float BrakeSpeed = 0.003f*60;



    void Start() 
    {
        rb = this.GetComponent<Rigidbody2D>();

        /*
        GameObject dt;
        dt = GameObject.Find("/UICanvas/DebugText");
        dt.GetComponent<UnityEngine.UI.Text>().text  = "";
        dt.GetComponent<UnityEngine.UI.Text>().text += "\nTouchScreenKeyboard = "+TouchScreenKeyboard.isSupported;
        TouchScreenKeyboard.Open("KB_Test");
        //Input.acceleration.x
        */
		
	}

    void FixedUpdate() 
    {
		PrevPosition = rb.position;//transform.position;

		rb.position += CollDeltaPos; CollDeltaPos = Vector2.zero;
		if (_IsGravitated) rb.velocity += GravitySpeed;



        if (_LockControls == 0)
            RecieveInputFromPlayer();
        else
            _LockControls--;

	}


    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


    void RecieveInputFromPlayer()
    {        
        //saTravel
        if (Input.touchCount > 0)
        {
            //if (Input.touches[0].phase == TouchPhase.Began)
            {
                
                Vector2 Delta = Camera.main.ScreenToWorldPoint(Input.touches[0].position) - this.transform.position;
                rb.velocity = Delta.normalized * BaseSpeed;
                //Debug.Log("Speed = " + Speed);
				//Debug.Log("DeltaNormalized = " + Delta.normalized);

            }
        }
    }
}





public enum StarActionType
{
    saFree = 0, saTravel = 1, saDash = 2, saTeleport = 3, saGhost = 4, saJet = 5
}