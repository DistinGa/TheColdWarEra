using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Collections;

public class NewsHistory : MonoBehaviour
{
    public Button LeftEnd, LeftStep, RightEnd, RightStep;
    public Image fl_UnreadNews;
    public Image ImgDecade, ImgYear, ImgMonth;
    public Image[] newsPositions = new Image[10];   //Камни, отмечающие показываемую новость.
    public Sprite[] Decades = new Sprite[5], Years = new Sprite[10], Months = new Sprite[12];
    public float TimeInterval = 20;

    float restTime;
    int _curVisibleNews;
    VideoRealPlayRolex[] newsHistory = new VideoRealPlayRolex[10];

    int curVisibleNews
    {
        get { return _curVisibleNews; }

        set
        {
            _curVisibleNews = Mathf.Clamp(value, 0, newsPositions.Length - 1);
        }
    }

    void SetStonesInteractable()
    {
        for (int i = 0; i < newsPositions.Length; i++)
            newsPositions[i].enabled = (i == curVisibleNews);

        if (curVisibleNews == (newsHistory.Length - 1))
        {
            LeftEnd.interactable = false;
            LeftStep.interactable = false;
        }
        else
        {
            LeftEnd.interactable = (newsHistory[newsHistory.Length - 1] != null);
            LeftStep.interactable = (newsHistory[curVisibleNews + 1] != null);
        }

        RightEnd.interactable = (curVisibleNews > 0);
        RightStep.interactable = (curVisibleNews > 0);
    }
	
	void Update ()
    {
        if (restTime <= float.Epsilon)
            return;

        restTime -= Time.deltaTime;
        //Если время вышло и в окне просмотра не последняя новость, то показываем последнюю.
        if (restTime <= float.Epsilon && curVisibleNews > 0)
        {
            if (curVisibleNews != 0)
            {
                curVisibleNews = 0;
                ShowLastNews();
            }
        }
    }

    public void InsertNews(VideoRealPlayRolex news)
    {
        for (int i = newsHistory.Length - 1; i > 0; i--)
            newsHistory[i] = newsHistory[i - 1];

        newsHistory[0] = news;

        if (curVisibleNews == 0)
        {
            //Стояли на последней новости, но сейчас она поменялась. Показываем.
            ShowNews();
        }
        else
        {
            //В окне просмотра была не последняя новость
            if (curVisibleNews < newsHistory.Length - 1)
                curVisibleNews++;
            else
                ShowNews(); //В окне просмотра была самая старая новость. После добавления новости старая поменялась, нужно её показать.

            fl_UnreadNews.enabled = true;
        }

        SetStonesInteractable();
    }

    void StartTimer()
    {
        restTime = TimeInterval;
    }

    void ChangeNewsPosition(int p)
    {
        StartTimer();
        curVisibleNews = p;
        ShowNews();
        SetStonesInteractable();
    }

    public void ShowFirstNews()
    {
        ChangeNewsPosition(newsHistory.Length - 1);
    }

    public void ShowLastNews()
    {
        ChangeNewsPosition(0);
    }

    public void ScrollNews(int p)
    {
        ChangeNewsPosition(curVisibleNews + p);
    }

    void ShowNews()
    {
        if(curVisibleNews == 0)
            fl_UnreadNews.enabled = false;

        GameManagerScript.GM.SetInfo(newsHistory[curVisibleNews].mVideoRolexPattern.mText, newsHistory[curVisibleNews].mCountry);
        ImgDecade.sprite = Decades[newsHistory[curVisibleNews].mSetMonth / 120];
        ImgYear.sprite = Years[newsHistory[curVisibleNews].mSetMonth / 12 % 10];
        ImgMonth.sprite = Months[newsHistory[curVisibleNews].mSetMonth % 12];
    }

    //Переключение на страну, чью новость нажали.
    public void SnapToCountryFromNews()
    {
        if (newsHistory[curVisibleNews] == null)
            return;

        CountryScript c = newsHistory[curVisibleNews].mCountry;
        if (c != null)
            FindObjectOfType<CameraScript>().SetNewPosition(c.Capital);
    }
}
