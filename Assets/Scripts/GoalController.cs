using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour 
{	
    UnityEngine.UI.Text text_field;

	void Start ()
    {
        text_field = GetComponent<UnityEngine.UI.Text>();
	}
	
    public void Refresh(int a1, int a2)
    {
        //text_field.text = string.Format(" Goal (Collect): {0}/{1}", LevelInit.ItemsCollected, LevelInit.ItemGoal);
        text_field.text = string.Format(" Goal (Collect): {0}/{1}", a1, a2);
    }
}
