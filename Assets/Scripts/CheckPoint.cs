using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MyTile
{
    public GameObject PlayerPrefab;
    public Sprite spr_off, spr_on;
        
    protected static CheckPoint ActiveCP;
    [Header("Only one must be active")]
    public bool IsActive = false;

    protected Vector3 SpawnPoint;
    SpriteRenderer sr;

    void Awake()
    {
        if (IsActive)
        {
            if (ActiveCP == null)
                ActiveCP = gameObject.GetComponent<CheckPoint>();
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

        SpawnPoint = transform.position + sr.bounds.extents;
        SpawnPoint.z = 0; //hardfix... i'll check it later
        SwitchCPSprite();
    }    

    void SetCPActive(bool B)
    {
        IsActive = B;
        if (B)
        {
            ActiveCP.SetCPActive(false);
            ActiveCP = gameObject.GetComponent<CheckPoint>();
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


    public static void SpawnPlayer()
    {
        if (ActiveCP == null)
        {
            Debug.LogError("No active CheckPoints in the scene! Must be exactly 1 active!");
            return;
        }

        //creating a player and setting camera to him
        GameObject pl = Instantiate(ActiveCP.PlayerPrefab, ActiveCP.SpawnPoint, Quaternion.identity);
        Camera.main.GetComponent<CameraController>().SetTarget(pl);
    }

    void OnTriggerEnter2D(Collider2D coll)
	{        
		if (coll.gameObject.tag == "Player")
		{
            if (IsActive)
                return;

            SetCPActive(true);
            //Player pl = coll.GetComponentInParent<Player>();
            //pl.CheckPoint = gameObject;			
        }
	}
}
