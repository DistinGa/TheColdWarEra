﻿
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class CountryScript : MonoBehaviour
{
    public Sprite SprAmerican;
    public Sprite SprSoviet;
    public Sprite SprNeutral;
    public Sprite FlagS;
    public Sprite FlagNs;

    [Space(10)]
    public string Name;
    public Sprite PicName;
    public Region Region;
    public Authority Authority;
    public int Score;
    [SerializeField]
    Sprite[] scorePics = new Sprite[5];
    [SerializeField]
    private float support;
    [Space(10)]
    public int SovInf;
    public int AmInf;
    public int NInf;
    public Authority LastAut;   //чьё влияние было установлено последним
    [Space(10)]
    public int GovForce;
    public int OppForce;
    [Space(10)]
    public int KGB;
    public int CIA;

    [Space(10)]
    private Transform StatePanel;
    public List<StateSymbol> Symbols = new List<StateSymbol>();
    [SerializeField]
    Sprite countryChar, statsName;

    [HideInInspector]
    public int DiscounterUsaMeeting, DiscounterRusMeeting; //Сколько ждать до возможности следующего митинга протеста (0 - можно митинговать)
    [HideInInspector]
    public int DiscounterUsaParade, DiscounterRusParade; //Сколько ждать до возможности следующего парада (0 - можно)
    [HideInInspector]
    public int DiscounterUsaSpy, DiscounterRusSpy, DiscounterUsaInfl, DiscounterRusInfl; //Дискаунтер для возможности засылки шпионов или повышения влияния (0 - можно)

    // Use this for initialization
    void Start()
    {
        SetAuthority();
        StatePanel = transform.Find("Capital/StateCanvas");
    }

    //Возвращает список всех стран или отпределённой принадлежности
    public static List<CountryScript> Countries()
    {
        List<CountryScript> result = new List<CountryScript>();

        GameObject Countries = GameObject.Find("Countries");
        for (int idx = 0; idx < Countries.transform.childCount; idx++)
        {
            result.Add(Countries.transform.GetChild(idx).GetComponent<CountryScript>());
        }

        return result;
    }

    public static List<CountryScript> Countries(Authority aut)
    {
        List<CountryScript> result = new List<CountryScript>();

        GameObject Countries = GameObject.Find("Countries");
        CountryScript Country;
        for (int idx = 0; idx < Countries.transform.childCount; idx++)
        {
            Country = Countries.transform.GetChild(idx).GetComponent<CountryScript>();
            if (Country.Authority == aut)
                result.Add(Countries.transform.GetChild(idx).GetComponent<CountryScript>());
        }

        return result;
    }

    //Установка цвета границы.
    public void SetAuthority()
    {
        SpriteRenderer Spr = GetComponent<SpriteRenderer>();
        switch (Authority)
        {
            case Authority.Neutral:
                Spr.sprite = SprNeutral;
                break;
            case Authority.Amer:
                Spr.sprite = SprAmerican;
                break;
            case Authority.Soviet:
                Spr.sprite = SprSoviet;
                break;
            default:
                break;
        }
    }

    public void OnMouseUpAsButton()
    {
        if (!FindObjectOfType<CameraScript>().setOverMenu)
            GameManagerScript.GM.SnapToCountry((Vector2)Input.mousePosition);
    }

    //Добавление влияния.
    //Inf - чьё влияние добавляется.
    //Auto - true: влияние добавляется в результате случайного события или от космогонки.
    //      false: явное добавление
    public void AddInfluence(Authority Inf, int Amount, bool Auto = true)
    {
        switch (Inf)
        {
            case Authority.Neutral:
                //определяем максимально допустимое количество влияния, которое может быть добавлено/отнято
                if (Amount > 0)
                    Amount = Mathf.Min(Amount, 100 - NInf);
                else
                    Amount = -Mathf.Min(-Amount, NInf);

                NInf += Amount;

                //Распределяем "минус" по другим влияниям.
                AmInf -= Amount / 2;
                SovInf -= (Amount - Amount / 2);  //чтобы не накапливалось расхождение из-за округления
                //Если американского влияния было мало, отнимаем остаток от советсткого, а американское обнуляем
                if (AmInf < 0)
                {
                    SovInf += AmInf;
                    AmInf = 0;
                }

                //Если советсткого влияния было мало, отнимаем остаток от американского, а советское обнуляем
                if (SovInf < 0)
                {
                    AmInf += SovInf;
                    SovInf = 0;
                }
                break;
            case Authority.Amer:
                //определяем максимально допустимое количество влияния, которое может быть добавлено/отнято
                if (Amount > 0)
                    Amount = Mathf.Min(Amount, 100 - AmInf);
                else
                    Amount = -Mathf.Min(-Amount, AmInf);

                AmInf += Amount;
                DiscounterUsaInfl = GameManagerScript.GM.MAX_INFLU_CLICK;

                //Распределяем "минус" по другим влияниям.
                NInf -= Amount; //Сначала отнимаем от нейтрального влияния
                if (NInf < 0)    //Если нейтрального влияния не хватило, отнимаем от влияния соперника.
                {
                    SovInf += NInf; //NInf отрицательное
                    NInf = 0;
                    if (SovInf < 0) //Если и влияния соперника не хватило, значит была попытка добавить слишком большое влияние.
                    {
                        SovInf = 0;
                        AmInf = 100;
                    }
                }
                break;
            case Authority.Soviet:
                //определяем максимально допустимое количество влияния, которое может быть добавлено/отнято
                if (Amount > 0)
                    Amount = Mathf.Min(Amount, 100 - SovInf);
                else
                    Amount = -Mathf.Min(-Amount, SovInf);

                SovInf += Amount;
                DiscounterRusInfl = GameManagerScript.GM.MAX_INFLU_CLICK;

                //Распределяем "минус" по другим влияниям.
                NInf -= Amount; //Сначала отнимаем от нейтрального влияния
                if (NInf < 0)    //Если нейтрального влияния не хватило, отнимаем от влияния соперника.
                {
                    AmInf += NInf; //NInf отрицательное
                    NInf = 0;
                    if (AmInf < 0) //Если и влияния соперника не хватило, значит была попытка добавить слишком большое влияние.
                    {
                        SovInf = 100;
                        AmInf = 0;
                    }
                }
                break;
        }

        //SovInf = Mathf.Clamp(SovInf, 0, 100);
        //AmInf = Mathf.Clamp(AmInf, 0, 100);

        if (!Auto)
        {
            LastAut = Inf;
        }
    }

    //Добавление шпионов.
    //Inf - чей шпион добавляется.
    public void AddSpy(Authority Inf, int Amount)
    {
        if (Inf == Authority.Amer)
        {
            CIA += Amount;
            if (CIA > 5)
                CIA = 5;
        }
        if (Inf == Authority.Soviet)
        {
            KGB += Amount;
            if (KGB > 5)
                KGB = 5;
        }
        KGB = Mathf.Clamp(KGB, 0, 5);
        CIA = Mathf.Clamp(CIA, 0, 5);

        //устанавливаем дискаунтер, чтобы отключить возможность повторного повышения в пределах отведённого периода.
        if (Inf == Authority.Amer)
            DiscounterUsaSpy = GameManagerScript.GM.MAX_SPY_CLICK;
        if (Inf == Authority.Soviet)
            DiscounterRusSpy = GameManagerScript.GM.MAX_SPY_CLICK;
    }

    //Добавление вооружённых сил.
    //Inf - чьи силы добавляются.
    //возвращает true, если добавили правительственные силы и false - если оппозиционные
    public bool AddMilitary(Authority Inf, int Amount)
    {
        bool res = false;

        if (Authority == Authority.Neutral)
        {
            if (SovInf > AmInf) //Если советсткое влияние, то советский игрок добавляет нейтральные силы, американский - оппозиционные.
            {
                if (Inf == Authority.Soviet)
                {
                    GovForce += Amount;
                    res = true;
                }
                else
                    OppForce += Amount;
            }
            else //Если влияние проамериканское, то американский игрок добавляет нейтральные силы, советский - оппозиционные.
            {
                if (Inf == Authority.Amer)
                {
                    GovForce += Amount;
                    res = true;
                }
                else
                    OppForce += Amount;
            }
        }
        else    //Если режим страны не нейтральный, то игрок, чей режим установлен, добавляет правительственные силы. Другой игрок добавляет оппозиционные силы.
        {
            if (Inf == Authority)
            {
                GovForce += Amount;
                res = true;
            }
            else
                OppForce += Amount;
        }

        //Устранение выхода за допустимую границу.
        GovForce = Mathf.Clamp(GovForce, 0, 10);
        OppForce = Mathf.Clamp(OppForce, 0, 10);

        return res;
    }

    public Transform Capital
    {
        get { return transform.FindChild("Capital"); }
    }

    public float Support
    {
        get {return support;}

        set
        {
            support = value;
            if (support < 0f) support = 0f;
            if (support > 100f) support = 100f;
        }
    }

    //Проверка возможности добавить влияние
    public bool CanAddInf(Authority Aut)
    {
        return (Aut == Authority.Amer && DiscounterUsaInfl == 0 && AmInf < 100) || (Aut == Authority.Soviet && DiscounterRusInfl == 0 && SovInf < 100);
    }

    //Проверка возможности добавить войска
    public bool CanAddMil(Authority Aut)
    {
        return ((Aut == Authority && GovForce < 10) ||  //свои войска 
            (Authority == Authority.Neutral && Aut == Authority.Amer && (AmInf > SovInf) && GovForce < 10) ||   //поддержка нейтрального правительства
            (Authority == Authority.Neutral && Aut == Authority.Soviet && (AmInf < SovInf) && GovForce < 10) || //поддержка нейтрального правительства
            (Support <= (100 - GameManagerScript.GM.INSTALL_PUPPER_REVOL) && Authority != Authority.Neutral && Authority != Aut && OppForce < 10) ||    //оппозиция в чужой стране
            (Support <= (100 - GameManagerScript.GM.INSTALL_PUPPER_REVOL) && Authority == Authority.Neutral && Aut == Authority.Amer && (AmInf < SovInf) && OppForce < 10) ||  //оппозиция в нейтральной стране
            (Support <= (100 - GameManagerScript.GM.INSTALL_PUPPER_REVOL) && Authority == Authority.Neutral && Aut == Authority.Soviet && (AmInf > SovInf) && OppForce < 10)    //оппозиция в нейтральной стране
            );
    }

    //Проверка возможности добавить шпиона
    public bool CanAddSpy(Authority Aut)
    {
        return (Aut == Authority.Amer && DiscounterUsaSpy == 0 && CIA < 5) || (Aut == Authority.Soviet && DiscounterRusSpy == 0 && KGB < 5);
    }

    //Проверка возможности организовать восстание
    public bool CanOrgMeeting(Authority Aut)
    {
        return (HaveSpy(Aut) && Authority != Aut && ((Aut == Authority.Amer && DiscounterUsaMeeting == 0) || (Aut == Authority.Soviet && DiscounterRusMeeting == 0)));
    }

    //Проверка возможности организовать парад
    public bool CanOrgParade(Authority Aut)
    {
        return (HaveSpy(Aut) && (Authority == Aut || Authority == Authority.Neutral) && ((Aut == Authority.Amer && DiscounterUsaParade == 0) || (Aut == Authority.Soviet && DiscounterRusParade == 0)));
    }

    //Проверка возможности сменить правительство
    //Aut - на какое правительство хотим поменять
    public bool CanChangeGov(Authority Aut)
    {
        return (Authority != Aut && Support <= (100 - GameManagerScript.GM.INSTALL_PUPPER_OPPO) && GetInfluense(Aut) >= GameManagerScript.GM.INSTALL_PUPPER_INFLU);
    }

    //Проверка наличия шпионов
    //Aut - чьих шпионов проверяем
    public bool HaveSpy(Authority Aut)
    {
        return (Aut == Authority.Amer && CIA > 0) || (Aut == Authority.Soviet && KGB > 0);
    }

    //Смена правительства
    public void ChangeGov(Authority NewAut)
    {
        Authority = NewAut;

        //меняем местами войска
        int mil = GovForce;
        GovForce = OppForce;
        OppForce = mil;

        //Читтинг. Добавляем 3 военных и 1 шпиона. (Чтобы не возникало ситуации, когда после смены власти сразу происходит революция, т.к. нет правительственных сил)
        if(GovForce < 3)
            GovForce = 3;
        if(!HaveSpy(NewAut))
            AddSpy(NewAut, 1);

        Support = 100 - Support;    // оппозиция стала поддержкой
        SetAuthority(); //Смена цвета границ

        //Steam achievments
#if !myDEBUG
        GameManagerScript GM = GameManagerScript.GM;

        if (Name == "Land of Chill" || Name == "The Great Desert")
        {
            if(GM.FindCountryById(3).Authority == GM.Player.Authority && GM.FindCountryById(44).Authority == GM.Player.Authority)
                SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_2");
        }

        if (Name == "Warm Sea" || Name == "Frozen Sea")
        {
            if (GM.FindCountryById(1).Authority == GM.Player.Authority && GM.FindCountryById(2).Authority == GM.Player.Authority)
                SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_3");
        }
#endif
    }

    //Обработка начала месяца
    public void NextMonth()
    {
        //Уменьшаем дискаунтеры
        if (DiscounterRusInfl > 0) DiscounterRusInfl--;
        if (DiscounterRusMeeting > 0) DiscounterRusMeeting--;
        if (DiscounterRusParade > 0) DiscounterRusParade--;
        if (DiscounterRusSpy > 0) DiscounterRusSpy--;
        if (DiscounterUsaInfl > 0) DiscounterUsaInfl--;
        if (DiscounterUsaMeeting > 0) DiscounterUsaMeeting--;
        if (DiscounterUsaParade > 0) DiscounterUsaParade--;
        if (DiscounterUsaSpy > 0) DiscounterUsaSpy--;

        //Обработка значков состояний
        for(int indx = Symbols.Count-1; indx >=0; indx--)
        {
            StateSymbol item = Symbols[indx];

            if (--item.MonthsToShow <= 0)
            {
                //Destroy(item.Symbol.gameObject);
                Symbols.Remove(item);
            }
        }
        UpdateStates();
    }

    //Добавление состояния
    public void AddState(States state, Authority aut, int lifeTime)
    {
        bool exist = false;

        foreach (var item in Symbols)
        {
            if (state == item.State && aut == item.Authority)
            {
                exist = true;
                break;
            }
        }

        if(!exist)
            Symbols.Insert(0, new StateSymbol(state, aut, lifeTime, this));
    }

    //Удаление состояния
    public void DelState(States state, Authority aut)
    {
        bool exist = false;
        StateSymbol ss = null;

        foreach (var item in Symbols)
        {
            if (state == item.State && aut == item.Authority)
            {
                exist = true;
                ss = item;
                break;
            }
        }

        if (exist)
        {
            //Destroy(ss.Symbol.gameObject);
            Symbols.Remove(ss);
            UpdateStates();
        }
    }

    void UpdateStates()
    {
        for (int i = 0; i < StatePanel.childCount; i++)
        {
            Image im = StatePanel.GetChild(i).GetComponent<Image>();

            if (Symbols.Count > i)
            {
                im.enabled = true;
                im.sprite = Symbols[i].Symbol;
            }
            else
            {
                im.enabled = false;
            }
        }
    }

    //Проверка состояний в стране
    public void TestStates()
    {
        //Проверка возможности мирно сменить власть обеими державами
        if (CanChangeGov(Authority.Amer))
            AddState(States.SYM_PEACE, Authority.Amer, 10000);
        else
            DelState(States.SYM_PEACE, Authority.Amer);

        if (CanChangeGov(Authority.Soviet))
            AddState(States.SYM_PEACE, Authority.Soviet, 10000);
        else
            DelState(States.SYM_PEACE, Authority.Soviet);

        //Проверка возможности ввода оппозиционных войск
        Authority Aut = Authority.Neutral;
        //Сначала убираем значки обеих держав
        DelState(States.SYM_REVOL, Authority.Amer);
        DelState(States.SYM_REVOL, Authority.Soviet);

        //Возможность начала революции
        if (Support <= 100 - GameManagerScript.GM.INSTALL_PUPPER_REVOL)
        {
            switch (Authority)
            {
                case Authority.Neutral:
                    if (AmInf < SovInf)
                        Aut = Authority.Amer;
                    if (SovInf < AmInf)
                        Aut = Authority.Soviet;
                    break;
                case Authority.Amer:
                    Aut = Authority.Soviet;
                    break;
                case Authority.Soviet:
                    Aut = Authority.Amer;
                    break;
            }
        }

        if (Aut != Authority.Neutral)
        {
            AddState(States.SYM_REVOL, Aut, 10000);
        }

        //Проверка состояния войны
        if (GovForce > 0 && OppForce > 0)
        {
            AddState(States.SYM_WAR, Authority.Amer, 10000);
            UpdateStates();
            //Добавление страны в правый список
            GameManagerScript.GM.AddWarFlag(this);
        }
        else
        {
            //Война закончилась
            DelState(States.SYM_WAR, Authority.Amer);
            //Удаление страны из правого списка
            GameManagerScript.GM.RemoveWarFlag(this);

            //Удаление роликов о войне
            GameManagerScript.GM.VQueue.ClearVideoQueue(this, VideoQueue.V_PUPPER_REVOLUTION);
        }

        //Удаление "просроченных" состояний
        foreach (var item in Symbols)
        {
            if (item.MonthsToShow <= 0)
                DelState(item.State, item.Authority);
        }
    }

    //Возвращает влияние заданной стороны
    public int GetInfluense(Authority aut)
    {
        int result = 0;

        switch (aut)
        {
            case Authority.Neutral:
                result = NInf;
                break;
            case Authority.Amer:
                result = AmInf;
                break;
            case Authority.Soviet:
                result = SovInf;
                break;
        }

        return result;
    }

    //Возвращает количество военных заданной стороны
    public int GetForces(Authority aut)
    {
        int result = 0;

        if (Authority == Authority.Neutral)
        {
            if ((SovInf > AmInf && aut == Authority.Soviet) || (SovInf <= AmInf && aut == Authority.Amer))
                result = GovForce;
            else
                result = OppForce;
        }
        else
        {
            if (Authority == aut)
                result = GovForce;
            else
                result = OppForce;
        }

        return result;
    }

    //Возвращает Score в виде спрайта с количеством существ.
    public Sprite GetScoreAsSprite()
    {
        return scorePics[Score - 1];
    }

    //Возвращает персонажа для страны.
    public Sprite GetCountryChar()
    {
        return countryChar;
    }

    public Sprite GetStatsNameSprite()
    {
        return statsName;
    }

    //перечисление состояний страны
    public enum States
    {
        SYM_PEACE, // дипломат-смена мирным путем возможна
        SYM_REVOL, // автомат -смена военным путем возможна (ввод революционеров)
        SYM_SPY, // шпион   -пойман
        SYM_WAR, // идут военные действия
        SYM_PARAD, // парад
        SYM_RIOT // митинг
    }

    // описание значка для состояния страны
    public class StateSymbol
    {
        public States State;    //Состояние
        public Authority Authority; // за какую сторону
        public int MonthsToShow; //сколько месяцев показывать (discaunter)

        public Sprite Symbol; // сам значок

        // конструктор
        public StateSymbol(States state, Authority authority, int life, CountryScript Country)
        {
            State = state;
            Authority = authority;
            MonthsToShow = life;

            //Symbol = Instantiate(GameManagerScript.GM.StatePrefab);
            //Symbol.SetParent(Country.StatePanel);
            //Symbol.GetComponent<StatePrefab>().Init((int)state, authority);

            Symbol = GameManagerScript.GM.StatePrefab.GetComponent<StatePrefab>().TakePicture((int)state, authority);
        }

    }
}

