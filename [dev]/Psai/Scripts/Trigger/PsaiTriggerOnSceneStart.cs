//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using UnityEngine;
using System.Collections;
using psai.net;

public class PsaiTriggerOnSceneStart : PsaiTriggerOnSignal
{
    void Start()
    {
        StartCoroutine(Coroutine_TriggerWhenSoundtrackHasLoaded());
    }


    IEnumerator Coroutine_TriggerWhenSoundtrackHasLoaded()
    {
        while (!PsaiCore.IsInstanceInitialized() || PsaiCore.Instance.GetSoundtrackInfo().themeCount == 0)
        {
            yield return null;
        }

        PsaiCore.Instance.TriggerMusicTheme(this.themeId, this.intensity);
    }
}
