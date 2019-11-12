using UnityEngine;
using psai.net;

public class PsaiTriggerOnSignal : PsaiSynchronizedTrigger
{
    public float intensity = 1.0f;
    public PsaiTriggerOnSignal()
    {
        /// this make sure the PsaiSynchronizedTrigger.Update() returns ASAP without wasting CPU cycles.
        this.triggerEvaluationNeedsUpdateMethod = false;
        this.fireContinuously = false;
    }

    public void OnSignal()
    {
        TryToFireSynchronizedShotTrigger();
    }

    public override float CalculateTriggerIntensity()
    {
        return this.intensity;
    }
}

