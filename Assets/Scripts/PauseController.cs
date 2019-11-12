using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseController : MonoBehaviour 
{
    public GameObject pnl_main, pnl_confirm, pnl_tutorial;
    public UnityEngine.UI.Text confirm_text;
    int case_confirm;

	void Start () 
	{
        Time.timeScale = 0;
        if (!pnl_tutorial.activeSelf)
        {            
            StartCoroutine(StartTheGame());
        }
	}

    IEnumerator StartTheGame() 
    {
        yield return new WaitForSecondsRealtime(1);
        Time.timeScale = 1;
    }

    //OnButtonClick
	public void _ClickButton(int B)
	{
        if (GameInit.audio_sfx != null)
            GameInit._PlaySound_Select();
        
        switch (B)
        {
            case 0: //activate pause-menu
                {
                    if (Time.timeScale != 0)
                    {
                        Time.timeScale = 0;
                        pnl_main.SetActive(true);
                    }
                    break;
                }
            case 1: //restart lvl
                {
                    pnl_main.SetActive(false);
                    pnl_confirm.SetActive(true);
                    case_confirm = 1;
                    confirm_text.text = "Restart";
                    //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    break;
                }
            case 2: //main menu
                {
                    pnl_main.SetActive(false);
                    pnl_confirm.SetActive(true);
                    case_confirm = 2;
                    confirm_text.text = "Main Menu";
                    //SceneManager.LoadScene("MainMenu");
                    break;
                }
            case 3: //continue
                {
                    Time.timeScale = 1;
                    pnl_main.SetActive(false);
                    break;
                }
            case -1: //confirm - yes
                {
                    Time.timeScale = 1;
                    if (GameInit.audio_bgm != null)
                        GameInit.audio_bgm.Stop();

                    switch (case_confirm)
                    {
                        case 1:
                            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                            break;
                        case 2:
                            SceneManager.LoadScene("MainMenu");
                            break;
                    }
                    break;
                }
            case -2: //confirm - no
                {
                    pnl_confirm.SetActive(false);
                    pnl_main.SetActive(true);
                    break;
                }
            case -3: //tutorial - close
                {
                    Time.timeScale = 1;
                    pnl_tutorial.SetActive(false);
                    break;
                }
        }
        
	}

	


}
