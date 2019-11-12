using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameInit : MonoBehaviour 
{
	public AudioMixer MainAudiomixer;

    public static AudioSource audio_bgm; //plays only BGM throughout all game
    public static AudioSource audio_sfx; //plays only SFX throughout all game

    bool __loaded = false; //exists ONLY to mute sound in "onvaluechange" slider

	void Start() 
	{
		DontDestroyOnLoad(this.gameObject);

        //force screen orientation
        Screen.orientation = ScreenOrientation.Landscape;

        //Player prefs
        /*
        //PlayerPrefs.DeleteAll(); return;
        //Debug.Log(PlayerPrefs.GetFloat("sett_volbgm"));
        //Debug.Log(PlayerPrefs.GetFloat("sett_volsfx"));
        */

        //load settings from playerprefs
        float v1, v2;
        v1 = PlayerPrefs.GetFloat("sett_volbgm", 1);
        v2 = PlayerPrefs.GetFloat("sett_volsfx", 1);
        GameObject _menu = GameObject.Find("/UICanvas/Menu_Controller/m_Settings");
        _menu.transform.Find("Slider_BGM").GetComponent<UnityEngine.UI.Slider>().value = v1;
        _menu.transform.Find("Slider_SFX").GetComponent<UnityEngine.UI.Slider>().value = v2;

        //audios
        AudioSource[] audios = GetComponents<AudioSource> ();
        audio_bgm = audios[0];
        audio_sfx = audios[1];

        //start playing bgm
        audio_bgm.clip = (AudioClip)Resources.Load ("BGM/bgm00");
        if (!audio_bgm.isPlaying) 
            audio_bgm.PlayDelayed(1); 

        __loaded = true;
	}
	
	//settings - music
	public void _SetVolumeBGM (float V1)
	{
		MainAudiomixer.SetFloat ("vol_bgm", Mathf.Log(V1) * 20);
        PlayerPrefs.SetFloat("sett_volbgm", V1);
	}

	//settings - sound
	public void _SetVolumeSFX (float V2)
	{
		MainAudiomixer.SetFloat ("vol_sfx", Mathf.Log(V2) * 20);
        PlayerPrefs.SetFloat("sett_volsfx", V2);

        if (__loaded)
            _PlaySound_Select();
	}

    public static void _PlaySound_Select ()
    {
        if (!audio_sfx.isPlaying)
            audio_sfx.Play();
    }
}
