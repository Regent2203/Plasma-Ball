//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using UnityEngine;
using psai.net;
using System.Collections.Generic;

public class PsaiTriggerOnPlayerCollision : PsaiColliderBasedTrigger
{
    public float intensity = 1.0f;
    private bool isColliding = false;

    private void OnTriggerEnter(Collider other) 
    {
        if (other == playerCollider)
        {
            isColliding = true;

            TryToFireSynchronizedShotTrigger();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isColliding = false;
    }

    public override float CalculateTriggerIntensity()
    {
        return this.intensity;
    }

    public override bool EvaluateTriggerCondition()
    {
        return isColliding;
    }

}