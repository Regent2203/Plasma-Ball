using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGameActions;
using UnityEngine.EventSystems;

public class StarController : MonoBehaviour 
{
	Rigidbody2D rb;
	ParticleSystem ps;
    GameObject eff_touch1, eff_touch2;

	//auxiliary variables
	[NonSerialized] public float k_Slow = 1; //for slow areas
	//[NonSerialized] public GameObject CheckPoint;

	//characteristics
	[NonSerialized] public int health = 100;
	[NonSerialized] public float BaseSpeed = 36.6f;
	//[NonSerialized] private float MaxSpeed = 20.0f;

	//actions
	List<GameAction> GameActionList;
	GameAction CurAction;


    void Start() 
    {
        rb = this.GetComponent<Rigidbody2D> ();
		ps = this.GetComponent<ParticleSystem> ();
		eff_touch1 = (GameObject)Resources.Load ("eff_touch1");
        eff_touch2 = (GameObject)Resources.Load ("eff_touch2");

		//relocating player to the starting area
		//CheckPoint = GameObject.FindGameObjectWithTag ("StartArea");
		//transform.position = CheckPoint.transform.position;

		//shifting camera to the player
		Camera.main.transform.position = this.transform.position + new Vector3(0, 0, Camera.main.transform.position.z);

		//actions
		CreateActionList();
	}

    void FixedUpdate() 
    {
		//Debug.Log ("Move: " + rb.position + "_" + Time.frameCount);

		//spinning the star
		rb.rotation += -0.9f * rb.velocity.magnitude * Mathf.Sign(rb.velocity.x);
		ps.Emit ( (int)(rb.velocity.magnitude / 25.0f));

		//inputs from human player
		RecieveInputFromPlayer ();

		//actions
		PerformAction (CurAction);
		ProcessActions();
	}

	float GetSpeed()
	{
		return BaseSpeed * k_Slow;
	}

	//-----------------------------------------Inputs from human player-----------------------------------------------

    void RecieveInputFromPlayer()
    {
        //saTravel
        #if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = Input.GetTouch(0).position;

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                if (results.Count == 0)
                {
    				SetNewCurrentAction ((int)saType.saTravel);
                    CurAction.target = Camera.main.ScreenToWorldPoint (Input.GetTouch(0).position);
                }
            }
        }
        #endif

        #if UNITY_EDITOR || UNITY_WEBGL

        if (Input.GetMouseButtonDown(0))
        {
            
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                SetNewCurrentAction ((int)saType.saTravel);
                CurAction.target = Camera.main.ScreenToWorldPoint (Input.mousePosition);
            }
        }
        #endif

        //
    }



	//-----------------------------------------Game Actions (engine/common)-----------------------------------------------
	void CreateActionList()
	//starting set of skills
	{
		GameActionList = new List<GameAction>();

		//for (int i = 0; i <= Enum.GetNames(typeof(gaType)).Length - 1; i++)
		for (int i = 0; i <= 2; i++)
		{
			GameAction GA = new GameAction(i);
			GetCustomPresetsForAction(GA);
			GameActionList.Add(GA);
		}

		CurAction = GameActionList[0];
	}
		
	void ProcessActions()
	//processing all your actions
	{            
		for (int i = GameActionList.Count - 1; i > 0; i--) //>=0, but ignore "free"
		{
			GameActionList [i].ProcessAction ();
			if (GameActionList [i].ac == 0)
				GetCustomPresetsForAction (GameActionList [i]);			
		}
	}

	public bool SetNewCurrentAction(int NAtype, bool IgnoreCD = false, bool IgnoreBreak = false)
	//use THIS method in player controls
	{
		GameAction NewAction; //new action (temporary action)
		NewAction = GameActionList[(int)NAtype];

		//if scripted action doesn't belong to this character at all
		if (NewAction == null)
		{
			Debug.Log ("ERROR: gameobject <"+this.name+"> failed to set action ("+NAtype+") as current. [StarController.cs]");
			return false;
		}

		if ((!IgnoreCD) && (NewAction.CheckAvailable()==false))
			return false; //new action currently has cooldown or delay, or has zero charges        
		
		if ((!IgnoreBreak) && (CurAction.Breakable == false))
			return false; //current action either must get to its end (ac==0), or it must be breakable
		
		//success!
		{
			CurAction = NewAction;

			CurAction.ac = CurAction.ac_max;
			if (CurAction.MaxCharges != -1) 
				CurAction.CurCharges = CurAction.MaxCharges - 1;
			CurAction.CurCooldown = CurAction.MaxCooldown;

			//Debug.Log("new action is set: " + CurAction.id);
			return true;
		}
	}



	//-----------------------------------------Game Actions (custom)-----------------------------------------------
	void GetCustomPresetsForAction(GameAction ga)
	//starting techical parameters for each action
	{
		ga.GetCommonPresetsForAction();

		switch (ga.id)
		{
		case (int)saType.saFree:
			{
				ga.ac_max = -1;
				ga.Breakable = true;
				break;
			}
		case (int)saType.saTravel:
			{
				ga.ac_max = 22;
				ga.Breakable = false;
				break;
			}		
        case (int)saType.faReflect:
            {
                ga.ac_max = 42;
                ga.Breakable = false;
                break;
            }

		}
	}

	void PerformAction(GameAction ThisAction)
	//script itself
	{
		switch (ThisAction.id) 
		{
    		case (int)saType.saTravel:
    			{
    				switch (CurAction.ac)
    				{
        				case 22:
        					{
        						//calculate velocity to reach the target
        						Vector2 Delta = CurAction.target - (Vector2)this.transform.position;
        						rb.velocity = (Delta * 60) / 13.6f; //velocity

        						if (rb.velocity.magnitude > BaseSpeed) 
                                {
                                    //Debug.Log ("Too far");
                                    rb.velocity = rb.velocity.normalized * BaseSpeed;
        							
                                    //creates visual effect at target coordinates
                                    Instantiate (eff_touch2, CurAction.target, Quaternion.identity);
        						}
                                else
                                    Instantiate (eff_touch1, CurAction.target, Quaternion.identity);
        						
        						rb.velocity *= k_Slow;
        						//rb.velocity = Delta.normalized * GetSpeed(); /* old variant */
        						rb.gravityScale = 0;
        						break;
        					}                
                        default: //case 13..3
                            {
                                if (CurAction.ac == 13)
                                    CurAction.Breakable = true;
                                
                                if ((CurAction.ac >= 3) && (CurAction.ac <= 12))
                                    rb.velocity *= 0.8f;                                
                                
                                break;
                            }
        				case 2:
        					{
        						rb.velocity = Vector2.zero;
        						rb.gravityScale = 1;
        						break;
        					}
        				case 1:
        					{
        						SetNewCurrentAction(0, false, true);
        						break;
        					}

    				}
    				break; 
                }//###_end_of_case saTravel

            case (int)saType.faReflect:
                {
                    switch (CurAction.ac)
                    {
                        case 42:
                            {
                                //rb.velocity *= k_Slow;
                                break;
                            }
                        default: //case 13..3
                            {
                                if (CurAction.ac == 13)
                                    CurAction.Breakable = true;

                                if ((CurAction.ac >= 3) && (CurAction.ac <= 30))
                                    rb.velocity *= 0.93f;

                                break;
                            }
                        case 2:
                            {
                                rb.velocity = Vector2.zero;
                                break;
                            }
                        case 1:
                            {
                                SetNewCurrentAction(0, false, true);
                                break;
                            }

                    }
                    break; 
                }//###_end_of_case faReflect


		}//@@@_Switch.Action


	}



	public enum saType //StarActionType
	{
        //star action, forced action
        saFree = 0, saTravel = 1, faReflect = 2, saTeleport = 3, saGhost = 4, saJet = 5 
	}

}