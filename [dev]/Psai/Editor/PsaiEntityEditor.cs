using UnityEngine;
using UnityEditor;

using psai.Editor;
using System.Collections.Generic;
using System.IO;

public class PsaiEntityEditor : EditorWindow
{
    private PsaiMusicEntity _entity;
    private PsaiMusicEntity _tmpEntity;
    private bool _deselectAnyGuiElement;
    private Vector2 _scrollPos;
    private bool _disableSelectionButton;

    private AudioClip _lastSelectedAudioClip = null;    

    PsaiEntityEditor()
    {
        # if (UNITY_5_0 )
            this.title = "Psai Entity";
        #else
            this.titleContent.text = "Psai Entity";
        #endif
        EditorModel.Instance.PsaiProjectLoadedEvent += HandleEvent_ProjectLoaded;
    }


    public PsaiMusicEntity Entity
    {
        get
        {
            return _entity;
        }

        set
        {
            CheckForChangesAndApplyToProjectIfConfirmed();
            DeselectAnyGuiElement();
            _entity = value;
            if (_entity != null)
            {
                _tmpEntity = (PsaiMusicEntity)_entity.ShallowCopy();
            }
            _lastSelectedAudioClip = null;
            UpdateSelectionButton();
            Repaint();
        }
    }


    /// <summary>
    /// Blurs Unity focus
    /// Should be called from inside the OnGUI() handler
    /// </summary>
    private void DeselectAnyGuiElement()
    {
        _deselectAnyGuiElement = true;
    }

    public void CheckForChangesAndApplyToProjectIfConfirmed()
    {
        if (TempEntityHasBeenEdited())
        {
            if (EditorUtility.DisplayDialog("Apply changes?", "You have made changes to " + _entity.GetClassString() + " " + _entity.Name + ". Do you wish to apply them to your temporary copy of your psai Project?", "Apply", "Discard"))
            {
                ApplyChangesToPsaiProject();
            }
        }
    }

    private void ApplyChangesToPsaiProject()
    {
        PnxHelperz.CopyTo(_tmpEntity, _entity);
        EditorModel.Instance.ProjectDataChanged();
        Debug.Log("applied changes to psai Project");
    }


    void OnDisable()
    {
        CheckForChangesAndApplyToProjectIfConfirmed();
    }


    private bool TempEntityHasBeenEdited()
    {
        if (_entity != null && _tmpEntity != null)
        {
            if (_tmpEntity is Theme)
            {
                Theme tmpTheme = _tmpEntity as Theme;
                Theme theme = _entity as Theme;

                if (PnxHelperz.PublicInstancePropertiesEqual<Theme>(theme, tmpTheme, "Id", "ThemeTypeInt", 
                    "RestSecondsMin", "RestSecondsMax", "MusicPhaseSecondsGeneral", "MusicPhaseSecondsAfterRest", "IntensityAfterRest",
                    "WeightingSwitchGroups", "WeightingIntensityVsVariance", "WeightingLowPlaycountVsRandom"
                    ) == false)
                {
                    return true;
                }
            }
            else if (_tmpEntity is Segment)
            {
                Segment tmpSegment = _tmpEntity as Segment;
                Segment segment = _entity as Segment;

                if (PnxHelperz.PublicInstancePropertiesEqual<Segment>(segment, tmpSegment, "Intensity", "IsUsableAtStart", "IsUsableAtEnd", "IsUsableInMiddle") == false)
                {
                    return true;
                }
            }

        }
        return false;
    }


    void OnGUIforTheme()
    {
        Theme tmpTheme = _tmpEntity as Theme;        

        int tmpThemeId = EditorGUILayout.IntField("Id", tmpTheme.Id);

        if (tmpTheme.Id != tmpThemeId && tmpThemeId > 0)
        {
            //Debug.Log("tmpTheme.Id=" + tmpTheme.Id.ToString() + "  tmpThemeid=" + tmpThemeId);
            int initialThemeId = tmpThemeId;
            while (EditorModel.Instance.Project.CheckIfThemeIdIsInUse(tmpThemeId) == true)
            {
                if (initialThemeId > tmpTheme.Id)
                {
                    tmpThemeId++;
                }
                else
                {
                    tmpThemeId--;
                }
            }
            if (tmpThemeId == 0)
            {
                tmpThemeId = EditorModel.Instance.Project.GetNextFreeThemeId(initialThemeId);
                //Debug.Log("was 0, tmpThemeId=" + tmpThemeId);
            }

            tmpTheme.Id = tmpThemeId;
        }

        tmpTheme.ThemeTypeInt = (int)(psai.net.ThemeType)EditorGUILayout.EnumPopup("Theme Type", (psai.net.ThemeType)tmpTheme.ThemeTypeInt);
        if (tmpTheme.ThemeTypeInt == 0)
        {
            tmpTheme.ThemeTypeInt = (int)psai.net.ThemeType.basicMood;
        }

        if ((psai.net.ThemeType)tmpTheme.ThemeTypeInt == psai.net.ThemeType.highlightLayer)
        {
            GUI.enabled = false;
        }

        int themeDuration = EditorGUILayout.IntField("Theme Duration (sec.)", tmpTheme.MusicPhaseSecondsGeneral);
        if (themeDuration >= 0)
        {
            tmpTheme.MusicPhaseSecondsGeneral = themeDuration;
        }

        if ((psai.net.ThemeType)tmpTheme.ThemeTypeInt != psai.net.ThemeType.basicMood)
        {
            GUI.enabled = false;
        }

        if ((psai.net.ThemeType)tmpTheme.ThemeTypeInt != psai.net.ThemeType.basicMood)
        {
            GUI.enabled = false;
        }

        int restTimeMin = EditorGUILayout.IntField(new GUIContent("Rest Time Min (seconds)", "Sets the minimum time that the music will stay silent in Rest Mode. The actual resting time is calculated each time randomly between 'Min Rest Time' and 'Max Rest Time'."), tmpTheme.RestSecondsMin);
        if (restTimeMin >= 0 && restTimeMin <= tmpTheme.RestSecondsMax)
        {
            tmpTheme.RestSecondsMin = restTimeMin;
        }

        int restTimeMax = EditorGUILayout.IntField(new GUIContent("Rest Time Max (seconds)", "Sets the maximum time that the music will stay silent in Rest Mode. The actual resting time is calculated each time randomly between 'Min Rest Time' and 'Max Rest Time'."), tmpTheme.RestSecondsMax);
        if (restTimeMax >= 0 && restTimeMax >= tmpTheme.RestSecondsMin)
        {
            tmpTheme.RestSecondsMax = restTimeMax;
        }

        int themeDurationAfterRest = EditorGUILayout.IntField(new GUIContent("Theme Dur. after Rest", "Sets the timespan that a Basic Mood will keep playing after waking up from Rest Mode."),tmpTheme.MusicPhaseSecondsAfterRest);
        if (themeDurationAfterRest >= 0)
        {
            tmpTheme.MusicPhaseSecondsAfterRest = themeDurationAfterRest;
        }

        tmpTheme.IntensityAfterRest = EditorGUILayout.Slider(new GUIContent("Intensity after Rest", "Controls the starting intensity that a Basic Mood will eventually wake up with from Rest Mode."),tmpTheme.IntensityAfterRest, 0f, 1.0f);
        GUI.enabled = true;

        EditorGUILayout.Space();
        if ((psai.net.ThemeType)tmpTheme.ThemeTypeInt == psai.net.ThemeType.highlightLayer)
        {
            GUI.enabled = false;
        }
        EditorGUILayout.LabelField("Weightings");
        tmpTheme.WeightingSwitchGroups = EditorGUILayout.Slider(new GUIContent("Jump between Groups", "Set this weighting to the very left if you never want psai to switch between different groups of instrumentation. See the psai Manual for more information about Groups."),tmpTheme.WeightingSwitchGroups, 0f, 1.0f);
        tmpTheme.WeightingIntensityVsVariance = EditorGUILayout.Slider(new GUIContent("ignore Segment Intensity", "This controls the tradeoff between repetition avoidance versus an exact match of the current dynamic musical intensity: Setting this to the right will force psai to concentrate on repetition avoidance, even if this means that a chosen Segment might not match the current Dynamic Intensity at all. Setting this to the very left will determine psai to always play back those Segments that match the current Dynamic Intensity the best, even if this causes the same Segments to be repeated over and over again."), tmpTheme.WeightingIntensityVsVariance, 0f, 1.0f);
        tmpTheme.WeightingLowPlaycountVsRandom = EditorGUILayout.Slider(new GUIContent("Random Factor", "This adds a random factor to the choice of Segments. Use this to break up the strict order of Segment playback, thereby reducing the predictability of the music. Please note: This weighing has less effect the further the upper 'ignore Segment Intensity' slider is set to the left."), tmpTheme.WeightingLowPlaycountVsRandom, 0f, 1.0f);
        GUI.enabled = true;
    }

    void OnGUIforSegment()
    {
        Segment tmpSegment = _tmpEntity as Segment;
        Segment segment = _entity as Segment;

        string selectInProjectString = "Select AudioClip";

        if (_disableSelectionButton)
        {
            GUI.enabled = false;
        }

        if (GUILayout.Button(selectInProjectString, GUILayout.MaxWidth(GUI.skin.label.CalcSize(new GUIContent(selectInProjectString)).x + PsaiEditorWindow.BUTTON_MARGIN), GUILayout.Height(PsaiEditorWindow.LINE_HEIGHT)))
        {

            PsaiEditorWindow editor = (PsaiEditorWindow)EditorWindow.GetWindow(typeof(PsaiEditorWindow), false);

            if (!editor._pathToPsaiProject.Contains("/Resources/"))
            {
                Debug.LogError("Could not select the AudioClip. For editing, the .psai Project must reside in a subfolder of a folder named 'Resources' within your Unity Project.");
            }
            else
            {
                string path = editor._pathToPsaiProject.Substring(editor._pathToPsaiProject.LastIndexOf("/Resources/"));
                path = Path.GetDirectoryName(path);
                path = path.Remove(0, "/Resources".Length);
                path += Path.GetDirectoryName(segment.AudioData.FilePathRelativeToProjectDir) + "/" + Path.GetFileNameWithoutExtension(segment.AudioData.FilePathRelativeToProjectDir);
                if (path.StartsWith("/"))
                {
                    path = path.Substring(1, path.Length - 1);
                }
                _lastSelectedAudioClip = (AudioClip)Resources.Load(path, typeof(AudioClip));
                if (_lastSelectedAudioClip != null)
                {
                    Selection.activeObject = _lastSelectedAudioClip;
                    UpdateSelectionButton();
                    Repaint();
                }
                else
                {
                    Debug.LogError("could not find the following AudioClip within the Resources folder: " + path);
                }
            }
        }

        GUI.enabled = true;

        EditorGUILayout.Separator();
        tmpSegment.Intensity = EditorGUILayout.Slider("Intensity", tmpSegment.Intensity, 0f, 1.0f);
    }

    void OnGUI()
    {
        if (_entity != null)
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            if (_deselectAnyGuiElement)
            {
                string dummyString = "_______Ommi_is_die_beste___";
                GUI.SetNextControlName(dummyString);
                GUI.TextField(new Rect(-100, -100, 1, 1), "");
                GUI.FocusControl(dummyString);
                _deselectAnyGuiElement = false;
            }
            string entityString = Entity == null ? " " : Entity.GetClassString() + " " + Entity.Name;
            EditorGUILayout.LabelField(entityString, EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            if (_tmpEntity is Theme)
            {
                OnGUIforTheme();
            }
            else if (_tmpEntity is Segment)
            {
                OnGUIforSegment();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            if (TempEntityHasBeenEdited() == false)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Apply", GUILayout.MaxWidth(this.position.width / 3), GUILayout.Height(PsaiEditorWindow.LINE_HEIGHT)))
            {
                ApplyChangesToPsaiProject();
            }
            if (GUILayout.Button("Discard", GUILayout.MaxWidth(this.position.width / 3), GUILayout.Height(PsaiEditorWindow.LINE_HEIGHT)))
            {
                PnxHelperz.CopyTo(_entity, _tmpEntity);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }
    }

    private void HandleEvent_ProjectLoaded(object sender, System.EventArgs e)
    {
        _entity = null;
        _lastSelectedAudioClip = null;
    }

    private void UpdateSelectionButton()
    {
        _disableSelectionButton = (Selection.activeObject != null && Selection.activeObject == _lastSelectedAudioClip);
        //Debug.Log("UpdateSelectionButton()  Selection.activeObject=" + Selection.activeObject + "  _last clip=" + _lastSelectedAudioClip + "   ->" + _disableSelectionButton);        
    }

    void OnSelectionChange()
    {
        UpdateSelectionButton();
        Repaint();
    }
}
