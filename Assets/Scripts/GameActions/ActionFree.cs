using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionFree : GameAction
{
    const GAtype ThisActionType = GAtype.Free;
    //
    public Vector2 Gravity;
    public Vector2 MaxVelocity = new Vector2(0.25f, 0.25f); //делай больше нуля оба значения
    public GameAction[] PlayerActions; //listens to key input of such actions



    override protected void Awake()
    {
        Type = ThisActionType;
    }

    override protected void StartCustom() //start
    {
        Breakable = true;
    }

    override protected void ProcessCustom() //fixed update
    {
        if (pl.ex_Grav != Vector2.zero) //inside slow area. Change condition into extra boolean instead?
            pl.Speed += pl.ex_Grav;
        else //normal conditions
            pl.Speed += Gravity;

        //ограничение максимальной скорости (каждая ось индивидуально)        
        /*
        if (Mathf.Sign(pl.Speed.x) == Mathf.Sign(MaxVelocity.x))
            if (Mathf.Abs(pl.Speed.x) > Mathf.Abs(MaxVelocity.x))
                pl.Speed = new Vector2 (MaxVelocity.x, pl.Speed.y);
        if (Mathf.Sign(pl.Speed.y) == Mathf.Sign(MaxVelocity.y))
            if (Mathf.Abs(pl.Speed.y) > Mathf.Abs(MaxVelocity.y))
                pl.Speed = new Vector2 (pl.Speed.x, MaxVelocity.y);
        */

        //ограничение максимальной скорости (каждая ось индивидуально, берется всегда модуль)        
        if (Mathf.Abs(pl.Speed.x) > Mathf.Abs(MaxVelocity.x))
            pl.Speed = new Vector2(MaxVelocity.x * Mathf.Sign(pl.Speed.x), pl.Speed.y);
        if (Mathf.Abs(pl.Speed.y) > Mathf.Abs(MaxVelocity.y))
            pl.Speed = new Vector2(pl.Speed.x, MaxVelocity.y * Mathf.Sign(pl.Speed.y));
    }

    override protected void UpdateCustom() //update
    {
        for (int i = 0; i < PlayerActions.Length; i++)
            if (PlayerActions[i] != null)
                PlayerActions[i].RecieveInputFromPlayer();
    }    

    protected override void ActionActivated()
    {
        //base.StartAction();
    }

    protected override void ActionDeactivated()
    {
        //base.FinishAction();
    }

    override public void RecieveInputFromPlayer() //input
    {
    }
}
