using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifetimeParticle : MonoBehaviour 
{
	public int Lifetime;
	public float dAlpha;
	public Vector3 dScale;
	public Vector3 Speed;

	SpriteRenderer sr;

	void Start () 
	{
		sr = this.GetComponent<SpriteRenderer> ();
	}
	

	void FixedUpdate () 
	{
		sr.color = new Color (sr.color.r, sr.color.g, sr.color.b, sr.color.a - dAlpha);
		transform.localScale += dScale;
		transform.position += Speed;

		Lifetime--;
		if (Lifetime == 0)
			Destroy (gameObject);
	}
}
