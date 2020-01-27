#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public class e_MyTile : MonoBehaviour
{
    SpriteRenderer sr;
    BoxCollider2D bc;
    Grid grid;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        bc = GetComponent<BoxCollider2D>();

        grid = transform.parent.GetComponent<Grid>();
    }

    void Update()
    {
        Vector4 s = 0.01f * sr.sprite.border;   //внутренние бордеры спрайта (из спрайт эдитора)
        Vector2 c = 0.01f * sr.sprite.rect.center; //центр текстуры спрайта
        //Debug.Log(sr.sprite.border); //LBRT - xyzw
        Vector2 v;
        /*
        v.x = (sr.size.x - s.z) / s.x;
        v.y = (sr.size.y - s.w) / s.y;
        v.x = Mathf.RoundToInt(v.x) * s.x + s.z;
        v.y = Mathf.RoundToInt(v.y) * s.y + s.w;
        */
        v.x = (sr.size.x - s.x - s.z) / (c.x - s.x);
        v.y = (sr.size.y - s.y - s.w) / (c.y - s.y);
        v.x = Mathf.RoundToInt(v.x) * (c.x - s.x) + s.x + s.z;
        v.y = Mathf.RoundToInt(v.y) * (c.y - s.y) + s.y + s.w;

        //тайлы спрайта
        sr.size = v;

        //коллайдер
        if (bc != null)
        {
            bc.size = v;
            bc.offset = bc.size * -0.5f;
        }

        //grid.
        //Debug.Log("Works."+ grid.WorldToCell(transform.position));
    }
}
#endif