using System;
using UnityEngine;


public enum GAtype //GameActionType
{
    //player's actions, including forced actions
    Unassigned = -1, Free = 0, Dash = 1, Reflected = 2
}

[RequireComponent(typeof(Player))]
public abstract class GameAction : MonoBehaviour
{
    public GAtype Type { get; protected set; } = GAtype.Unassigned;
    
    protected Player pl;
    protected Rigidbody2D rb;
    protected ParticleSystem ps;

    //NIY
    //int CurCharges, MaxCharges;
    //int CurCooldown, MaxCooldown;

    public bool Breakable { get; protected set; } = false; //can be stopped/overriden while performing
    public bool IsActive { get; protected set; } = false;
    //public int delay; //one action can add delay to another action(s)
    protected int ac = -1, ac_max = -1; //action counter (timer)
	


    protected abstract void Awake(); //Type = ThisActionType; 

    void Start()
	{
        if (Type == GAtype.Unassigned)
        {            
            Debug.LogErrorFormat("{0}: has gameaction '{1}' with unassigned type.", gameObject, this.GetType());
            enabled = false;
            return;
        }

        pl = GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();
        ps = GetComponent<ParticleSystem>();        
        
        StartCustom(); //custom for each action
	}    

    void FixedUpdate()
    {
        ProcessBasicParameters();

        if (IsActive)
            ProcessCustom();
    }

    void Update()
    {
        if (GameInit._Inst.Paused)
            return;

        if (IsActive)
            UpdateCustom();
    }

    protected abstract void StartCustom();
    protected abstract void ProcessCustom();
    protected abstract void UpdateCustom();
    public abstract void RecieveInputFromPlayer();

    protected virtual void ActionActivated() //характерно для большинства
    {
        ac = ac_max;
        /*
		if (CurAction.MaxCharges != -1) 
			CurAction.CurCharges = CurAction.MaxCharges - 1;
		CurAction.CurCooldown = CurAction.MaxCooldown;
        */
    }

    protected virtual void ActionDeactivated() //характерно для большинства
    {
        ac = -1;
    }

    //---------------------------------------------------------------------------Common-------------------------------------------------------------------------------
    public void Activate()
    {
        IsActive = true;
        ActionActivated();
    }
    public void Deactivate()
    {
        IsActive = false;
        ActionDeactivated();
    }

    


	//if action is not on cooldown and can be performed
    /*
	public bool CheckAvailable()
	{
        
        if (delay > 0)
            return false;

        if ((CurCooldown==-1) || (CurCharges>0))
        
        return true;
    }
    */

	//doing all that "add +1 to action counter"
	protected void ProcessBasicParameters ()
	{
        /*
		if (CurCooldown>0)
		{
			CurCooldown--;

			if (CurCooldown == 0)
			{
				//no charges
				if (MaxCharges == -1)
					CurCooldown = -1;
				//has charges
				else
				{
					CurCharges += 1;

					if (CurCharges == MaxCharges)
						CurCooldown = -1;
					else 
						CurCooldown = MaxCooldown;
				}
			}
		}
        */
		//if (delay>0) delay--;

		if (ac>0) ac--;
	}            
}


