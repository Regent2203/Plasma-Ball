//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using psai.net;
using UnityEngine.Audio;

/// <summary>
/// Holds basic settings of the psai playback engine and is responsible for filtering and managing concurrent calls from Trigger Scripts that might otherwise interfere with each other.
/// </summary>
public class PsaiCoreManager : MonoBehaviour
{
    private static PsaiCoreManager _instance;
    public static PsaiCoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PsaiCoreManager>();

                if (_instance == null && Time.frameCount > 0)       // the Time.frameCount check was added to avoid false errors directly after stopping the game and returning to the Unity Editor.
                {
                    Debug.LogError("A 'PsaiCoreManager' component is missing in your Scene! psai won't work without it. Please add it manually to any GameObject in your Scene (found in 'Psai/Scripts/Essential'), or drag & drop the 'Psai.prefab' from the 'Psai' folder into your Scene. The Psai.prefab also comes with the PsaiPlayer user interface, which may safely be removed from your Scene if not needed. We further recommend to add/keep a 'PsaiSoundtrackLoader' component for convenient loading of Soundtracks.");
                }
            }
            return _instance;
        }
    }

    public enum PlaybackControlType
    {
        None,
        ReturnToBasicMood,
        GoToRest,
        StopMusic
    }


    /// <summary>
    /// Sets the amount of log information that psai writes to the Unity console.
    /// </summary>
    /// <remarks>
    /// Please stop your game before changing this setting. Changes at runtime will have no effect.
    /// </remarks>
    public psai.net.LogLevel logLevel = psai.net.LogLevel.errors;
    private psai.net.LogLevel logLevelInLastFrame;

    public bool logTriggerScripts;

    public float volume = 1.0f;
    private float volumeInLastFrame;

    public AudioMixerGroup outputAudioMixerGroup;

    public bool keepBetweenScenes = false;

    /// <summary>
    /// The interval in seconds in which continuously firing Trigger Scripts will be evaluated.
    /// </summary>
    /// <remarks>
    /// Higher intervals will save some CPU cycles, while low intervals (e.g. 0.2 seconds) will make your soundtrack react as quickly as possible.
    /// </remarks>
    public float triggerEvaluationIntervalInSeconds = 0.20f;


    //////////////////////////////////////////////////////////////////////////
    // These are the default latency values (milliseconds) that psai will allow
    // the target system to buffer and play back compressed audio files.
    // Depending on your target system specifications you may choose to reduce
    // these values to make your soundtrack react quicker. Doing so may result
    // in audio stuttering on slower machines, so make sure to test your settings.
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// The time (in milliseconds) needed by your target platform for buffering sounds. Increase, if your sound stutters.
    /// </summary>
    public int bufferLatencyMs = 100;

    /// <summary>
    /// The time (in milliseconds) needed by your target platform for playing back buffered sounds. Increase, if your sound stutters.
    /// </summary>
    public int playbackLatencyMs = 50;


    private List<TriggerMusicThemeCall> _synchronizedOneShotTriggerCallsForNextTick = new List<TriggerMusicThemeCall>();

    public struct TriggerMusicThemeCall
    {
        public int themeId;
        public int musicDurationInSeconds;
        public float intensity;
        public bool forceImmediateStopBeforeTriggering;
        public PsaiSynchronizedTrigger senderBehaviour;

        public TriggerMusicThemeCall(PsaiSynchronizedTrigger senderTrigger, int themeId, float intensity, int musicDurationInSeconds)
        {
            this.senderBehaviour = senderTrigger;
            this.themeId = themeId;
            this.intensity = intensity;
            this.musicDurationInSeconds = musicDurationInSeconds;
            this.forceImmediateStopBeforeTriggering = false;
        }

        public override string ToString()
        {
            return "themeId: " + themeId + " intensity:" + intensity.ToString("F4") + " (" + this.senderBehaviour + ")";
        }
    }

    /** We cannot store the PsaiPlaybackControl behaviours to execute their calls later, as their gameObjects are likely to get destroyed
     * and thus be null when we access it in the next frame. An alternative would be to clone them, but we want to avoid memory allocation at runtime.
     * So we use a sinlge struct for all playback control calls (PsaiStopMusic, ReturnToLastBasicMood)
     */
    [System.Serializable]
    public struct PlaybackControlCall
    {
        public PlaybackControlType playbackControlType;
        public PsaiTriggerBase sendingBehaviour;
        public bool immediately;
        public bool keepSilentUntilNextTrigger;
        public float fadeOutSeconds;
        public bool dontExecuteIfOtherTriggersAreFiring;
        public ThemeType restrictBlockToThisThemeType;
        public bool overrideDefaultRestTime;
        public int overrideRestSecondsMin;
        public int overrideRestSecondsMax;

        public void Init()
        {
            playbackControlType = PlaybackControlType.None;         
            immediately = false;
            fadeOutSeconds = 3.0f;                          // TODO: magic number
            dontExecuteIfOtherTriggersAreFiring = false;
            restrictBlockToThisThemeType = ThemeType.none;
            overrideDefaultRestTime = false;
            overrideRestSecondsMin = 10;
            overrideRestSecondsMax = 30;
        }
    }

    private PlaybackControlCall playbackControlCallFiringInThisTick;    
    private float triggerTickIntervalCounter;
    private Dictionary<int, TriggerMusicThemeCall> mapThemeIdsToTriggerCalls = new Dictionary<int, TriggerMusicThemeCall>();
    private List<PsaiSynchronizedTrigger> continuousTriggersInScene = new List<PsaiSynchronizedTrigger>();

    private GameObject asyncLoadersNode;     // a child node where the AsyncLoader-components can go

    //////////////////////////////////////////////////////////////////////////    


    void Awake()
    {
        if (PsaiCore.IsInstanceInitialized())
        {            
            PsaiCore.Instance.Release();
            PsaiCore.Instance = null;
        }

        PsaiCore.Instance.SetLogLevel(logLevel);

        this.asyncLoadersNode = new GameObject();
        this.asyncLoadersNode.name = "AsyncLoaders";
        this.asyncLoadersNode.transform.SetParent(this.transform);
    }

    void Start()
    {
        PsaiCore.Instance.SetMaximumLatencyNeededByPlatformToBufferSounddata(bufferLatencyMs);
        PsaiCore.Instance.SetMaximumLatencyNeededByPlatformToPlayBackBufferedSounddata(playbackLatencyMs);
		PsaiCore.Instance.SetVolume(this.volume);

        if (keepBetweenScenes)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void Update()
    {

        if (this.logLevelInLastFrame != this.logLevel)
        {
            PsaiCore.Instance.SetLogLevel(logLevel);
            this.logLevelInLastFrame = this.logLevel;
        }

        if (this.volumeInLastFrame != this.volume)
        {
            PsaiCore.Instance.SetVolume(volume);
            this.volumeInLastFrame = this.volume;
        }

        PsaiCore.Instance.Update();

        triggerTickIntervalCounter += Time.deltaTime;
        if (triggerTickIntervalCounter > triggerEvaluationIntervalInSeconds)
        {
            triggerTickIntervalCounter -= triggerEvaluationIntervalInSeconds;
            ProcessAllIncomingTriggerCallsForThisTick();
        }
    }


    public void OnApplicationPause(bool paused)
    {
        if (PsaiCore.IsInstanceInitialized())
        {
            PsaiCore.Instance.SetPaused(paused);
        }        
    }

    public void OnApplicationExit()
    {
        //PsaiCore.Instance.Release();
    }

    public GameObject GetAsyncLoadersNode()
    {
        return this.asyncLoadersNode;
    }
    

    public bool RegisterContinuousTrigger(PsaiSynchronizedTrigger continuousTrigger)
    {
        if (!continuousTriggersInScene.Contains(continuousTrigger))
        {
            continuousTriggersInScene.Add(continuousTrigger);

            //Debug.Log("ContinuousTrigger registered successfully: " + continuousTrigger.ToString());
            return true;
        }
        return false;
    }

    public bool UnregisterContinuousTrigger(PsaiSynchronizedTrigger continuousTrigger)
    {
        if (continuousTriggersInScene.Contains(continuousTrigger))
        {
            continuousTriggersInScene.Remove(continuousTrigger);
            //Debug.Log("ContinuousTrigger unregistered successfully: " + continuousTrigger.ToString());
            return true;
        }
        return false;
    }

    public void RegisterOneShotTriggerCall(TriggerMusicThemeCall triggerCall)
    {
        _synchronizedOneShotTriggerCallsForNextTick.Add(triggerCall);
    }
       

    void ProcessAllIncomingTriggerCallsForThisTick()
    {
        mapThemeIdsToTriggerCalls.Clear();
        for(int i=0;i< continuousTriggersInScene.Count;i++)
        //foreach (PsaiSynchronizedTrigger triggerBehaviour in continuousTriggersInScene)
        {
            PsaiSynchronizedTrigger triggerBehaviour = continuousTriggersInScene[i];
            if (triggerBehaviour.EvaluateAllTriggerConditions() == true)
            {
                float calculatedIntensity = triggerBehaviour.CalculateTriggerIntensity();
                if (triggerBehaviour.TriggerType == PsaiTriggerBase.PsaiTriggerType.triggerMusicTheme)
                {
                    TriggerMusicThemeCall triggerCall = new TriggerMusicThemeCall(triggerBehaviour, triggerBehaviour.themeId, calculatedIntensity, triggerBehaviour.musicDurationInSeconds);
                    ProcessTriggerCall(triggerCall);
                }                            
                else
                {                    
                    CopyControlTriggerValuesToPlaybackCallFiringInThisTick(triggerBehaviour);
                }
            }
        }

        // Now we check for synchronized One-Shot Triggers
        for (int i = 0; i < _synchronizedOneShotTriggerCallsForNextTick.Count; i++)
            //foreach (TriggerMusicThemeCall triggerCall in _synchronizedOneShotTriggerCallsForNextTick)
        { 
            TriggerMusicThemeCall triggerCall = _synchronizedOneShotTriggerCallsForNextTick[i];
            if (triggerCall.intensity > 0)
            {
                ProcessTriggerCall(triggerCall);
            }
        }
        _synchronizedOneShotTriggerCallsForNextTick.Clear();

        var mapThemeIdsToTriggerCallsEnumerator = mapThemeIdsToTriggerCalls.GetEnumerator();

        // Execute explicit calls to GoToRest, StopMusic,...
        if (playbackControlCallFiringInThisTick.playbackControlType != PlaybackControlType.None)
        {
            bool executeControlCall = true;

            if (mapThemeIdsToTriggerCalls.Keys.Count > 0 && playbackControlCallFiringInThisTick.dontExecuteIfOtherTriggersAreFiring == true)
            {
                while (mapThemeIdsToTriggerCallsEnumerator.MoveNext())
                //foreach (int themeId in mapThemeIdsToTriggerCalls.Keys)
                {
                    int themeId = mapThemeIdsToTriggerCallsEnumerator.Current.Key;
                    ThemeInfo themeInfo = PsaiCore.Instance.GetThemeInfo(themeId);
                    if (themeInfo.type == playbackControlCallFiringInThisTick.restrictBlockToThisThemeType)
                    {     
                        #if !(PSAI_NOLOG)
                        if (this.logTriggerScripts)
                        {
                            Debug.LogWarning(string.Format("psai [{0}]: skipping {1} as there are other Triggers firing: {2}", (int)(Time.timeSinceLevelLoad * 1000), playbackControlCallFiringInThisTick.playbackControlType, themeInfo.type));
                        }
                        #endif

                        executeControlCall = false;
                        break;
                    }
                }
            }

            if (executeControlCall)
            {                
                switch (playbackControlCallFiringInThisTick.playbackControlType)
                {
                    case PlaybackControlType.ReturnToBasicMood:
                        {
                            PsaiCore.Instance.ReturnToLastBasicMood(playbackControlCallFiringInThisTick.immediately);
                        }
                        break;

                    case PlaybackControlType.GoToRest:
                        {
                            if (playbackControlCallFiringInThisTick.overrideDefaultRestTime)
                            {
                                PsaiCore.Instance.GoToRest(playbackControlCallFiringInThisTick.immediately, 
                                                            playbackControlCallFiringInThisTick.fadeOutSeconds, 
                                                            playbackControlCallFiringInThisTick.overrideRestSecondsMin * 1000,
                                                            playbackControlCallFiringInThisTick.overrideRestSecondsMax * 1000);
                            }
                            else
                            {
                                PsaiCore.Instance.GoToRest(playbackControlCallFiringInThisTick.immediately, playbackControlCallFiringInThisTick.fadeOutSeconds);
                            }                            
                        }
                        break;


                    case PlaybackControlType.StopMusic:
                        {
                            PsaiCore.Instance.StopMusic(playbackControlCallFiringInThisTick.immediately, playbackControlCallFiringInThisTick.fadeOutSeconds);
                        }
                        break;

                }
                playbackControlCallFiringInThisTick.sendingBehaviour.hasFired = true;
            }         
            playbackControlCallFiringInThisTick.Init();
        }
         mapThemeIdsToTriggerCallsEnumerator = mapThemeIdsToTriggerCalls.GetEnumerator();
        //enumerator.
        while (mapThemeIdsToTriggerCallsEnumerator.MoveNext()) 
            //foreach (TriggerMusicThemeCall triggerCall in mapThemeIdsToTriggerCalls.Values)
            {
                TriggerMusicThemeCall triggerCall = mapThemeIdsToTriggerCallsEnumerator.Current.Value;
                // adding up triggers may exceed the limit                
                float triggerIntensity = Mathf.Min(1.0f, triggerCall.intensity);

            if (logTriggerScripts)
            {
                Debug.Log(string.Format("psai [{0}]: PsaiCoreManager executing TriggerCall {1}", (int)(Time.timeSinceLevelLoad * 1000), triggerCall.ToString()));
            }

            if (triggerCall.forceImmediateStopBeforeTriggering)
            {
                PsaiCore.Instance.StopMusic(true);
            }
            if (triggerCall.senderBehaviour.overrideMusicDurationInSeconds && triggerCall.musicDurationInSeconds > 0)
            {
                PsaiCore.Instance.TriggerMusicTheme(triggerCall.themeId, triggerIntensity, triggerCall.musicDurationInSeconds);
                triggerCall.senderBehaviour.hasFired = true;
            }
            else
            {
                PsaiCore.Instance.TriggerMusicTheme(triggerCall.themeId, triggerIntensity);
                triggerCall.senderBehaviour.hasFired = true;
            }
        }
    }


    /// <summary>
    /// Checks if a TriggerCall for a given Theme has already been stored in the map for this tick, and updates the map accordingly.
    /// </summary>
    /// <param name="triggerCall"></param>
    private void ProcessTriggerCall(TriggerMusicThemeCall triggerCall)
    {
        bool storeNewTriggerCall = false;        
        if (mapThemeIdsToTriggerCalls.ContainsKey(triggerCall.themeId))
        {
            TriggerMusicThemeCall storedTriggerCall = mapThemeIdsToTriggerCalls[triggerCall.themeId];
            if (triggerCall.senderBehaviour.addUpIntensities)
            {
                triggerCall.intensity += storedTriggerCall.intensity;

                if (triggerCall.intensity > triggerCall.senderBehaviour.limitIntensitySum)
                {
                    triggerCall.intensity = triggerCall.senderBehaviour.limitIntensitySum;
                }
                storeNewTriggerCall = true;
            }
            else
            {
                if (storedTriggerCall.intensity < triggerCall.intensity)
                {
                    storeNewTriggerCall = true;
                }
            }
        }
        else
        {
            storeNewTriggerCall = true;
        }

        if (storeNewTriggerCall)
        {
            mapThemeIdsToTriggerCalls[triggerCall.themeId] = triggerCall;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////
    // for some reason the sender Behaviour is null in the next Update loop after calling these Synchronized...methods, 
    // so we fill a struct.

    public void RegisterSynchronizedOneShotPlaybackCommandForNextTick(PsaiTriggerBase trigger)
    {        
        CopyControlTriggerValuesToPlaybackCallFiringInThisTick(trigger);
    }

    private void CopyControlTriggerValuesToPlaybackCallFiringInThisTick(PsaiTriggerBase trigger)
    {       
        switch(trigger.TriggerType)
        {
            case PsaiTriggerBase.PsaiTriggerType.returnToLastBasicMood:
                {
                    this.playbackControlCallFiringInThisTick.playbackControlType = PlaybackControlType.ReturnToBasicMood;
                }                
                break;


            case PsaiTriggerBase.PsaiTriggerType.stopMusic:
                {
                    if (trigger._keepSilentUntilNextTrigger)
                        this.playbackControlCallFiringInThisTick.playbackControlType = PlaybackControlType.StopMusic;
                    else
                        this.playbackControlCallFiringInThisTick.playbackControlType = PlaybackControlType.GoToRest;
                }
                break;
        }
        this.playbackControlCallFiringInThisTick.sendingBehaviour = trigger;
        this.playbackControlCallFiringInThisTick.immediately = trigger._immediately;
        this.playbackControlCallFiringInThisTick.fadeOutSeconds = trigger._fadeOutSeconds;
        this.playbackControlCallFiringInThisTick.dontExecuteIfOtherTriggersAreFiring = trigger._dontExecuteIfOtherTriggersAreFiring;
        this.playbackControlCallFiringInThisTick.restrictBlockToThisThemeType = trigger._restrictBlockToThisThemeType;
        this.playbackControlCallFiringInThisTick.keepSilentUntilNextTrigger = trigger._keepSilentUntilNextTrigger;
        if (trigger._overrideDefaultRestTime)
        {
            this.playbackControlCallFiringInThisTick.overrideRestSecondsMin = trigger._overrideRestSecondsMin;
            this.playbackControlCallFiringInThisTick.overrideRestSecondsMax = trigger._overrideRestSecondsMax;
        }
        else
        {
            this.playbackControlCallFiringInThisTick.overrideRestSecondsMin = -1;
            this.playbackControlCallFiringInThisTick.overrideRestSecondsMin = -1;
        }
    }

}
