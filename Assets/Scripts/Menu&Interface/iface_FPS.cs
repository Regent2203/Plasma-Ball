using UnityEngine;

public class iface_FPS : MonoBehaviour 
{
    UnityEngine.UI.Text text_field;

    void Awake()
    {
        text_field = GetComponent<UnityEngine.UI.Text>();        
    }

    void Update()
    {
        if (Time.deltaTime > 0)
            text_field.text = string.Format("FPS: {0}", Mathf.RoundToInt(1.0f / Time.deltaTime));
    }
}
