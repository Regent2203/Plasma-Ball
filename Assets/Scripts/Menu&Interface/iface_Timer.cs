using System;
using UnityEngine;

public class iface_Timer : MonoBehaviour 
{
    UnityEngine.UI.Text text_field;

    void Awake()
    {
        text_field = GetComponent<UnityEngine.UI.Text>();
    }

    void Update()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(Time.timeSinceLevelLoad);
        text_field.text = string.Format(" Time: {0:00}:{1:00}.{2:0}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds /100);
    }
}
