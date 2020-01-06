#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public class e_MyTile : MonoBehaviour
{
    SpriteRenderer sr;
    BoxCollider2D bc;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        bc = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        Vector4 s = 0.01f * sr.sprite.border;  //внутренние бордеры спрайта (из спрайт эдитора)
        //Debug.Log(sr.sprite.border); //LBRT - xyzw
        Vector2 v;
        v.x = (sr.size.x - s.z) / s.x;
        v.y = (sr.size.y - s.w) / s.y;
        v.x = Mathf.RoundToInt(v.x) * s.x + s.z;
        v.y = Mathf.RoundToInt(v.y) * s.y + s.w;

        //тайлы спрайта
        sr.size = v;

        //коллайдер
        if (bc != null)
        {
            bc.size = v;
            bc.offset = bc.size * 0.5f;
        }

        //Debug.Log("Works.");
    }
}
#endif