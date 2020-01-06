using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelInit : MonoBehaviour 
{
    public static LevelInit _Inst { get; private set; }
    //
    int LevelNumber { get; set; } //number of current level
    public int item_Picked { get; protected set; } = 0;
    public int item_Goal { get; protected set; } = 0;
    


    void Awake()
    {
        _Inst = gameObject.GetComponent<LevelInit>();
    }

    void Start()
	{
        LevelNumber = int.Parse(SceneManager.GetActiveScene().name.Substring(5,2));

        //TODO: load highscore from playerprefs?
        /*BestScore*/

        //starting the game
        //check "menu_IngameMenu"
        //GameInit._Inst.SetPause(true);

        //spawn player
        CheckPoint.SpawnPlayer();
    }

    //------------------------items------------------------------------
    public void AddPickItem()
    {
        item_Picked += 1;
        iface_Goal._Inst.Refresh();

        if (item_Picked >= item_Goal)
            Victory();
    }

    public void AddGoalItem()
    {
        item_Goal += 1;
        iface_Goal._Inst.Refresh();
    }

    //------------------------win and lose-----------------------------
    //TODO: lose and respawn, more win conditions/goals (finish point, +limited in time, +destroy enemy plasma)
    void Victory()
    {
        Time.timeScale = 0.3f;        
        GameInit._Inst.StartCoroutine("LoadStage", LevelNumber+1);
    }
	
    
}
