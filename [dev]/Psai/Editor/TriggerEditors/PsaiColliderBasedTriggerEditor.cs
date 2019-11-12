using UnityEditor;
using UnityEngine;
using psai.net;

[CustomEditor(typeof(PsaiColliderBasedTrigger), true)]
public class PsaiColliderBasedTriggerEditor : PsaiSynchronizedTriggerEditor
{
    public void InspectorGuiPlayerCollider(PsaiColliderBasedTrigger trigger)
    {
        EditorGUILayout.Separator();
        trigger.playerCollider = EditorGUILayout.ObjectField("Player Collider", trigger.playerCollider, typeof(Collider), true) as Collider;
    }

    public void InspectorGuiLocalCollider(PsaiColliderBasedTrigger trigger)
    {
        trigger.localCollider = EditorGUILayout.ObjectField("Local Collider", trigger.localCollider, typeof(Collider), true) as Collider;
    }
}
