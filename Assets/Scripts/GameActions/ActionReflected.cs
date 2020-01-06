using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionReflected : GameAction
{
    const GAtype ThisActionType = GAtype.Reflected;
    //
    [Header("Audio")]
    public AudioClip sound;

    override protected void Awake()
    {
        Type = ThisActionType;
    }

    override protected void StartCustom() //start
    {
        ac_max = 43;
    }

    override protected void ProcessCustom() //fixed update
    {
        switch (ac)
        {
            //43..0
            case 42:
            {
                //ps.Emit(10);
                break;
            }
            case 31:
            {
                Breakable = true;
                break;
            }
            default: //case 30..3
            {
                if ((ac >= 3) && (ac <= 30))
                    pl.Speed *= 0.93f;

                break;
            }            
            case 0:
            {
                pl.Speed = Vector2.zero;
                Deactivate();
                pl.PerformDefaultAction();
                break;
            }
        }

    /*
    Vector2 refl = new Vector2
        (
        ReflectPower * Mathf.Cos(Mathf.Atan2(coll.transform.position.y - coll.contacts[0].point.y, coll.transform.position.x - coll.contacts[0].point.x) + ReflectAngle),
        ReflectPower * Mathf.Sin(Mathf.Atan2(coll.transform.position.y - coll.contacts[0].point.y, coll.transform.position.x - coll.contacts[0].point.x) + ReflectAngle)
        );
    */
    }

    override protected void UpdateCustom() //update
    {
        
    }

    protected override void ActionActivated()
    {        
        base.ActionActivated();
        
        Breakable = false;

        AudioInit._Inst.PlayOneShot(sound); //убедись, что не возникает многократных вызовов gameaction-а (ну, ты конечно услышишь)
    }

    protected override void ActionDeactivated()
    {
        base.ActionDeactivated();
    }

    override public void RecieveInputFromPlayer() //input
    {
    }
}
