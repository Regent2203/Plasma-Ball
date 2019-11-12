using UnityEngine;
using System.Collections;
using psai.net;


public abstract class PsaiTriggerBase : MonoBehaviour
{
    [System.Serializable]
    public enum PsaiTriggerType
    {
        triggerMusicTheme,
        returnToLastBasicMood,
        stopMusic
    }

    public PsaiTriggerType TriggerType;

    public int themeId = 1;


    public bool _immediately = false;
    public bool  _keepSilentUntilNextTrigger = true;
    public bool _overrideDefaultFadeoutTime = false;
    public float _fadeOutSeconds = 3.0f;
    public bool _dontExecuteIfOtherTriggersAreFiring = false;
    public ThemeType _restrictBlockToThisThemeType = ThemeType.none;
    public bool _overrideDefaultRestTime = false;
    public int _overrideRestSecondsMin = 30;
    public int _overrideRestSecondsMax = 60;

    public bool overrideMusicDurationInSeconds = false;
    public int musicDurationInSeconds = 10;

    protected PsaiTriggerCondition[] _triggerConditionsInGameObject;

    /// <summary>
    /// if set to false the trigger will only fire once in each first tick when the condition was met
    /// </summary> 
    public bool fireContinuously = false;

    /// <summary>
    /// Setting this to true will disable the Trigger script after firing a single time.
    /// </summary>
    public bool deactivateAfterFiringOnce = false;


    /// <summary>
    /// This will be set to true as soon as the Trigger has at least fired once.
    /// </summary>
    public bool hasFired = false;


    /// <summary>
    /// If this is set to true, psai will reset the 'hasFired' flag whenever the Trigger component is disabled.
    /// </summary>
    public bool resetHasFiredStateOnDisable = false;


    protected void Awake()
    {
        _triggerConditionsInGameObject = this.gameObject.GetComponents<PsaiTriggerCondition>();
    }

    /// <summary>
    /// Calculates an intensity value that will be used to trigger a psai Theme.
    /// </summary>
    /// <remarks>
    /// Override this method for any custom ContinuousTriggers you may wish to implement to map
    /// the intensity of a game situation to the intensity of the music.   
    /// If the trigger condition failed, the return value must be 0.
    /// </remarks>
    /// <returns>
    /// The trigger intensity between 0.01f and 1.0f.
    /// </returns>
    public abstract float CalculateTriggerIntensity();

    /// <summary>
    /// Evaluates all PsaiTriggerConditions attached to this GameObject and returns true if they all succeeded, false otherwise.
    /// </summary>
    /// <remarks>
    /// PsaiTriggerConditions in child or parent nodes are ignored.
    /// </remarks>
    /// <returns></returns>
    public bool EvaluateAllTriggerConditions()
    {
        if (!fireContinuously && deactivateAfterFiringOnce && hasFired)
        {
            return false;
        }
        else
        {
            for(int i=0;i< _triggerConditionsInGameObject.Length;i++)
            //foreach (PsaiTriggerCondition condition in _triggerConditionsInGameObject)
            {
                PsaiTriggerCondition condition = _triggerConditionsInGameObject[i];
                if (condition.EvaluateTriggerCondition() == false)
                    return false;
            }
        }

        return EvaluateTriggerCondition();
    }


    /// <summary>
    /// Override this to define a general condition if this Trigger should fire in the current tick or not.
    /// </summary>
    /// <returns></returns>
    public virtual bool EvaluateTriggerCondition()
    {
        return true;
    }

    public void FireDirectOneShotTrigger()
    {
        switch (TriggerType)
        {
            case PsaiTriggerType.triggerMusicTheme:
                {
                    float intensity = CalculateTriggerIntensity();
                    if (this.overrideMusicDurationInSeconds && this.musicDurationInSeconds > 0)
                    {
                        PsaiCore.Instance.TriggerMusicTheme(themeId, intensity, musicDurationInSeconds);
                    }
                    else
                    {
                        PsaiCore.Instance.TriggerMusicTheme(themeId, intensity);
                    }
                }
                break;

            case PsaiTriggerType.returnToLastBasicMood:
                {
                    PsaiCore.Instance.ReturnToLastBasicMood(_immediately);
                }
                break;

            case PsaiTriggerType.stopMusic:
                {
                    if (_keepSilentUntilNextTrigger)
                    {
                        PsaiCore.Instance.StopMusic(_immediately, _fadeOutSeconds);
                    }
                    else
                    {
                        if (_overrideDefaultRestTime)
                        {
                            PsaiCore.Instance.GoToRest(_immediately, _fadeOutSeconds, _overrideRestSecondsMin * 1000, _overrideRestSecondsMax * 1000);
                        }
                        else
                        {
                            PsaiCore.Instance.GoToRest(_immediately, _fadeOutSeconds);
                        }                        
                    }
                }
                break;
        }

        if (PsaiCoreManager.Instance.logTriggerScripts)
        {
            Debug.Log(string.Format("psai [{0}]: Trigger fired: {1}", (int)(Time.timeSinceLevelLoad * 1000), this));
        }

        hasFired = true;
    }

    public void TryToFireDirectOneShotTrigger()
    {
        if (EvaluateAllTriggerConditions())
        {
            FireDirectOneShotTrigger();
        }
    }
}
