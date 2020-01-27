using UnityEngine;

public class ActionFree : GameAction
{
    const GAtype ThisActionType = GAtype.Free;
    //
    public Vector2 Gravity;
    public Vector2 MaxVelocity = new Vector2(0.25f, 0.25f); //используются значения по модулю
    public GameAction[] PlayerActions; //checks key input of these actions



    override protected void Awake()
    {
        Type = ThisActionType;
    }

    override protected void StartCustom() //start
    {
        Breakable = true;

        if (MaxVelocity.x < 0)
            MaxVelocity.x = Mathf.Abs(MaxVelocity.x);
        if (MaxVelocity.y < 0)
            MaxVelocity.y = Mathf.Abs(MaxVelocity.y);
    }

    override protected void ProcessCustom() //fixed update
    {
        if (Equals(pl.ex_Grav,Vector2.zero)) //inside blue area. Change condition into extra boolean instead?            
            pl.Speed += Gravity;

        //ограничение максимальной скорости (каждая ось индивидуально)        
        if (Mathf.Abs(pl.Speed.x) > MaxVelocity.x)
            pl.Speed = new Vector2(MaxVelocity.x * Mathf.Sign(pl.Speed.x), pl.Speed.y);
        if (Mathf.Abs(pl.Speed.y) > MaxVelocity.y)
            pl.Speed = new Vector2(pl.Speed.x, MaxVelocity.y * Mathf.Sign(pl.Speed.y));
    }

    override protected void UpdateCustom() //update
    {
        for (int i = 0; i < PlayerActions.Length; i++)
            if (PlayerActions[i] != null)
                PlayerActions[i].RecieveInputFromPlayer();
    }

    //вызов пустых методов вообще не лучшая практика для в Юнити, но в малых количествах не страшно. Можно переделать под делегаты
    protected override void OnActionActivated()
    {
    }

    protected override void OnActionDeactivated()
    {
    }

    override public void RecieveInputFromPlayer() //input
    {
    }

}
