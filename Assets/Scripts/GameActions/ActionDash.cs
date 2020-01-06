using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActionDash : GameAction
{
    const GAtype ThisActionType = GAtype.Dash;
    //
    public GameObject eff_touch1, eff_touch2; //onmouseclick effects
    public float BaseSpeed = 0.61f;    
    Vector2 StartPos, EndPos;
    
    [Header ("Audio")]    
    public AudioClip sound;


    override protected void Awake()
    {
        Type = ThisActionType;
    }



    override protected void StartCustom() //start
    {
        ac_max = 23;
        //eff_touch1 = (GameObject)Resources.Load("eff_touch1");
        //eff_touch2 = (GameObject)Resources.Load("eff_touch2");
    }

    override protected void ProcessCustom() //fixed update
    {
        switch (ac)
        {
            //23..0
            case 15:
            {
                Breakable = true;
                break;
            }
            default: //case 12..3
            {
                if ((ac >= 3) && (ac <= 12))
                    pl.Speed *= 0.8f;

                break;
            }
            case 2:
            {
                pl.Speed = Vector2.zero;
                break;                
            }
            case 0:
            {
                //Debug.LogFormat("Clicked {0},{1}", EndPos.x, EndPos.y);
                //Debug.LogFormat("Position {0},{1}", rb.position.x, rb.position.y);
                //Debug.LogFormat("Difference {0},{1}", EndPos.x-rb.position.x, EndPos.y-rb.position.y);
                //Debug.Log("Travel distance: " + (StartPos - rb.position).magnitude); // = Travel distance: 8,278006 при скорости 0.61f

                Deactivate();                    
                pl.PerformDefaultAction();
                break;
            }

        }
    }

    override protected void UpdateCustom() //update
    {
        RecieveInputFromPlayer(); //multi-dash
    }

    protected override void ActionActivated()
    {
        base.ActionActivated();
        
        Breakable = false;
        
        //calculate velocity to reach the target                                        
        pl.Speed = (EndPos - StartPos) * 0.07369f; //0,0736892435207222608922970096905f  = BaseSpeed / (StartPos - rb.position).magnitude

        if (pl.Speed.magnitude > BaseSpeed)
        {
            //Debug.Log ("Too far");
            pl.Speed = pl.Speed.normalized * BaseSpeed;

            //creates visual effect at target coordinates
            Instantiate(eff_touch2, EndPos, Quaternion.identity); //заменить на пулинг единственного экземпляра??
        }
        else
            Instantiate(eff_touch1, EndPos, Quaternion.identity);
        
        AudioInit._Inst.PlayOneShot(sound);
    }

    protected override void ActionDeactivated()
    {
        base.ActionDeactivated();
    }


    override public void RecieveInputFromPlayer() //input
    {
        #if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                //здесь мы исключаем нажатие на кнопки интерфейса (на кнопку "меню" в правом верхнем углу)
                List<RaycastResult> results = new List<RaycastResult>();
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = Input.GetTouch(0).position;                
                EventSystem.current.RaycastAll(pointerData, results);
                
                if (results.Count == 0)
                {                    
                    StartPos = rb.position;
                    EndPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                    pl.PerformNewAction(Type);
                }
            }
        }
        #endif

        #if UNITY_EDITOR || UNITY_WEBGL
        if (Input.GetMouseButtonDown(0))
        {
            //здесь мы исключаем нажатие на кнопки интерфейса (на кнопку "меню" в правом верхнем углу)
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                StartPos = rb.position;
                EndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pl.PerformNewAction(Type);
            }
        }
        #endif
    }    
}