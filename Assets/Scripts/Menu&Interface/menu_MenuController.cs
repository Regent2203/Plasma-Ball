using UnityEngine;
using UnityEngine.SceneManagement;

public class menu_MenuController : MonoBehaviour 
{
	Animator ar;

	void Start () 
	{
		ar = GetComponent<Animator> ();
	}

    //OnButtonClick
	public void _SetSubmenu(int M)
	{
		ar.SetInteger ("MState", M);
        AudioInit._Inst._PlaySound_Select();        
	}

	//Exit
	public void _Exit ()
	{
		//if (Input.GetKeyDown(KeyCode.Escape))
		Application.Quit();
	}

	//Play chosen stage
    public void _PlayStage(int N)
	{
        //GameInit._Inst.StartCoroutine("LoadStage", N);
        SceneManager.LoadScene("Level" + N.ToString("00"));
    }


}
