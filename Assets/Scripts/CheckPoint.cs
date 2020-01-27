using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MyTile
{    
    public Sprite spr_off, spr_on;
    
    public static CheckPoint ActiveCP;
    [Header("Only one must be active")]
    public bool IsActive = false;

    protected Vector3 SpawnPoint;
    SpriteRenderer sr;


    void Awake()
    {
        if (IsActive)
        {
            if (ActiveCP == null)
                ActiveCP = this;
            else
            {                
                Debug.LogError("Several CheckPoints are active! Fix it in Editor!");
                IsActive = false;
            }
        }
    }

    void Start () 
	{
        sr = GetComponent<SpriteRenderer>();

        SpawnPoint = sr.bounds.center;        
        SwitchCPSprite();
    }    

    void SetCPActive(bool B)
    {
        IsActive = B;
        if (B)
        {
            ActiveCP.SetCPActive(false);
            ActiveCP = this;
        }

        SwitchCPSprite();
    }

    void SwitchCPSprite()
    {
        if (IsActive)
            sr.sprite = spr_on;
        else
            sr.sprite = spr_off;
    }

    void OnTriggerEnter2D(Collider2D coll)
	{
        if (IsActive)
            return;

        if (coll.gameObject.tag == "Player")
		{
            SetCPActive(true);		
        }
	}

    public IEnumerator SpawnPlayer(float t) //под мультиплеер придется всё переделывать
    {
        if (t > 0)
            yield return new WaitForSeconds(t / 2);

        PlayerController pl = LevelInit._Inst.Player;
        pl.transform.position = ActiveCP.SpawnPoint;        

        if (t > 0)
            yield return new WaitForSeconds(t / 2);

        pl.RevivePlayer();
    }    
}
