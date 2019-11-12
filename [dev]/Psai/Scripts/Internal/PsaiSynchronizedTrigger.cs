using UnityEngine;
using System.Collections;
using psai.net;

/// <summary>
/// This is the Base Class for synchronized One-Shot and continuous Trigger Events.
/// </summary>
public abstract class PsaiSynchronizedTrigger : PsaiTriggerBase
{

    /// <summary>
    /// This needs to be defined by the inheriting subclass. Set to true if the trigger evaluation needs to be done regularly from within the Update method.
    /// </summary>
    /// <remarks>
    /// Triggers like TriggerOnPlayerCollision() don't need the Update() method to evaluate their trigger condition, as they are called externally
    /// by an Event like OnTriggerEnter. Setting 'triggerEvaluationNeedsUpdate' to false makes sure the PsaiSynchronizedTrigger.Update() returns ASAP without wasting CPU cycles.
    /// Other Triggers like TriggerWhenInRange() need to constantly call the Update method for their evaluation (in this case, the distance calculation), 
    /// so set it to true in these cases.
    /// </remarks>
    protected bool triggerEvaluationNeedsUpdateMethod = false;

    private float _tickCounter;


    /// <summary>
    /// Enable this to have the PsaiCoreManager filter and synchronize this trigger script, to avoid problems caused by overlapping triggers.
    /// </summary>
    /// <remarks>
    /// Set this to true to have PsaiCoreManager evaluate, synchronize and filter all Trigger scripts that are currently firing,
    /// to prevent wild jumps of intensity caused by overlapping Triggers.
    ///</remarks>
    //public bool synchronizeByPsaiCoreManager = true;


    /// <summary>
    /// Defines if the PsaiCoreManager shall filter out or add up overlapping Trigger scripts.
    /// </summary>
    /// <remarks>
    /// If set to true, the PsaiCoreManager sum up intensities of overlapping triggers of the same kind.
    /// Use this for instance if you have multiple minor enemies closing in, that add up to a bigger threat.
    /// Set this to false if you wish to only execute the trigger with the highest intensity.    
    /// </remarks>
    public bool addUpIntensities = false;

    /// <summary>
    /// Sets the maximum intensity when adding up intensities from this Trigger:
    /// </summary>
    public float limitIntensitySum = 1.0f;


    /// <summary>
    /// Set this to true if you want to make sure that this Theme will interrupt all the music that might be playing.
    /// </summary>
    /// <remarks>
    /// Please note that even if this is set to true, this Theme may still be interrupted by Triggers firing afterwards.
    /// </remarks>
    public bool interruptAnyTheme = false;


    private bool triggerGetsEvaluatedByPsaiCoreManager = false;


    public virtual void OnEnable()
    {
        if (triggerEvaluationNeedsUpdateMethod || fireContinuously)
        {
            PsaiCoreManager pcm = PsaiCoreManager.Instance;
            if (pcm != null)
            {                
                pcm.RegisterContinuousTrigger(this);
                triggerGetsEvaluatedByPsaiCoreManager = true;
            }
        }

        _triggerConditionsInGameObject = this.gameObject.GetComponents<PsaiTriggerCondition>();
    }

    public virtual void OnDisable()
    {
        if (triggerGetsEvaluatedByPsaiCoreManager)
        {
            PsaiCoreManager pcm = PsaiCoreManager.Instance;
            if (pcm != null)
            {
                pcm.UnregisterContinuousTrigger(this);
                triggerGetsEvaluatedByPsaiCoreManager = false;
            }
        }

        if (resetHasFiredStateOnDisable)
        {
            hasFired = false;
        }        
    }

    protected void Update()
    {      
        if (triggerEvaluationNeedsUpdateMethod && !triggerGetsEvaluatedByPsaiCoreManager)
        {
            _tickCounter += Time.deltaTime;
            if (_tickCounter > PsaiCoreManager.Instance.triggerEvaluationIntervalInSeconds)
            {
                _tickCounter -= PsaiCoreManager.Instance.triggerEvaluationIntervalInSeconds;


                TryToFireSynchronizedShotTrigger();
            }
        }
    }


    /// <summary>
    /// Evaluates all TriggerConditions and, if all of them have passed, fires a direct or synchronized Trigger.
    /// </summary>
    protected void TryToFireSynchronizedShotTrigger()
    {
        if (EvaluateAllTriggerConditions())
        {

            if (PsaiCoreManager.Instance.logTriggerScripts)
            {
                Debug.Log(string.Format("psai [{0}]: firing synchronized One-Shot Trigger: {1}", (int)(Time.timeSinceLevelLoad * 1000), this));
            }

            if (this.TriggerType == PsaiTriggerType.triggerMusicTheme)
            {
                float intensity = CalculateTriggerIntensity();
                FireSynchronizedOneShotTriggerMusicTheme(intensity);
            }
            else
            {
                PsaiCoreManager.Instance.RegisterSynchronizedOneShotPlaybackCommandForNextTick(this);
            }
        }        
    }


    private void FireSynchronizedOneShotTriggerMusicTheme(float intensity)
    {
        PsaiCoreManager.TriggerMusicThemeCall triggerCall = new PsaiCoreManager.TriggerMusicThemeCall(this, this.themeId, intensity, 0);
        triggerCall.forceImmediateStopBeforeTriggering = this.interruptAnyTheme;
        if (overrideMusicDurationInSeconds && musicDurationInSeconds > 0)
        {
            triggerCall.musicDurationInSeconds = this.musicDurationInSeconds;
        }
        PsaiCoreManager.Instance.RegisterOneShotTriggerCall(triggerCall);
    }
}
