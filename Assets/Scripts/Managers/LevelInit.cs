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

    public CameraController MainCamera;
    public PlayerController Player;


    void Awake()
    {
        _Inst = gameObject.GetComponent<LevelInit>();
    }

    void Start()
	{
        if (MainCamera == null)
        {
            Debug.LogWarning("Main camera is not set in LevelInit!");
            MainCamera = Camera.main.GetComponent<CameraController>();            
        }

        if (Player == null)
        {            
            Debug.LogWarning("Player is not set in LevelInit!");
            Player = GameObject.Find("/ObjectGrid").GetComponent<PlayerController>();
        }

        LevelNumber = int.Parse(SceneManager.GetActiveScene().name.Substring(5,2));

        //TODO: load highscore from playerprefs?
        /*BestScore*/

        //starting the game
        //It's not here anymore. Check "IngameMenu" file
        //GameInit._Inst.SetPause(true);

        //spawn player
        //CheckPoint.ActiveCP.StartCoroutine("SpawnPlayer", 0);
        MainCamera.SetTarget(Player.gameObject); //TODO: возможно, стоит поместить в другое место.
        iface_Health._Inst.Refresh();
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
        Time.timeScale = 0.4f;        
        GameInit._Inst.StartCoroutine("LoadStage", LevelNumber+1);
    }
	
    
}
