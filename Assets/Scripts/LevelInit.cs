using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class LevelInit : MonoBehaviour 
{
    public Vector2 LvlGravity;
    public GameObject GoalController;
    GoalController goal;

    [NonSerialized] public int ItemsCollected = 0, ItemGoal = 0;
    int LvlNumber;

	void Start()
	{
        LvlNumber = int.Parse(SceneManager.GetActiveScene().name.Substring(5,2));
        goal = GoalController.GetComponent<GoalController>();
        RefreshGoal();

	    //force screen orientation
        Screen.orientation = ScreenOrientation.Landscape;

        //load settings from playerprefs
        /*BestScore*/

        //start playing bgm
        if (GameInit.audio_bgm != null)
        {
            GameInit.audio_bgm.clip = (AudioClip)Resources.Load("BGM/bgm01");
            if (!GameInit.audio_bgm.isPlaying)
                GameInit.audio_bgm.PlayDelayed(1);
        }

        //load gravity from script
        Physics2D.gravity = LvlGravity;  //(0.0f, -6.92f)


        //platforms check
        /*
        #if UNITY_ANDROID
        Debug.Log("andro");
        #endif

        #if UNITY_EDITOR
        Debug.Log("editor");
        #endif
        */
	}

    public void Victory()
    {
        Time.timeScale = 0.3f;
        StartCoroutine(LoadNextStage());
    }
	
    IEnumerator LoadNextStage() 
    {
        yield return new WaitForSecondsRealtime(1);
        Time.timeScale = 1;
        if (LvlNumber == 4)
        {
            GameInit.audio_bgm.Stop();
            SceneManager.LoadScene("MainMenu"); /*zaglushka*/
        }
        else
            SceneManager.LoadScene("Stage"+(LvlNumber+1).ToString("00"));
    }

    public void RefreshGoal()
    {
        goal.Refresh(ItemsCollected, ItemGoal);
    }
}
