using UnityEngine;
using UnityEngine.Audio;

public class AudioInit : Singleton<AudioInit>
{
    public AudioMixer MainAudiomixer;

    AudioSource audio_bgm; //plays only BGM throughout all game
    AudioSource audio_sfx; //plays only SFX throughout all game

    bool __loaded = false;


    protected override void Awake()
    {
        if (MainAudiomixer == null)
        {
            Debug.LogErrorFormat("Main Audiomixer is not set for {0}!", gameObject);
            Destroy(gameObject);
        }
        
        base.Awake();
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        //load settings from playerprefs
        float v1, v2; //v is for volume
        v1 = PlayerPrefs.GetFloat("sett_volbgm", 1);
        v2 = PlayerPrefs.GetFloat("sett_volsfx", 1);
        GameObject _menu = GameObject.Find("/UICanvas/Menu_Controller/m_Settings");
        if (_menu != null)
        {
            _menu.transform.Find("Slider_BGM").GetComponent<UnityEngine.UI.Slider>().value = v1;
            _menu.transform.Find("Slider_SFX").GetComponent<UnityEngine.UI.Slider>().value = v2;
        }
        else
        {
            _SetVolumeBGM(v1);
            _SetVolumeSFX(v2);
        }

        //audios
        AudioSource[] audios = GetComponents<AudioSource>();
        audio_bgm = audios[0];
        audio_sfx = audios[1];

        //start playing bgm
        if (audio_bgm.clip == null)
            audio_bgm.clip = (AudioClip)Resources.Load("BGM/bgm00");
        if (!audio_bgm.isPlaying) //если вернулись через игровое меню
            audio_bgm.PlayDelayed(1.0f);

        if (audio_sfx.clip == null)
            audio_sfx.clip = (AudioClip)Resources.Load("SFX/Select_Menu01");

        __loaded = true;
    }

    //settings - music
    public void _SetVolumeBGM(float V1)
    {
        MainAudiomixer.SetFloat("vol_bgm", Mathf.Log(V1) * 20);
        PlayerPrefs.SetFloat("sett_volbgm", V1);
        PlayerPrefs.Save();
    }

    //settings - sound
    public void _SetVolumeSFX(float V2)
    {
        MainAudiomixer.SetFloat("vol_sfx", Mathf.Log(V2) * 20);
        PlayerPrefs.SetFloat("sett_volsfx", V2);
        PlayerPrefs.Save();

        _PlaySound_Select();
    }


    //used in menu
    public void _PlaySound_Select()
    {
        if (!__loaded)
            return;

        if (!audio_sfx.isPlaying)
            audio_sfx.Play();
    }

    //for other sounds
    public void PlayOneShot(AudioClip sound)
    {
        audio_sfx.PlayOneShot(sound);
    }
}
