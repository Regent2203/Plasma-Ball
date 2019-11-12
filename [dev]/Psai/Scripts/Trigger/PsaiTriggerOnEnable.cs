using UnityEngine;
using psai.net;

public class PsaiTriggerOnEnable : PsaiTriggerOnSignal
{
    new void OnEnable()
    {
        base.OnEnable();
        TryToFireSynchronizedShotTrigger();
    }
}
