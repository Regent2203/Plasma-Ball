using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerController : MonoBehaviour 
{   
    static UnityEngine.UI.Text text_field;

    void Start ()
    {
        text_field = GetComponent<UnityEngine.UI.Text>();
        //Refresh();
    }

    void Update()
    {
        Refresh();
    }

    public static void Refresh()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(Time.timeSinceLevelLoad);
        text_field.text = string.Format(" Time: {0:00}:{1:00}.{2:0}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds /100);
    }
}
