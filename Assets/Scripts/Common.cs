using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGameActions
{
	//-----------------------------------------Game Actions--------------------------------------------
	public class GameAction
	{
		public int CurCharges, MaxCharges;
		public int CurCooldown, MaxCooldown;
		public int ac, ac_max; //action counter (timer), start_AC
		public bool Breakable; //can be stopped/overriden while performing
		public int Delay; //one action can add delay to another action
		public int id;

		public Vector2 target;
		public int ChargeRate;
		//parameters used for different purposes: all custom
		public float a1, a2, a3;
		public Vector2 av;

		public GameAction(int index)
		{
			id = index;
			GetCommonPresetsForAction();
			//GetCustomPresetsForAction(); //not in this file
		}

		//inner technical parameters (same for all actions)
		public void GetCommonPresetsForAction()
		{
			Delay = 0;

			ac = -1; //ac_max
			MaxCooldown = -1; CurCooldown = -1;
			MaxCharges  = -1; CurCharges = MaxCharges;

			Breakable = false;
			target = new Vector2(float.NaN, float.NaN);
			ChargeRate = -1;

			a1 = float.NaN;
			a2 = float.NaN;
			a3 = float.NaN;
			av = new Vector2 (float.NaN, float.NaN);
		}

		//if action is not on cooldown and can be performed
		public bool CheckAvailable ()
		{
			if (  (Delay==0)  &&  ((CurCooldown==-1) || (CurCharges>0))  )
				return true;
			else return false;
		}

		//doing all that "add +1 to action counter"
		public void ProcessAction ()
		{
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

			if (Delay>0) Delay--;

			if (ac>0) ac--;
		}            
	}

}
