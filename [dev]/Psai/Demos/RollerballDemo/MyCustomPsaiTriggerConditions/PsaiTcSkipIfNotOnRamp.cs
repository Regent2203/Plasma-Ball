using UnityEngine;
using psai.net;
using System.Collections.Generic;


/// This is an example for a custom Trigger Condition, that can be attached to any GameObject that also has a PsaiSynchronizedTrigger component, 
/// to further define when it should fire or not.
/// In our example game we only want a Trigger to fire if the player is currently on the ramp (with the given RampCollider).
/// Returning true within the overridden interface method EvaluateTriggerCondition() will allow the trigger to fire.

public class PsaiTcSkipIfNotOnRamp : PsaiTriggerCondition
{
    public Collider PlayerCollider;
    public Collider RampCollider;

    public override bool EvaluateTriggerCondition()
    {

        // Raycast check - are we rolling (or jumping) on top of the RampCollider?
        RaycastHit raycastHit;
        if (Physics.Raycast(PlayerCollider.gameObject.transform.position, Vector3.down, out raycastHit))
        {
            if (raycastHit.collider == RampCollider)
            {
                return true;
            }
        }

        return false;
    }
}
