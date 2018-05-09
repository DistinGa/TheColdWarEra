using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

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
    [Space(10)]
    public RectTransform UsaEasy;
    public RectTransform UsaMed;
    public RectTransform UsaHard;
    public RectTransform UssrEasy;
    public RectTransform UssrMed;
    public RectTransform UssrHard;
    [Space(10)]
    [SerializeField]
    Image EasyPlate;
    [SerializeField]
    Image MediumPlate, HardPlate, clockFill;
    [SerializeField]
    Sprite RedGlowEasy, RedGlowMedium, RedGlowHard, YellowGlowEasy, YellowGlowMedium, YellowGlowHard;

    [Space(10)]
    [SerializeField]
    Button btnPlay;
    [SerializeField]
    Image WizardGlow, DemonGlow;
    [SerializeField]
    Text CurSentence;
    [TextArea(2, 5)]
    [SerializeField]
    string[] Sentences;
    float musVolume, sndVolume;
    bool animIsEnded;

    private AudioSource AS;

    public void Start()
    {
        AS = GetComponent<AudioSource>();

        //if (SteamApps.BIsDlcInstalled((AppId_t)508430))
        //{
        //    VideoNews.interactable = true;
        //    Voice.interactable = true;
        //    VideoNews.isOn = SettingsScript.Settings.mVideo;
        //    Voice.isOn = SettingsScript.Settings.mVoiceOn;
        //}
        //else
        {
            VideoNews.interactable = false;
            Voice.interactable = false;
            VideoNews.isOn = false;
            Voice.isOn = false;
        }

        musVolume = SettingsScript.Settings.mMusicVol;
        sndVolume = SettingsScript.Settings.mSoundVol;
        MusicVolume.value = musVolume;
        SoundVolume.value = sndVolume;

        //экран кампаний
        UsaEasy.gameObject.SetActive(SavedSettings.Mission1USA);
        UsaMed.gameObject.SetActive(SavedSettings.Mission2USA);
        UsaHard.gameObject.SetActive(SavedSettings.Mission3USA);
        UssrEasy.gameObject.SetActive(SavedSettings.Mission1SU);
        UssrMed.gameObject.SetActive(SavedSettings.Mission2SU);
        UssrHard.gameObject.SetActive(SavedSettings.Mission3SU);

        AudioSource MusicSource = GameObject.Find("StartScreen").GetComponent<AudioSource>();
        MusicSource.volume = SettingsScript.Settings.mMusicVol;
        //MusicSource.enabled = SettingsScript.Settings.mMusicOn;

        //CurSentence.text = Sentences[Random.Range(0, Sentences.Length)];
    }

    public void SetOptions(bool done)
    {
        if (done)
        {
            musVolume = SettingsScript.Settings.mMusicVol;
            sndVolume = SettingsScript.Settings.mSoundVol;
        }
        else
        {
            //отмена
            SettingsScript.Settings.mMusicVol = musVolume;
            MusicVolume.value = musVolume;
            SettingsScript.Settings.mSoundVol = sndVolume;
            SoundVolume.value = sndVolume;
            GameObject.Find("Canvas/StartScreen").GetComponent<AudioSource>().volume = musVolume;
        }
    }

    public void Exit()
    {
        SettingsScript.Settings.SaveSettings();
        Application.Quit();
    }

    public void LoadScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    public void InvokeStart()
    {
        SettingsScript.Settings.SaveSettings();
        if (SettingsScript.Settings.playerSelected == Authority.Amer)
        {
            WizardGlow.enabled = true;
            DemonGlow.enabled = false;
        }
        else
        {
            WizardGlow.enabled = false;
            DemonGlow.enabled = true;
        }

        animIsEnded = false;
        //Animator.Play(0);
        Invoke(("StartGame"), 5f);
        StartCoroutine(StartGameAsync());
    }

    public void StartGame()
    {
        animIsEnded = true;
        //LoadScene("GameScene");
    }

    IEnumerator StartGameAsync()
    {
        yield return null;
        var async = SceneManager.LoadSceneAsync("GameScene");
        async.allowSceneActivation = false;
        while (async.progress < 0.9f || !animIsEnded)
        {
            //clockFill.fillAmount = 1 - async.progress;
            yield return new WaitForEndOfFrame();
        }

        async.allowSceneActivation = true;
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName("GameScene"));
        //Resources.UnloadUnusedAssets();
        yield return null;
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
        if(SettingsScript.Settings.mSoundOn)
            AS.PlayOneShot(ac, SettingsScript.Settings.mSoundVol);
    }

    public void SetGlow(bool val)
    {
        if (!val)
            return;

        if(SettingsScript.Settings.playerSelected == Authority.Amer)
        {
            EasyPlate.sprite = YellowGlowEasy;
            MediumPlate.sprite = YellowGlowMedium;
            HardPlate.sprite = YellowGlowHard;
        }
        else
        {
            EasyPlate.sprite = RedGlowEasy;
            MediumPlate.sprite = RedGlowMedium;
            HardPlate.sprite = RedGlowHard;
        }
    }
}
