using UnityEditor;
using UnityEngine;
using psai.net;

[CustomEditor(typeof(PsaiTriggerOnEnable), true)]
public class PsaiTriggerEditorOnEnableEditor : PsaiTriggerOnSignalEditor
{
    public override void OnInspectorGUI()
    {
        PsaiTriggerOnEnable trigger = (PsaiTriggerOnEnable)target;

        InspectorGuiTriggerTypeBase(trigger);

        if (trigger.TriggerType == PsaiTriggerBase.PsaiTriggerType.triggerMusicTheme)
        {
            InspectorIntensity(trigger);
            InspectorGuiSynchronizeAndSumUpIntensities(trigger, false);
            if (!trigger.fireContinuously)
            {
                InspectorGuiForceImmediateInterruption(trigger);
                InspectorGuiDeactivateAfterFiringOnce(trigger);
            }            
        }
        trigger.resetHasFiredStateOnDisable = false;
    }

}
