using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    Transform tr;
    Collider2D cldr;
    ParticleSystem ps;
    SpriteRenderer sr;

    public CompositeCollider2D World_Collider; //gameobject "TilesGrid", containg composite collider of all solid tiles in the level


    //auxiliary
    public float k_Slow { get; set; } = 1; //for slow areas
    public Vector2 ex_Grav { get; set; } = Vector2.zero; //for gravity areas
    public Vector2 Speed { get; set; }    
    public Vector2 Push { get; set; } //NIY (TODO)


    //in-game
    public int Health = 100; //TODO: BaseHealth, CurrentHealth
    public float WallReflectSpeed = 0.1f;
    public int WallReflectDamage = 5; //переделать в будущем индивидуально под каждый кусок стены? Пока так сойдет.

    //gameactions
    public GameAction DefaultAction;
    public GameAction CurAction { get; protected set; }    
    public Dictionary<GAtype,GameAction> GameActionList { get; protected set; }

    //events
    //public event Action OnPlayerKilled;



    void Start() 
    {
        tr = transform;
        cldr = GetComponent<CircleCollider2D>();
        ps = GetComponent<ParticleSystem>();
        sr = GetComponent<SpriteRenderer>();

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
        
        if (World_Collider == null)
        {
            Debug.LogWarningFormat("World collider ('TilesGrid') is not set for {0}. Please fix it in Unity Editor.", gameObject);
            World_Collider = GameObject.Find("/TilesGrid").GetComponent<CompositeCollider2D>();
        }
    }

    void FixedUpdate() 
    {
        CheckCollision();

        if (CurAction.Type != GAtype.Reflected)
            if (!Equals(ex_Grav, Vector2.zero)) //inside blue area. Change condition into extra boolean instead?
                Speed += ex_Grav;

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
        World_Collider.GenerateGeometry(); //да, именно в таком порядке.

        if (n > 0)
        {
            //Debug.Log(n);
            Vector2 normal = Vector2.zero;
            for (int i = 0; i < n; i++)
            {
                normal += cont[i].normal;
            }
            ReflectVelocity(normal.normalized);
            //TODO: добавить в будущем задержку от многократных моментальных срабатываний, но сперва обдумать - а надо ли
            //либо заменить проверкой на "расплющивание с двух сторон" (как в Сонике), если есть два взаимнообратных контактпоинта

            ReduceHealth(WallReflectDamage);
            DamageSFX(normal);
        }

        
    }

    void ReflectVelocity(Vector2 normal)
    {
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
     
    public void ReduceHealth(int dmg)
    {
        //уменьшаем здоровье
        Health -= dmg;
        iface_Health._Inst.Refresh();

        if (Health <= 0)
            KillPlayer(true);
    }

    void DamageSFX(Vector2 dir)
    {
        //визуальный эффект
        var shape = ps.shape;
        shape.rotation = new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, dir) - shape.arc / 2 - 180);
        ps.Emit(WallReflectDamage * 1);
    }

    public void KillPlayer(bool DoRespawn)
    {
        //gameObject.SetActive(false);
        cldr.enabled = false;
        this.enabled = false;
        sr.enabled = false;
        
        Speed = Vector2.zero;
        PerformNewAction(GAtype.Free, true, true);
        CurAction.enabled = false;

        //OnPlayerKilled();

        if (DoRespawn)
            CheckPoint.ActiveCP.StartCoroutine("SpawnPlayer", 2.0f);
    }

    public void RevivePlayer()
    {
        //gameObject.SetActive(true);
        cldr.enabled = true;
        this.enabled = true;
        sr.enabled = true;

        CurAction.enabled = true;

        Health = 100; //zaglushka, peredelay kak-nibud'! TODO: 
        iface_Health._Inst.Refresh();
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

    public void PerformNewAction(GAtype action_type, bool IgnoreCD = false, bool IgnoreBreak = false)
    //use THIS method in input modules
    {
        GameAction NewAction = GameActionList[(action_type)];

        //if no such action exist here        
        if (NewAction == null)
        {
            Debug.LogErrorFormat("Gameobject {0} failed to set action {1} as current.", gameObject, action_type);
            return;
        }

        PerformNewAction(NewAction, IgnoreCD, IgnoreBreak);
    }

    public void PerformNewAction(GameAction NewAction, bool IgnoreCD = false, bool IgnoreBreak = false)
	//use THIS method in input modules
	{           
        /*
		if ((!IgnoreCD) && (NewAction.CheckAvailable()==false))
			return; //new action currently has cooldown or delay, or has zero charges        
		*/
		if ((!IgnoreBreak) && (CurAction.Breakable == false))
			return; //current action either must get to its end (ac==-1), or it must be breakable
		
		//success!
		{
            CurAction.Deactivate();
			CurAction = NewAction;
            CurAction.Activate();            

			//Debug.LogFormat("{0}: new action is set: {1}", gameObject, CurAction.Type);			
		}
	}
    
    public void PerformDefaultAction()
    {
        CurAction = DefaultAction;
        CurAction.Activate(); //вообще не нужно для ActionFree, но на будущее...
    }
}