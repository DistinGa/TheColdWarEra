using UnityEngine;
using System.Collections.Generic;

public class CountryScript : MonoBehaviour
{
    public Sprite SprAmerican;
    public Sprite SprSoviet;
    public Sprite SprNeutral;
    public Sprite FlagS;
    public Sprite FlagNs;

    [Space(10)]
    public string Name;
    public Region Region;
    public Authority Authority;
    public int Score;
    public float Support;
    [Space(10)]
    public float SovInf;
    public float AmInf;
    public float NInf;
    [Space(10)]
    public int GovForce;
    public int OppForce;
    [Space(10)]
    public int KGB;
    public int CIA;

    [Space(10)]
    private Transform StatePanel;
    public List<StateSymbol> Symbols = new List<StateSymbol>();

    [HideInInspector]
    public int DiscounterUsaMeeting, DiscounterRusMeeting; //Сколько ждать до возможности следующего митинга протеста (0 - можно митинговать)
    [HideInInspector]
    public int DiscounterUsaParade, DiscounterRusParade; //Сколько ждать до возможности следующего парада протеста (0 - можно)
    [HideInInspector]
    public int DiscounterUsaSpy, DiscounterRusSpy, DiscounterUsaInfl, DiscounterRusInfl; //Дискаунтер для возможности засылки шпионов или повышения влияния (0 - можно)

    // Use this for initialization
    void Start()
    {
        SetAuthority();
        StatePanel = transform.Find("Capital/Canvas/Panel");
        AddState(States.SYM_PARAD, Authority.Amer, 2);
        AddState(States.SYM_WAR, Authority.Amer, 1);
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
    public void AddInfluence(Authority Inf, float Amount)
    {
        if (Inf == Authority.Amer)
        {
            AmInf += Amount;

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

            //Влияние повысили, устанавливаем дискаунтер, чтобы отключить возможность повторного повышения в пределах отведённого периода.
            DiscounterUsaInfl = GameManagerScript.GM.MAX_INFLU_CLICK;
        }

        if (Inf == Authority.Soviet)
        {
            SovInf += Amount;

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
    }

    //Добавление вооружённых сил.
    //Inf - чьи силы добавляются.
    public void AddMilitary(Authority Inf, int Amount)
    {
        if (Authority == Authority.Neutral)
        {
            if (SovInf > AmInf) //Если советсткое влияние, то советский игрок добавляет нейтральные силы, американский - оппозиционные.
            {
                if (Inf == Authority.Soviet)
                    GovForce += Amount;
                else
                    OppForce += Amount;
            }
            else //Если режим проамериканский, то американский игрок добавляет нейтральные силы, советский - оппозиционные.
            {
                if (Inf == Authority.Amer)
                    GovForce += Amount;
                else
                    OppForce += Amount;
            }
        }
        else    //Если режим страны не нейтральный, то игрок, чей режим установлен, добавляет правительственные силы. Другой игрок добавляет оппозиционные силы.
        {
            if (Inf == Authority)
                GovForce += Amount;
            else
                OppForce += Amount;
        }

        //Устранение выхода за допустимую границу.
        if (GovForce > 10)
            GovForce = 10;
        if (OppForce > 10)
            OppForce = 10;
    }

    public Transform Capital
    {
        get { return transform.FindChild("Capital"); }
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
        return (Authority != Aut && Support <= (100 - GameManagerScript.GM.INSTALL_PUPPER_OPPO) &&
            ((Aut == Authority.Amer && AmInf >= GameManagerScript.GM.INSTALL_PUPPER_INFLU) || Aut == Authority.Soviet && SovInf >= GameManagerScript.GM.INSTALL_PUPPER_INFLU));
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

        Support = 100 - Support;    // оппозиция стала поддержкой
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
                Destroy(item.Symbol.gameObject);
                Symbols.Remove(item);
            }
        }
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
            Symbols.Add(new StateSymbol(state, aut, lifeTime, this));
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
            Destroy(ss.Symbol.gameObject);
            Symbols.Remove(ss);
        }
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

        public RectTransform Symbol; // сам значок

        // конструктор
        public StateSymbol(States state, Authority authority, int life, CountryScript Country)
        {
            State = state;
            Authority = authority;
            MonthsToShow = life;

            Symbol = Instantiate(GameManagerScript.GM.StatePrefab);
            Symbol.SetParent(Country.StatePanel);
            Symbol.GetComponent<StatePrefab>().Init((int)state, authority);
        }

    }
}

