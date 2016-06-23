using UnityEngine;
using System.Collections.Generic;

public class AI : MonoBehaviour {
    public PlayerScript AIPlayer;

    // типы ботов
    const int AI_SCIENTIST = 0;  // учёный
    const int AI_MILITARIST = 1;  // военный
    const int AI_DIPLOMAT = 2;  // дипломат
    const int AI_MAX = 3;       //количество типов

    public int mAIKind; // тип бота, AI_*

    const int AIWAR_PAUSE = 12;  // как часто вводить военных-революционеров по п.4

    // настроечные параметры
    public const int DIFF_NUM = 3; // число уровней сложности

    [System.Serializable]
    public class ByType
    //вспомогательный класс для представления двумерных массивов
    {
        public int[] values;

        public ByType(int size)
        {
            values = new int[size];
        }
    }

    public ByType[] START_BUDGET_AI = new ByType[DIFF_NUM]; // начальный бюджет бота
    public int[] START_BUDGET_PLR = new int[DIFF_NUM];         // начальный бюджет игрока
    public int[] START_BUDGET_PLR_LE = new int[DIFF_NUM];       // начальный бюджет игрока для LE

    public ByType[] MIN_BUDGET_AI = new ByType[DIFF_NUM];   // мин. бюджет бота
    public ByType[] MIN_SPACE_BUDGET_AI = new ByType[DIFF_NUM];   // мин. бюджет бота для покупки космотехнологий
    public ByType[] MIL_PAUSE_AI = new ByType[DIFF_NUM];   // Добавлять милитари раз в два (переменная) месяца
    public ByType[] MIL_HOWMANY_AI_from = new ByType[DIFF_NUM]; // сколько добавлять милитари "от"
    public ByType[] MIL_HOWMANY_AI_to = new ByType[DIFF_NUM]; // сколько добавлять милитари "до"
    public ByType[] MIL_WAR_AI = new ByType[DIFF_NUM];   // Во время военных действий бот добавляет по 5 (переменная) милитари
    public ByType[] SPY_PAUSE_AI = new ByType[DIFF_NUM];   // Пусть бот отправляет по одному шпиону каждые два (переменная) месяца 
    public ByType[] DIPLOMAT_PAUSE_AI = new ByType[DIFF_NUM];   // Раз в шесть (перменная) месяцев бот выбирает support – parade
    public ByType[] INFLU_PAUSE_AI = new ByType[DIFF_NUM];   // Частота подъема influ
    public ByType[] NEUTRAL_SUPPORT_AI = new ByType[DIFF_NUM];   // Поддержка нейтральных где воюет противник
    public int[] ADD_AI_BUDGET = new int[DIFF_NUM];          // Ежегодный бонус к бюджету AI

    const int AIPOWER_STANDARD = 1; // стандартная сила

    public int mPower; // сила игры AIPOWER_*

    public int mAILevel; // текущий уровень трудности 0..DIFF_LEVEL-1

    public int mMilCount;       // счетчик месяцев для военных п.3
    public int mDiplomatCount;  // последний месяц, когда дипломаты устраивали митинг
    public int mSpyCount;       // последний месяц, когда засылали шпионов 
    public int mInfluCount;     // последний месяц, когда повышали influ

    //  Бот должен копить сумму и не тратить ее в диапазоне от 500 до 700 ( минимум 550 для БУ )
    public int mMinGNP = 0; // минимум, ниже которого не тратить

    CountryScript mTargetCountry;    // целевая страна, на которую ведется атака
    int mTargetStartMonth; // когда стали атаковать нейтральную страну (чтобы не дольше 10 месяцев) 


    void Start ()
    {
        if (GameManagerScript.GM.AI == null)
            Destroy(gameObject);    //Играем против другого игрока, АИ не нужен

        mAIKind =  // AI_SCIENTIST;
                   // AI_DIPLOMAT;
                   // AI_MILITARIST;
                    Random.Range(0, AI_MAX);
        mMinGNP = MIN_BUDGET_AI[mAILevel].values[mAIKind];

        mPower = AIPOWER_STANDARD;

        mMilCount = mDiplomatCount = mSpyCount = 0;

        // выбираем страну атаки:
        SetTargetCountry();
    }

    // выбрать страну для целевой атаки:
    private void SetTargetCountry()
    {
        List<CountryScript> countries = new List<CountryScript>();
        GameObject goCountries = GameObject.Find("Countries");
        CountryScript c;
        for (int i = 2; i < goCountries.transform.childCount; i++)
        {
            c = goCountries.transform.GetChild(i).GetComponent<CountryScript>();
            if (mAIKind == AI_SCIENTIST && c.Authority == Authority.Neutral &&
                c.GetInfluense(AIPlayer.Authority) >= 50 ||   // учёному -- нейтрал

                mAIKind == AI_DIPLOMAT && c.Authority == Authority.Neutral &&
                (c.GetInfluense(AIPlayer.Authority) < c.GetInfluense(GameManagerScript.GM.Player.Authority)) || // дипломату -- где влияние оппонента меньше

                mAIKind == AI_MILITARIST && c.Authority == GameManagerScript.GM.Player.Authority) // другим -- просто противника-чела
                countries.Add(c);
        }

        if (countries.Count <= 0)
        {
            mTargetCountry = null; // все исчерпали :)
            return;
        }

        int n = countries.Count == 1 ? 0 : Random.Range(0, countries.Count);
        mTargetCountry = countries[n];
        mTargetStartMonth = GameManagerScript.GM.CurrentMonth();
    }

    // ход AI
    public void AIturn()
    {
        GameManagerScript GM = GameManagerScript.GM;

        // Бот должен копить сумму и не тратить ее в диапазоне от 500 до 700 ( минимум 550 для БУ )
        if (AIPlayer.Budget < mMinGNP) return;

        // раз в 10 лет меняем ориентацию:
        if ((GM.CurrentMonth() % (10 * 12)) == 0)
        {
            int currori = mAIKind;
            for (int i = 0; i < 100; i++)
            {
                mAIKind = Random.Range(0, AI_MAX);
                if (currori != mAIKind) break;
            }
            //ShowAiName();
        }


        // Сразу покупать космическую гонку когда сумма больше 700 ( 650 для БУ )
        if (AIPlayer.Budget >= MIN_SPACE_BUDGET_AI[mAILevel].values[mAIKind])
        {
            // ищем рандомно очередную доступную технологию:
            SpaceRace SR = FindObjectOfType<SpaceRace>();

            List<int> techIndxs = new List<int>();
            for (int i = 1; i < SpaceRace.TechCount; i++)
                if (!AIPlayer.GetTechStatus(i) && AIPlayer.GetTechStatus(SR.GetPrevTechNumber(i)))
                {
                    techIndxs.Add(i);
                }

            if (techIndxs.Count <= 0) goto NEXT;

            // запустить технологию:
            SR.LaunchTech(AIPlayer, Random.Range(0, techIndxs.Count));
        }

    NEXT:
        List<CountryScript> countries; // промежуточный список стран 
        List<bool> flags;        // флажки к ним, вариант действия

        // смотрим везде, где можно свою власть поставить или нагнать революционеров:
        //GameObject goCountries = GameObject.Find("Countries");
        //CountryScript c = null;
        //foreach (int idx = 0; idx < goCountries.transform.childCount; idx++)
        foreach (CountryScript c in CountryScript.Countries())
        {
            if (c.CanChangeGov(AIPlayer.Authority)) // можно мирно сменить власть прям сейчас?
                GM.ChangeGovernment(c, AIPlayer.Authority);
            else // пореже, чтобы деньги зря не тратил на быстрый нагон революционеров
            if (
                c.Support < 100 - GM.INSTALL_PUPPER_OPPO &&
                (c.Authority == GM.Player.Authority || // страна противника/чела
                 (c.Authority == Authority.Neutral) && c.GetInfluense(AIPlayer.Authority) < c.GetInfluense(GM.Player.Authority)) // страна нейтрал, влияние бота ниже и высокая оппозиция
                ) // можно вводить революционеров в страну противника?
            {
                AddMilitary(c, MIL_WAR_AI[mAILevel].values[mAIKind]); // п.4
            }
        }

        // 3- Добавлять милитари раз в два месяца ( раз в три месяц для БУ ) в одной случайной стране 
        if (GM.CurrentMonth() - mMilCount >= MIL_PAUSE_AI[mAILevel].values[mAIKind])
        {
            mMilCount = GM.CurrentMonth();

            // список наших (нечеловеческих, ботовских) стран
            countries = new List<CountryScript>();

            // первым делом защищать свою страну AI :)
            CountryScript myc = AIPlayer.MyCountry;
            if (myc.GovForce < 10)
                countries.Add(myc); // только к себе!
            else
            { // куда угодно
                foreach (CountryScript c in CountryScript.Countries(AIPlayer.Authority))
                    countries.Add(c);
            }

            if (countries.Count > 0)
            {
                int n = countries.Count == 1 ? 0 : Random.Range(0, countries.Count);
                AddMilitary(countries[n], Random.Range(MIL_HOWMANY_AI_from[mAILevel].values[mAIKind], MIL_HOWMANY_AI_to[mAILevel].values[mAIKind] + 1));
            }
        }



        // 5- Пусть бот отправляет по одному шпиону каждые два месяца ( раз в три месяц для БУ, и раз в месяц для БД ) 
        // туда, где есть шпион противника или где оппозиция 50 и выше. 
        // и свой influence меньше чем у оппонента.

        int dd = SPY_PAUSE_AI[mAILevel].values[mAIKind];

        countries = new List<CountryScript>();
        flags = new List<bool>();

        foreach (CountryScript c in CountryScript.Countries())
            if (GM.CurrentMonth() - mSpyCount >= dd)

                // влияние бота тут ниже?
                if (c.GetInfluense(AIPlayer.Authority) < c.GetInfluense(GM.Player.Authority))

                    if (AIPlayer.Authority == Authority.Soviet && (c.CIA > 0 || c.Support < 50) && c.KGB < 5)
                    {
                        countries.Add(c);
                        flags.Add(true);
                    }
                    else
                    if (AIPlayer.Authority == Authority.Amer && (c.KGB > 0 || c.Support < 50) && c.CIA < 5)
                    {
                        countries.Add(c);
                        flags.Add(false);
                    }

        if (countries.Count > 0)
        {
            int n = countries.Count == 1 ? 0 : Random.Range(0, countries.Count);
            AddSpyToCountry(countries[n]);
        }


        // 6 - Раз в шесть месяцев бот рендомно выбирает parade - riot 
        dd = DIPLOMAT_PAUSE_AI[mAILevel].values[mAIKind];
        if (GM.CurrentMonth() - mDiplomatCount >= dd)
        {
            countries = new List<CountryScript>();
            flags = new List<bool>();

            foreach (CountryScript c in CountryScript.Countries())
                if (c.Authority != Authority.Neutral && // п.6 - для войны в странах альянса или в странах противника
                   (c.HaveSpy(AIPlayer.Authority))) // в стране есть шпионы бота

                    if (c.Support < 50 && c.Authority == AIPlayer.Authority) // парады в моей 
                    { // Использовать support parade в стране AI можно только если оппозиция человека (там где шпион) от 50 и выше. 
                        countries.Add(c);
                        flags.Add(true);
                    }
                    else

                    if (c.Support >= 19 && c.Authority != AIPlayer.Authority && // riot -- в противника или нейтральной
                        (AIPlayer.Authority == Authority.Soviet && c.KGB > 3 || AIPlayer.Authority == Authority.Amer && c.CIA > 3)) // в стране больше 3 шпионов
                    { // Использовать support riot можно только если оппозиция не выше 81.
                      // c.mLastAIDiplomat = GM.CurrentMonth();
                      // GameEngine.mBoardForm.SupportRiot(c, true);
                        countries.Add(c);
                        flags.Add(false);
                    }

            if (countries.Count > 0)
            {
                mDiplomatCount = GM.CurrentMonth();
                int n = countries.Count == 1 ? 0 : Random.Range(0, countries.Count);
                GM.CallMeeting(countries[n], AIPlayer, flags[n]);
            }
        }


        // 7 -  Пусть поднимает свой influence «вручную-add» один раз в два месяца ( раз в месяц для БД ) 
        // в любой стране на карте в которой его influence уже 49 и выше, но которая не в его сфере влияния. 
        // Или поднимает свой influence в любой стране на карте которая в его сфере влияния но influence ниже 51.
        dd = INFLU_PAUSE_AI[mAILevel].values[mAIKind];
        countries = new List<CountryScript>();
        foreach (CountryScript c in CountryScript.Countries())
            if (GM.CurrentMonth() - mInfluCount >= dd &&
                (c.Authority == AIPlayer.Authority && c.GetInfluense(AIPlayer.Authority) < 51 ||
                 c.Authority != AIPlayer.Authority && c.GetInfluense(AIPlayer.Authority) >= 49 && c.GetInfluense(AIPlayer.Authority) <= 85))
                countries.Add(c);

        if (countries.Count > 0)
        {
            // отобрать три с самым высоким influ:
            int max3 = 3; // сколько отобрать
            while (countries.Count > max3) // отбросить с самым маленьким
            {
                double min = countries[0].GetInfluense(AIPlayer.Authority); int ind = 0;
                for (int i = 1; i < countries.Count; i++)
                    if (countries[i].GetInfluense(AIPlayer.Authority) < min)
                    {
                        min = countries[i].GetInfluense(AIPlayer.Authority);
                        ind = i;
                    }
                countries.RemoveAt(ind);
            }


            int n = countries.Count == 1 ? 0 : Random.Range(0, countries.Count);

            AddInfluence(countries[n]);
        }



        // п.8 персональная тактика:
        if (mTargetCountry != null)
        {
            // а может, уже страна наша?
            // а может, атакуем уже 10 лет?
            // выбираем новую цель:
            if (mTargetCountry.Authority == AIPlayer.Authority || GM.CurrentMonth() - mTargetStartMonth >= 10 * 12)
            {
                SetTargetCountry();
                return;
            }

            // шпионов -- всегда
            if (mTargetCountry.CanAddSpy(AIPlayer.Authority) && GM.PayCost(AIPlayer.Authority, GM.SPY_COST))
                mTargetCountry.AddSpy(AIPlayer.Authority, 1);

            if (mAIKind == AI_SCIENTIST)
            {
                if ((GM.CurrentMonth() % 4) == 0) // каждые 4 месяца повышать влияние:
                    AddInfluence(mTargetCountry);
            }

            int mnum = 4; // раз в 4 месяца

            if (mAIKind == AI_SCIENTIST)
                if (mTargetCountry.Authority == Authority.Neutral)
                    mnum = 6;  // раз в полгода для ученых
                else
                    mnum = 12; // раз в год в нейтральных

            if ((GM.CurrentMonth() % mnum) == 0) // каждые N месяцев riot:
                GM.CallMeeting(mTargetCountry, AIPlayer, false);
        }
        else
            SetTargetCountry();


        // защита своей базовой страны ---------------------------
        CountryScript mc = AIPlayer.MyCountry;

        if (((GM.CurrentMonth() + 1) % 3) == 0 && mc.GetInfluense(AIPlayer.Authority) < 50)
            // 1- AI каждые 3 месяца вручную добавляет 1 influence в своей ( СССР-США ) стране если он меньше 50
            AddInfluence(mc);

        // 2- AI каждые 3 месяца вручную добавляет 1 шпиона в свою страну ( СССР-США ) если их меньше 5
        if (((GM.CurrentMonth() + 2) % 3) == 0)
            AddSpyToCountry(mc);

        // 3- AI каждые 6 месяцев делает парады в своей стране ( СССР-США ) если оппозиция 50 и выше
        if (((GM.CurrentMonth() + 4) % 6) == 0 && mc.Support <= 50)
            GM.CallMeeting(mc, AIPlayer, true);


        // НЕЙТРАЛЬНЫЕ страны -- НЕТ: добавлять раз в год в нейтральные страны, в которых начилась война, милитари-пропровительственные войска как помощь


        // пункт 10 будет посылать дополнительные милитари в страны альянса во время боя, что сделает труднее их захватить
        if (Random.Range(0, 12) == 0)
        {
            foreach (CountryScript c in CountryScript.Countries())
                // if( GameEngine.IsOppoInfluBigInNeutral(c) )

            if (c.Authority == AIPlayer.Authority &&    //Страна в альянсе АИ
                    c.OppForce > 0 &&   //в стране война
                    c.GetInfluense(AIPlayer.Authority) > c.GetInfluense(GM.Player.Authority))   //влияние АИ больше
                {
                    AddMilitary(c, NEUTRAL_SUPPORT_AI[mAILevel].values[mAIKind]);
                }
        }

    }

    public void AddMilitary(CountryScript Country, int Amount)
    {
        GameManagerScript GM = GameManagerScript.GM;
        bool playVideo = false;
        bool milType = false;

        for (int i = 1; i <= Amount; i++)
        {
            if (!GM.PayCost(AIPlayer.Authority, GM.MILITARY_COST))
                break; //Не хватило денег

            playVideo = true;
            milType = Country.AddMilitary(AIPlayer.Authority, 1);
        }

        if(playVideo)
            GM.VQueue.AddRolex(GM.GetMySideVideoType(AIPlayer.Authority), VideoQueue.V_PRIO_NULL, milType?VideoQueue.V_PUPPER_MIL_ADDED: VideoQueue.V_PUPPER_REV_ADDED, Country);
    }

    public void AddInfluence(CountryScript Country)
    {
        GameManagerScript GM = GameManagerScript.GM;

        if (!GM.PayCost(AIPlayer.Authority, GM.INFLU_COST))
            return; //Не хватило денег

        Country.AddInfluence(AIPlayer.Authority, 1);
        mInfluCount = GM.CurrentMonth();

        // если увеличивается оппозиция, видео показать:
        if (Country.Authority != AIPlayer.Authority)
            GM.VQueue.AddRolex(AIPlayer.Authority == Authority.Amer ? VideoQueue.V_TYPE_USA : VideoQueue.V_TYPE_USSR, VideoQueue.V_PRIO_NULL, VideoQueue.V_PUPPER_OPPO_INFLU, Country);
    }

    public void AddSpyToCountry(CountryScript Country)
    {
        GameManagerScript GM = GameManagerScript.GM;

        if (Country.CanAddSpy(AIPlayer.Authority) && GM.PayCost(AIPlayer.Authority, GM.SPY_COST))
        {
            Country.AddSpy(AIPlayer.Authority, 1);
            mSpyCount = GM.CurrentMonth();
        }

    }

}
