
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(VideoQueue))]
public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript GM;
    public PlayerScript Player;
    public AI AI;

    private Camera MainCamera;
    private RectTransform DownMenu;
    //private RectTransform UpMenu;
    private RectTransform WarFlagsPanel;
    private CountryScript Country;  //Выбранная в данный момент страна
    public VideoQueue VQueue;  //Видео-очередь

    public GameObject[] Menus;
    [Space(10)]
    public GameObject Marker;    //Маркер указывающий на страну, с которой работаем.
    public GameObject PausePlate;    //Надпись "Pause"
    public RectTransform StatePrefab;   //префаб значка состояния страны
    public RectTransform FlagButtonPrefab;   //префаб флага в правой панели
    [Space(10)]
    public Sprite SignNeutral;
    public Sprite SignUSA;
    public Sprite SignSU;

    int mMonthCount = -1;     // счетчик месяцев с нуля (-1 потому что в первом кадре значение уже увеличивается)
    float TickCount;
    public bool IsPoused;  //игра на паузе

    [Space(10)]
    [Tooltip("время (сек) между итерациями")]
    public float Tick = 6;   //время (сек) между итерациями
    [Tooltip("начальный бюджет игрока")]
    public int START_BUDGET = 300; // начальный бюджет игрока
    [Tooltip("мин.бюджет")]
    public int MIN_BUDGET = 200; // мин.бюджет
    [Tooltip("общее число месяцев игры")]
    public int MAX_MONTHS_NUM = 600; // общее число месяцев игры

    [Tooltip("раз во сколько месяцев можно повышать влияние")]
    public int MAX_INFLU_CLICK = 1;
    [Tooltip("раз во сколько месяцев можно засылать шпиона")]
    public int MAX_SPY_CLICK = 1;
    [Tooltip("раз во сколько месяцев можно поддерживать восстания")]
    public int MAX_RIOT_MONTHS = 3;
    [Tooltip("раз во сколько месяцев можно поддерживать парады")]
    public int MAX_PARAD_MONTHS = 3;

    [Tooltip("ежемесячный рост поддержки")]
    public float SUPPORT_GROW = 0.1f;
    [Tooltip("ежемесячный рост оппозиции")]
    public float OPPO_GROW = 0.1f;
    [Tooltip("необходимый % pro-влияния для смены правительства")]
    public float INSTALL_PUPPER_INFLU = 80;
    [Tooltip("необходимый % оппозиции для смены правительства")]
    public float INSTALL_PUPPER_OPPO = 80;
    [Tooltip("необходимый % оппозиции для ввода революционеров")]
    public float INSTALL_PUPPER_REVOL = 80;
    [Tooltip("стоимость увеличения влияния")]
    public int INFLU_COST = 2;
    [Tooltip("стоимость добавления вооруженных сил")]
    public int MILITARY_COST = 3;
    [Tooltip("стоимость добавления шпиона")]
    public int SPY_COST = 1;
    [Tooltip("стоимость организации парада")]
    public int PARADE_COST = 1;
    [Tooltip("стоимость организации восстания")]
    public int RIOT_COST = 1;
    //Дискаунтер для кризиса при опускании бюджета до 200. Кризис не чаще раза в год.
    int CryzisDiscounter = 0;

    //new
    ClockScript clock;
    bool prevPauseState;

    public void Awake()
    {
        GM = this;
    }

    // Use this for initialization
    void Start()
    {
        if (SettingsScript.Settings.playerSelected == Authority.Amer)
        {
            Player = transform.Find("AmerPlayer").GetComponent<PlayerScript>();
            AI.AIPlayer = transform.Find("SovPlayer").GetComponent<PlayerScript>();
            GameObject.Find("Canvas").GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
        }
        else
        {
            Player = transform.Find("SovPlayer").GetComponent<PlayerScript>();
            AI.AIPlayer = transform.Find("AmerPlayer").GetComponent<PlayerScript>();
            GameObject.Find("Canvas").GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
        }

        Player.ActivateControls(true);

        if (AI != null)
        {
            AI.AIPlayer.ActivateControls(false);
            Player.Budget = AI.START_BUDGET_PLR[SettingsScript.Settings.AIPower];
        }

        MainCamera = FindObjectOfType<Camera>();
        DownMenu = GameObject.Find("DownMenu").GetComponent<RectTransform>();
        WarFlagsPanel = Player.pnlWarFlags.transform.FindChild("Flags").GetComponent<RectTransform>();
        Marker.GetComponent<SpriteRenderer>().sprite = Player.SprMarker;

        MainCamera.GetComponent<CameraScript>().SetNewPosition(Player.MyCountry.Capital);
        VQueue = FindObjectOfType<VideoQueue>();


        //UpMenu = GameObject.Find("UpMenu").GetComponent<RectTransform>();
        //GameObject.Find("VideoLoader").GetComponent<LoadVideoInfo>().LoadInfo();
        clock = FindObjectOfType<ClockScript>();
    }

    void Update()
    {
        //Пауза
        if (Input.GetKeyDown(KeyCode.P))
        {
            IsPoused = !IsPoused;
            PausePlate.SetActive(IsPoused);

        }

        if (IsPoused)
            return;

        TickCount -= Time.deltaTime;
        if (TickCount <= 0)
        {
            TickCount = Tick;

            NextMonth();

            // прошел год?
            if (mMonthCount % 12 == 0 && mMonthCount > 0)
                NewYear();

            //Проверяем на конец игры по времени
            if (mMonthCount >= MAX_MONTHS_NUM)
            {
                StopGame();
                return;
            }

            //Обновление информации в верхнем меню
            ShowHighWinInfo();
            //Обновление информации в нижнем меню
            SnapToCountry();
        }
    }

    public void ToggleTechMenu(GameObject Menu)
    {
        //Если меню активно - выключаем.
        if (Menu.activeSelf)
            Menu.SetActive(false);
        else
        //Если меню не активно - включаем его и выключаем другие меню.
        {
            foreach (var item in Menus)
            {
                item.SetActive(item == Menu);
            }
        }
    }

    public void ToggleGameObject(GameObject GO)
    {
        GO.SetActive(!GO.activeSelf);
    }

    public void LoadScene(string SceneName)
    {
        if (SettingsScript.Settings.playerSelected == Authority.Amer)
            SceneName += "Wiz";
        else
            SceneName += "Dem";

        SceneManager.LoadScene(SceneName);
    }

    //Переход к карте под курсором
    public void SnapToCountry(Vector2 PointerPosition)
    {
        SnapToCountry(MainCamera.ScreenToWorldPoint(PointerPosition) - new Vector3(0, 0, MainCamera.transform.position.z));
    }

    //Переход к карте под маркером
    public void SnapToCountry(Vector3 MarkerPosition)
    {
        Marker.transform.position = MarkerPosition;
        Country = Physics2D.OverlapPoint(MarkerPosition).GetComponent<CountryScript>();
        SnapToCountry();
    }

    //Переход к текущей карте (обновление выводимой информации)
    private void SnapToCountry()
    {
        //Заполнение значений в нижнем меню
        Transform trCNS =  DownMenu.Find("CountryNameScore");
        switch (Country.Authority)
        {
            case Authority.Neutral:
                trCNS.FindChild("Panel").GetComponent<Image>().sprite = SignNeutral;
                break;
            case Authority.Amer:
                trCNS.FindChild("Panel").GetComponent<Image>().sprite = SignUSA;
                break;
            case Authority.Soviet:
                trCNS.FindChild("Panel").GetComponent<Image>().sprite = SignSU;
                break;
        }
        trCNS.FindChild("Panel/Name").GetComponent<Image>().sprite = Country.PicName;
        trCNS.FindChild("Score").GetComponent<Image>().sprite = Country.GetScoreAsSprite();

        FindObjectOfType<uiSupport>().Support = Country.Support;

        DownMenu.Find("Influence/LightInf").GetComponent<Image>().sprite = Resources.Load<Sprite>("Infl/Light/" + Country.AmInf.ToString("f0"));
        DownMenu.Find("Influence/DarkInf").GetComponent<Image>().sprite = Resources.Load<Sprite>("Infl/Dark/" + Country.SovInf.ToString("f0"));

        //DownMenu.Find("SpyLeft").GetComponent<Image>().fillAmount = Country.CIA * 0.2f;
        //DownMenu.Find("SpyRight").GetComponent<Image>().fillAmount = Country.KGB * 0.2f;

        ShowMilitary();

        //Доступность кнопок
        //Влияние
        Player.btnAddInf.GetComponent<Button>().interactable = Country.CanAddInf(Player.Authority);
        //Войска
        Player.btnAddMil.GetComponent<Button>().interactable = Country.CanAddMil(Player.Authority);
        //Шпионы
        Player.btnAddSpy.GetComponent<Button>().interactable = Country.CanAddSpy(Player.Authority);
        //Организация парада
        Player.btnParade.GetComponent<Button>().interactable = Country.CanOrgParade(Player.Authority);
        //Организация восстания
        Player.btnRiot.GetComponent<Button>().interactable = Country.CanOrgMeeting(Player.Authority);
        //Смена правительства
        Player.btnChangeGov.GetComponent<Button>().interactable = Country.CanChangeGov(Player.Authority);
    }

    public void ShowMilitary()
    {
        UnitsDisplay PlayerArmy = Player.ArmyPlate.GetComponent<UnitsDisplay>();
        UnitsDisplay PlayerSpies = Player.SpyPlate.GetComponent<UnitsDisplay>();
        UnitsDisplay OppArmy = AI.AIPlayer.ArmyPlate.GetComponent<UnitsDisplay>();
        UnitsDisplay OppSpies = AI.AIPlayer.SpyPlate.GetComponent<UnitsDisplay>();

        if (Country.Authority == Authority.Neutral)
        {
            if (Player.Authority == Authority.Soviet)
                PlayerArmy.SetAmount(Country.GetForces(Player.Authority), Country.SovInf > Country.AmInf);
            else
                PlayerArmy.SetAmount(Country.GetForces(Player.Authority), Country.SovInf <= Country.AmInf);

            //Армию противника видно, если в стране есть свои шпионы или если это нейтральная армия
            if ((Player.Authority == Authority.Amer && Country.SovInf > Country.AmInf) || (Player.Authority == Authority.Soviet && Country.SovInf <= Country.AmInf))
                OppArmy.SetAmount(Country.GetForces(GetOpponentTo(Player).Authority), true);
            else if (Country.HaveSpy(Player.Authority))
                OppArmy.SetAmount(Country.GetForces(GetOpponentTo(Player).Authority));
            else
                OppArmy.SetAmount(0);
        }
        else
        {
            PlayerArmy.SetAmount(Country.GetForces(Player.Authority));
            //Армию противника видно, если в стране есть свои шпионы
            if (Country.HaveSpy(Player.Authority))
                OppArmy.SetAmount(Country.GetForces(GetOpponentTo(Player).Authority));
            else
                OppArmy.SetAmount(0);
        }

        //Шпионы
        if (Player.Authority == Authority.Soviet)
        {
            PlayerSpies.SetAmount(Country.KGB);
            OppSpies.SetAmount(Country.CIA);
        }
        else
        {
            PlayerSpies.SetAmount(Country.CIA);
            OppSpies.SetAmount(Country.KGB);
        }
    }

    public void AddInfluence()
    {
        if (!PayCost(Player.Authority, INFLU_COST))
            return; //Не хватило денег

        Country.AddInfluence(Player.Authority, 1, false);
        // если увеличивается оппозиция, видео показать:
        if (Country.Authority != Player.Authority)
            VQueue.AddRolex(Player.Authority == Authority.Amer?VideoQueue.V_TYPE_USA: VideoQueue.V_TYPE_USSR, VideoQueue.V_PRIO_NULL, VideoQueue.V_PUPPER_OPPO_INFLU, Country);

        SnapToCountry();
    }

    public void AddSpy()
    {
        if (!PayCost(Player.Authority, SPY_COST))
            return; //Не хватило денег

        Country.AddSpy(Player.Authority, 1);
        SnapToCountry();
    }

    public void AddMilitary()
    {
        if (!PayCost(Player.Authority, MILITARY_COST))
            return; //Не хватило денег

        bool mil = Country.AddMilitary(Player.Authority, 1);
        SnapToCountry();

        VQueue.AddRolex(GetMySideVideoType(Player.Authority), VideoQueue.V_PRIO_NULL, mil?VideoQueue.V_PUPPER_MIL_ADDED:VideoQueue.V_PUPPER_REV_ADDED, Country);
    }

    public void OrganizeRiot()
    {
        if (!CallMeeting(Country, Player, false))
            return;

        SoundManager.SM.PlaySound("sound/riot");
        SnapToCountry();
    }

    public void OrganizeParade()
    {
        if (!CallMeeting(Country, Player, true))
            return;

        SoundManager.SM.PlaySound("sound/parad");
        SnapToCountry();
    }

    //Организация митинга в поддержку правительства или против
    //с - страна, в которой организуем
    //p - игрок, который организует
    //parade: true - парад, false - восстание
    public bool CallMeeting(CountryScript c, PlayerScript p, bool parade)
    {
        if (!PayCost(p, parade? PARADE_COST: RIOT_COST))
            return false; //Не хватило денег

        if (p.Authority == Authority.Amer)
        {
            if (parade)
            {
                c.DiscounterUsaParade = MAX_RIOT_MONTHS;
                c.Support += c.CIA; //увеличиваем поддержку на 1% за каждого шпиона
                if (c.Support > 100) c.Support = 100;
                c.AddState(CountryScript.States.SYM_PARAD, Authority.Amer, 3);
                VQueue.AddRolex(VQueue.LocalType(Authority.Amer), VideoQueue.V_PRIO_NULL, VideoQueue.V_PUPPER_SUPPORT, c);
            }
            else
            {
                c.DiscounterUsaMeeting = MAX_RIOT_MONTHS;
                c.Support -= c.CIA; //увеличиваем оппозицию на 1% за каждого шпиона
                if (c.Support < 0) c.Support = 0;
                c.AddState(CountryScript.States.SYM_RIOT, Authority.Amer, 3);
                VQueue.AddRolex(VQueue.LocalType(Authority.Amer), VideoQueue.V_PRIO_NULL, VideoQueue.V_PUPPER_RIOTS, c);
            }
        }
        else if (p.Authority == Authority.Soviet)   //то же самое за советскую сторону
        {
            if (parade)
            {
                c.DiscounterRusParade = MAX_RIOT_MONTHS;
                c.Support += c.KGB;
                c.AddState(CountryScript.States.SYM_PARAD, Authority.Soviet, 3);
                VQueue.AddRolex(VQueue.LocalType(Authority.Soviet), VideoQueue.V_PRIO_NULL, VideoQueue.V_PUPPER_SUPPORT, c);
            }
            else
            {
                c.DiscounterRusMeeting = MAX_RIOT_MONTHS;
                c.Support -= c.KGB;
                c.AddState(CountryScript.States.SYM_RIOT, Authority.Soviet, 3);
                VQueue.AddRolex(VQueue.LocalType(Authority.Soviet), VideoQueue.V_PRIO_NULL, VideoQueue.V_PUPPER_RIOTS, c);
            }
        }

        if (c.Support < 0)
            c.Support = 0;
        if (c.Support > 100)
            c.Support = 100;

        // проверка разоблачения шпиона, организовавшего акцию
        if (c.CIA > 0 && c.KGB > 0)
        {
            int shot = Random.Range(0, c.CIA + c.KGB);

            if (p.Authority == Authority.Amer)
            {
                if (shot > c.CIA)
                {
                    c.CIA--;
                    c.AddInfluence(Authority.Amer, -2);    //обществу не нравиться когда в их стране орудуют чужие шпионы
                    c.AddState(CountryScript.States.SYM_SPY, Authority.Amer, 3);
                }
            }
            else //проверка разоблачения шпиона игрока за СССР
            {
                if (shot > c.KGB)
                {
                    c.KGB--;
                    c.AddInfluence(Authority.Soviet, -2);  //обществу не нравиться когда в их стране орудуют чужие шпионы
                    c.AddState(CountryScript.States.SYM_SPY, Authority.Soviet, 3);
                }
            }
        }

        return true;
    }

    void Revolution(CountryScript Country)
    {
        Authority NewAut = 0;

        switch (Country.Authority)
        {
            case Authority.Neutral:
                //В нейтральной стране побеждает тот, у кого было меньше влияния
                if (Country.AmInf < Country.SovInf)
                    NewAut = Authority.Amer;
                else
                    NewAut = Authority.Soviet;
                break;
            case Authority.Amer:
                NewAut = Authority.Soviet;
                break;
            case Authority.Soviet:
                NewAut = Authority.Amer;
                break;
        }

        ChangeGovernment(Country, NewAut, true);
    }

    //Смена власти
    //revolution = true - в результате революции, false - мирным способом
    public void ChangeGovernment(CountryScript Country, Authority NewAut, bool revolution = false)
    {
        if (!revolution && !Country.CanChangeGov(NewAut))
            return;

        //Почистить ролики
        VQueue.ClearVideoQueue(Country, VideoQueue.V_PUPPER_REVOLUTION);

        Country.ChangeGov(NewAut);
        SoundManager.SM.PlaySound("sound/cuop");
        VQueue.AddRolex(VQueue.LocalType(Country.Authority), VideoQueue.V_PRIO_NULL, revolution ? VideoQueue.V_PUPPER_WAR : VideoQueue.V_PUPPER_PEACE, Country);

        //Steam achievments
        //Ачивка за дипломатическую смену власти (в любой стране)
#if !myDEBUG
        if (Country.Authority == Player.Authority && !revolution)
            SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_7");
#endif
        //Если в главной стране правительство сменилось, тогда победа нокаутом
        if (Player.MyCountry == Country || Player.OppCountry == Country)
        {
            //Steam achievments
            //Ачивка связанная с переворотом в стране противника (мирным или вооруженным)
#if !myDEBUG
            if (Player.OppCountry.Authority == Player.Authority)
                SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_5");
#endif

            StopGame();
        }
    }

    //обработка нажатия кнопки "NewGovButton"
    public void NewGovernment()
    {
        if (Country.CanChangeGov(Player.Authority))
        {
            ChangeGovernment(Country, Player.Authority, false);
            SnapToCountry();
        }
    }

    //Возвращает tru в случае победы
    public bool CheckGameResult()
    {
        //Проверка победы нокаутом
        if (Player.MyCountry.Authority != Player.Authority)
            return false;

        if(Player.OppCountry.Authority == Player.Authority)
            return true;

        //Если очки равны, проверяем по бюджету
        if (Player.Score == GetOpponentTo(Player).Score)
            return (Player.Budget > GetOpponentTo(Player).Budget);

        //проверка выигрыша по счёту
        return (Player.Score > GetOpponentTo(Player).Score);
    }

    //Ежемесячное обновление информации
    void NextMonth()
    {
        mMonthCount++;
        if (mMonthCount == 0) return;   //первый месяц не считаем

        GameObject Countries = GameObject.Find("Countries");
        CountryScript Country;
        for (int idx = 0; idx < Countries.transform.childCount; idx++)
        {
            Country = Countries.transform.GetChild(idx).GetComponent<CountryScript>();

            Country.NextMonth();

            //Если влияние соответствует правительству, поддержка увеличивается.
            if ((Country.Authority == Authority.Amer && Country.AmInf > 50) || (Country.Authority == Authority.Soviet && Country.SovInf > 50))
            {
                Country.Support += SUPPORT_GROW;
                if (Country.Support > 100) Country.Support = 100;
            }

            //Если влияние не соответствует правительству, растёт оппозиция.
            if ((Country.Authority == Authority.Amer && Country.SovInf > 50) ||
                (Country.Authority == Authority.Soviet && Country.AmInf > 50) ||
                (Country.Authority == Authority.Neutral && (Country.SovInf + Country.AmInf) > 50))
            {
                Country.Support -= OPPO_GROW;
                if (Country.Support < 0) Country.Support = 0;
            }

            //Боевые действия
            if (Country.OppForce > 0)
            {
                int r = Random.Range(0, 100);

                if (r > 33) //r <= 33 - никто не погиб
                {
                    if (Country.GovForce > 0)
                    {
                        if (r > 33 && r < 66)
                            Country.GovForce--;
                        else
                            Country.OppForce--;
                    }

                    if (Country.GovForce == 0)  //революция
                    {
                        SoundManager.SM.PlaySound("sound/thecall");
                        Revolution(Country);
                    }
                    else
                    {
                        VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_NULL, VideoQueue.V_PUPPER_REVOLUTION, Country);
                        //военная помощь AI
                        if(AI != null)
                            AI.InWarSupport(Country);
                    }
                }
            }

            // разборки шпионов раз в год
            TestSpyCombat(Country);

            Country.TestStates();
        }
        //Ход AI
        AI.AIturn();

        //Финансовый кризис при опускании бюджета до 200
        CryzisDiscounter -= 1;
        if (Player.Budget <= 200d && CryzisDiscounter <= 0)
        {
            CountryScript c = Player.MyCountry;
            c.Support -= 50f;
            CryzisDiscounter = 12;
            VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_FINANCE, c);
        }


        //Случайные события
        TestRandomEvent();
    }

    //Ежегодное обновление информации
    void NewYear()
    {
        GameObject.Find("AmerPlayer").GetComponent<PlayerScript>().AnnualGrowthBudget();
        GameObject.Find("SovPlayer").GetComponent<PlayerScript>().AnnualGrowthBudget();

        // + бонус для AI
        if(AI != null)
            AI.AddYearBonus();
    }

    //Окончание игры и показ окна, говорящего об этом.
    void StopGame()
    {
        string SceneName = "";

        if (CheckGameResult())
        {
            SceneName = "WinScreen";

            //Steam achievments
            //Ачивка за первую победу
#if !myDEBUG
            SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_6");
#endif
        }
        else
            SceneName = "LostScreen";

        //проверка выполнения миссий
        if (AI != null && SceneName == "WinScreen")
        {
            //Игра за СССР
            if (Player.Authority == Authority.Soviet)
            {
                //Проверка победы в косической гонке
                if (!SavedSettings.Mission1SU)
                {
                    bool WinSR = true;
                    for (int i = 1; i < SpaceRace.TechCount; i++)
                    {
                        if (!Player.GetTechStatus(i))
                        {
                            WinSR = false;
                            break;
                        }
                    }
                    SavedSettings.Mission1SU = WinSR;
                }
                //Проверка победы с 50 очками или более
                if (!SavedSettings.Mission2SU)
                {
                    SavedSettings.Mission2SU = (Player.Score >= 50 && SettingsScript.Settings.AIPower == 1);
                }
                //Проверка победы с переворотом в стране оппонента
                if (!SavedSettings.Mission3SU)
                {
                    if(Player.OppCountry.Authority == Player.Authority && SettingsScript.Settings.AIPower == 2)
                        {
                        SavedSettings.Mission3SU = true;
                        //Steam achievments
                        //Ачивка за выполнение вссех миссий за СССР
#if !myDEBUG
                        SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_8");
#endif
                    }
                }
            }

            //Игра за США
            if (Player.Authority == Authority.Amer)
            {
                //Проверка победы в косической гонке
                if (!SavedSettings.Mission1USA)
                {
                    bool WinSR = true;
                    for (int i = 1; i < SpaceRace.TechCount; i++)
                    {
                        if (!Player.GetTechStatus(i))
                        {
                            WinSR = false;
                            break;
                        }
                    }
                    SavedSettings.Mission1USA = WinSR;
                }
                //Проверка победы с 50 очками или более
                if (!SavedSettings.Mission2USA)
                {
                    SavedSettings.Mission2USA = (Player.Score >= 50 && SettingsScript.Settings.AIPower == 1);
                }
                //Проверка победы с переворотом в стране оппонента
                if (!SavedSettings.Mission3USA)
                {
                    if (Player.OppCountry.Authority == Player.Authority && SettingsScript.Settings.AIPower == 2)
                    {
                        SavedSettings.Mission3USA = true;
                        //Steam achievments
                        //Ачивка за выполнение вссех миссий за США
#if !myDEBUG
                        SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_9");
#endif
                    }
                }
            }
        }

        LoadScene(SceneName);
    }

    // эмуляция схватки шпионов в стране раз в год.
    public void TestSpyCombat(CountryScript c)
    {
        if (Random.Range(0, 12) != 0) return;

        // случайно раз в год схватки шпионов:
        int r = Random.Range(1, 100);
        if (c.KGB == 0 || c.CIA == 0 || r < 34) return;

        // Если шпион погибает, то influence страны, к которой принадлежал шпион, 
        // понижается на 2% ( обществу не нравится когда в их стране орудуют чужие шпионы ). 
        if (r < 67)
        {
            c.KGB--;
            c.AddInfluence(Authority.Soviet, -2);
            
        }
        else
        {
            c.CIA--;
            c.AddInfluence(Authority.Amer, -2);
        }

        c.AddState(CountryScript.States.SYM_SPY, Authority.Amer, 3);
    }

    // проверка случайного события раз в год
    void TestRandomEvent()
    {
        if (Random.Range(0, 12) != 1)
            return;

        //Выбор страны, в которой произойдёт случайное событие
        Transform TCountries = GameObject.Find("Countries").transform;
        int cn = Random.Range(0, TCountries.childCount - 1);
        CountryScript c = TCountries.GetChild(cn).GetComponent<CountryScript>();
        
        int n = Random.Range(0, 7);
        switch (n)
        {
            case 0: // Наводнение ( повышается оппозиция + 10 )
                c.Support -= 10f;
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_FLOOD, c);
                break;

            case 1: // Индустриализация ( повышается value страны + 1 )
                c.Score++;
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_INDUSTR, c);
                break;

            case 2: // Нобелевский лауреат  ( повышается support на +20 )
                c.Support += 20f;
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_NOBEL, c);
                break;

            case 3: // Финансовый кризис ( повышается оппозиция на + 50 )
                c.Support -= 50f;
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_FINANCE, c);
                break;

            case 4: // Политический кризис ( повышается оппозиция на +25 )
                c.Support -= 25f;
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_POLITIC, c);
                break;

            case 5: // Национализм ( повышается нейтральность на + 30 )
                if (c.NInf >= 99) return;
                c.AddInfluence(Authority.Neutral, 30);
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_NAZI, c);
                break;

            case 6: // Коммунистическое движение ( советский influence + 30 )
                c.AddInfluence(Authority.Soviet, 30);
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_COMMI, c);
                break;

            case 7: // Демократическое движение ( американский influence + 30 )
                c.AddInfluence(Authority.Amer, 30);
                VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_PRESSING, VideoQueue.V_PUPPER_EVENT_DEMOCR, c);
                break;
        }
    }
    
    //Обновление информации в верхнем меню
    void ShowHighWinInfo()
    {
        //string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        //int m = mMonthCount % 12;
        //int y = mMonthCount / 12;
        //string CurrentDate = months[m] + " " + (1950 + y);

        Player.pnlStates.Find("Score").GetComponent<Text>().text = Player.Score.ToString("f0");
        Player.pnlStates.Find("Budget").GetComponent<Text>().text = Player.Budget.ToString("f0");
        AI.AIPlayer.pnlStates.Find("Score").GetComponent<Text>().text = AI.AIPlayer.Score.ToString("f0");
        AI.AIPlayer.pnlStates.Find("Budget").GetComponent<Text>().text = AI.AIPlayer.Budget.ToString("f0");

        clock.ShowDate(CurrentMonth());
        //UpMenu.Find("Date").GetComponent<Text>().text = CurrentDate;
        //PlayerScript AmerPlayer = GameObject.Find("AmerPlayer").GetComponent<PlayerScript>();
        //PlayerScript SovPlayer = GameObject.Find("SovPlayer").GetComponent<PlayerScript>();

        //UpMenu.Find("StatesRight/Score").GetComponent<Text>().text = SovPlayer.Score.ToString("f0");
        //UpMenu.Find("StatesRight/Budget").GetComponent<Text>().text = SovPlayer.Budget.ToString("f0");
        //UpMenu.Find("StatesLeft/Score").GetComponent<Text>().text = AmerPlayer.Score.ToString("f0");
        //UpMenu.Find("StatesLeft/Budget").GetComponent<Text>().text = AmerPlayer.Budget.ToString("f0");
        //UpMenu.Find("USScore").GetComponent<Text>().text = GameObject.Find("AmerPlayer").GetComponent<PlayerScript>().Score.ToString("f0");
        //UpMenu.Find("USBudget").GetComponent<Text>().text = GameObject.Find("AmerPlayer").GetComponent<PlayerScript>().Budget.ToString("f0");
        //UpMenu.Find("SovScore").GetComponent<Text>().text = GameObject.Find("SovPlayer").GetComponent<PlayerScript>().Score.ToString("f0");
        //UpMenu.Find("SovBudget").GetComponent<Text>().text = GameObject.Find("SovPlayer").GetComponent<PlayerScript>().Budget.ToString("f0");
    }

    public bool PayCost(Authority Aut, int Money)
    {
        if(Aut == Authority.Neutral)
            return false;

        PlayerScript Player = null;

        switch (Aut)
        {
            case Authority.Amer:
                Player = transform.FindChild("AmerPlayer").GetComponent<PlayerScript>();
                break;
            case Authority.Soviet:
                Player = transform.FindChild("SovPlayer").GetComponent<PlayerScript>();
                break;
        }

        if (Player.Budget - Money < MIN_BUDGET)
            return false;

        Player.Budget -= Money;
        ShowHighWinInfo();

        return true;
    }

    public bool PayCost(PlayerScript Player, int Money)
    {
        if (Player.Budget - Money < MIN_BUDGET)
            return false;

        Player.Budget -= Money;
        ShowHighWinInfo();

        return true;
    }

    // текущая эпоха (тег видеоролика)
    public int GetCurrentEpoch()
    {
        if (mMonthCount <= 20 * 12)
            return 1;
        else
            return 2;
    }

    // принадлежит ли эпоха (тег видеоролика) текущей эпохе
    // 1- 1950-1970, 2- 1970-2000 0-не проверяем
    internal bool IsCurrentEpoch(int epoch)
    {
        return (epoch == 0 || epoch == GetCurrentEpoch());
    }

    public CountryScript FindCountryById(int Id)
    {
        GameObject GoCountry = GameObject.Find("C (" + Id.ToString() + ")");
        if (GoCountry != null)
            return GoCountry.GetComponent<CountryScript>();
        else
            return null;
    }

    public int CurrentMonth()
    {
        return mMonthCount;
    }

    //Установка текста новости и страны в нижнем меню.
    public void SetInfo(string InfoText, CountryScript Country = null)
    {
        Player.pnlInfo.transform.FindChild("Text").GetComponent<Text>().text = InfoText;
        Image tmpImage = Player.pnlInfo.transform.FindChild("CountryChar").GetComponent<Image>();
        Text tmpCntrName = Player.pnlInfo.transform.FindChild("CountryName").GetComponent<Text>();
        if (Country != null)
        {
            tmpCntrName.text = Country.Name;
            tmpImage.sprite = Country.GetCountryChar();
            tmpImage.enabled = true;
        }
        else
        {
            tmpCntrName.text = "";
            tmpImage.sprite = null;
            tmpImage.enabled = false;
        }
    }

    // определить локальный тип видеоролика
    public int GetMySideVideoType(Authority aut)
    {
        return aut == Authority.Amer ? VideoQueue.V_TYPE_USA : VideoQueue.V_TYPE_USSR;
    }

    //Добавление флага на правую панель, где показываются флаги стран, в которых идёт война
    public void AddWarFlag(CountryScript Country)
    {
        //Если флаг этой страны уже есть в списке, второй раз не добавляем
        for (int i = 0; i < WarFlagsPanel.childCount; i++)
        {
            if (WarFlagsPanel.GetChild(i).GetComponent<FlagButton>().Country == Country)
                return;
        }

        RectTransform fb = Instantiate<RectTransform>(FlagButtonPrefab);
        fb.SetParent(WarFlagsPanel);
        fb.localScale = Vector3.one;
        ////Новый флаг должен появиться вверху списка
        //fb.SetAsFirstSibling();
        //Установка страны
        fb.GetComponent<FlagButton>().Country = Country;
    }

    //Удаление флага из правой панели
    public void RemoveWarFlag(CountryScript Country)
    {
        for (int i = 0; i < WarFlagsPanel.childCount; i++)
        {
            if (WarFlagsPanel.GetChild(i).GetComponent<FlagButton>().Country == Country)
                Destroy(WarFlagsPanel.GetChild(i).gameObject);
        }
    }

    //Определение оппонента
    public PlayerScript GetOpponentTo(PlayerScript pl)
    {
        PlayerScript AmP = transform.Find("AmerPlayer").GetComponent<PlayerScript>();
        PlayerScript SovP = transform.Find("SovPlayer").GetComponent<PlayerScript>();
        PlayerScript retValue = null;

        if (pl == AmP)
            retValue = SovP;
        if (pl == SovP)
            retValue = AmP;

        return retValue;
    }

    // повысить влияние в странах при открытии технологий
    //govType - в каких странах повышаем влияние (нейтральная -- глобально)
    //Aut - чьё влияние повышаем
    //proc - величина повышения
    public void AddInfluenceInCountries(Authority govType, Authority Aut, int proc)
    {
        GameObject Countries = GameObject.Find("Countries");
        CountryScript c;
        for (int idx = 0; idx < Countries.transform.childCount; idx++)
        {
            c = Countries.transform.GetChild(idx).GetComponent<CountryScript>();
            if (c.Authority == govType || govType == Authority.Neutral)
            {
                c.AddInfluence(Aut, proc);
                //Если повысилось влияние страны, которая отображается в нижнем меню, обновляем отображение
                if (c == Country)
                    SnapToCountry();
            }
        }
    }

    public void ExitMenu(bool st)
    {
        if (st)
        {
            prevPauseState = IsPoused;
            IsPoused = true;
        }
        else
            IsPoused = prevPauseState;
    }
}

public enum Region
{
    USSR = 1,
    USA,
    Europe,
    Asia,
    Other
}

public enum Authority
{
    Neutral,
    Amer,
    Soviet
}

