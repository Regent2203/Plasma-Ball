using System;
using UnityEngine;

public class iface_Health : MonoBehaviour 
{
    public static iface_Health _Inst { get; private set; }
    UnityEngine.UI.Text text_field;    

    void Awake()
    {
        _Inst = GetComponent<iface_Health>();
        text_field = GetComponent<UnityEngine.UI.Text>();
    }

    public void Refresh()
    {
        text_field.text = string.Format(" Health: {0}", LevelInit._Inst.Player.Health);
    }
    
    
}
