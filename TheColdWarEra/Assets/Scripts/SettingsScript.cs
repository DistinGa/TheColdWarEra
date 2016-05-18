using UnityEngine;
using System.Collections;

public class SettingsScript : MonoBehaviour {
    public static SettingsScript Settings;
    public bool mVideo; // tru-используем avi-видео, fals-используем картинки
    public bool mMusicOn;   //вкл/выкл фоновой музыки

    public void Awake()
    {
        //singletone
        if (Settings == null)
            Settings = this;
        else
            Destroy(gameObject);
    }
}
