using UnityEditor;
using UnityEngine;
using psai.net;

[CustomEditor(typeof(PsaiSynchronizedTrigger), true)]
public class PsaiSynchronizedTriggerEditor : PsaiTriggerBaseEditor
{
    
    protected void InspectorGuiOverrideDefaultCalmDownSettings(PsaiSynchronizedTrigger trigger)
    {
        EditorGUILayout.Separator();
        trigger.overrideMusicDurationInSeconds = EditorGUILayout.Toggle("override default calm-down time", trigger.overrideMusicDurationInSeconds);

        if (trigger.overrideMusicDurationInSeconds)
        {
            EditorGUI.indentLevel++;
            trigger.musicDurationInSeconds = EditorGUILayout.IntField("calm down time (seconds)", trigger.musicDurationInSeconds);
            EditorGUI.indentLevel--;
        }

    }

    protected void InspectorGuiContiuously(PsaiSynchronizedTrigger trigger)
    {
        EditorGUILayout.Separator();
        trigger.fireContinuously = EditorGUILayout.Toggle("fire continuously", trigger.fireContinuously);
    }

    protected void InspectorGuiSynchronizeAndSumUpIntensities(PsaiSynchronizedTrigger trigger, bool showResetOnDisableOption = true)
    {
        EditorGUILayout.Separator();
        InspectorGuiOverrideDefaultCalmDownSettings(trigger);

        //trigger.synchronizeByPsaiCoreManager = EditorGUILayout.Toggle("synchronize with concurrent Triggers ", trigger.synchronizeByPsaiCoreManager);

        //if (trigger.synchronizeByPsaiCoreManager)
        {
            EditorGUI.indentLevel++;
            trigger.addUpIntensities = EditorGUILayout.Toggle("sum-up overlapping Triggers", trigger.addUpIntensities);

            if (trigger.addUpIntensities)
            {
                EditorGUI.indentLevel++;
                trigger.limitIntensitySum = EditorGUILayout.Slider(new GUIContent("...up to this Intensity limit:"), trigger.limitIntensitySum, 0.1f, 1.0f);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }
    }
}
