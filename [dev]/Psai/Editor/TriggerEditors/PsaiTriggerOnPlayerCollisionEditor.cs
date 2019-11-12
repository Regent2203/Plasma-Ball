using UnityEditor;
using UnityEngine;
using psai.net;

[CustomEditor(typeof(PsaiTriggerOnPlayerCollision), true)]
class PsaiTriggerOnPlayerCollisionEditor : PsaiColliderBasedTriggerEditor
{

    public override void OnInspectorGUI()
    {
        PsaiTriggerOnPlayerCollision trigger = (PsaiTriggerOnPlayerCollision)target;

        InspectorGuiTriggerTypeBase(trigger);

        if (trigger.TriggerType == PsaiTriggerBase.PsaiTriggerType.triggerMusicTheme)        
        {
            trigger.intensity = EditorGUILayout.Slider("Intensity", trigger.intensity, 0.1f, 1.0f);
            InspectorGuiSynchronizeAndSumUpIntensities(trigger);
            InspectorGuiContiuously(trigger);
            if (!trigger.fireContinuously)
            {
                InspectorGuiForceImmediateInterruption(trigger);
                InspectorGuiDeactivateAfterFiringOnce(trigger);
            }            
        }
        else
        {
            InspectorGuiDeactivateAfterFiringOnce(trigger);
        }

        InspectorGuiPlayerCollider(trigger);
    }
}

