using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour 
{
	UnityEngine.UI.Text fps_text;

	void Start () 
	{
		fps_text = GameObject.Find("/UICanvas/Text_FPS").GetComponent<UnityEngine.UI.Text>();
        
		//TouchScreenKeyboard.Open("KB_Test");
        //Input.acceleration.x
		//Debug.Log ("Math: " + Mathf.MoveTowards(8,10,-3));

	}
	
	void Update () 
	{		
		float fps;

		fps = 1.0f / Time.deltaTime;
		fps_text.text  = "FPS: " + fps;
		//Debug.Log (fps);
	}
}
