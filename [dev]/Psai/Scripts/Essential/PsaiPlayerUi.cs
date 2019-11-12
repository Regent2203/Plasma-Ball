//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using psai.net;

public class PsaiPlayerUi : MonoBehaviour
{

    //static readonly int _tabOffsetX1 = 13;      // the offset of the stripline panels, section headers, ..., in pixels, from the left screen edge
    static readonly int _tabOffsetX2 = 20;      // the offset of the Theme Type names and Toggle Groups in pixels, from the left screen edge
    static readonly int _tabOffsetX3 = 130;     // the offset of the "Segment Intensity" font and Theme Trigger buttons from the left screen edge
    

    static readonly float GUI_FLASH_FREQUENCY = 1.0f;

    static readonly Color COLOR_WHITE_TRANSPARENT_10 = new Color(1f, 1f, 1f, 0.10f);
    static readonly Color COLOR_CONTROLBOX_ACTIVE = new Color(0.63f, 0.63f, 0.63f, 0.43f);

    static readonly Color COLOR_LIGHTGREY = new Color(0.85f, 0.85f, 0.85f);
    static readonly Color COLOR_MIDDLEGREY = new Color(0.63f, 0.63f, 0.63f);
    static readonly Color COLOR_DARKGREEN = new Color(0, 0.6f, 0, 1.0f);
    static readonly Color COLOR_LIGHTGREEN = new Color(0, 1.0f, 0, 1.0f);
    static readonly Color COLOR_LIGHTYELLOW = new Color(1.0f, 1.0f, 0, 1.0f);
    static readonly Color COLOR_DARKYELLOW = new Color(0.7f, 0.7f, 0, 1.0f);
    static readonly Color COLOR_LIST_BACKGROUND_SELECTED = new Color(1.0f, 1.0f, 1.0f, 0.13f);


    /// <summary>
    /// Sets the step size for the +/- buttons in the Intensity Section.
    /// </summary>
    [Tooltip("Sets the step size of the '+'/'-' buttons in the Intensity Control Canvas.")]
    public float _addToIntensityStepsize = 0.05f;

    /// <summary>
    /// show Theme / Segment lists instead of current Theme / Segment
    /// </summary>
    [Tooltip("Sets which rider is being displyed in the lower info section.")]
    public InfoSectionSelection _infoSectionSelection = InfoSectionSelection.entity;

    private class ThemeTriggerButtonUi
    {
        public GameObject goTriggerItemRoot;
        public GameObject goButton;
        public Button button;
        public Text themeNameText;
        public Slider intensitySlider;
        public Image indicatorImage;
        public Text textIntensityValue;

        public ThemeTriggerButtonUi(GameObject themeTriggerPanelItemBlueprint)
        {
            this.goTriggerItemRoot = GameObject.Instantiate(themeTriggerPanelItemBlueprint);
            this.goTriggerItemRoot.SetActive(true);
            this.button = this.goTriggerItemRoot.GetComponentsInChildren<Button>(true)[0];
            this.goButton = button.gameObject;
            this.themeNameText = button.GetComponentsInChildren<Text>(true)[0];
            this.intensitySlider = this.goTriggerItemRoot.GetComponentsInChildren<Slider>(true)[0];
            this.indicatorImage = this.goTriggerItemRoot.transform.Find("IndicatorPanel").GetComponentsInChildren<Image>(true)[0];
            this.textIntensityValue = this.goTriggerItemRoot.transform.Find("TextIntensityValue").GetComponent<Text>();
        }
    }


    private class SegmentListViewEntry
    {
        public GameObject gameObj;
        public int segmentId;
        public Image backgroundImage;
        public Text textName;
        public Text textSuitabilities;
        public Text textLength;
        public Text textIntensity;
        public Text textPlaycount;

        public override string ToString()
        {
            string result = string.Format("textName InstanceId: {0}  {1}  {2}  {3}", textName.GetInstanceID(), textName.text, textSuitabilities.text, textPlaycount.text);
            return result;
        }
    }


    private class ThemeListViewEntry
    {
        public GameObject gameObj;
        public int themeId;
        public Image backgroundImage;
        public Text textName;
        public Text textThemeId;
        public Text textThemeType;
    }

    private class ControlSectionPanel
    {
        public GameObject panelObject;
        public Image bgImage;
        public List<Image> buttonBgImages = new List<Image>();
        public List<UnityEngine.UI.Selectable> selectables = new List<UnityEngine.UI.Selectable>();
        public List<Text> labelTexts = new List<Text>();


        public void SetInteractable(bool interactable)
        {

            foreach (Selectable selectable in selectables)
            {
                selectable.interactable = interactable;
            }

            if (interactable)
            {
                SetAllTextsToColor(COLOR_LIGHTGREY);
                SetAllImagesToColor(COLOR_MIDDLEGREY);
                bgImage.color = COLOR_CONTROLBOX_ACTIVE;
                //bgImage.color = COLOR_WHITE_TRANSPARENT_10;
            }
            else
            {
                SetAllTextsToColor(COLOR_CONTROLBOX_ACTIVE);
                SetAllImagesToColor(COLOR_CONTROLBOX_ACTIVE);
                bgImage.color = COLOR_WHITE_TRANSPARENT_10;
            }
            
        }

        private void SetAllTextsToColor(Color argColor)
        {
            foreach (Text text in labelTexts)
            {
                text.color = argColor;
            }
        }

        private void SetAllImagesToColor(Color argColor)
        {
            foreach (Image image in buttonBgImages)
            {
                image.color = argColor;
            }
        }
    }


    private enum ThemeSorting
    {
        name,
        id,
        themeType
    }    
    
    private enum SegmentSorting
    {
        name,
        suitabilites,
        intensity,
        playcount,
        length
    }

    public enum InfoSectionSelection
    {        
        entity,
        list,
        log,
        description,
        off
    }
 

    static Color _colorIndicatorDefault = new Color(0.58f, 0.63f, 0.66f);

    Transform _intensitySection;
    Transform _intensityIndicatorSlidersParent;
    Transform _intensityControlsParent;

    Transform _engineControlSection;
    GameObject _themeTypeLabelBlueprint;

    Slider _dynamicIntensitySlider;
    Slider _segmentIntensitySlider;

    Text _errorTextWindow;
    Transform _themesTriggerCanvas;    
    GameObject _themesTriggerSectionScrollView;
    Transform _themesTriggerSectionScrollViewContent;
    GameObject _themeTriggerPanelItemBlueprint;
    int _themesTriggerSectionScrollPositionInLastFrame;

    Transform _infoSection;
    GameObject _currentThemeSection;
    GameObject _currentSegmentSection;

    Text _currentSegmentNameValue;
    Text _currentSegmentIdValue;
    Text _currentSegmentIntensityValue;
    Text _currentSegmentSuitabilitesValue;
    Text _currentSegmentPlaycountValue;
    Text _currentSegmentRemainingMsValue;
    Text _currentThemeNameValue;
    Text _currentThemeIdValue;
    Text _currentThemeTypeValue;
    Text _psaiStateValue;

    GameObject _soundtrackDescriptionPanel;
    UnityEngine.UI.Image _soundtrackImage;
    Text _soundtrackHeadlineText;
    Text _soundtrackDescriptionText;

    Button _buttonHoldIntensity;
    Color _buttonColorHoldNormal;
    Color _buttonColorHoldHighlighted;
    Image _dynamicIntensityBarFillImage;
    Image _segmentIntensityBarFillImage;
    Text _dynamicIntensityValueText;
    Text _segmentIntensityValueText;

    GameObject _menuModePanel;
    Text _menuModePanelText;
    GameObject _cutSceneModePanel;
    Text _cutScenePanelText;

    Toggle _toggleBasicControls;

    Toggle _toggleInfoSectionOff;
    Toggle _toggleInfoSectionDescription;    
    Toggle _toggleInfoSectionEntity;
    Toggle _toggleInfoSectionList;

    ControlSectionPanel _stopMusicPanel = new ControlSectionPanel();
    Image _buttonBgImageStopMusicByEndSegment;
    GameObject _buttonStopMusicViaEndSegment;
    GameObject _buttonStopMusicImmediately;
    Slider _stopMusicFadeoutSecondsSlider;
    Text _stopMusicFadeoutValueText;

    ControlSectionPanel _returnToBasicMoodPanel = new ControlSectionPanel();
    Image _buttonBgImageReturnToBasicMoodByEndSegment;

    ControlSectionPanel _pausePanel = new ControlSectionPanel();
    Button _buttonPause;

    Button _buttonMenuModeEnter;
    Text _buttonMenuModeEnterText;
    Button _buttonMenuModeConfigure;    
    Button _buttonCutSceneModeEnter;
    Text _buttonCutSceneModeEnterText;
    Button _buttonCutSceneModeConfigure;
    Text _buttonCutSceneModeConfigureText;
    
    Toggle _toggleTooltip;

    int _themeTriggerPanelItemHeight;
    int _buttonSpacing;

    int _lineHeightListView;
    GameObject _themeListView;
    GameObject _themeListScrollView;
    GameObject _themeListScrollViewContent;
    GameObject _themeListViewEntryGameObject;
    SegmentSorting _segmentSorting = SegmentSorting.name;

    GameObject _segmentListView;
    GameObject _segmentListScrollView;
    GameObject _segmentListScrollViewContent;
    GameObject _segmentListViewEntryGameObject;    

    List<SegmentListViewEntry> _segmentListViewEntries = new List<SegmentListViewEntry>();
    List<ThemeListViewEntry> _themeListViewEntries = new List<ThemeListViewEntry>();
    Dictionary<GameObject, int> _themeListViewButtonsToThemeIds = new Dictionary<GameObject, int>();
    Dictionary<GameObject, int> _segmentListViewEntriesToSegmentIds = new Dictionary<GameObject, int>();

    bool _flashIncrease;

    int[] _themeIds = new int[0];
    List<int> _themeIdsList = new List<int>();

    bool _sortThemesAscending;
    bool _sortSegmentsAscending;

    int _selectedThemeId = -1;
    Dictionary<int, ThemeInfo> _themeInfos = new Dictionary<int, ThemeInfo>();
    Dictionary<ThemeTriggerButtonUi, int> _themeTriggerButtonsToThemeIds = new Dictionary<ThemeTriggerButtonUi, int>();
    Dictionary<int, ThemeTriggerButtonUi> _themeIdsToTriggerButtonUis = new Dictionary<int, ThemeTriggerButtonUi>();
    Dictionary<GameObject, int> _themeTriggerButtonGoToThemeIds = new Dictionary<GameObject, int>();
    Dictionary<int, Slider> _themeIdsToTriggerSliders = new Dictionary<int, Slider>();
    Dictionary<GameObject, int> _themeTriggerSlidersToThemeIds = new Dictionary<GameObject, int>();
    Dictionary<ThemeType, List<int>> _themeTypesToThemeIds = new Dictionary<ThemeType, List<int>>();
    Dictionary<int, SegmentInfo> _segmentInfos = new Dictionary<int, SegmentInfo>();
    List<int> _segmentIdsListOfSelectedTheme = new List<int>();
    Dictionary<int, int> _playbackCountdowns = new Dictionary<int, int>();

    bool _configureMenuMode = false;
    int _menuThemeId = -1;
    float _menuThemeIntensity = 1.0f;

    bool _configureCutScene = false;
    int _cutSceneThemeId = -1;
    float _cutSceneThemeIntensity = 1.0f;

    bool _configureStopMusic = false;    

    /// <summary>
    /// Show Menu Mode and CutScene Mode
    /// </summary>
    bool _showAdvancedControlSection = false;

    int _segmentListScrollPositionInLastFrame = 0;
    int _themeListScrollPositionInLastFrame = 0;

    int _targetSegmentIdInLastFrame;
    int _remainingMillisecondsOfSegmentPlaybackInLastFrame;
    int _playingThemeIdInLastFrame;

    //int _selectedThemeIdInLastFrame;
    PsaiState _psaiStateInLastFrame;

    float _flashIntensity;

    bool _initialized = false;

    ThemeType _currentThemeType;

    private int SelectedThemeId
    {
        get
        {
            return _selectedThemeId;
        }

        set
        {
            _selectedThemeId = value;
            UpdateThemeListView();
            LoadDataToSegmentListView(_selectedThemeId);
        }
    }

    void Awake()
    {        
        _intensitySection = this.gameObject.transform.Find("IntensitySectionCanvas/IntensitySection");
        _intensityIndicatorSlidersParent = _intensitySection.transform.Find("IntensityBars");
        _dynamicIntensitySlider = _intensityIndicatorSlidersParent.transform.Find("DynamicIntensity").GetComponent<Slider>();
        _segmentIntensitySlider = _intensityIndicatorSlidersParent.transform.Find("SegmentIntensity").GetComponent<Slider>();
        _dynamicIntensityBarFillImage = _dynamicIntensitySlider.transform.Find("Fill Area/Fill").GetComponent<Image>();
        _dynamicIntensityValueText = _intensityIndicatorSlidersParent.transform.Find("DynamicIntensityValue").GetComponent<Text>();
        _segmentIntensityValueText = _intensityIndicatorSlidersParent.transform.Find("SegmentIntensityValue").GetComponent<Text>();
        _segmentIntensityBarFillImage = _segmentIntensitySlider.transform.Find("Fill Area/Fill").GetComponent<Image>();

        _intensityControlsParent = _intensityIndicatorSlidersParent.transform.Find("IntensityControls");
        _buttonHoldIntensity = _intensityControlsParent.transform.Find("ButtonHold").GetComponent<Button>();
        _buttonColorHoldNormal = _buttonHoldIntensity.colors.normalColor;
        _buttonColorHoldHighlighted = _buttonHoldIntensity.colors.highlightedColor;

        _themesTriggerCanvas = this.gameObject.transform.Find("ThemeTriggerCanvas");
        if (_themesTriggerCanvas)
        {
            GameObject themesTriggerSection = _themesTriggerCanvas.transform.Find("ThemeTriggerSection").gameObject;
            _themesTriggerSectionScrollView = themesTriggerSection.transform.Find("ScrollView").gameObject;
            _errorTextWindow = _themesTriggerSectionScrollView.transform.Find("ErrorText").gameObject.GetComponent<Text>();
            _themesTriggerSectionScrollViewContent = _themesTriggerSectionScrollView.transform.Find("Content");
            _themeTriggerPanelItemBlueprint = themesTriggerSection.transform.Find("ThemeTriggerPanelItemBlueprint").gameObject;
            _themeTriggerPanelItemHeight = (int)((RectTransform)(_themeTriggerPanelItemBlueprint.transform)).rect.height;
            _themeTriggerPanelItemBlueprint.SetActive(false);
            _themeTypeLabelBlueprint = themesTriggerSection.transform.Find("ThemeTypeLabelBlueprint").gameObject;
            _themeTypeLabelBlueprint.SetActive(false);
        }

        Transform toggleTooltipGo = this.gameObject.transform.Find("TooltipsCanvas");
        if (toggleTooltipGo != null)
        {
            Transform toggleTooltipsTransform = _themesTriggerCanvas.transform.Find("ToggleTooltips");
            if (toggleTooltipsTransform)
            {
                _toggleTooltip =  toggleTooltipsTransform.GetComponentInChildren<Toggle>();

                if (_toggleTooltip != null && psai.TooltipView.Instance != null)
                {
                    psai.TooltipView.Instance.TurnedOn = _toggleTooltip.isOn;
                }                         
            }           
        }

        //_buttonHoldIntensity.interactable = false;        
        _engineControlSection = this.gameObject.transform.Find("EngineControlsCanvas/EngineControlSection");
        if (_engineControlSection && _engineControlSection.gameObject.activeInHierarchy)
        {
            {   // Stop Music
                _stopMusicPanel.panelObject = _engineControlSection.transform.Find("StopMusic").gameObject;
                _stopMusicPanel.bgImage = _stopMusicPanel.panelObject.GetComponent<Image>();
                Text labelSection = _stopMusicPanel.panelObject.transform.Find("Text").GetComponent<Text>();
                _buttonStopMusicImmediately = _engineControlSection.transform.Find("StopMusic/ButtonImmediately").gameObject;
                _buttonStopMusicViaEndSegment = _engineControlSection.transform.Find("StopMusic/ButtonEndSegment").gameObject;
                Button buttonStopMusicByEndSegment = _buttonStopMusicViaEndSegment.GetComponentsInChildren<Button>(true)[0];
                Button buttonStopMusicImmediately = _buttonStopMusicImmediately.GetComponentsInChildren<Button>(true)[0];
                _buttonBgImageStopMusicByEndSegment = buttonStopMusicByEndSegment.GetComponent<Image>();
                Image bgImageButtonStopMusicImmediately = buttonStopMusicImmediately.GetComponent<Image>();
                _stopMusicPanel.selectables.Add(buttonStopMusicByEndSegment);
                _stopMusicPanel.selectables.Add(buttonStopMusicImmediately);
                _stopMusicPanel.buttonBgImages.Add(_buttonBgImageStopMusicByEndSegment);
                _stopMusicPanel.buttonBgImages.Add(bgImageButtonStopMusicImmediately);
                Text labelButtonStopMusicByEndSegment = buttonStopMusicByEndSegment.GetComponentInChildren<Text>();
                Text labelButtonStopMusicImmediately = buttonStopMusicImmediately.GetComponentInChildren<Text>();
                _stopMusicPanel.labelTexts.Add(labelSection);
                _stopMusicPanel.labelTexts.Add(labelButtonStopMusicByEndSegment);
                _stopMusicPanel.labelTexts.Add(labelButtonStopMusicImmediately);

                _stopMusicFadeoutSecondsSlider = _stopMusicPanel.panelObject.GetComponentInChildren<Slider>();
                Text stopMusicFadeoutLabel = _stopMusicFadeoutSecondsSlider.gameObject.GetComponentsInChildren<Text>()[0];
                _stopMusicFadeoutValueText = _stopMusicFadeoutSecondsSlider.gameObject.GetComponentsInChildren<Text>()[1];
                _stopMusicPanel.labelTexts.Add(_stopMusicFadeoutValueText);
                _stopMusicPanel.labelTexts.Add(stopMusicFadeoutLabel);
                //_stopMusicPanel.selectables.Add(_stopMusicFadeoutSecondsSlider);

                ConfigureStopMusic(false);
            }


            {   // Return To Basic Mood
                _returnToBasicMoodPanel.panelObject = _engineControlSection.transform.Find("ReturnToBasicMood").gameObject;
                _returnToBasicMoodPanel.bgImage = _returnToBasicMoodPanel.panelObject.GetComponent<Image>();
                Text labelSection = _returnToBasicMoodPanel.panelObject.transform.Find("Text").GetComponent<Text>();
                _returnToBasicMoodPanel.labelTexts.Add(labelSection);

                Button buttonReturnToLastBasicMoodByEndSegment = _returnToBasicMoodPanel.panelObject.transform.Find("ButtonEndSegment").GetComponentsInChildren<Button>(true)[0];
                Button buttonReturnToLastBasicMoodImmediately = _returnToBasicMoodPanel.panelObject.transform.Find("ButtonImmediately").GetComponentsInChildren<Button>(true)[0];
                _returnToBasicMoodPanel.selectables.Add(buttonReturnToLastBasicMoodByEndSegment);
                _returnToBasicMoodPanel.selectables.Add(buttonReturnToLastBasicMoodImmediately);

                Image bgImageImmediately = buttonReturnToLastBasicMoodImmediately.GetComponent<Image>();
                Image bgImageEndSegment = buttonReturnToLastBasicMoodByEndSegment.GetComponent<Image>();
                _buttonBgImageReturnToBasicMoodByEndSegment = bgImageEndSegment;

                _returnToBasicMoodPanel.buttonBgImages.Add(bgImageImmediately);
                _returnToBasicMoodPanel.buttonBgImages.Add(bgImageEndSegment);

                Text labelButtonByEndSegment = buttonReturnToLastBasicMoodByEndSegment.GetComponentInChildren<Text>();
                Text labelButtonImmediately = buttonReturnToLastBasicMoodImmediately.GetComponentInChildren<Text>();
                _returnToBasicMoodPanel.labelTexts.Add(labelButtonByEndSegment);
                _returnToBasicMoodPanel.labelTexts.Add(labelButtonImmediately);
            }

            {   // Pause
                _pausePanel.panelObject = _engineControlSection.transform.Find("Pause").gameObject;
                _pausePanel.bgImage = _pausePanel.panelObject.GetComponent<Image>();

                _buttonPause = _pausePanel.panelObject.transform.Find("ButtonPause").GetComponentsInChildren<Button>(true)[0];
                _pausePanel.selectables.Add(_buttonPause);
                Image bgImageButtonPause = _buttonPause.GetComponent<Image>();
                _pausePanel.buttonBgImages.Add(bgImageButtonPause);

                //Text labelButtonPause = bgImageButtonPause.transform.GetComponentInChildren<Text>();
                Image labelButtonPause = bgImageButtonPause.transform.GetComponentInChildren<Image>();
                _pausePanel.buttonBgImages.Add(labelButtonPause);
            }


            _menuModePanel = _engineControlSection.transform.Find("MenuMode").gameObject;
            _menuModePanelText = _menuModePanel.transform.Find("ThemeText").GetComponentInChildren<Text>();
            _buttonMenuModeEnter = _engineControlSection.transform.Find("MenuMode/ButtonEnter").GetComponent<Button>();
            _buttonMenuModeEnterText = _buttonMenuModeEnter.transform.GetComponentsInChildren<Text>()[0];
            _buttonMenuModeConfigure = _engineControlSection.transform.Find("MenuMode/ButtonConfigure").GetComponent<Button>();
            _cutSceneModePanel = _engineControlSection.transform.Find("CutSceneMode").gameObject;
            _cutScenePanelText = _cutSceneModePanel.transform.Find("ThemeText").GetComponentInChildren<Text>();
            _buttonCutSceneModeEnter = _engineControlSection.transform.Find("CutSceneMode/ButtonEnter").GetComponent<Button>();
            _buttonCutSceneModeEnterText = _buttonCutSceneModeEnter.transform.GetComponentsInChildren<Text>()[0];
            _buttonCutSceneModeConfigure = _engineControlSection.transform.Find("CutSceneMode/ButtonConfigure").GetComponent<Button>();
            _buttonCutSceneModeConfigureText = _buttonCutSceneModeConfigure.transform.GetComponentsInChildren<Text>()[0];
        }
 

        _infoSection = gameObject.transform.Find("InfoSectionCanvas/InfoSection");
        if (_infoSection)
        {
            Transform toggleGroupInfoSection = _infoSection.transform.Find("ToggleGroup");
            _toggleInfoSectionOff = toggleGroupInfoSection.Find("ToggleHideInfoSection").GetComponent<Toggle>();
            _toggleInfoSectionDescription = toggleGroupInfoSection.Find("ToggleDescription").GetComponent<Toggle>();
            _toggleInfoSectionEntity = toggleGroupInfoSection.Find("ToggleEntityView").GetComponent<Toggle>();
            _toggleInfoSectionList = toggleGroupInfoSection.Find("ToggleListView").GetComponent<Toggle>();

            _toggleBasicControls = _engineControlSection.transform.Find("ToggleGroup/ToggleBasic").GetComponentInChildren<Toggle>();

            _currentThemeSection = _infoSection.transform.Find("CurrentThemeSection").gameObject;
            _currentThemeNameValue = _currentThemeSection.transform.Find("ValueName").GetComponent<Text>();
            _currentThemeIdValue = _currentThemeSection.transform.Find("ValueId").GetComponent<Text>();
            _currentThemeTypeValue = _currentThemeSection.transform.Find("ValueType").GetComponent<Text>();
            _psaiStateValue = _currentThemeSection.transform.Find("ValuePsaiState").GetComponent<Text>();

            _currentSegmentSection = _infoSection.transform.Find("CurrentSegmentSection").gameObject;
            _currentSegmentNameValue = _currentSegmentSection.transform.Find("ValueName").GetComponent<Text>();
            _currentSegmentIdValue = _currentSegmentSection.transform.Find("ValueId").GetComponent<Text>();
            _currentSegmentIntensityValue = _currentSegmentSection.transform.Find("ValueIntensity").GetComponent<Text>();
            _currentSegmentPlaycountValue = _currentSegmentSection.transform.Find("ValuePlaycount").GetComponent<Text>();
            _currentSegmentSuitabilitesValue = _currentSegmentSection.transform.Find("ValueSuitabilities").GetComponent<Text>();
            _currentSegmentRemainingMsValue = _currentSegmentSection.transform.Find("ValueRemainingMs").GetComponent<Text>();

            _buttonSpacing = 10;

            _themeListView = _infoSection.transform.Find("ThemeListView").gameObject;
            _themeListScrollView = _themeListView.transform.Find("ThemeListScrollView").gameObject;
            _themeListScrollViewContent = _themeListScrollView.transform.Find("Content").gameObject;
            _themeListViewEntryGameObject = _themeListScrollViewContent.transform.Find("ListViewEntry").gameObject;
            Text themeListViewHeadlineName = _themeListViewEntryGameObject.transform.Find("Name").GetComponent<Text>();
            _lineHeightListView = themeListViewHeadlineName.font.lineHeight;

            _segmentListView = _infoSection.transform.Find("SegmentListView").gameObject;
            _segmentListScrollView = _segmentListView.transform.Find("SegmentListScrollView").gameObject;
            _segmentListScrollViewContent = _segmentListScrollView.transform.Find("Content").gameObject;
            _segmentListViewEntryGameObject = _segmentListScrollViewContent.transform.Find("ListViewEntry").gameObject;
            _soundtrackDescriptionPanel = _infoSection.transform.Find("SoundtrackDescriptionPanel").gameObject;
            _soundtrackImage = _soundtrackDescriptionPanel.transform.GetComponentInChildren<Image>();
            Transform descriptionScrollviewContent = _soundtrackDescriptionPanel.transform.Find("ScrollView/Content");
            _soundtrackHeadlineText = descriptionScrollviewContent.transform.Find("TextPanel/Headline").GetComponentInChildren<Text>();
            _soundtrackDescriptionText = descriptionScrollviewContent.transform.Find("TextPanel/Description").GetComponentInChildren<Text>();
        }

        UpdateActiveStatesOfAllPanels();
    }


    void BuildDatastructuresBasedOnCurrentPsaiSoundtrack()
    {
        SoundtrackInfo soundtrackInfo = PsaiCore.Instance.GetSoundtrackInfo();

        _themeTypesToThemeIds[ThemeType.basicMood] = new List<int>();
        _themeTypesToThemeIds[ThemeType.basicMoodAlt] = new List<int>();
        _themeTypesToThemeIds[ThemeType.dramaticEvent] = new List<int>();
        _themeTypesToThemeIds[ThemeType.action] = new List<int>();
        _themeTypesToThemeIds[ThemeType.shock] = new List<int>();
        _themeTypesToThemeIds[ThemeType.highlightLayer] = new List<int>();
        
        _themeIdsList.Clear();


        if (soundtrackInfo.themeCount == 0)
        {
            if (_errorTextWindow)
            {
                _errorTextWindow.text = "NO SOUNDTRACK LOADED\n\n Troubleshooting:\n\n1. The folder containing all psai soundtrack data must be located within the 'Resources' folder of your project.\n2. Your Scene must contain the 'Psai.prefab' Game Object with both a PsaiSoundtrackLoader and a PsaiCoreManager-Component.\n3. The PsaiSoundtrackLoader-Component needs to hold the path to the soundtrack file. Drag & Drop that file from your Soundtrack folder in your Project window onto the PsaiPlayerUi component.\n\nPlease see the log output window for more information.";
            }            
        }
        else
        {            
            if (_errorTextWindow)
            {
                _errorTextWindow.gameObject.SetActive(false);
            }            

            _themeIds = soundtrackInfo.themeIds;
            _themeIdsList.AddRange(_themeIds);

            // Build _themeInfos Cache
            foreach (int themeId in _themeIds)
            {
                ThemeInfo themeInfo = PsaiCore.Instance.GetThemeInfo(themeId);
                _themeInfos[themeId] = themeInfo;

                if (_themeTypesToThemeIds.ContainsKey(themeInfo.type))
                {
                    _themeTypesToThemeIds[themeInfo.type].Add(themeId);
                }
            }

            RebuildSegmentInfoCache();

            _menuThemeId = _themeIds[0];
            _menuThemeIntensity = 1.0f;
            _cutSceneThemeId = _themeIds[0];
            _cutSceneThemeIntensity = 1.0f;

            _flashIntensity = 0.0f;
            _flashIncrease = true;

            _buttonHoldIntensity.interactable = true;

            UpdateAdvancedControlSection();
            _initialized = true;

        }
    }

    void Update()
    {
        if (!this._initialized)
        {
            BuildDatastructuresBasedOnCurrentPsaiSoundtrack();

            if (_themesTriggerCanvas && _themesTriggerCanvas.gameObject.activeInHierarchy)
            {
                BuildThemesTriggerSection();
            }

            if (_themeListView)
            {
                BuildThemeListView();
            }

            if (_segmentListView)
            {
                BuildSegmentListView();
            }

            if (_soundtrackDescriptionPanel)
            {
                LoadSoundtrackDescriptionData();
            }

            if (_themeListView)
            {
                if (_themeIds.Length > 0)
                {
                    SelectedThemeId = _themeIds[0];
                    RewriteThemeListViewEntries();
                }
                UpdateThemeListView();
            }

            if (_segmentListView)
            {
                UpdateSegmentListViewColorsAndText(true, false);
            }


            if (_infoSection && _infoSection.gameObject.activeInHierarchy)
            {
                switch (_infoSectionSelection)
                {
                    case InfoSectionSelection.off:
                        _toggleInfoSectionOff.isOn = true;
                        break;

                    case InfoSectionSelection.description:
                        _toggleInfoSectionDescription.isOn = true;
                        break;

                    case InfoSectionSelection.entity:
                        _toggleInfoSectionEntity.isOn = true;
                        break;

                    case InfoSectionSelection.list:
                        _toggleInfoSectionList.isOn = true;
                        break;
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // cycle GUI color flashing
        //////////////////////////////////////////////////////////////////////////
        float flashDeltaIntensity = Time.deltaTime * GUI_FLASH_FREQUENCY;

        if (_flashIncrease)
        {
            _flashIntensity += flashDeltaIntensity;
            if (_flashIntensity > 1.0f)
            {
                _flashIntensity = 2.0f - _flashIntensity;
                _flashIncrease = false;
            }
        }
        else
        {
            _flashIntensity -= flashDeltaIntensity;
            if (_flashIntensity < 0.0f)
            {
                _flashIntensity = -_flashIntensity;
                _flashIncrease = true;
            }
        }
        //////////////////////////////////////////////////////////////////////////

        //TODO: add lastError to log window

        /////////////////////////////////////////////////////////////////////////

        UpdateActiveStatesOfAllPanels();    // in case the user changed one of the _showPanel booleans        


        PsaiInfo psaiInfo = PsaiCore.Instance.GetPsaiInfo();
        int currentSegmentId = PsaiCore.Instance.GetCurrentSegmentId(); 
        SegmentInfo segmentInfo = PsaiCore.Instance.GetSegmentInfo(currentSegmentId);
        float currentIntensity = PsaiCore.Instance.GetCurrentIntensity();        
        _segmentIntensitySlider.value = segmentInfo.intensity;
        if (segmentInfo.intensity > 0)
        {
            _segmentIntensityBarFillImage.color = COLOR_DARKGREEN;
        }
        else
        {
            _segmentIntensityBarFillImage.color = Color.clear;
        }

        ThemeType upcomingThemeType = ThemeType.none;
        if (psaiInfo.themesQueued > 0)
        {
            ThemeInfo upcomingThemeInfo = PsaiCore.Instance.GetThemeInfo(psaiInfo.upcomingThemeId);
            if (upcomingThemeInfo != null)
            {
                upcomingThemeType = upcomingThemeInfo.type;
            }
        }


        int currentThemeId = PsaiCore.Instance.GetCurrentThemeId();
        int remainingMsOfSegmentPlaybackInThisFrame = PsaiCore.Instance.GetRemainingMillisecondsOfCurrentSegmentPlayback();
        bool newSegmentPlaybackStartedInThisFrame =  remainingMsOfSegmentPlaybackInThisFrame > _remainingMillisecondsOfSegmentPlaybackInLastFrame;
        bool currentThemeChangedInThisFrame = (currentThemeId != _playingThemeIdInLastFrame);
        bool psaiStateChangedInThisFrame = _psaiStateInLastFrame != psaiInfo.psaiState;


        if (currentThemeChangedInThisFrame)
        {
            ThemeInfo currentThemeInfo = PsaiCore.Instance.GetThemeInfo(currentThemeId);

            if (currentThemeInfo != null)
            {
                _currentThemeType = currentThemeInfo.type;
            }
            else
            {
                _currentThemeType = ThemeType.none;
            }            
        }

        /////////////////////////////////////////////////////////////////////////
        // Dynamic Intensity Bar
        /////////////////////////////////////////////////////////////////////////

        if (_intensitySection && _intensitySection.gameObject.activeInHierarchy)
        {
            Color intensityColor;

            // non-interrupting Trigger has been received: show upcoming intensity
            if (Mathf.Abs(psaiInfo.upcomingIntensity - currentIntensity) > Mathf.Epsilon)
            {
                _dynamicIntensitySlider.value = psaiInfo.upcomingIntensity;
                intensityColor = new Color(0.5f + 0.5f * _flashIntensity, 0.5f + 0.5f * _flashIntensity, 0, 0.66f);
            }
            else
            {
                _dynamicIntensitySlider.value = currentIntensity;
                if (currentIntensity > 0)
                {
                    intensityColor = COLOR_LIGHTGREEN;
                }
                else
                {
                    intensityColor = Color.clear;
                }                
            }

            if (_dynamicIntensityBarFillImage.color != intensityColor)
            {
                _dynamicIntensityBarFillImage.color = intensityColor;
            }

            _dynamicIntensityValueText.text = (currentIntensity * 100).ToString("F1") + " %";
            _segmentIntensityValueText.text = (segmentInfo.intensity * 100).ToString("F1") + " %";
        }


        /////////////////////////////////////////////////////////////////////////
        // Theme Trigger Buttons
        /////////////////////////////////////////////////////////////////////////

        if (_themesTriggerCanvas && _themesTriggerCanvas.gameObject.activeInHierarchy)
        {
            int themesTriggerSectionScrollPosition = (int)((RectTransform)_themesTriggerSectionScrollViewContent.transform).anchoredPosition.y;
            if (themesTriggerSectionScrollPosition != _themesTriggerSectionScrollPositionInLastFrame)
            {
                UpdateThemeTriggerUisActiveStates();
            }
            _themesTriggerSectionScrollPositionInLastFrame = themesTriggerSectionScrollPosition;


            if (_configureMenuMode || _configureCutScene)
            {
                Color indicatorColor = Color.Lerp(COLOR_LIGHTYELLOW, COLOR_MIDDLEGREY, _flashIntensity);

                foreach (ThemeTriggerButtonUi themeTriggerButtonUi in _themeTriggerButtonsToThemeIds.Keys)
                {
                    themeTriggerButtonUi.button.interactable = true;
                    themeTriggerButtonUi.indicatorImage.color = indicatorColor;
                }
            }
            else
            {
                foreach (ThemeTriggerButtonUi themeTriggerButtonUi in _themeTriggerButtonsToThemeIds.Keys)
                {
                    themeTriggerButtonUi.button.interactable = true;

                    int themeId = _themeTriggerButtonsToThemeIds[themeTriggerButtonUi];
                    ThemeInfo themeInfo = _themeInfos[themeId];
                    if (themeInfo.type == ThemeType.highlightLayer
                        && currentSegmentId != -1
                        && PsaiCore.Instance.CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(currentSegmentId, themeInfo.id) == false)
                    {
                        themeTriggerButtonUi.button.interactable = false;
                    }
                    else if (_currentThemeType == ThemeType.action || upcomingThemeType == ThemeType.action)
                    {
                        // if Action is playing or queued, triggers to basicMoodAlt und dramaticEvent will be ignored.
                        if (themeInfo.type == ThemeType.basicMoodAlt || themeInfo.type == ThemeType.dramaticEvent)
                        {
                            themeTriggerButtonUi.button.interactable = false;
                        }
                    }


                    Color indicatorColor = _colorIndicatorDefault;

                    if (themeId == psaiInfo.lastBasicMoodThemeId)
                    {
                        indicatorColor = COLOR_DARKGREEN;
                    }


                    if (themeId == psaiInfo.upcomingThemeId)
                    {
                        indicatorColor = Color.Lerp(COLOR_LIGHTYELLOW, COLOR_MIDDLEGREY, _flashIntensity);
                    }

                    if (themeId == psaiInfo.effectiveThemeId)
                    {
                        switch (psaiInfo.psaiState)
                        {
                            case PsaiState.playing:
                                indicatorColor = new Color(0.1f, 1.0f, 0.1f, 1.0f);
                                break;

                            case PsaiState.rest:
                                indicatorColor = new Color(0.0f, 0.5f, 0.0f, _flashIntensity);
                                break;
                        }
                    }

                    themeTriggerButtonUi.indicatorImage.color = indicatorColor;
                }
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////////////
        /// Engine Control Section (Stop Music, ...)
        /////////////////////////////////////////////////////////////////////////////////////////////

        if (_engineControlSection && _engineControlSection.gameObject.activeInHierarchy)
        {
            UpdateControlSectionButtons();
        }



        ///////////////////////////////////////////////////////////
        // Info Section
        ///////////////////////////////////////////////////////////

        if (_infoSection && _infoSection.gameObject.activeInHierarchy)
        {
            if (currentThemeChangedInThisFrame)
            {
                SelectedThemeId = currentThemeId;
            }

            if (newSegmentPlaybackStartedInThisFrame)
            {
                UpdateSegmentListViewColorsAndText(true, true);
            }
            else if (_targetSegmentIdInLastFrame != psaiInfo.targetSegmentId)
            {
                UpdateSegmentListViewColorsAndText(false, false);
            }

            // (de)activate all the entries which are currently (in)visible to avoid breaking the Canvas vertex limit
            int segmentListScrollPosition = (int)((RectTransform)_segmentListScrollViewContent.transform).anchoredPosition.y;
            int themeListScrollPosition = (int)((RectTransform)_themeListScrollViewContent.transform).anchoredPosition.y;
            if (_infoSectionSelection == InfoSectionSelection.list)
            {
                if (segmentListScrollPosition != _segmentListScrollPositionInLastFrame)
                    UpdateSegmentListViewItemsActiveStates();

                if (themeListScrollPosition != _themeListScrollPositionInLastFrame)
                    UpdateThemeListViewItemsActiveStates();
            }
            _segmentListScrollPositionInLastFrame = segmentListScrollPosition;
            _themeListScrollPositionInLastFrame = themeListScrollPosition;


            if (_infoSectionSelection == InfoSectionSelection.entity || psaiStateChangedInThisFrame)
            {
                UpdateInfoSectionEntityView();
            }
        }


        _playingThemeIdInLastFrame = currentThemeId;
        _psaiStateInLastFrame = psaiInfo.psaiState;
        _targetSegmentIdInLastFrame = psaiInfo.targetSegmentId;
        _remainingMillisecondsOfSegmentPlaybackInLastFrame = remainingMsOfSegmentPlaybackInThisFrame;


        // decrement countdowns
        if (_playbackCountdowns.Count > 0)
        {
            List<int> snippetIdsToRemove = new List<int>();
            int[] countdownKeysList = new int[_playbackCountdowns.Count];
            _playbackCountdowns.Keys.CopyTo(countdownKeysList, 0);

            foreach (int snippetId in countdownKeysList)
            {
                int countDownMs = _playbackCountdowns[snippetId];
                countDownMs -= (int)(Time.deltaTime * 1000.0f);

                if (countDownMs > 0)
                    _playbackCountdowns[snippetId] = countDownMs;
                else
                    snippetIdsToRemove.Add(snippetId);
            }

            foreach (int snippetId in snippetIdsToRemove)
            {
                _playbackCountdowns.Remove(snippetId);
            }

            if (snippetIdsToRemove.Count > 0)
            {
                UpdateSegmentListViewColorsAndText(false, false);
            }
        }
    }

    private void UpdateInfoSectionEntityView()
    {
        PsaiInfo psaiInfo = PsaiCore.Instance.GetPsaiInfo();
        int currentSegmentId = PsaiCore.Instance.GetCurrentSegmentId();
        ThemeInfo currentThemeInfo = PsaiCore.Instance.GetThemeInfo(PsaiCore.Instance.GetCurrentThemeId());
        SegmentInfo segmentInfo = PsaiCore.Instance.GetSegmentInfo(currentSegmentId);

        //if (psaiInfo.psaiState == PsaiState.playing)
        {
            if (currentThemeInfo != null)
            {
                _currentThemeNameValue.text = currentThemeInfo.name;
                _currentThemeIdValue.text = currentThemeInfo.id.ToString();
                _currentThemeTypeValue.text = Theme.ThemeTypeToString(currentThemeInfo.type);
            }
            else
            {
                _currentThemeNameValue.text = "";
                _currentThemeIdValue.text = "";
                _currentThemeTypeValue.text = "";
            }
            _psaiStateValue.text = psaiInfo.psaiState.ToString();

            if (segmentInfo != null && currentSegmentId != -1)
            {
                _currentSegmentNameValue.text = segmentInfo.name;
                _currentSegmentIdValue.text = segmentInfo.id.ToString();
                _currentSegmentIntensityValue.text = segmentInfo.intensity.ToString("F2");
                _currentSegmentPlaycountValue.text = segmentInfo.playcount.ToString();
                _currentSegmentSuitabilitesValue.text = Segment.GetStringFromSegmentSuitabilities(segmentInfo.segmentSuitabilitiesBitfield);
                _currentSegmentRemainingMsValue.text = PsaiCore.Instance.GetRemainingMillisecondsOfCurrentSegmentPlayback().ToString();
            }
            else
            {
                _currentSegmentNameValue.text = "";
                _currentSegmentIdValue.text = "";
                _currentSegmentIntensityValue.text = "";
                _currentSegmentPlaycountValue.text = "";
                _currentSegmentSuitabilitesValue.text = "";
                _currentSegmentRemainingMsValue.text = "";
            }
        }


        if (psaiInfo.remainingMillisecondsInRestMode > 0)
        {
            string wakeUpString = " , waking up in: " + psaiInfo.remainingMillisecondsInRestMode.ToString();
            _psaiStateValue.text = psaiInfo.psaiState.ToString() + wakeUpString;
        }
        else
        {
            _psaiStateValue.text = psaiInfo.psaiState.ToString();
        }  
    }

    private void StorePlaybackCountdownForSegment(int snippetId)
    {
        if (_segmentInfos.ContainsKey(snippetId))
        {
            SegmentInfo snippetInfo = _segmentInfos[snippetId];
            _playbackCountdowns[snippetId] = snippetInfo.fullLengthInMilliseconds;
        }
    }

    private void ConfigureStopMusic(bool configure)
    {
        _configureStopMusic = configure;

        _stopMusicFadeoutSecondsSlider.gameObject.SetActive(_configureStopMusic);
        _buttonStopMusicImmediately.gameObject.SetActive(!_configureStopMusic);
        _buttonStopMusicViaEndSegment.gameObject.SetActive(!_configureStopMusic);
    }


    public void OnClick_ButtonHold()
    {
        PsaiInfo psaiInfo = PsaiCore.Instance.GetPsaiInfo();
        if (psaiInfo != null)
        {
            bool intensityIsHeld = !psaiInfo.intensityIsHeld;

            PsaiCore.Instance.HoldCurrentIntensity(intensityIsHeld);
           
            if (intensityIsHeld)
            {
                ColorBlock cb = _buttonHoldIntensity.colors;
                cb.normalColor = COLOR_DARKYELLOW;
                cb.highlightedColor = COLOR_LIGHTYELLOW;
                _buttonHoldIntensity.colors = cb;
            }
            else
            {
                ColorBlock cb = _buttonHoldIntensity.colors;
                cb.normalColor = _buttonColorHoldNormal;
                cb.highlightedColor = _buttonColorHoldHighlighted;
                _buttonHoldIntensity.colors = cb;
            }
        }
    }



    private void LoadDataToThemePanelItem(ThemeTriggerButtonUi themeTriggerButtonUi, ThemeInfo themeInfo)
    {
        themeTriggerButtonUi.themeNameText.text = themeInfo.name;
        themeTriggerButtonUi.intensitySlider.value = 0.75f;
        themeTriggerButtonUi.indicatorImage.color = _colorIndicatorDefault;
    }

    private void BuildThemesTriggerSection()
    {
        int yPos = -(int)(((RectTransform)_themeTriggerPanelItemBlueprint.transform).rect.height * 0.5);
        BuildThemeSectionForThemeType(ThemeType.basicMood, ref yPos);
        BuildThemeSectionForThemeType(ThemeType.basicMoodAlt, ref yPos);
        BuildThemeSectionForThemeType(ThemeType.dramaticEvent, ref yPos);
        BuildThemeSectionForThemeType(ThemeType.action, ref yPos);
        BuildThemeSectionForThemeType(ThemeType.shock, ref yPos);
        BuildThemeSectionForThemeType(ThemeType.highlightLayer, ref yPos);
    }

    private void BuildThemeSectionForThemeType(ThemeType themeType, ref int yPos)
    {
        List<int> themeIds = _themeTypesToThemeIds[themeType];
        
        if (themeIds.Count > 0)
        {
            int xPos = _tabOffsetX2;
            GameObject themeTypeLabel = GameObject.Instantiate(_themeTypeLabelBlueprint);
            themeTypeLabel.SetActive(true);
            themeTypeLabel.transform.SetParent(_themesTriggerSectionScrollViewContent.transform);
            themeTypeLabel.GetComponent<Text>().text = Theme.ThemeTypeToString(themeType);
            ((RectTransform)(themeTypeLabel.transform)).anchoredPosition = new Vector2(xPos, yPos);

            xPos = _tabOffsetX3;
            for (int i = 0; i < themeIds.Count; i++)
            {
                int themeId = themeIds[i];
                ThemeTriggerButtonUi themeTriggerButtonUi = new ThemeTriggerButtonUi(_themeTriggerPanelItemBlueprint);
                _themeTriggerButtonsToThemeIds[themeTriggerButtonUi] = themeId;
                _themeTriggerButtonGoToThemeIds[themeTriggerButtonUi.goButton] = themeId;
                _themeIdsToTriggerSliders[themeId] = themeTriggerButtonUi.intensitySlider;
                _themeTriggerSlidersToThemeIds[themeTriggerButtonUi.intensitySlider.gameObject] = themeId;
                _themeIdsToTriggerButtonUis[themeId] = themeTriggerButtonUi;
                themeTriggerButtonUi.goTriggerItemRoot.transform.SetParent(_themesTriggerSectionScrollViewContent.transform);

                ThemeInfo themeInfo = PsaiCore.Instance.GetThemeInfo(themeId);
                LoadDataToThemePanelItem(themeTriggerButtonUi, themeInfo);

                RectTransform rectTransform = ((RectTransform)themeTriggerButtonUi.goTriggerItemRoot.transform);
                rectTransform.anchoredPosition = new Vector2(xPos, yPos);

                xPos += (int)(rectTransform.rect.width);

                if ((i < (themeIds.Count - 1) && xPos > Screen.width - (int)(rectTransform.rect.width)))
                {
                    xPos = _tabOffsetX3;
                    yPos -= _themeTriggerPanelItemHeight;
                }
                else
                {
                    xPos += _buttonSpacing;
                }
            }

            xPos = _tabOffsetX3;
            yPos -= _themeTriggerPanelItemHeight + _buttonSpacing;
        }

        ((RectTransform)_themesTriggerSectionScrollViewContent.transform).sizeDelta = new Vector2(Screen.width - _tabOffsetX3, -yPos);           
    }

    /// <summary>
    /// Writes new text to the ThemeListViewEntries, call this after sorting.
    /// </summary>
    private void RewriteThemeListViewEntries()
    {
        for (int i = 0; i < _themeIdsList.Count; i++)
        {
            int themeId = _themeIdsList[i];
            ThemeListViewEntry themeListViewEntry = _themeListViewEntries[i];
            ThemeInfo themeInfo = _themeInfos[themeId];

            themeListViewEntry.textName.text = themeInfo.name;
            themeListViewEntry.textThemeId.text = themeInfo.id.ToString();
            themeListViewEntry.textThemeType.text = Theme.ThemeTypeToString(themeInfo.type);
            themeListViewEntry.themeId = themeId;

            _themeListViewButtonsToThemeIds[themeListViewEntry.textName.gameObject] = themeInfo.id;
        }
    }

    private void BuildThemeListView()
    {
        _themeListViewButtonsToThemeIds.Clear();
        _themeListViewEntries.Clear();

        int yPos = 0;
        ((RectTransform)_themeListScrollViewContent.transform).sizeDelta = new Vector2(((RectTransform)_themeListScrollViewContent.transform).sizeDelta.x, _lineHeightListView * _themeInfos.Count);

        for (int i = 0; i < _themeIdsList.Count; i++)
        {
            int themeId = _themeIdsList[i];
            ThemeInfo themeInfo = _themeInfos[themeId];
            yPos -= _lineHeightListView;

            ThemeListViewEntry themeListViewEntry = new ThemeListViewEntry();
            themeListViewEntry.themeId = themeInfo.id;

            themeListViewEntry.gameObj = GameObject.Instantiate(_themeListViewEntryGameObject);
            themeListViewEntry.gameObj.transform.SetParent(_themeListScrollViewContent.transform);
            ((RectTransform)themeListViewEntry.gameObj.transform).anchoredPosition = new Vector2(0, yPos);
            ((RectTransform)themeListViewEntry.gameObj.transform).offsetMax = new Vector2(0, ((RectTransform)themeListViewEntry.gameObj.transform).offsetMax.y);
            themeListViewEntry.backgroundImage = themeListViewEntry.gameObj.transform.GetComponent<Image>();

            GameObject nameTextObject = themeListViewEntry.gameObj.transform.GetChild(0).gameObject;
            themeListViewEntry.textName = nameTextObject.transform.GetComponent<Text>();

            _themeListViewButtonsToThemeIds[nameTextObject] = themeInfo.id;

            nameTextObject.gameObject.GetComponent<Button>().onClick.AddListener(() => OnClick_ThemeListViewEntry(nameTextObject));

            GameObject idTextObject = themeListViewEntry.gameObj.transform.GetChild(1).gameObject;
            themeListViewEntry.textThemeId = idTextObject.transform.GetComponent<Text>();

            GameObject themeTypeTextObject = themeListViewEntry.gameObj.transform.GetChild(2).gameObject;
            themeListViewEntry.textThemeType = themeTypeTextObject.transform.GetComponent<Text>();
            _themeListViewEntries.Add(themeListViewEntry);
        }

        _themeListViewEntryGameObject.SetActive(false);

    }

    /// <summary>
    /// Builds the SegmentListView based on the Theme with the most Segments.
    /// </summary>
    private void BuildSegmentListView()
    {
        if (_themeInfos.Count > 0)
        {
            ThemeInfo themeInfoWithMostSegments = null;
            foreach (ThemeInfo themeInfo in _themeInfos.Values)
            {
                if (themeInfoWithMostSegments == null || themeInfo.segmentIds.Length > themeInfoWithMostSegments.segmentIds.Length)
                {
                    themeInfoWithMostSegments = themeInfo;
                }
            }

            int yPos = 0;
            ((RectTransform)_segmentListScrollViewContent.transform).sizeDelta = new Vector2(((RectTransform)_segmentListScrollViewContent.transform).sizeDelta.x, _lineHeightListView * themeInfoWithMostSegments.segmentIds.Length);

            _segmentListViewEntries.Clear();
            foreach (int segmentId in themeInfoWithMostSegments.segmentIds)
            {
                SegmentListViewEntry segmentListViewEntry = new SegmentListViewEntry();
                yPos -= _lineHeightListView;

                GameObject listViewEntryGameObject = GameObject.Instantiate(_segmentListViewEntryGameObject);
                listViewEntryGameObject.transform.SetParent(_segmentListScrollViewContent.transform);
                ((RectTransform)listViewEntryGameObject.transform).anchoredPosition = new Vector2(((RectTransform)_segmentListViewEntryGameObject.transform).anchoredPosition.x, yPos);
                ((RectTransform)listViewEntryGameObject.transform).offsetMax = new Vector2(0, ((RectTransform)listViewEntryGameObject.transform).offsetMax.y);
                segmentListViewEntry.backgroundImage = listViewEntryGameObject.GetComponent<Image>();
                segmentListViewEntry.gameObj = listViewEntryGameObject;

                GameObject nameTextObject = listViewEntryGameObject.transform.GetChild(0).gameObject;
                segmentListViewEntry.textName = nameTextObject.transform.GetComponent<Text>();

                GameObject suitabiltiesTextObject = listViewEntryGameObject.transform.GetChild(1).gameObject;
                segmentListViewEntry.textSuitabilities = suitabiltiesTextObject.transform.GetComponent<Text>();

                GameObject lengthTextObject = listViewEntryGameObject.transform.GetChild(2).gameObject;
                segmentListViewEntry.textLength = lengthTextObject.transform.GetComponent<Text>();

                GameObject intensityTextObject = listViewEntryGameObject.transform.GetChild(3).gameObject;
                segmentListViewEntry.textIntensity = intensityTextObject.transform.GetComponent<Text>();

                GameObject playcountTextObject = listViewEntryGameObject.transform.GetChild(4).gameObject;
                segmentListViewEntry.textPlaycount = playcountTextObject.transform.GetComponent<Text>();

                _segmentListViewEntries.Add(segmentListViewEntry);

                segmentListViewEntry.textName.text = segmentId.ToString();   // just to avoid the 'segmentId is never used' warning;
            }
        }
        _segmentListViewEntryGameObject.SetActive(false);

    }

    public void OnClick_ThemeHeaderName()
    {
        //Debug.Log("OnClick_ThemeHeaderName()");
        _themeIdsList.Sort(CompareThemesByName);

        _sortThemesAscending = !_sortThemesAscending;
        if (!_sortThemesAscending)
        {
            _themeIdsList.Reverse();
        }

        RewriteThemeListViewEntries();
    }

    public void OnClick_ThemeHeaderId()
    {
        _themeIdsList.Sort();
        _sortThemesAscending = !_sortThemesAscending;
        if (!_sortThemesAscending)
        {
            _themeIdsList.Reverse();
        }

        RewriteThemeListViewEntries();
    }

    public void OnClick_ThemeHeaderType()
    {
        _themeIdsList.Sort(CompareThemesByType);
        _sortThemesAscending = !_sortThemesAscending;
        if (!_sortThemesAscending)
        {
            _themeIdsList.Reverse();
        }
        RewriteThemeListViewEntries();
    }                                    


    public void OnClick_SegmentHeaderName()
    {
        if (_segmentSorting == SegmentSorting.name)
        {
            _sortSegmentsAscending = !_sortSegmentsAscending;
        }        
        _segmentSorting = SegmentSorting.name;

        LoadDataToSegmentListView(SelectedThemeId);
    }

    public void OnClick_SegmentHeaderSuitabilites()
    {
        if (_segmentSorting == SegmentSorting.suitabilites)
        {
            _sortSegmentsAscending = !_sortSegmentsAscending;
        }
        _segmentSorting = SegmentSorting.suitabilites;

        LoadDataToSegmentListView(SelectedThemeId);
    }

    public void OnClick_SegmentHeaderIntensity()
    {        
        if (_segmentSorting == SegmentSorting.intensity)
        {
            _sortSegmentsAscending = !_sortSegmentsAscending;
        }
        _segmentSorting = SegmentSorting.intensity;

        LoadDataToSegmentListView(SelectedThemeId);
    }

    public void OnClick_SegmentHeaderLength()
    {
        if (_segmentSorting == SegmentSorting.length)
        {
            _sortSegmentsAscending = !_sortSegmentsAscending;
        }
        _segmentSorting = SegmentSorting.length;

        LoadDataToSegmentListView(SelectedThemeId);
    }

    public void OnClick_SegmentHeaderPlaycount()
    {
        if (_segmentSorting == SegmentSorting.playcount)
        {
            _sortSegmentsAscending = !_sortSegmentsAscending;
        }
        _segmentSorting = SegmentSorting.playcount;

        LoadDataToSegmentListView(SelectedThemeId);
    }

    private void LoadSoundtrackDescriptionData()
    {

        if (_soundtrackImage != null && _soundtrackHeadlineText != null)
        {
            PsaiSoundtrackLoader loader = PsaiCoreManager.Instance.GetComponent<PsaiSoundtrackLoader>();

            string soundtrackDirectory = System.IO.Path.GetDirectoryName(loader.pathToSoundtrackFileWithinResourcesFolder);
            string fileBaseDir = soundtrackDirectory + "/";
            string imagePath = fileBaseDir + "image";
            string descriptionPath = fileBaseDir + "description";

            Sprite imageSprite = (Sprite)Resources.Load(imagePath, typeof(Sprite));

            if (_soundtrackImage)
            {
                _soundtrackImage.sprite = imageSprite;
                if (imageSprite == null)
                {
                    _soundtrackImage.color = Color.clear;
                }
            }

            TextAsset soundtrackDescription = (TextAsset)Resources.Load(descriptionPath, typeof(TextAsset));

            if (soundtrackDescription != null)
            {
                string[] lines = soundtrackDescription.text.Split(new[] { '\n' });
                if (lines != null && lines.Length > 0)
                {
                    _soundtrackHeadlineText.text = lines[0];

                    _soundtrackDescriptionText.text = "";
                    if (lines.Length > 1)
                    {
                        string descr = "";
                        for (int i = 1; i < lines.Length; i++)
                        {
                            descr += lines[i].Trim();
                            descr += "\n";
                        }
                        _soundtrackDescriptionText.text = descr;
                    }
                }
            }
        }
    }

    private void LoadDataToSegmentListView(int themeId)
    {
        if (_segmentListView)
        {
            if (_themeInfos.ContainsKey(themeId))
            {
                ThemeInfo themeInfo = _themeInfos[themeId];

                ((RectTransform)_segmentListScrollViewContent.transform).sizeDelta = new Vector2(((RectTransform)_segmentListScrollViewContent.transform).sizeDelta.x, _lineHeightListView * (themeInfo.segmentIds.Length + 2));

                _segmentListViewEntriesToSegmentIds.Clear();
                _segmentIdsListOfSelectedTheme.Clear();
                _segmentIdsListOfSelectedTheme.AddRange(themeInfo.segmentIds);

                switch (_segmentSorting)
                {
                    case SegmentSorting.name:
                        _segmentIdsListOfSelectedTheme.Sort(CompareSegmentsByName);
                        break;

                    case SegmentSorting.suitabilites:
                        _segmentIdsListOfSelectedTheme.Sort(CompareSegmentsBySuitablilies);
                        break;

                    case SegmentSorting.intensity:
                        _segmentIdsListOfSelectedTheme.Sort(CompareSegmentsByIntensity);
                        break;

                    case SegmentSorting.playcount:
                        _segmentIdsListOfSelectedTheme.Sort(CompareSegmentsByPlaycount);
                        break;

                    case SegmentSorting.length:
                        _segmentIdsListOfSelectedTheme.Sort(CompareSegmentsByLength);
                        break;
                }

                if (!_sortSegmentsAscending)
                {
                    _segmentIdsListOfSelectedTheme.Reverse();
                }

                int segmentListViewEntryIndex = 0;
                for (int i = 0; i < _segmentIdsListOfSelectedTheme.Count; i++)
                {
                    int segmentId = _segmentIdsListOfSelectedTheme[i];

                    // as the playcount might have changed, we fetch the segmentInfo freshly from PsaiCore and store the update in the _segmentInfo cache
                    SegmentInfo segmentInfo = PsaiCore.Instance.GetSegmentInfo(segmentId);
                    _segmentInfos[segmentId] = segmentInfo;

                    SegmentListViewEntry segmentListViewEntry = _segmentListViewEntries[segmentListViewEntryIndex];
                    segmentListViewEntry.segmentId = segmentId;

                    segmentListViewEntry.textName.text = segmentInfo.name;
                    segmentListViewEntry.textSuitabilities.text = Segment.GetStringFromSegmentSuitabilities(_segmentInfos[segmentId].segmentSuitabilitiesBitfield);
                    float fullLengthInSeconds = segmentInfo.fullLengthInMilliseconds * 0.001f;
                    segmentListViewEntry.textLength.text = fullLengthInSeconds.ToString("F2");
                    segmentListViewEntry.textIntensity.text = segmentInfo.intensity.ToString("F2");
                    segmentListViewEntry.textPlaycount.text = segmentInfo.playcount.ToString();

                    segmentListViewEntry.gameObj.SetActive(true);
                    _segmentListViewEntriesToSegmentIds[segmentListViewEntry.gameObj] = segmentId;

                    segmentListViewEntryIndex++;
                    //Debug.Log("segmentListViewEntries[" + segmentListViewEntryIndex + "]=" + segmentListViewEntry.ToString());
                }

                // clear the unneeded remaining segmentListViewEntries
                while (segmentListViewEntryIndex < _segmentListViewEntries.Count)
                {
                    SegmentListViewEntry segmentListViewEntry = _segmentListViewEntries[segmentListViewEntryIndex];
                    segmentListViewEntry.gameObj.SetActive(false);
                    segmentListViewEntryIndex++;
                }
            }
            else
            {
                foreach (SegmentListViewEntry entry in _segmentListViewEntries)
                {
                    entry.textName.text = string.Empty;
                    entry.textSuitabilities.text = string.Empty;
                    entry.textLength.text = string.Empty;
                    entry.textIntensity.text = string.Empty;
                    entry.textPlaycount.text = string.Empty;
                }
            }

            UpdateSegmentListViewColorsAndText(true, false);

        }
    }

    /// <summary>
    /// Updates the item colors of the ThemeListView.
    /// </summary>
    private void UpdateThemeListView()
    {
        foreach(ThemeListViewEntry themeListViewEntry in _themeListViewEntries)
        {
            if (themeListViewEntry.themeId == SelectedThemeId)
            {
                themeListViewEntry.backgroundImage.color = COLOR_LIST_BACKGROUND_SELECTED;
            }
            else
            {
                themeListViewEntry.backgroundImage.color = Color.clear;
            }
        }
    }

    /// <summary>
    /// Updates the item colors and text of the SegmentListView.
    /// </summary>
    private void UpdateSegmentListViewColorsAndText(bool updatePlaycountText, bool autoScrollToCurrentSegment)
    {
        int currentSegmentId = PsaiCore.Instance.GetCurrentSegmentId();
        PsaiInfo psaiInfo = PsaiCore.Instance.GetPsaiInfo();

        for (int i = 0; i < _segmentListViewEntries.Count; i++ )
        {
            SegmentListViewEntry segmentListViewEntry = _segmentListViewEntries[i];
            segmentListViewEntry.textName.color = COLOR_LIGHTGREY;

            if (psaiInfo.psaiState == PsaiState.playing && psaiInfo.paused == false)
            {
                segmentListViewEntry.textName.GetComponent<Button>().interactable = false;
            }
            else
            {
                segmentListViewEntry.textName.GetComponent<Button>().interactable = true;
            }


            if (segmentListViewEntry.segmentId == currentSegmentId)
            {
                segmentListViewEntry.textName.color = Color.green;
                //segmentListViewEntry.backgroundImage.color = COLOR_LIST_BACKGROUND_SELECTED;

                // auto-scroll if current Segment changed
                if (autoScrollToCurrentSegment)
                {
                    int verticalPosition = _lineHeightListView * i;
                    ((RectTransform)_segmentListScrollViewContent.transform).anchoredPosition = new Vector2(0, verticalPosition);
                }
                if (updatePlaycountText)
                {
                    SegmentInfo segmentInfo = PsaiCore.Instance.GetSegmentInfo(currentSegmentId);
                    segmentListViewEntry.textPlaycount.text = segmentInfo.playcount.ToString();
                }
            }
            else if (segmentListViewEntry.segmentId == psaiInfo.targetSegmentId)
            {
                segmentListViewEntry.textName.color = Color.yellow;
            }
            else
            {
                segmentListViewEntry.backgroundImage.color = Color.clear;
            }

            // Text color

            if (_playbackCountdowns.ContainsKey(segmentListViewEntry.segmentId))
            {
                segmentListViewEntry.textName.color = COLOR_LIGHTGREEN;
            }
        }

        UpdateSegmentListViewItemsActiveStates();
    }

    // (de)activate the (in)visible entries to avoid breaking the vertex limit.
    // Call this each time the scroll position has changed.
    private void UpdateSegmentListViewItemsActiveStates()
    {
        if (_segmentListView)
        {
            int contentPositionOffset = (int)((RectTransform)_segmentListScrollViewContent.transform).anchoredPosition.y;
            int segmentListScrollRectHeight = (int)((RectTransform)_segmentListScrollView.transform).rect.height;

            if (_segmentIdsListOfSelectedTheme != null)
            {
                for (int i = 0; i < _segmentIdsListOfSelectedTheme.Count; i++)
                {
                    SegmentListViewEntry segmentListViewEntry = _segmentListViewEntries[i];
                    int verticalPosition = _lineHeightListView * i;
                    bool itemIsVisible = ((verticalPosition - contentPositionOffset > -_lineHeightListView * 2) && (verticalPosition - contentPositionOffset < segmentListScrollRectHeight - _lineHeightListView));
                    segmentListViewEntry.gameObj.SetActive(itemIsVisible);
                }
            }
        }
    }


    // (de)activate the (in)visible entries to avoid breaking the vertex limit.
    // Call this each time the scroll position has changed.
    private void UpdateThemeListViewItemsActiveStates()
    {
        if (_themeListView)
        {
            int contentPositionOffset = (int)((RectTransform)_themeListScrollViewContent.transform).anchoredPosition.y;
            int segmentListScrollRectHeight = (int)((RectTransform)_themeListScrollView.transform).rect.height;

            if (_themeListViewEntries != null)
            {
                for (int i = 0; i < _themeListViewEntries.Count; i++)
                {
                    ThemeListViewEntry themeListViewEntry = _themeListViewEntries[i];
                    int verticalPosition = _lineHeightListView * i;
                    bool itemIsVisible = ((verticalPosition - contentPositionOffset > -_lineHeightListView * 2) && (verticalPosition - contentPositionOffset < segmentListScrollRectHeight - _lineHeightListView));
                    themeListViewEntry.gameObj.SetActive(itemIsVisible);
                }
            }
        }
    }

    private void UpdateThemeTriggerUisActiveStates()
    {
        if (_themeListView)
        {
            int contentPositionOffset = (int)((RectTransform)_themesTriggerSectionScrollViewContent.transform).anchoredPosition.y;
            int scrollRectHeight = (int)((RectTransform)_themesTriggerSectionScrollView.transform).rect.height;

            //Debug.Log("contentPositionOffset=" + contentPositionOffset + "  scrollRectHeight=" + scrollRectHeight + "  _themeTriggerPanelItemHeight=" + _themeTriggerPanelItemHeight);

            foreach (ThemeTriggerButtonUi triggerUi in _themeTriggerButtonsToThemeIds.Keys)
            {
                float verticalPosition = ((RectTransform)triggerUi.goTriggerItemRoot.transform).anchoredPosition.y;
                bool itemIsVisible = ((-verticalPosition - contentPositionOffset > -_themeTriggerPanelItemHeight) && (-verticalPosition - contentPositionOffset < scrollRectHeight));
                triggerUi.goTriggerItemRoot.SetActive(itemIsVisible);
                //Debug.Log("verticalPosition=" + verticalPosition + "   -> " + itemIsVisible);
            }
        }
    }

    public void UpdateMenuModeSection()
    {       
        if (PsaiCore.Instance.MenuModeIsActive())
        {
            _buttonMenuModeEnterText.text = "Leave immediately";
            _buttonMenuModeConfigure.interactable = false;
        }
        else
        {
            _buttonMenuModeEnterText.text = "Enter";
            _buttonMenuModeConfigure.interactable = true;
        }
    }


    public void OnClick_ThemeTriggerButton(GameObject sender)
    {

        //Debug.Log("OnClick_ThemeTriggerButton() sender=" + sender);

        if (_themeTriggerButtonGoToThemeIds.ContainsKey(sender))
        {

            int themeId = _themeTriggerButtonGoToThemeIds[sender];
            float intensity = _themeIdsToTriggerSliders[themeId].value;

            if (_configureMenuMode)
            {
                _menuThemeId = themeId;
                _menuThemeIntensity = intensity;
                _configureMenuMode = false;
                UpdateAdvancedControlSection();
            }
            else if (_configureCutScene)
            {
                _cutSceneThemeId = themeId;
                _cutSceneThemeIntensity = intensity;
                _configureCutScene = false;
                UpdateAdvancedControlSection();
            }
            else
            {
                PsaiCore.Instance.TriggerMusicTheme(themeId, intensity);                
            }
        }        
        else
        {
            Debug.LogError("sender not found! sender=" + sender);
        }
    }

    public void OnChange_ThemeIntensitySlider(GameObject sender)
    {
        if (_themeTriggerSlidersToThemeIds.ContainsKey(sender))
        {
            int themeId = _themeTriggerSlidersToThemeIds[sender];
            float intensity = _themeIdsToTriggerSliders[themeId].value;
            int intensityPercentage = (int)(intensity * 100f);

            ThemeTriggerButtonUi triggerButtonUi = _themeIdsToTriggerButtonUis[themeId];
            triggerButtonUi.textIntensityValue.text = intensityPercentage.ToString() + " %";
        }
    }

    public void OnChange_StopMusicFadeoutMillisSlider()
    {
        if (_stopMusicFadeoutSecondsSlider.value == 0)
        {
            _stopMusicFadeoutSecondsSlider.value = 0.01f;
        }
        _stopMusicFadeoutValueText.text = string.Format("{0} sec", _stopMusicFadeoutSecondsSlider.value.ToString("F2"));
    }


    public void OnClick_AddToCurrentIntensityPlus()
    {
        PsaiCore.Instance.AddToCurrentIntensity(_addToIntensityStepsize);
    }

    public void OnClick_AddToCurrentIntensityMinus()
    {
        PsaiCore.Instance.AddToCurrentIntensity(-_addToIntensityStepsize);
    }


    public void OnClick_StopMusicImmediately()
    {
        PsaiCore.Instance.StopMusic(true, _stopMusicFadeoutSecondsSlider.value);     
    }

    public void OnClick_StopMusicByEndSegment()
    {
        PsaiCore.Instance.StopMusic(false);
    }

    public void OnClick_ReturnToLastBasicMoodImmediately()
    {
        PsaiCore.Instance.ReturnToLastBasicMood(true);
    }

    public void OnClick_ReturnToLastBasicMoodByEndSegment()
    {
        PsaiCore.Instance.ReturnToLastBasicMood(false);
    }


    public void OnClick_Pause()
    {
        PsaiInfo psaiInfo = PsaiCore.Instance.GetPsaiInfo();
        PsaiCore.Instance.SetPaused(!psaiInfo.paused);
        UpdateControlSectionButtons();
        UpdateSegmentListViewColorsAndText(false, false);
    }

    public void OnClick_ConfigureMenuMode()
    {
        _configureMenuMode = !_configureMenuMode;
    }


    public void OnClick_ConfigureStopMusic()
    {
        _configureStopMusic = !_configureStopMusic;
        ConfigureStopMusic(_configureStopMusic);
    }

  

    public void OnClick_MenuModeEnterOrLeave()
    {
        if (!PsaiCore.Instance.MenuModeIsActive())
        {
            PsaiCore.Instance.MenuModeEnter(_menuThemeId, _menuThemeIntensity);
        }
        else
        {
            PsaiCore.Instance.MenuModeLeave();
        }

        UpdateMenuModeSection();
    }


    public void OnClick_CutSceneEnterOrLeaveImmediately()
    {
        if (!PsaiCore.Instance.CutSceneIsActive())
        {
            PsaiCore.Instance.CutSceneEnter(_cutSceneThemeId, _cutSceneThemeIntensity);
        }
        else
        {
            PsaiCore.Instance.CutSceneLeave(true, false);
        }

        UpdateAdvancedControlSection();
    }

    public void OnClick_CutSceneConfigureOrLeaveSmoothly()
    {
        if (!PsaiCore.Instance.CutSceneIsActive())
        {
            _configureCutScene = !_configureCutScene;
        }
        else
        {
            PsaiCore.Instance.CutSceneLeave(false, false);
        }

        UpdateAdvancedControlSection();
    }


    public void OnClick_ToggleListView()
    {
        _infoSectionSelection = InfoSectionSelection.list;
        UpdateInfoSectionEntityView();
        UpdateSegmentListViewColorsAndText(true, true);
    }

    public void OnClick_ToggleEntityView()
    {
        _infoSectionSelection = InfoSectionSelection.entity;
        UpdateInfoSectionEntityView();
        UpdateSegmentListViewColorsAndText(true, true);
    }

    public void OnClick_ToggleInfoSectionOff()
    {
        _infoSectionSelection = InfoSectionSelection.off;
        UpdateActiveStatesOfAllPanels();
    }

    public void OnClick_ToggleSoundtrackDescription()
    {
        _infoSectionSelection = InfoSectionSelection.description;
        UpdateActiveStatesOfAllPanels();
    }



    public void OnClick_ToggleAdvancedControlSection()
    {       
        _showAdvancedControlSection = !_toggleBasicControls.isOn;
               
        UpdateActiveStatesOfAllPanels();
    }

    public void OnClick_ToggleTooltips()
    {                
        if (_toggleTooltip != null)
        {
            psai.TooltipView.Instance.TurnedOn = !psai.TooltipView.Instance.TurnedOn;
        }         
    }


    private void UpdateAdvancedControlSection()
    {
        if (_engineControlSection && _engineControlSection.gameObject.activeInHierarchy)
        {
            string menuThemeIntensityString = (_menuThemeIntensity * 100).ToString();
            string cutSceneIntensityString = (_cutSceneThemeIntensity * 100).ToString();
            _menuModePanelText.text = "Theme: " + _menuThemeId + "  " + menuThemeIntensityString + " %";
            _cutScenePanelText.text = "Theme: " + _cutSceneThemeId + "  " + cutSceneIntensityString + " %";


            if (PsaiCore.Instance.CutSceneIsActive())
            {
                _buttonCutSceneModeEnterText.text = "Leave immediately";
                _buttonCutSceneModeConfigureText.text = "Leave smoothly";
            }
            else
            {
                _buttonCutSceneModeEnterText.text = "Enter";
                _buttonCutSceneModeConfigureText.text = "Select Theme";
            }        
        }
    }

    private void UpdateActiveStatesOfAllPanels()
    {
        if (_engineControlSection && _engineControlSection.gameObject.activeInHierarchy)
        {
            _engineControlSection.gameObject.SetActive(true);
            _stopMusicPanel.panelObject.SetActive(!_showAdvancedControlSection);
            _returnToBasicMoodPanel.panelObject.SetActive(!_showAdvancedControlSection);
            _menuModePanel.SetActive(_showAdvancedControlSection);
            _cutSceneModePanel.SetActive(_showAdvancedControlSection);
            _pausePanel.panelObject.SetActive(true);
        }


        if (_infoSection && _infoSection.gameObject.activeInHierarchy)
        {
            _infoSection.gameObject.SetActive(true);
            _currentThemeSection.SetActive(false);
            _currentSegmentSection.SetActive(false);
            _segmentListView.SetActive(false);
            _themeListView.SetActive(false);
            _soundtrackDescriptionPanel.SetActive(false);

            switch (_infoSectionSelection)
            {
                case InfoSectionSelection.entity:
                    _currentSegmentSection.SetActive(true);
                    _currentThemeSection.SetActive(true);
                    break;

                case InfoSectionSelection.list:
                    _segmentListView.SetActive(true);
                    _themeListView.SetActive(true);
                    break;

                case InfoSectionSelection.description:
                    _soundtrackDescriptionPanel.SetActive(true);
                    break;
            }
        }
    }



    public void OnClick_ThemeListViewEntry(GameObject sender)
    {       
        if (_themeListViewButtonsToThemeIds.ContainsKey(sender))
        {
            int themeId = _themeListViewButtonsToThemeIds[sender];
            SelectedThemeId = themeId;
        } 
        else
        {
            Debug.LogError("sender not found!" + sender);
        }
    }


    public void OnClick_SegmentListViewEntry(GameObject sender)
    {        
        if (_segmentListViewEntriesToSegmentIds.ContainsKey(sender))
        {
            int segmentId = _segmentListViewEntriesToSegmentIds[sender];
            PsaiCore.Instance.PlaySegment(segmentId);
            StorePlaybackCountdownForSegment(segmentId);
            UpdateSegmentListViewColorsAndText(true, true);
        }
    }

    private void UpdateControlSectionButtons()
    {

        PsaiInfo psaiInfo = PsaiCore.Instance.GetPsaiInfo();

        if (psaiInfo.psaiState == PsaiState.playing)
        {
            _stopMusicPanel.SetInteractable(true);
            _returnToBasicMoodPanel.SetInteractable(true);
            _pausePanel.SetInteractable(true);

            if (psaiInfo.upcomingPsaiState == PsaiState.silence)
            {
                _buttonBgImageStopMusicByEndSegment.color = new Color(0.5f + 0.5f * _flashIntensity, 0.5f + 0.5f * _flashIntensity, 0, 1);
            }
            else
            {
                _buttonBgImageStopMusicByEndSegment.color = COLOR_MIDDLEGREY;
            }

            
            if (psaiInfo.returningToLastBasicMood)            
            {
                _buttonBgImageReturnToBasicMoodByEndSegment.color = new Color(0.5f + 0.5f * _flashIntensity, 0.5f + 0.5f * _flashIntensity, 0, 1);
            }
            else
            {
                _buttonBgImageReturnToBasicMoodByEndSegment.color = COLOR_MIDDLEGREY;
            }
        }
        else
        {
            // called once when changing to Rest or Stop.
            if (_psaiStateInLastFrame != psaiInfo.psaiState)
            {
                _stopMusicPanel.SetInteractable(false);
                _returnToBasicMoodPanel.SetInteractable(false);
                _pausePanel.SetInteractable(false);
            }
        }

        /// Button Pause
        if (PsaiCore.Instance.GetPsaiInfo().paused == true)
        {

            ColorBlock cb = _buttonHoldIntensity.colors;
            cb.normalColor = COLOR_DARKYELLOW;
            cb.highlightedColor = COLOR_LIGHTYELLOW;
            _buttonPause.colors = cb;
        }
        else
        {
            ColorBlock cb = _buttonHoldIntensity.colors;
            cb.normalColor = _buttonColorHoldNormal;
            cb.highlightedColor = _buttonColorHoldHighlighted;
            _buttonPause.colors = cb;
        }
    }


    private int CompareThemesByName(int themeId1, int themeId2)
    {
        if (_themeInfos.ContainsKey(themeId1) && _themeInfos.ContainsKey(themeId2))
        {
            string themeName1 = _themeInfos[themeId1].name;
            string themeName2 = _themeInfos[themeId2].name;

            return themeName1.CompareTo(themeName2);
        }

        return 0;
    }

    private int CompareThemesByType(int themeId1, int themeId2)
    {
        if (_themeInfos.ContainsKey(themeId1) && _themeInfos.ContainsKey(themeId2))
        {
            ThemeType type1 = _themeInfos[themeId1].type;
            ThemeType type2 = _themeInfos[themeId2].type;

            return type1.CompareTo(type2);
        }

        return 0;
    }

    private int CompareSegmentsByName(int segmentId1, int segmentId2)
    {
        if (_segmentInfos.ContainsKey(segmentId1) && _segmentInfos.ContainsKey(segmentId2))
        {
            return _segmentInfos[segmentId1].name.CompareTo(_segmentInfos[segmentId2].name);
        }

        return 0;
    }

    private int CompareSegmentsBySuitablilies(int segmentId1, int segmentId2)
    {
        if (_segmentInfos.ContainsKey(segmentId1) && _segmentInfos.ContainsKey(segmentId2))
        {
            string strSuitabilities1 = Segment.GetStringFromSegmentSuitabilities(_segmentInfos[segmentId1].segmentSuitabilitiesBitfield);
            string strSuitabilities2 = Segment.GetStringFromSegmentSuitabilities(_segmentInfos[segmentId2].segmentSuitabilitiesBitfield);
            return strSuitabilities1.CompareTo(strSuitabilities2);
        }

        return 0;
    }


    private int CompareSegmentsByIntensity(int snippetId1, int snippetId2)
    {
        if (_segmentInfos.ContainsKey(snippetId1) && _segmentInfos.ContainsKey(snippetId2))
        {
            return _segmentInfos[snippetId1].intensity.CompareTo(_segmentInfos[snippetId2].intensity);
        }

        return 0;
    }


    private int CompareSegmentsByPlaycount(int segmentId1, int segmentId2)
    {
        if (_segmentInfos.ContainsKey(segmentId1) && _segmentInfos.ContainsKey(segmentId2))
        {
            return _segmentInfos[segmentId1].playcount.CompareTo(_segmentInfos[segmentId2].playcount);
        }
        return 0;
    }


    private int CompareSegmentsByLength(int segmentId1, int segmentId2)
    {
        if (_segmentInfos.ContainsKey(segmentId1) && _segmentInfos.ContainsKey(segmentId2))
        {
            return _segmentInfos[segmentId1].fullLengthInMilliseconds.CompareTo(_segmentInfos[segmentId2].fullLengthInMilliseconds);
        }
        return 0;
    }


    private void RebuildSegmentInfoCache()
    {
        _segmentInfos.Clear();
        foreach (int themeId in _themeIds)
        {
            ThemeInfo themeInfo = PsaiCore.Instance.GetThemeInfo(themeId);
            int[] segmentIdsz = themeInfo.segmentIds;
            foreach (int segmentId in segmentIdsz)
            {
                SegmentInfo segmentInfo = PsaiCore.Instance.GetSegmentInfo(segmentId);
                _segmentInfos[segmentId] = segmentInfo;
            }
        }
    }
}

