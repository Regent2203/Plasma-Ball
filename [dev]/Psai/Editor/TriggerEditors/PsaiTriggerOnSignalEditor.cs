using UnityEditor;
using UnityEngine;
using psai.net;

[CustomEditor(typeof(PsaiTriggerOnSignal), true)]
public class PsaiTriggerOnSignalEditor : PsaiSynchronizedTriggerEditor
{
    public override void OnInspectorGUI()
    {
        PsaiTriggerOnSignal trigger = target as PsaiTriggerOnSignal;

        InspectorGuiTriggerTypeBase(trigger);

        if (trigger.TriggerType == PsaiTriggerBase.PsaiTriggerType.triggerMusicTheme)
        {
            InspectorIntensity(trigger);
            InspectorGuiSynchronizeAndSumUpIntensities(trigger);
            InspectorGuiForceImmediateInterruption(trigger);
        }
    }


    public void InspectorIntensity(PsaiTriggerOnSignal trigger)
    {
        trigger.intensity = EditorGUILayout.Slider(new GUIContent("Intensity:"), trigger.intensity, 0.1f, 1.0f);
    }
}