//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;
using psai.net;

[System.Serializable]
public class PsaiSoundtrackLoader : MonoBehaviour 
{
    /// <summary>
    /// The the path of your soundtrack file as exported by the Psai Editor.
    /// </summary>
    /// <remarks>
    /// Please note that all soundtracks must be located somewhere below a parent folder called 'Resources'. So e.g. if you exported your soundtrack to 'MyUnityProject/Assets/Resources/MySoundtrack/soundtrack.xml' , this variable set this variable to 'MySoundtrack/soundtrack.xml'.
    /// </remarks>
    public string pathToSoundtrackFileWithinResourcesFolder;

    /// <summary>
    /// This is just used to refill the nulled-out TextAsset in the Editor when returning from the game. 
    /// </summary>
    [HideInInspector]
    public string PathToTextAsset;

    public void Start()
    {
        LoadSoundtrack();
    }
    
    private void LoadSoundtrack()
    {      
        if (pathToSoundtrackFileWithinResourcesFolder == null || pathToSoundtrackFileWithinResourcesFolder.Length == 0)
        {
            Debug.LogError("No path to psai soundtrack file set!");
        }
        else
        {
            PsaiResult psaiResult = PsaiResult.error_file;

            if (pathToSoundtrackFileWithinResourcesFolder.EndsWith(".xml"))
            {
                psaiResult = PsaiCore.Instance.LoadSoundtrackFromProjectFile(pathToSoundtrackFileWithinResourcesFolder);
            }
            #if (PSAI_BUILT_BY_VS || PSAI_STANDALONE || PSAI_EDITOR_STANDALONE)
            else if (pathToSoundtrackFileWithinResourcesFolder.EndsWith(".bytes"))
            {
                Debug.LogWarning("Binary psai soundtrack files are deprecated! Instead, you can now load psai Project files directly by xml. To do so, rename your 'MyProject.psai' file to 'MyProject.xml' . Please make sure that there is no 'MyProject.bytes' or 'MyProject.psai' file in the same directory, as this might cause problems at runtime due to Unity ignoring file extensions.");
                psaiResult = PsaiCore.Instance.LoadSoundtrack(pathToSoundtrackFileWithinResourcesFolder);
            }
            #endif

            
            if (psaiResult != PsaiResult.OK)
            {
                Debug.LogError("Failed to load psai soundtrack from path '" + pathToSoundtrackFileWithinResourcesFolder + "' Please make sure the file is located within your Assets/Resources/ folder. [" + PsaiCore.Instance.GetVersion() + "]");
            }
        }
    }
} 

