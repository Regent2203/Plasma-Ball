using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInit : Singleton<GameInit>
{
    //todo: statistics like "total time played", unlocks, etc    
    public bool Paused { get; protected set; }


    void Start() 
	{
		DontDestroyOnLoad(gameObject);

        //force screen orientation
        //Screen.orientation = ScreenOrientation.Landscape;        
	}  

    public void SetPause(bool B)
    {
        if (B)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;

        Paused = B;
    }        

    public IEnumerator LoadStage(int N)
    {
        yield return new WaitForSecondsRealtime(1.5f);
        
        if (N > 9) /*zaglushka - predel sozdannyh mnoyu urovnei*/
        {
            Time.timeScale = 1;
            LoadMenuScene();
            yield break;
        }
        
        SceneManager.LoadScene("Level" + N.ToString("00"));
    }

    public void LoadMenuScene()
    {
        Destroy(AudioInit._Inst.gameObject); //это фикс бага со слайдерами (dont destroy on load + missing object) при возвращении в меню
        Destroy(GameInit._Inst.gameObject); //та же фигня, но для кнопок выбора уровня. TODO: перемистить эту функцию на досуге??
        SceneManager.LoadScene("MainMenu");
    }
}
