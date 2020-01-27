using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour 
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
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
		//if (Input.GetKeyDown(KeyCode.Escape))
		Application.Quit();
        #endif
	}

	//Play chosen stage
    public void _PlayStage(int N)
	{
        //GameInit._Inst.StartCoroutine("LoadStage", N);
        SceneManager.LoadScene("Level" + N.ToString("00"));
    }


}
