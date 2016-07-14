using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuScript : MonoBehaviour
{
    public Animator Animator;
    [Space(10)]
    public Toggle VideoNews;
    public Toggle Voice;
    public Toggle Music;
    public Slider MusicVolume;
    public Toggle Sound;
    public Slider SoundVolume;
    [Space(10)]
    public Toggle Easy;
    public Toggle Medium;
    public Toggle Hard;

    private AudioSource AS;

    public void Start()
    {
        VideoNews.isOn = SettingsScript.Settings.mVideo;
        Voice.isOn = SettingsScript.Settings.mVoiceOn;
        Music.isOn = SettingsScript.Settings.mMusicOn;
        Sound.isOn = SettingsScript.Settings.mSoundOn;
        MusicVolume.value = SettingsScript.Settings.mMusicVol;
        SoundVolume.value = SettingsScript.Settings.mSoundVol;

        AS = GetComponent<AudioSource>();
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void LoadScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    public void InvokeStart()
    {
        SettingsScript.Settings.SaveSettings();
        Animator.Play(0);
        Invoke(("StartGame"), 5f);
    }

    public void StartGame()
    {
        LoadScene("GameScene");
    }

    public void SelectPlayer(bool Amer)
    {
        if (Amer)
            SettingsScript.Settings.playerSelected = Authority.Amer;
        else
            SettingsScript.Settings.playerSelected = Authority.Soviet;

        SetAILevel(Amer);
    }

    public void SetAILevel(bool Amer)
    {
        Easy.interactable = true;

        if (Amer)
        {
            Easy.isOn = !SavedSettings.Mission1USA;
            Medium.interactable = SavedSettings.Mission1USA;
            Medium.isOn = (SavedSettings.Mission1USA && !SavedSettings.Mission2USA);
            Hard.interactable = SavedSettings.Mission2USA;
            Hard.isOn = SavedSettings.Mission2USA;
        }
        else
        {
            Easy.isOn = !SavedSettings.Mission1SU;
            Medium.interactable = SavedSettings.Mission1SU;
            Medium.isOn = (SavedSettings.Mission1SU && !SavedSettings.Mission2SU);
            Hard.interactable = SavedSettings.Mission2SU;
            Hard.isOn = SavedSettings.Mission2SU;
        }
    }

    public void OpenManual()
    {
        System.Diagnostics.Process.Start(Application.streamingAssetsPath + "/Quick guide.pdf");
    }

    public void PlaySound(AudioClip ac)
    {
        AS.PlayOneShot(ac);
    }
}
