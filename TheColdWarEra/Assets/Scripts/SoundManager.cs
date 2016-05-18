﻿using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager SM;
    public AudioSource trackAudioSource;
    public AudioSource otherAudioSource;
    public AudioClip[] Tracks;

    int curTrack = 0;   //трек играющий в данный момент

    public void Awake()
    {
        //singletone
        if (SM == null)
            SM = this;
        else
            Destroy(gameObject);
    }

    public void Start()
    {
        if (SettingsScript.Settings.mMusicOn)
        {
            PlayNexTrack();
        }
    }

    public void PlayNexTrack()
    {
        if (Tracks.Length == 0)
            return;

        if (curTrack == 0)
        {
            //Если первый вызов или проиграли все треки, перемешиваем их.
            TossTracks();
        }
        trackAudioSource.PlayOneShot(Tracks[curTrack]);

        //Запуск отложенного старта следующего трека
        Invoke("PlayNexTrack", Tracks[curTrack].length + 0.5f);

        curTrack++;
        //Если стартанули последний трек, сбрасываем счётчик
        if (curTrack == Tracks.Length)
            curTrack = 0;
    }

    //Случайное перемешивание треков.
    void TossTracks()
    {
        AudioClip tempAC;
        int tempIndx;

        for (int i = 0; i < Tracks.Length; i++)
        {
            tempIndx = Random.Range(0, Tracks.Length - 1);
            //Замена треков с индексами i и tempIndx
            tempAC = Tracks[i];
            Tracks[i] = Tracks[tempIndx];
            Tracks[tempIndx] = tempAC;
        }
    }

    public void PlaySound(AudioClip ac)
    {
        otherAudioSource.PlayOneShot(ac);
    }


    public void OnDestroy()
    {
        CancelInvoke();
    }
}