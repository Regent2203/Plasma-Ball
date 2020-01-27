using UnityEngine;

public class MyParticle : MonoBehaviour 
{
	public int Steps;
    [Header("Parameters")] //f is for final
    public Color fColor = new Color(1, 1, 1, 0);
    public Vector2 fScale = new Vector2(1, 1);
    public Vector2 fShift = Vector2.zero;

    private Color dColor;
    private Vector3 dScale;
    private Vector3 dShift;

    SpriteRenderer sr;
    Transform tr;


	void Start() 
	{
        if (Steps <= 0)
        {
            enabled = false;
            Debug.LogErrorFormat("{0}, script MyParticle: incorrect steps number.", gameObject);
            return;
        }

		sr = GetComponent<SpriteRenderer>();
        tr = transform;

        dColor = (fColor - sr.color) / Steps;
        dScale = (new Vector3 (fScale.x, fScale.y, 1) - tr.localScale) / Steps;
        dShift = fShift / Steps;
	}
	

	void FixedUpdate() 
	{
		sr.color += dColor;
		transform.localScale += dScale;
		transform.position += dShift;

		Steps--;
        if (Steps == 0)
        {
            //gameObject.SetActive(false);
            Destroy(gameObject);
        }
	}
}
