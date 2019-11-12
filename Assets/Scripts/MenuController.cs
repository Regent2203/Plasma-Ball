using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour 
{
	Animator ar;

	void Start () 
	{
		ar = GetComponent<Animator> ();
	}

    //OnButtonClick
	public void _SetSubmenu (int M)
	{
		ar.SetInteger ("MState", M);
        GameInit._PlaySound_Select();
        
	}

	//Exit
	public void _Exit ()
	{
		//if (Input.GetKeyDown(KeyCode.Escape))
		Application.Quit ();
	}

	//Play chosen stage
    public void _PlayStage (string S)
	{
		SceneManager.LoadScene ("Stage" + S);
	}


}
