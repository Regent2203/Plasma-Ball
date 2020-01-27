using System;
using UnityEngine;

public class Item : MonoBehaviour 
{
    public AudioClip sound; //OnPickUp
    //
    MyParticle part; //pick-up vfx, starts disabled, since it kills the object when lifetime ends
    Collider2D cldr;

    //public event Action OnPickUp;


	void Start()
	{
        //eff_pickitem = (GameObject)Resources.Load ("eff_pickitem");
        part = GetComponent<MyParticle>();
        cldr = GetComponent<Collider2D>();
        LevelInit._Inst.AddGoalItem();
    }
	
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            //animation and sound
            part.enabled = true;
            cldr.enabled = false;
            AudioInit._Inst.PlayOneShot(sound);
            //Instantiate (eff_pickitem, transform.position, Quaternion.identity);

            //goal counter refresh
            LevelInit._Inst.AddPickItem();
            //Destroy(gameObject);
        }        
    }
}
