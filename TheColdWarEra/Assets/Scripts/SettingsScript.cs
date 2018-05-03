using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SettingsScript : MonoBehaviour
{
    public static SettingsScript Settings;
    public bool mVideo { get; set; } // tru-используем avi-видео, fals-используем картинки
    public bool mVoiceOn { get; set; }   //вкл/выкл голосовых сообщений
    public float mMusicVol { get; set; }
    public float mSoundVol { get; set; }
    public int AIPower { get; set; }
    public Authority playerSelected;// { get; set; }

    public void Awake()
    {
        //Singletone
        if (Settings != null)
        {
            Destroy(gameObject);
            return;
        }

        Settings = this;
        DontDestroyOnLoad(gameObject);
        LoadSettings();
    }

    public void SaveSettings()
    {
        SavedSettings.VideoNews = mVideo;
        SavedSettings.MusicEnable = mMusicOn;
        SavedSettings.SoundEnable = mSoundOn;
        SavedSettings.Voice = mVoiceOn;

        SavedSettings.SoundVolume = mMusicVol;
        SavedSettings.MusicVolume = mSoundVol;
    }

    public void LoadSettings()
    {
        mVideo = SavedSettings.VideoNews;
        mVoiceOn = SavedSettings.Voice;

        mMusicVol = SavedSettings.SoundVolume;
        mSoundVol = SavedSettings.MusicVolume;
    }

    public bool mMusicOn
    {
        get { return mMusicVol > float.Epsilon; }
    }

    public bool mSoundOn
    {
        get { return mSoundVol > float.Epsilon; }
    }
}
