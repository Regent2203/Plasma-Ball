#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace MyCustomEditors
{
    [CustomEditor(typeof(MyTile), true)] 
    [CanEditMultipleObjects]
    public class MyTile_Editor : Editor
    {
        Transform tr;
        SpriteRenderer sr;
        BoxCollider2D bc;
        Grid grid;
        

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            //делает нормальный размер тайла в редакторе
            if (GUILayout.Button("Correction!")) 
            {
                foreach (MyTile tile in targets)
                {
                    tr = tile.transform;
                    sr = tile.GetComponent<SpriteRenderer>();
                    bc = tile.GetComponent<BoxCollider2D>();
                    grid = tile.GetComponentInParent<Grid>();
                    
                    //расчеты для спрайта и коллайдера
                    Vector4 s = 0.01f * sr.sprite.border;   //внутренние бордеры спрайта (из окна спрайт эдитора)
                    Vector2 c = 0.01f * sr.sprite.rect.center; //центр текстуры спрайта
                    //Debug.Log(sr.sprite.border); //LBRT - xyzw
                    Vector2 v;
                    v.x = (sr.size.x - s.x - s.z) / (c.x - s.x);
                    v.y = (sr.size.y - s.y - s.w) / (c.y - s.y);
                    v.x = Mathf.RoundToInt(v.x) * (c.x - s.x) + s.x + s.z;
                    v.y = Mathf.RoundToInt(v.y) * (c.y - s.y) + s.y + s.w;

                    //фиксим рисунок спрайта
                    sr.size = v;

                    //фиксим коллайдер
                    if (bc != null)
                    {
                        if (!bc.isTrigger) //стенки
                        {
                            bc.size = v;
                            bc.offset = bc.size * -0.5f;
                        }
                        else //зоны
                        {
                            bc.size = v - new Vector2(s.x + s.z, s.y + s.w);
                            bc.offset = (bc.size + new Vector2(s.x + s.z, s.y + s.w)) * -0.5f;
                        }
                    }

                    //расчеты для позиции
                    //Debug.Log(Vector3.Scale(grid.cellSize + grid.cellGap, grid.LocalToCell(tr.localPosition)));
                    Vector3 p;
                    p.x = tr.localPosition.x / (grid.cellSize.x + grid.cellGap.x);
                    p.y = tr.localPosition.y / (grid.cellSize.y + grid.cellGap.y);
                    p.x = Mathf.RoundToInt(p.x) * (grid.cellSize.x + grid.cellGap.x);
                    p.y = Mathf.RoundToInt(p.y) * (grid.cellSize.y + grid.cellGap.y);
                    p.z = 0;

                    //фиксим позицию
                    tr.localPosition = p;
                    
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

                var newbtn = Instantiate(script.ButtonPrefab, script.transform).transform;                
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
    }
}
#endif