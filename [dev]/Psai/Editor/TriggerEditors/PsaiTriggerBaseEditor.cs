using UnityEditor;
using UnityEngine;
using System.Collections;
using psai.net;
using System.Collections.Generic;

[CustomEditor(typeof(PsaiTriggerBase), true)]
public class PsaiTriggerBaseEditor : Editor
{
    protected static string[] _triggerTypeChoices = new[] { "Trigger Music Theme", "Return to last Basic Mood", "Stop Music" };
    protected int _triggerTypeChoiceIndex;  

    protected static string[] optionsEnd = new string[] { "by End-Segment", "by fade-out" };
    protected int optionEndSelectedIndex;

    protected static string[] optionsStop = new string[] { "keep silent until next Trigger", "wake up automatically again" };
    int optionsStopSelectedIndex;

    private static PsaiSoundtrackLoader _psaiSoundtrackLoader;
    private static PsaiSoundtrackLoader PsaiSoundtrackLoader
    {
        get
        {
            if (_psaiSoundtrackLoader == null)
            {
                _psaiSoundtrackLoader = GameObject.FindObjectOfType<PsaiSoundtrackLoader>();
            }

            return _psaiSoundtrackLoader;
        }
    }

    private static Dictionary<int, ThemeInfo> _themeInfos;
    private static Dictionary<int, ThemeInfo> ThemeInfos
    {
        get
        {
            if (_themeInfos == null || _themeInfos.Count == 0)
            {
                _themeInfos = new Dictionary<int, ThemeInfo>();

                // watch out - don't create another Psai instance here, as it will flood your Psai GameObject with more PsaiAudioChannels
                if (PsaiCore.IsInstanceInitialized())
                {
                    SoundtrackInfo soundtrackInfo = PsaiCore.Instance.GetSoundtrackInfo();

                    int[] themeIds = soundtrackInfo.themeIds;
                    foreach (int themeId in themeIds)
                    {
                        ThemeInfo themeInfo = PsaiCore.Instance.GetThemeInfo(themeId);
                        _themeInfos[themeId] = themeInfo;
                    }
                }

            }
            return _themeInfos;
        }
    }  


    protected void InspectorGuiTriggerTypeBase(PsaiTriggerBase trigger)
    {
        _triggerTypeChoiceIndex = (int)trigger.TriggerType;
        _triggerTypeChoiceIndex = EditorGUILayout.Popup("Command: ", _triggerTypeChoiceIndex, _triggerTypeChoices);

        trigger.TriggerType = (PsaiTriggerBase.PsaiTriggerType)_triggerTypeChoiceIndex;

        if (trigger.TriggerType == PsaiTriggerBase.PsaiTriggerType.returnToLastBasicMood)
        {
            InspectorGuiReturnToLastBasicMood(trigger);
        }
        else if (trigger.TriggerType == PsaiTriggerBase.PsaiTriggerType.stopMusic)
        {
            InspectorGuiPsaiStopMusic(trigger);
        }
        else if (trigger.TriggerType == PsaiTriggerBase.PsaiTriggerType.triggerMusicTheme)
        {
            InspectorGuiThemeSelection(trigger);
        }
    }

    protected void InspectorGuiReturnToLastBasicMood(PsaiTriggerBase trigger)
    {
        InspectorGuiEndOrFade(trigger);
        InspectorGuiDontExecuteIfTriggersAreFiring(trigger);
    }


    protected void InspectorGuiPsaiStopMusic(PsaiTriggerBase trigger)
    {
        optionsStopSelectedIndex = trigger._keepSilentUntilNextTrigger ? 0 : 1;
        optionsStopSelectedIndex = GUILayout.SelectionGrid(optionsStopSelectedIndex, optionsStop, optionsStop.Length, EditorStyles.radioButton);

        if (optionsStopSelectedIndex == 1)
        {
            // override checkbox
            EditorGUI.indentLevel++;
            trigger._overrideDefaultRestTime = EditorGUILayout.Toggle("override default rest time", trigger._overrideDefaultRestTime);

            if (trigger._overrideDefaultRestTime)
            {
                EditorGUI.indentLevel++;
                trigger._overrideRestSecondsMin = EditorGUILayout.IntField("override rest seconds min", trigger._overrideRestSecondsMin);
                trigger._overrideRestSecondsMax = EditorGUILayout.IntField("override rest seconds max", trigger._overrideRestSecondsMax);

                if (trigger._overrideRestSecondsMax < trigger._overrideRestSecondsMin)
                {
                    trigger._overrideRestSecondsMax = trigger._overrideRestSecondsMin;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        trigger._keepSilentUntilNextTrigger = (optionsStopSelectedIndex == 0);


        InspectorGuiEndOrFade(trigger);
        InspectorGuiDontExecuteIfTriggersAreFiring(trigger);
    }


    public void InspectorGuiEndOrFade(PsaiTriggerBase trigger)
    {

        optionEndSelectedIndex = trigger._immediately ? 1 : 0;
        optionEndSelectedIndex = GUILayout.SelectionGrid(optionEndSelectedIndex, optionsEnd, optionsEnd.Length, EditorStyles.radioButton);
        trigger._immediately = (optionEndSelectedIndex == 1);
        if (optionEndSelectedIndex == 1)
        {
            EditorGUI.indentLevel++;
            trigger._fadeOutSeconds = EditorGUILayout.FloatField("Fade-out Seconds", trigger._fadeOutSeconds);
            EditorGUI.indentLevel--;
        }
    }


    public void InspectorGuiDontExecuteIfTriggersAreFiring(PsaiTriggerBase trigger)
    {
        trigger._dontExecuteIfOtherTriggersAreFiring = EditorGUILayout.Toggle("don't execute if Triggers are currently firing", trigger._dontExecuteIfOtherTriggersAreFiring);

        if (trigger._dontExecuteIfOtherTriggersAreFiring)
        {
            EditorGUI.indentLevel++;
            trigger._restrictBlockToThisThemeType = (psai.net.ThemeType)EditorGUILayout.EnumPopup("...restricted to Triggers of this Type:", trigger._restrictBlockToThisThemeType);
            EditorGUI.indentLevel--;
        }
    }


    protected void InspectorGuiThemeSelection(PsaiTriggerBase trigger)
    {
        trigger.themeId = EditorGUILayout.IntField("ThemeId", trigger.themeId);
        if (trigger.themeId < 1)
        {
            trigger.themeId = 1;
        }

        if (PsaiCore.IsInstanceInitialized())
        {
            Color defaultContentColor = GUI.contentColor;
            string themeInfoString = "THEME NOT FOUND";
            GUI.contentColor = Color.red;
            if (ThemeInfos.ContainsKey(trigger.themeId))
            {
                GUI.contentColor = new Color(0, 0.85f, 0);
                ThemeInfo themeInfo = ThemeInfos[trigger.themeId];
                themeInfoString = themeInfo.name + " [" + psai.net.Theme.ThemeTypeToString(themeInfo.type) + "]";
            }
            EditorGUILayout.LabelField(" ", themeInfoString);

            GUI.contentColor = defaultContentColor;
        }
    }

    protected void InspectorGuiForceImmediateInterruption(PsaiSynchronizedTrigger trigger)
    {
        trigger.interruptAnyTheme = EditorGUILayout.Toggle("force immediate interruption of all Theme Types", trigger.interruptAnyTheme);
    }

    protected void InspectorGuiDeactivateAfterFiringOnce(PsaiSynchronizedTrigger trigger)
    {
        trigger.deactivateAfterFiringOnce = EditorGUILayout.Toggle("deactivate after firing once", trigger.deactivateAfterFiringOnce);
        if (trigger.deactivateAfterFiringOnce)
        {
            EditorGUI.indentLevel++;
            trigger.resetHasFiredStateOnDisable = EditorGUILayout.Toggle("reset 'has fired' state on disable", trigger.resetHasFiredStateOnDisable);

            EditorGUI.BeginDisabledGroup(true);
            trigger.hasFired = EditorGUILayout.Toggle("has fired", trigger.hasFired);
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel--;
        }
    }

}
