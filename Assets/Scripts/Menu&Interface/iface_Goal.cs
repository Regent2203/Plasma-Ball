using UnityEngine;

public class iface_Goal : MonoBehaviour 
{
    public static iface_Goal _Inst { get; private set; }
    UnityEngine.UI.Text text_field;

	void Awake ()
    {
        _Inst = GetComponent<iface_Goal>();
        text_field = GetComponent<UnityEngine.UI.Text>();        
    }
    
    public void Refresh()
    {
        text_field.text = string.Format(" Collected: {0}/{1}", LevelInit._Inst.item_Picked, LevelInit._Inst.item_Goal);        
    }
}
