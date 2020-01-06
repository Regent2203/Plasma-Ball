using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    Transform tr;
    Collider2D cldr;
    public CompositeCollider2D World_Collider { get; set; } //gameobject "TilesGrid", containg composite collider of all solid tiles in the level


    //auxiliary
    public float k_Slow { get; set; } = 1; //for slow areas
    public Vector2 ex_Grav { get; set; } = Vector2.zero; //for gravity areas
    public Vector2 Speed { get; set; }    
    public Vector2 Push { get; set; }


    //in-game
    //public GameObject CheckPoint;
    public int health = 100;
    public float WallReflectSpeed = 0.1f;

    //actions
    public GameAction DefaultAction;
    public GameAction CurAction { get; set; }    
    public Dictionary<GAtype,GameAction> GameActionList { get; protected set; }



    void Start() 
    {
        tr = transform;
        cldr = GetComponent<CircleCollider2D>();
		
		//shifting camera to the player
		Camera.main.transform.position = transform.position + new Vector3(0, 0, Camera.main.transform.position.z);

        //actions
        CreateGameActionList();

        //default action assigned in inspector
        if (DefaultAction == null)
        {
            Debug.LogErrorFormat("{0}: has unassigned Default action.", gameObject);
            return;
        }
        if (DefaultAction.gameObject != gameObject)
        {
            Debug.LogErrorFormat("Default action of {0} doesn't belong to it!", gameObject);
            return;
        }
        PerformDefaultAction();

        //used for collisions
        cont = new ContactPoint2D[4];
        fil = new ContactFilter2D();
        fil.layerMask.value = LayerMask.NameToLayer("Enviroment");
        /*
        if (World_Collider == null)
        {
            Debug.LogErrorFormat("World collider ('TilesGrid') is not set for {0}. Please fix it in Unity Editor.", gameObject);            
        }
        */
        World_Collider = GameObject.Find("/TilesGrid").GetComponent<CompositeCollider2D>();
    }

    void FixedUpdate() 
    {
        CheckCollision();

        tr.position += (Vector3)(Speed + Push) * k_Slow;
        Push = Vector2.zero;
    }

    ContactPoint2D[] cont;
    ContactFilter2D fil;    

    void CheckCollision()
    {
        //RaycastHit2D[] res = new RaycastHit2D[4];
        //Collider2D[] res = new Collider2D[4];
        //cldr.Cast(rb.velocity, res));                        

        int n = Physics2D.GetContacts(cldr, World_Collider, fil, cont);
        if (n > 0)
        {
            //if (n > 1) Debug.Log("Player collision contacts: " + n);

            Vector2 normal = Vector2.zero;
            for (int i = 0; i < n; i++)
            {
                normal += cont[i].normal;
            }
            ReflectVelocity(normal.normalized);
        }
    }

    public void ReflectVelocity(Vector2 normal) //(ContactPoint2D cp)
    {
        //Debug.Log(cp.normal);
        //ContactPoint2D cont = coll.GetContact(0); //collision2d

        Vector2 NewSpeed = Speed;
        const float tol = 0.001f; //to prevent getting float-number problems here (-1.44E-07 instead of 0)
        if (Mathf.Abs(normal.x) > tol)
            NewSpeed.x = -WallReflectSpeed * normal.x;
        if (Mathf.Abs(normal.y) > tol)
            NewSpeed.y = -WallReflectSpeed * normal.y;

        Speed = NewSpeed;
        PerformNewAction(GAtype.Reflected, true, true);
    }
        
    /*
    void OnCollisionExit2D(Collision2D coll)
    {
        Debug.Log("Exit");
    }
    void OnCollisionEnter2D(Collision2D coll)
    {
        Debug.Log("Enter");        
    }
    void OnCollisionStay2D(Collision2D coll)
    {
        Debug.LogWarning("Stay");
    }
    */

    //-----------------------------------------Game Actions---------------------------------------------------------

    void CreateGameActionList()
	//list of all actions available to this player (including forced actions)
	{
        GameActionList = new Dictionary<GAtype, GameAction>();

        List<GameAction> _GameActionList = new List<GameAction>();
        GetComponents(_GameActionList);
        
		//for (int i = 0; i < Enum.GetNames(typeof(gaType)).Length; i++)
        foreach (GameAction GA in _GameActionList)		
		{			
			GameActionList.Add(GA.Type, GA);
		}        
    }		
    
	public bool PerformNewAction(GAtype action_type, bool IgnoreCD = false, bool IgnoreBreak = false)
	//use THIS method in input modules
	{
        GameAction NewAction = GameActionList[(action_type)]; //new action (temporary action)

        //if no such action exist here
        if (NewAction == null)
		{
			Debug.LogErrorFormat("Gameobject {0} failed to set action {1} as current.", gameObject, action_type);
			return false;
		}
        /*
		if ((!IgnoreCD) && (NewAction.CheckAvailable()==false))
			return false; //new action currently has cooldown or delay, or has zero charges        
		*/
		if ((!IgnoreBreak) && (CurAction.Breakable == false))
			return false; //current action either must get to its end (ac==-1), or it must be breakable
		
		//success!
		{
            CurAction.Deactivate();
			CurAction = NewAction;
            CurAction.Activate();            

			//Debug.LogFormat("{0}: new action is set: {1}", gameObject, CurAction.Type);
			return true;
		}
	}
    
    public void PerformDefaultAction()
    {
        CurAction = DefaultAction;
        CurAction.Activate(); //вообще не нужно для ActionFree, но на будущее...
    }
}