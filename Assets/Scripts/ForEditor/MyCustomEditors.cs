#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace MyCustomEditors
{
    [CustomEditor(typeof(MyTile), true)] 
    [CanEditMultipleObjects]
    public class MyTile_Editor : Editor
    {
        SpriteRenderer sr;
        BoxCollider2D bc;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            //делает нормальный размер тайла в редакторе
            if (GUILayout.Button("Correction!")) 
            {
                foreach (MyTile tile in targets)
                {
                    sr = tile.GetComponent<SpriteRenderer>();
                    bc = tile.GetComponent<BoxCollider2D>();

                    Vector4 s = 0.01f * sr.sprite.border;  //внутренние бордеры спрайта (из спрайт эдитора)
                    //Debug.Log(sr.sprite.border); //LBRT - xyzw
                    Vector2 v;
                    v.x = (sr.size.x - s.z) / s.x;
                    v.y = (sr.size.y - s.w) / s.y;
                    v.x = Mathf.CeilToInt(v.x) * s.x + s.z;
                    v.y = Mathf.CeilToInt(v.y) * s.y + s.w;

                    //тайлы спрайта
                    sr.size = v;

                    //коллайдер
                    if (bc != null)
                    {
                        bc.size = v;
                        bc.offset = bc.size * 0.5f;
                    }
                }
            }
        }
    }

    [CustomEditor(typeof(e_LevelButtons), true)]
    public class LevelButtons_Editor : Editor
    {
        e_LevelButtons script;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            //добавляет следующую кнопку с номером уровня (OnClick надо прописывать вручную)
            if (GUILayout.Button("Add new button"))
            {
                script = (e_LevelButtons)target;
                string number = (script.transform.childCount + 1).ToString("00");

                Transform newbtn = Instantiate(script.ButtonPrefab, script.transform).transform;                
                newbtn.position = Vector2.zero;
                newbtn.name = "lvl_" + number;
                newbtn.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = number;                
            }

            //упорядочивает все кнопки
            if (GUILayout.Button("Put in order"))
            {
                script = (e_LevelButtons)target;

                float area_width = script.GetComponent<RectTransform>().rect.width;
                int btn_quantity = script.transform.childCount;
                Vector2 btn_size = script.ButtonPrefab.GetComponent<RectTransform>().rect.size;                
                float interval = (area_width - script.Offset.x * 2 - btn_size.x * script.ButtonsInRow) / (script.ButtonsInRow - 1);

                for (int k = 0, j = 0, i = 0; k < btn_quantity; k++)
                {
                    i = k % script.ButtonsInRow;
                    j = k / script.ButtonsInRow;
                    script.transform.GetChild(k).localPosition = new Vector3(script.Offset.x+(btn_size.x+interval)*i, -(script.Offset.y+(btn_size.y+interval)*j), 0);
                }

            }
        }

        void ButtonClicked(int buttonNo)
        {
            //Output this to console when the Button3 is clicked
            Debug.Log("Button clicked = " + buttonNo);
        }
    }
}
#endif