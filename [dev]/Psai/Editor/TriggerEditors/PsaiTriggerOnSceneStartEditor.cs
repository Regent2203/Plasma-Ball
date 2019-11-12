using UnityEditor;
using UnityEngine;
using psai.net;

[CustomEditor(typeof(PsaiTriggerOnSceneStart))]
public class PsaiTriggerOnSceneStartEditor : PsaiTriggerOnSignalEditor
{
    public override void OnInspectorGUI()
    {
        PsaiTriggerOnSceneStart trigger = target as PsaiTriggerOnSceneStart;
        InspectorGuiTriggerTypeBase(trigger);

        if (trigger.TriggerType == PsaiTriggerBase.PsaiTriggerType.triggerMusicTheme)
        {
            InspectorIntensity(trigger);
        }
    }
}
