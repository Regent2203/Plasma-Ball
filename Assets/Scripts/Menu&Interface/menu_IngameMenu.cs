using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class menu_IngameMenu : MonoBehaviour 
{
    enum im_choice { gamepause = 0, restart_lvl = 1, mainmenu = 2, continue_game = 3, confirm_yes = -1, confirm_no = -2, tut_close = -3 }

    public GameObject pnl_main, pnl_confirm, pnl_tutorial;
    public Text confirm_text;
    private im_choice case_confirm;

	void Start () 
	{
        GameInit._Inst.SetPause(true);
        if (!pnl_tutorial.activeSelf)
        {
            StartCoroutine(StartTheGame());
        }

        IEnumerator StartTheGame()
        {
            yield return new WaitForSecondsRealtime(1);
            GameInit._Inst.SetPause(false);
        }
    }    

    //OnButtonClick
	public void _ClickButton(int B)
	{
        AudioInit._Inst._PlaySound_Select();
        
        switch ( (im_choice) B )
        {
            case im_choice.gamepause: //activate pause-menu
            {
                GameInit._Inst.SetPause(true);
                pnl_main.SetActive(true);
                break;
            }   
            case im_choice.restart_lvl: //restart lvl
            {
                pnl_main.SetActive(false);
                pnl_confirm.SetActive(true);
                case_confirm = im_choice.restart_lvl;
                confirm_text.text = "Restart";                    
                break;
            }
            case im_choice.mainmenu: //main menu
            {
                pnl_main.SetActive(false);
                pnl_confirm.SetActive(true);
                case_confirm = im_choice.mainmenu;
                confirm_text.text = "Main Menu";                    
                break;
            }
            case im_choice.continue_game: //continue
            {
                GameInit._Inst.SetPause(false);
                pnl_main.SetActive(false);
                break;
            }
            case im_choice.confirm_yes: //confirm - yes
            {
                GameInit._Inst.SetPause(false); //PauseGame

                switch (case_confirm)
                {
                    case im_choice.restart_lvl:
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                        break;
                    case im_choice.mainmenu:
                        GameInit._Inst.LoadMenuScene();                        
                        break;
                }
                break;
            }
            case im_choice.confirm_no: //confirm - no
            {
                pnl_confirm.SetActive(false);
                pnl_main.SetActive(true);
                break;
            }
            case im_choice.tut_close: //close tutorial window
            {
                GameInit._Inst.SetPause(false);
                pnl_tutorial.SetActive(false);
                break;
            }
        }
        
	}

	


}
