using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reflector : MyTile
{
    public float coef_Bounce = 1;
    public float ReflectPower = 0;
    public float ReflectAngle = 0;

    PhysicsMaterial2D mat;
    AudioSource au;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        au = GetComponent<AudioSource>();

        PhysicsMaterial2D DefaultMaterial = rb.sharedMaterial;

        //creating own material for unique bouncy coefficient
        if (DefaultMaterial.bounciness != coef_Bounce)
        {
            mat = Instantiate(DefaultMaterial);
            mat.bounciness = coef_Bounce;
            rb.sharedMaterial = mat;
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            ApplyReflect(coll);
        }
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------

    void ApplyReflect(Collision2D coll)
    {
        Rigidbody2D crb = coll.gameObject.GetComponent<Rigidbody2D>();
        PlayerController pl = coll.gameObject.GetComponent<PlayerController>();

        //Debug.Log(coll.contacts[0].point);
     
        Vector2 refl = new Vector2
            (
                ReflectPower * Mathf.Cos(Mathf.Atan2(coll.transform.position.y - coll.contacts[0].point.y, coll.transform.position.x - coll.contacts[0].point.x) + ReflectAngle),
                ReflectPower * Mathf.Sin(Mathf.Atan2(coll.transform.position.y - coll.contacts[0].point.y, coll.transform.position.x - coll.contacts[0].point.x) + ReflectAngle)
            );
        //Debug.Log(refl);

        crb.velocity += refl;// * pl.k_Slow;
        crb.gravityScale = 1;
        //pl.SetNewCurrentAction(2, true, true);
        au.Play();
    }
}
