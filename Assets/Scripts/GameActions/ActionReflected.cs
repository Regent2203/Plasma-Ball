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
        Breakable = false;
    }

    override protected void ProcessCustom() //fixed update
    {
        switch (ac)
        {
            //43..0
            default: //case 30..3
            {
                if ((ac >= 3) && (ac <= 30))
                    pl.Speed *= 0.93f;

                break;
            }
            case 2:
            {
                pl.Speed = Vector2.zero;
                break;
            }
            case 0:
            {
                Deactivate();
                pl.PerformDefaultAction();
                break;
            }
        }    
    }

    override protected void UpdateCustom() //update
    {        
    }

    protected override void OnActionActivated()
    {        
        base.OnActionActivated();

        AudioInit._Inst.PlayOneShot(sound); //если возникнет слишком много вызовов этого gameaction-а за секунду (подряд), Я ЭТО УСЛЫШУ
    }

    override public void RecieveInputFromPlayer() //input
    {
    }
}
