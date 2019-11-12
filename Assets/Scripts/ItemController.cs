using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour 
{
    GameObject eff_pickitem;
    LevelInit LevelInit; //script

	void Start () 
	{
        eff_pickitem = (GameObject)Resources.Load ("eff_pickitem");
        LevelInit = Camera.main.GetComponent<LevelInit>();

        LevelInit.ItemGoal += 1;
	}
	
    void OnTriggerEnter2D(Collider2D coll)
    {        
        if (coll.gameObject.tag == "Player")
        {
            //animation with sound
            eff_pickitem.transform.localScale = this.transform.localScale;
            Instantiate (eff_pickitem, this.transform.position, Quaternion.identity);

            //goal counter refresh
            LevelInit.ItemsCollected += 1;
            LevelInit.RefreshGoal();

            Destroy(this.gameObject);

            if (LevelInit.ItemsCollected >= LevelInit.ItemGoal)
                LevelInit.Victory();            
        }
    }
}
