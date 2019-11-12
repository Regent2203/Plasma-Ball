using UnityEngine;
using psai.net;

[System.Serializable]
public abstract class PsaiPlaybackControl : MonoBehaviour
{
    public PsaiCoreManager.PlaybackControlCall PlaybackControlParams = new PsaiCoreManager.PlaybackControlCall();

    public abstract void OnSignal();
}
