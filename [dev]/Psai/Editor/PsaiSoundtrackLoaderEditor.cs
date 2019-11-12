using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PsaiSoundtrackLoader))]
public class PsaiSoundtrackLoaderEditor : Editor
{
    public TextAsset _textAsset;
    private TextAsset _previousTextAsset;

    private PsaiSoundtrackLoader _loader = null;

    public override void OnInspectorGUI()
    {
        _loader = target as PsaiSoundtrackLoader;
        _textAsset = EditorGUILayout.ObjectField(new GUIContent("drop Soundtrack file here:", "Drag & Drop your psai Soundtrack XML file onto this textfield, to have its relative path saved to the variable below. This field is just for convenience, so don't worry if the textfield is empty."), _textAsset, typeof(TextAsset), true) as TextAsset;

        if (_textAsset != _previousTextAsset)
        {
            if (_textAsset)
            {
                _loader.PathToTextAsset = AssetDatabase.GetAssetPath(_textAsset);

                if (!psai.Editor.EditorModel.CheckIfPathIsWithinSubdirOfAssetsResources(_loader.PathToTextAsset))
                {
                    Debug.LogError("Failed! Your soundtrack file needs to be located within a subdir named 'Resources', along with your audio files.  PathToTextAsset=" + _loader.PathToTextAsset);
                }
                else
                {
                    _loader.pathToSoundtrackFileWithinResourcesFolder = _loader.PathToTextAsset.Substring(_loader.PathToTextAsset.IndexOf("Resources") + "Resources/".Length);

                    /* This is necessary to tell Unity to update the PsaiSoundtrackLoader */
                    if (GUI.changed)
                    {
                        EditorUtility.SetDirty(_loader);
                    }
                }
            }
            _previousTextAsset = _textAsset;
        }


        if (_textAsset == null)
        {
            /* Even if the path is already set, load the TextAsset again if it has been nulled out, so the Editor does not look broken */
            if (!string.IsNullOrEmpty(_loader.PathToTextAsset))
            {
                _textAsset = (TextAsset)UnityEditor.AssetDatabase.LoadAssetAtPath(_loader.PathToTextAsset, typeof(TextAsset));
            }
        }

        EditorGUILayout.LabelField(new GUIContent("Path within Resources folder", "This holds the relative path to your Soundtrack XML file, as exported by the psai Editor. Please note that all your psai Soundtracks have to be located within a subdir of a folder named 'Resources'. This is to make sure that Unity exports all the soundfiles to your target platform. To set the reslative path, drag & drop the soundtrack XML file to the upper textfield."), new GUIContent(_loader.pathToSoundtrackFileWithinResourcesFolder));
    }
}