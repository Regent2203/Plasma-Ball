using UnityEngine;
using System.Collections;
using psai.net;

public class MyCustomPsaiScript : MonoBehaviour
{
    public UnityEngine.UI.Slider IntensityDisplaySlider;

    public int basicMoodThemeId = 1;
    public int actionThemeId = 3;
    public float intensityIncreasePerClick = 0.10f;


    void Update ()
    {
        // update the intensity slider to show the current dynamic intensity level
        float currentDynamicIntensityLevel = psai.net.PsaiCore.Instance.GetPsaiInfo().currentIntensity;
        IntensityDisplaySlider.value = currentDynamicIntensityLevel;
	}


    public void OnButtonClick()
    {
        // if our Theme is already playing, we'll increase the current Intensity level with each click.
        float triggerIntensity = 0;
        if (PsaiCore.Instance.GetCurrentThemeId() == actionThemeId)
        {
            float currentDynamicIntensityLevel = PsaiCore.Instance.GetPsaiInfo().currentIntensity;
            triggerIntensity = currentDynamicIntensityLevel + this.intensityIncreasePerClick;
        }
        else
        {
            triggerIntensity = this.intensityIncreasePerClick;
        }

        PsaiCore.Instance.TriggerMusicTheme(this.actionThemeId, triggerIntensity, 15);      // after each trigger, the Theme shall play for another 15 seconds
    }
}