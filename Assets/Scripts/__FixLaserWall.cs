using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;


[CustomEditor(typeof(SolidWall))]
[CanEditMultipleObjects]
public class __FixLaserWall : Editor 
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SolidWall myScript = (SolidWall)target;

        if (GUILayout.Button("Fix transforms"))
        {
            myScript.FixTransforms();
        }
    }

}
#endif