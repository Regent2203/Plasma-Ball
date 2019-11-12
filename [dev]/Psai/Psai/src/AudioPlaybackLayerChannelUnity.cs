//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if (!PSAI_STANDALONE)


using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using System.Collections;

namespace psai.net
{

    public class PsaiAsyncLoader : MonoBehaviour
    {
        private IEnumerator m_loadSegmentAsync_Coroutine;
        private IEnumerator m_playWhenReady_Coroutine;

        public void LoadSegmentAsync(AudioPlaybackLayerChannelUnity audioPlaybackChannel)
        {
            #if (!PSAI_NOLOG)
            if (LogLevel.debug <= Logger.Instance.LogLevel)
            {
                Logger.Instance.Log("LoadSegmentAsync() pathToClip=" + audioPlaybackChannel.PathToClip + "   audioPlaybackChannel.GetHashCode()=" + audioPlaybackChannel.GetHashCode(), LogLevel.debug);
            }
            #endif            

            if(m_loadSegmentAsync_Coroutine != null) 
            {                
                StopCoroutine(m_loadSegmentAsync_Coroutine);
                m_loadSegmentAsync_Coroutine = null;
            }
            if (m_playWhenReady_Coroutine != null) 
            {
                StopCoroutine(m_playWhenReady_Coroutine);
                m_playWhenReady_Coroutine = null;
            }
            m_loadSegmentAsync_Coroutine = LoadSegmentAsync_Coroutine(audioPlaybackChannel);
            StartCoroutine(m_loadSegmentAsync_Coroutine);
        }

        private IEnumerator LoadSegmentAsync_Coroutine(AudioPlaybackLayerChannelUnity audioPlaybackChannel)
        {           

            ResourceRequest request = Resources.LoadAsync(audioPlaybackChannel.PathToClip, typeof(AudioClip));
            yield return request;

            #if (!PSAI_NOLOG)
            if (LogLevel.debug <= Logger.Instance.LogLevel)
            {
                string logMessage = "LoadSegmentAsync_Coroutine complete, asset=" + (request.asset == null ? "null" : request.asset.name);
                logMessage += "  PlaybackIsPending=" + audioPlaybackChannel.PlaybackIsPending + "   audioPlaybackChannel.GetHashCode()" + audioPlaybackChannel.GetHashCode();
                Logger.Instance.Log(logMessage, LogLevel.debug);
            }
            #endif

            AudioClip clip = request.asset as AudioClip;

            if (clip == null)
            {
                #if (!PSAI_NOLOG)
                if (LogLevel.errors <= Logger.Instance.LogLevel)
                {
                    Logger.Instance.Log("The AudioClip '" + audioPlaybackChannel.PathToClip + "' was not found!", LogLevel.errors);
                }
                #endif
            }
            else
            {
                audioPlaybackChannel.AudioClip = clip;
                bool loadAudioDataResult = audioPlaybackChannel.AudioClip.LoadAudioData();
                if (loadAudioDataResult == false)
                {
                    if (clip.loadState == AudioDataLoadState.Unloaded && 
                        clip.loadType == AudioClipLoadType.Streaming && 
                        UnityVersionComparer.CompareCurrentVersionAgainst("5.3.5p5") == UnityVersionComparer.ComparisonResult.later
                        )
                    {
                        Debug.LogError("LOADING FAILED! Dead user, please note that Unity versions 5.3.5p6 and later suffer from an issue in Unity's reworked streaming audio system. Please note that we cannot fix this problem on psai side. As a temporary workaround, please set the LoadType of all the Audio Clips used in your psai soundtrack to 'compressed in memory' or 'decompress on load'. Another option may be to fall back to the last working Unity version 5.3.5p5. We hope this gets sorted out soon, thanks for your patience!");                       
                    }
                }

                if (audioPlaybackChannel.PlaybackIsPending)
                {
#if (!PSAI_NOLOG)
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log("Playback is pending but AudioClip is not ready to play!", LogLevel.debug);
                    }
#endif
                    PlayWhenReady(audioPlaybackChannel);
                }
            }            
        }

        public void PlayWhenReady(AudioPlaybackLayerChannelUnity audioPlaybackChannel) 
        {
            if (m_playWhenReady_Coroutine != null) 
            {
                StopCoroutine(m_playWhenReady_Coroutine);
                m_playWhenReady_Coroutine = null;
            }

            m_playWhenReady_Coroutine = Coroutine_PlayWhenReady(audioPlaybackChannel);
            StartCoroutine(m_playWhenReady_Coroutine);
        }

        /// <summary>
        /// Keeps checking the loading status and plays as soon as it's ready.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Coroutine_PlayWhenReady(AudioPlaybackLayerChannelUnity audioPlaybackChannel)
        {
            while (!audioPlaybackChannel.IsReadyToPlay())
            {
                if(audioPlaybackChannel.AudioClip == null) 
                {
                    yield break;
                }
                yield return null;
            }


            int missingMillis = Logik.GetTimestampMillisElapsedSinceInitialisation() -
                          audioPlaybackChannel.TargetPlaybackTimestamp;         


            #if (!PSAI_NOLOG)
            if (missingMillis > 0 && Time.frameCount > 60)
            {
                if (LogLevel.warnings <= Logger.Instance.LogLevel)
                {
                    Logger.Instance.Log(string.Format("playback timing problem detected! Missing milliseconds: {0} ", missingMillis), LogLevel.warnings);
                }

                if (audioPlaybackChannel.AudioClip.loadType == AudioClipLoadType.Streaming)
                {
                    Logger.Instance.Log("Please note: playback timing problems may occur when starting a Scene or when the Log Output in the PsaiCoreManager is set to Debug. If the warning shows up frequently during the game, please increase the 'Max Playback Latency' in PsaiCoreManager for the current target platform.", LogLevel.warnings);
                }
                else
                {
                    Logger.Instance.Log("We highly recommend setting the 'Load Type' of all psai Audio Clips to 'Streaming'.", LogLevel.warnings);
                }
            }
            #endif

            audioPlaybackChannel.PlayBufferedClip(-missingMillis);
        }
    }


    public class AudioPlaybackLayerChannelUnity : IAudioPlaybackLayerChannel
    {
        private AudioSource _audioSource = null;
        private Segment _segmentToLoad;
        private int _timeSamples;
        private bool _playbackHasBeenInterruptedByPause;
        private PsaiAsyncLoader _psaiAsyncLoader;
        public PlaybackChannel PlaybackChannel;

        /** The Logik.MillisElapsedSinceInitialization() when the scheduled Segment should is supposed to fire
         */
        public int TargetPlaybackTimestamp
        { 
            get;
            set;
        }

        public bool PlaybackIsPending
        {
            get;
            set;
        }


        public AudioClip AudioClip
        {
            get
            {
                if (_audioSource != null)
                {
                    return _audioSource.clip;
                }
                return null;
            }

            set
            {
                if (_audioSource != null)
                {
                    _audioSource.clip = value;
                }                
            }
        }


        public string PathToClip
        {
            get;
            set;
        }


        public AudioPlaybackLayerChannelUnity(PlaybackChannel parentPlaybackChannel)
        {
            PlaybackChannel = parentPlaybackChannel;
            AudioSource source = PlatformLayerUnity.PsaiGameObject.transform.Find(PlatformLayerUnity.NAME_OF_CHANNELS_CHILDNODE).gameObject.AddComponent<AudioSource>();
            if (PsaiCoreManager.Instance.outputAudioMixerGroup != null)
            {
                source.outputAudioMixerGroup = PsaiCoreManager.Instance.outputAudioMixerGroup;
            }
            
            source.loop = false;
            source.ignoreListenerVolume = true;
            source.ignoreListenerPause = true;
            _audioSource = source;
        }


        public void Release()
        {
            if (_audioSource != null)
            {
                GameObject.DestroyImmediate(_audioSource);
            }
        }

        PsaiResult IAudioPlaybackLayerChannel.LoadSegment(Segment segment)
        {
            _segmentToLoad = segment;
            AudioClip = null;


#if (!PSAI_BUILT_BY_VS)
            if (_psaiAsyncLoader == null)
            {
                GameObject psaiObject = PsaiCoreManager.Instance.GetAsyncLoadersNode();

                if (psaiObject == null)
                {
                    #if !(PSAI_NOLOG)
                    if (LogLevel.errors <= Logger.Instance.LogLevel)
                    {
                            Logger.Instance.Log("No 'Psai' object found in the Scene! Please make sure to add the Psai.prefab from the Psai.unitypackage to your Scene", LogLevel.errors);
                    }
                    #endif
                    return PsaiResult.initialization_error;
                }
                _psaiAsyncLoader = psaiObject.AddComponent<PsaiAsyncLoader>();
            }
#endif

            string psaiBinaryDirectoryName = Logik.Instance.m_psaiCoreSoundtrackDirectoryName;
            if (psaiBinaryDirectoryName.Length > 0)
            {
                PathToClip = psaiBinaryDirectoryName + "/" + segment.audioData.filePathRelativeToProjectDir;    // warning, to not use Path.Combine() here. Use '/' as directory sepearator for all Unity platforms.
            }
            else
            {
                PathToClip = segment.audioData.filePathRelativeToProjectDir;
            }

            _audioSource.clip = null;       // we reset the clip to prevent the situation in ScheduleSegmentPlayback(), where the previous clip was reported as readyToPlay, causing problems.
            _psaiAsyncLoader.LoadSegmentAsync(this);
            return PsaiResult.OK;
        }

        PsaiResult IAudioPlaybackLayerChannel.ReleaseSegment()
        {
            if (_audioSource.clip != null)
            {
                /* this only has an effect after calling Resources.UnloadUnusedAssets().
                 * Only calling Resources.UnloadUnusedAssets() is also possible, but it will free the chached clips
                 * more seldomly */
                Resources.UnloadAsset(_audioSource.clip);

                _audioSource.clip = null;
                _segmentToLoad = null;
            }

            return PsaiResult.OK;
        }


        public bool IsReadyToPlay()
        {
            bool readyToPlay = false;
            if (_audioSource.clip != null)
            {
                readyToPlay = (_audioSource.clip.loadState == AudioDataLoadState.Loaded);
            }

            #if (!PSAI_NOLOG)
            {
                if (!readyToPlay)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("readyToPlay returning false. _audioSource.clip=");
                    sb.Append(_audioSource.clip);
                    if (_audioSource.clip != null)
                    {
                        sb.Append("  _audioSource.clip.loadState=");
                        sb.Append(_audioSource.clip.loadState);
                    }
                    Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
            }
            #endif

            return readyToPlay;
        }


        public void PlayBufferedClip(int delayMillis)
        {
            #if (!PSAI_NOLOG)
            if (LogLevel.debug <= Logger.Instance.LogLevel)
            {
                string logMessage = string.Format("PlayBufferedClip()  _audioSource._clip: {0}  delayMillis: {1}", _audioSource.clip, delayMillis);
                logMessage += " IsReadToPlay=" + IsReadyToPlay().ToString();
                Logger.Instance.Log(logMessage, LogLevel.debug);
            }
            #endif


            if (delayMillis == 0)
            {
                _audioSource.timeSamples = 0;
                _audioSource.Play();
                PlaybackChannel.OnPlaybackHasStarted();
            }
            else
            {
                if (delayMillis > 0)
                {
                    try
                    {
                        _audioSource.timeSamples = 0;
                        _audioSource.PlayDelayed((uint) delayMillis*0.001f);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("_audioSource.PlayDelayed threw an exception: " + e.ToString());
                        Debug.LogWarning("_audioSource.clip=" + _audioSource.clip + "  delayMillis: " + delayMillis +
                                         "  IsReadyToPlay():" + IsReadyToPlay());
                    }

                }
                else
                {
                    // delayMillis < 0  -> skip the first samples to fix the timing
                    _audioSource.timeSamples = Mathf.Min(PlaybackChannel.Segment.audioData.GetSampleCountByMilliseconds(Mathf.Abs(delayMillis)), PlaybackChannel.Segment.audioData.sampleCountTotal-1);
                    if (!(_audioSource.timeSamples >= 0 && _audioSource.timeSamples < PlaybackChannel.Segment.audioData.sampleCountTotal))
                    {
                        string message = string.Format( "Timesamples {0} must be between 0 and {1}. Delay Ms {2}", _audioSource.timeSamples, PlaybackChannel.Segment.audioData.sampleCountTotal, delayMillis);
                        Logger.Instance.Log(message, LogLevel.errors);
                    }
                                        
                    _audioSource.Play();
                    PlaybackChannel.OnPlaybackHasStarted();
                }
            }

            PlaybackIsPending = false;
        }


        PsaiResult IAudioPlaybackLayerChannel.ScheduleSegmentPlayback(Segment segment, int delayMilliseconds)
        {
            if (_segmentToLoad != null && _segmentToLoad.Id == segment.Id)
            {			
				bool readyToPlay = IsReadyToPlay();

                #if (!PSAI_NOLOG)
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
                    Logger.Instance.Log(string.Format("ScheduleSegmentPlayback() Segment: {0}  isReadyToPlay: {1}   delayMilliseconds: {2}", segment.Name, readyToPlay, delayMilliseconds), LogLevel.debug);
                }
                #endif
																
                // new method PlayDelayed introduced in Unity Version 4.1.0.                    
	            if (readyToPlay)
	            {
                    PlayBufferedClip(delayMilliseconds);

                    #if (!PSAI_NOLOG)
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log(string.Format("_audioSource.PlayDelayed() fired, delayInMs:{0}", delayMilliseconds), LogLevel.debug);
                    }
                    #endif

                    return PsaiResult.OK;
	            }                
	            else
	            {
                    TargetPlaybackTimestamp = Logik.GetTimestampMillisElapsedSinceInitialisation() + delayMilliseconds;
                    PlaybackIsPending = true;

                    #if (!PSAI_NOLOG)
                    if (LogLevel.debug <= Logger.Instance.LogLevel)
                    {
                        Logger.Instance.Log("... play has not fired yet, PlaybackIsPending is now set to true.  TargetPlaybackTimestamp=" + TargetPlaybackTimestamp, LogLevel.debug);
                    }
                    #endif


	                if (delayMilliseconds <= 0)
	                {
                        // The AudioData was loaded and set as a Clip, but clip.loadingState is still returning Loading.
                        // This means that the Latency setting is not high enough.

                        #if (!PSAI_NOLOG)
	                    {
                            if (LogLevel.errors <= Logger.Instance.LogLevel)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append("Channel is not ready to play!");
                                sb.Append("_audioSource.clip: ");
                                sb.Append(_audioSource.clip);
                                if (_audioSource.clip != null)
                                {
                                    sb.Append("  loadState: ");
                                    sb.Append(_audioSource.clip.loadState);
                                }
                                Logger.Instance.Log(sb.ToString(), LogLevel.errors);
                            }
                        }
                        #endif

                        _psaiAsyncLoader.PlayWhenReady(this);
	                }
                }                
                
                return PsaiResult.OK;                
            }
            else
            {
                Logger.Instance.Log("ScheduleSegmentPlayback(): COULD NOT PLAY! No Segment loaded, or Segment Id to play did not match! Segment loaded: " + _segmentToLoad, LogLevel.errors);
            }

            return PsaiResult.notReady;
        }

        PsaiResult IAudioPlaybackLayerChannel.StopChannel()
        {
            _audioSource.volume = 0;
            _audioSource.Stop();

            return PsaiResult.OK;
        }

        PsaiResult IAudioPlaybackLayerChannel.SetVolume(float volume)
        {

            if (_audioSource != null)
            {
                _audioSource.volume = volume;
                return PsaiResult.OK;
            }
            else
            {
                #if (!PSAI_NOLOG)
                if (LogLevel.errors <= Logger.Instance.LogLevel)
                {
                    Logger.Instance.Log("SetVolume() failed, _audioSource is NULL!", LogLevel.errors);
                }
                #endif

                return PsaiResult.notReady;
            }
        }

        PsaiResult IAudioPlaybackLayerChannel.SetPaused(bool paused)
        {
            if (paused)
            {
                if (_audioSource.isPlaying)
                {
                    _playbackHasBeenInterruptedByPause = true;
                    _audioSource.Pause();
                    //_timeSamples = _audioSource.timeSamples;
                }
            }
            else
            {
                if (_playbackHasBeenInterruptedByPause)
                {
                    //_audioSource.timeSamples = _timeSamples;        // TODO: tut das Not?
                    _audioSource.Play();

                    _playbackHasBeenInterruptedByPause = false;
                }
            }

            return PsaiResult.OK;
        }
    }
}

#endif