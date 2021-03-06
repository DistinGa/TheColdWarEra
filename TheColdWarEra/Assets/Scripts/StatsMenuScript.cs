﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class StatsMenuScript : MonoBehaviour
{
    public RectTransform StatListsUS;
    public RectTransform StatListsSov;
    public RectTransform ListUSA;
    public RectTransform ListSU;
    [Space(10)]
    public Sprite[] ScoreSprites;
    public Sprite[] GreenArrows, RedArrows;
    public RectTransform ChartPanel;
    public RectTransform LinePrefab;
    public RectTransform YearTick;

    List<CountryScript> cntUSA = new List<CountryScript>(); //страны с американским влиянием
    List<CountryScript> cntSU = new List<CountryScript>();  //страны с советским влиянием

    public void OnEnable()
    {
        SoundManager.SM.PlaySound("sound/stat");

        PlayerScript AmericanPlayer = GameObject.FindGameObjectWithTag("American").GetComponent<PlayerScript>();
        PlayerScript SovietPlayer = GameObject.FindGameObjectWithTag("Soviet").GetComponent<PlayerScript>();

        //US
        transform.Find("USGNP/txtBudget").GetComponent<Text>().text = AmericanPlayer.Budget.ToString();
        transform.Find("USGNP/txtScore").GetComponent<Text>().text = AmericanPlayer.Score.ToString();
        transform.Find("USGNP/txtSum").GetComponent<Text>().text = (AmericanPlayer.Budget + AmericanPlayer.Score).ToString();
        int delta = 0;

        if (AmericanPlayer.LastBudget > 0)
            delta = (int)((AmericanPlayer.Budget + AmericanPlayer.Score - AmericanPlayer.LastBudget) / AmericanPlayer.LastBudget * 100);
        else
            delta = (int)(AmericanPlayer.Budget + AmericanPlayer.Score);

        Image arrows = null;
        if (delta > 0)
        {
            arrows = transform.Find("USGNP/UpArrows").GetComponent<Image>();
            arrows.gameObject.SetActive(true);
            transform.Find("USGNP/DownArrows").gameObject.SetActive(false);
        }
        if (delta < 0)
        {
            arrows = transform.Find("USGNP/DownArrows").GetComponent<Image>();
            arrows.gameObject.SetActive(true);
            transform.Find("USGNP/UpArrows").gameObject.SetActive(false);
        }
        if (delta == 0)
        {
            transform.Find("USGNP/UpArrows").gameObject.SetActive(false);
            transform.Find("USGNP/DownArrows").gameObject.SetActive(false);
        }

        if (Mathf.Abs(delta) > 0)
            arrows.sprite = (delta > 0 ? GreenArrows : RedArrows)[0];
        if (Mathf.Abs(delta) > 5)
            arrows.sprite = (delta > 0 ? GreenArrows : RedArrows)[1];
        if (Mathf.Abs(delta) > 10)
            arrows.sprite = (delta > 0 ? GreenArrows : RedArrows)[2];

        //Sov
        transform.Find("SUGNP/txtBudget").GetComponent<Text>().text = SovietPlayer.Budget.ToString();
        transform.Find("SUGNP/txtScore").GetComponent<Text>().text = SovietPlayer.Score.ToString();
        transform.Find("SUGNP/txtSum").GetComponent<Text>().text = (SovietPlayer.Budget + SovietPlayer.Score).ToString();

        if(SovietPlayer.LastBudget > 0)
            delta = (int)((SovietPlayer.Budget + SovietPlayer.Score - SovietPlayer.LastBudget) / SovietPlayer.LastBudget * 100);
        else
            delta = (int)(SovietPlayer.Budget + SovietPlayer.Score);

        if (delta > 0)
        {
            arrows = transform.Find("SUGNP/UpArrows").GetComponent<Image>();
            arrows.gameObject.SetActive(true);
            transform.Find("SUGNP/DownArrows").gameObject.SetActive(false);
        }
        if (delta < 0)
        {
            arrows = transform.Find("SUGNP/DownArrows").GetComponent<Image>();
            arrows.gameObject.SetActive(true);
            transform.Find("SUGNP/UpArrows").gameObject.SetActive(false);
        }
        if (delta == 0)
        {
            transform.Find("SUGNP/UpArrows").gameObject.SetActive(false);
            transform.Find("SUGNP/DownArrows").gameObject.SetActive(false);
        }

        if (Mathf.Abs(delta) > 0)
            arrows.sprite = (delta > 0 ? GreenArrows : RedArrows)[0];
        if (Mathf.Abs(delta) > 5)
            arrows.sprite = (delta > 0 ? GreenArrows : RedArrows)[1];
        if (Mathf.Abs(delta) > 10)
            arrows.sprite = (delta > 0 ? GreenArrows : RedArrows)[2];

        //Заполнение списков
        cntUSA.Clear();
        cntSU.Clear();

        GameObject Countries = GameObject.Find("Countries");
        for (int i = 0; i < Countries.transform.childCount; i++)
        {
            CountryScript Country = Countries.transform.GetChild(i).GetComponent<CountryScript>();
            if (Country.AmInf > 0)
                cntUSA.Add(Country);
            if (Country.SovInf > 0)
                cntSU.Add(Country);
        }

        //Сортировка списков по очкам страны
        SortByScore("left");
        SortByScore("right");

        ////Заполнение графика
        //DrawChart();
    }

    void DrawChart()
    {
        int YearsAmount = 10;   //количество лет на графике
        Color redBrush = new Color(1, 0, 0);
        Color blueBrush = new Color(0, 0, 1);

        float xScale, yScale, yOffset;

        //Сначала удалим предыдущие графики
        while (ChartPanel.childCount > 0)
            DestroyImmediate(ChartPanel.GetChild(0).gameObject);

        xScale = ChartPanel.rect.width / (YearsAmount - 1);

        //Определяем начальный элемент истории
        int FirstInd = GameManagerScript.GM.Player.History.Count - YearsAmount;
        if (FirstInd < 0)
            FirstInd = 0;

        //Рисуем годы на графике
        int InitYear = 51 + FirstInd;   //51-й год - первый, где есть статистика

        for (int i = 0; i < YearsAmount; i++)
        {
            RectTransform Year = Instantiate(YearTick);
            Year.SetParent(ChartPanel);
            Year.localPosition = new Vector3(xScale*i, 0, 0);
            int tmpYear = InitYear + i;
            if (tmpYear >= 100)
                tmpYear -= 100;
            Year.transform.Find("Text").GetComponent<Text>().text = tmpYear.ToString("d2");
        }

        //Если в истории меньше двух значений, нечего рисовать
        if (GameManagerScript.GM.Player.History.Count < 2)
            return;

        PlayerScript AmPlayer = GameObject.Find("GameManager/AmerPlayer").GetComponent<PlayerScript>();
        PlayerScript SovPlayer = GameObject.Find("GameManager/SovPlayer").GetComponent<PlayerScript>();
        int[] AmHist = AmPlayer.History.GetRange(FirstInd, Mathf.Min(YearsAmount, AmPlayer.History.Count)).ToArray();
        int[] SovHist = SovPlayer.History.GetRange(FirstInd, Mathf.Min(YearsAmount, SovPlayer.History.Count)).ToArray();

        yScale = ChartPanel.rect.height / (Mathf.Max(Mathf.Max(AmHist), Mathf.Max(SovHist)) - Mathf.Min(Mathf.Min(AmHist), Mathf.Min(SovHist)));
        yOffset = Mathf.Min(Mathf.Min(AmHist), Mathf.Min(SovHist));

        //Вывод графиков
        Vector2 p1, p2;
        RectTransform Line;
        //Американский график
        for (int ind = 0; ind < AmHist.Length - 1; ind++)
        {
            //Для рисования линии будем поворачивать и растягивать простой прямоугольник (Image с пустым спрайтом)
            //Начало линии будет в точке текущего значения статистики (х - год, у - значени), а конец в точке следующего значения из массива.
            p1.x = ind * xScale;
            p1.y = (AmHist[ind] - yOffset) * yScale;

            Line = Instantiate(LinePrefab);
            Line.GetComponent<Image>().color = blueBrush;
            Line.SetParent(ChartPanel);
            Line.localPosition = p1;

            p2.x = (ind + 1) * xScale;
            p2.y = (AmHist[ind + 1] - yOffset) * yScale;

            p1 = p2 - p1;
            Line.localScale = new Vector3(p1.magnitude, 1, 1);
            Line.rotation = Quaternion.FromToRotation(Vector3.right, p1);
        }

        //Советский график
        for (int ind = 0; ind < SovHist.Length - 1; ind++)
        {
            p1.x = ind * xScale;
            p1.y = (SovHist[ind] - yOffset) * yScale;

            Line = Instantiate(LinePrefab);
            Line.GetComponent<Image>().color = redBrush;
            Line.SetParent(ChartPanel);
            Line.localPosition = p1;

            p2.x = (ind + 1) * xScale;
            p2.y = (SovHist[ind + 1] - yOffset) * yScale;

            p1 = p2 - p1;
            Line.localScale = new Vector3(p1.magnitude, 1, 1);
            Line.rotation = Quaternion.FromToRotation(Vector3.right, p1);
        }
    }

    void FillList(RectTransform ListOnScreen, List<CountryScript> cntList)
    {
        //Заполняем списки
        int n = 0;
        foreach (CountryScript c in cntList)
        {
            if (ListOnScreen.childCount < n+1)
                break;

            Transform p = ListOnScreen.GetChild(n);
            p.gameObject.SetActive(true);
            p.GetComponent<StatsLineScript>().Init(c, ScoreSprites[c.Score - 1]);

            n++;
        }

        //Скрываем нижние оставшиеся панельки
        for (int i = n; i < ListOnScreen.childCount; i++)
        {
            ListOnScreen.GetChild(i).gameObject.SetActive(false);
        }

        //Перемещение в верх списка
        ListOnScreen.parent.localPosition = Vector3.zero;
    }

    public void SortByInf(string Side)
    {
        if (Side == "left")
        {
            cntUSA.Sort((a, b) => a.AmInf.CompareTo(b.AmInf));
            cntUSA.Reverse();
            FillList(ListUSA, cntUSA);
        }

        if (Side == "right")
        {
            cntSU.Sort((a, b) => a.SovInf.CompareTo(b.SovInf));
            cntSU.Reverse();
            FillList(ListSU, cntSU);
        }
    }

    public void SortByOpp(string Side)
    {
        if (Side == "left")
        {
            cntUSA.Sort((a, b) => a.Support.CompareTo(b.Support));
            FillList(ListUSA, cntUSA);
        }

        if (Side == "right")
        {
            cntSU.Sort((a, b) => a.Support.CompareTo(b.Support));
            FillList(ListSU, cntSU);
        }
    }

    public void SortByScore(string Side)
    {
        if (Side == "left")
        {
            cntUSA.Sort((a, b) => (a.Authority == Authority.Amer ? a.Score : 0).CompareTo(b.Authority == Authority.Amer?b.Score:0));
            cntUSA.Reverse();
            FillList(ListUSA, cntUSA);
        }

        if (Side == "right")
        {
            cntSU.Sort((a, b) => (a.Authority == Authority.Soviet ? a.Score : 0).CompareTo(b.Authority == Authority.Soviet ? b.Score : 0));
            cntSU.Reverse();
            FillList(ListSU, cntSU);
        }
    }

    public void ScrollStatMenuLeft(int dt)
    {
        ScrollStatMenu(StatListsUS, dt);
    }

    public void ScrollStatMenuRight(int dt)
    {
         ScrollStatMenu(StatListsSov, dt);
    }

    void ScrollStatMenu(RectTransform StatLists, int dt)
    {
        SoundManager.SM.PlaySound("sound/click2");

        Vector3 NewPos = StatLists.localPosition + Vector3.up * dt;

        if (NewPos.y < 0)
            NewPos -= Vector3.up * NewPos.y;

        float maxY = StatLists.rect.height - ((RectTransform)StatLists.parent).rect.height;
        if (NewPos.y > maxY)
            NewPos -= Vector3.up * (NewPos.y - maxY);

        StatLists.localPosition = NewPos;
    }
}
