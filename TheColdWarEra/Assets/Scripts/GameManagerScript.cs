using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(VideoQueue))]
public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript GM;
    public PlayerScript Player;

    private Camera MainCamera;
    private RectTransform DownMenu;
    private RectTransform UpMenu;
    private CountryScript Country;  //Выбранная в данный момент страна
    private VideoQueue VQueue;  //Видео-очередь

    public GameObject[] Menus;
    public RectTransform StatLists;
    [Space(10)]
    public GameObject Marker;    //Маркер указывающий на страну, с которой работаем.
    public GameObject PausePlate;    //Надпись "Pause"
    [Space(10)]
    public Sprite SignUSA;
    public Sprite SignSU;

    int mMonthCount = -1;     // счетчик месяцев с нуля (-1 потому что в первом кадре значение уже увеличивается)
    float TickCount;
    bool IsPoused;  //игра на паузе

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

    // Use this for initialization
    void Start()
    {
        GM = this;
        MainCamera = FindObjectOfType<Camera>();
        DownMenu = GameObject.Find("DownMenu").GetComponent<RectTransform>();
        UpMenu = GameObject.Find("UpMenu").GetComponent<RectTransform>();
        Marker.GetComponent<SpriteRenderer>().sprite = Player.GetComponent<PlayerScript>().SprMarker;

        MainCamera.GetComponent<CameraScript>().SetNewPosition(Player.MyCountry.GetComponent<CountryScript>().Capital);
        VQueue = FindObjectOfType<VideoQueue>();

        //GameObject.Find("VideoLoader").GetComponent<LoadVideoInfo>().LoadInfo();
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

            //Проверяем на конец игры по времени
            if (mMonthCount > MAX_MONTHS_NUM)
            {
                StopGame();
                return;
            }

            NextMonth();

            // прошел год?
            if (mMonthCount % 12 == 0)
                NewYear();

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

    public void ScrollStatMenu(int dt)
    {
        Vector3 NewPos = StatLists.localPosition + Vector3.up * dt;

        if (NewPos.y < 0)
            NewPos -= Vector3.up * NewPos.y;

        float maxY = StatLists.rect.height - ((RectTransform)StatLists.parent).rect.height;
        if (NewPos.y > maxY)
            NewPos -= Vector3.up * (NewPos.y - maxY);

        StatLists.localPosition = NewPos;
    }

    public void LoadScene(string SceneName)
    {
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
        DownMenu.Find("Flag").GetComponent<Image>().sprite = Country.Authority == Authority.Soviet ? Country.FlagS : Country.FlagNs;
        DownMenu.Find("Score").GetComponent<Text>().text = Country.Score + " score";
        DownMenu.Find("Sign").GetComponent<Image>().enabled = (Country.Authority != Authority.Neutral);
        string CountryState = "";
        switch (Country.Authority)
        {
            case Authority.Neutral:
                CountryState = "NEUTRAL";
                break;
            case Authority.Amer:
                CountryState = "AMERICAN";
                DownMenu.Find("Sign").GetComponent<Image>().sprite = SignUSA;
                break;
            case Authority.Soviet:
                CountryState = "SOVIET";
                DownMenu.Find("Sign").GetComponent<Image>().sprite = SignSU;
                break;
        }
        DownMenu.Find("CountryState").GetComponent<Text>().text = Country.Name + ": GOVERNMENT - PRO " + CountryState;
        DownMenu.Find("Support").GetComponent<Text>().text = Country.Support.ToString("g3");
        DownMenu.Find("Riots").GetComponent<Text>().text = (100 - Country.Support).ToString("g3");
        DownMenu.Find("Budget").GetComponent<Text>().text = Player.Budget.ToString("f0");
        DownMenu.Find("InfAmer").GetComponent<Text>().text = Country.AmInf.ToString("f0");
        DownMenu.Find("InfNeutral").GetComponent<Text>().text = Country.NInf.ToString("f0");
        DownMenu.Find("InfSoviet").GetComponent<Text>().text = Country.SovInf.ToString("f0");

        DownMenu.Find("SpyLeft").GetComponent<Image>().fillAmount = Country.CIA * 0.2f;
        DownMenu.Find("SpyRight").GetComponent<Image>().fillAmount = Country.KGB * 0.2f;

        ShowMilitary();

        //Доступность кнопок
        //Влияние
        DownMenu.Find("AddInfButton").GetComponent<Button>().interactable = Country.CanAddInf(Player.Authority);
        //Войска
        DownMenu.Find("AddMilButton").GetComponent<Button>().interactable = Country.CanAddMil(Player.Authority);
        //Шпионы
        DownMenu.Find("AddSpyButton").GetComponent<Button>().interactable = Country.CanAddSpy(Player.Authority);
        //Организация парада
        DownMenu.Find("SupParadeButton").GetComponent<Button>().interactable = Country.CanOrgParade(Player.Authority);
        //Организация восстания
        DownMenu.Find("SupRiotButton").GetComponent<Button>().interactable = Country.CanOrgMeeting(Player.Authority);
        //Смена правительства
        DownMenu.Find("NewGovButton").GetComponent<Button>().interactable = Country.CanChangeGov(Player.Authority);
    }

    public void ShowMilitary()
    {
        DownMenu.Find("MilitaryLeft").GetComponent<Image>().fillAmount = 0;
        DownMenu.Find("MilitaryLeft_n").GetComponent<Image>().fillAmount = 0;
        DownMenu.Find("MilitaryRight_n").GetComponent<Image>().fillAmount = 0;
        DownMenu.Find("MilitaryRight").GetComponent<Image>().fillAmount = 0;

        switch (Country.Authority)
        {
            case Authority.Neutral:
                if (Country.SovInf > Country.AmInf)
                {
                    DownMenu.Find("MilitaryRight_n").GetComponent<Image>().fillAmount = Country.GovForce * 0.1f;
                    //Силы оппозиции видны если есть шпионы либо если оппозиция - своя (в данном случае - американская).
                    if (Player.Authority == Authority.Amer || Country.KGB > 0)
                        DownMenu.Find("MilitaryLeft").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                else
                {
                    DownMenu.Find("MilitaryLeft_n").GetComponent<Image>().fillAmount = Country.GovForce * 0.1f;
                    //Силы оппозиции видны если есть шпионы либо если оппозиция - своя (в данном случае - советсткая).
                    if (Player.Authority == Authority.Soviet || Country.CIA > 0)
                        DownMenu.Find("MilitaryRight").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                break;
            case Authority.Amer:
                DownMenu.Find("MilitaryLeft").GetComponent<Image>().fillAmount = Country.GovForce * 0.1f;
                if (Player.Authority == Authority.Amer)
                {
                    //Если нет шпионов, то силы противника не видны
                    if (Country.CIA > 0)
                        DownMenu.Find("MilitaryRight").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                else
                {
                    DownMenu.Find("MilitaryRight").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                break;
            case Authority.Soviet:
                DownMenu.Find("MilitaryRight").GetComponent<Image>().fillAmount = Country.GovForce * 0.1f;
                if (Player.Authority == Authority.Soviet)
                {
                    //Если нет шпионов, то силы противника не видны
                    if (Country.KGB > 0)
                        DownMenu.Find("MilitaryLeft").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                else
                {
                    DownMenu.Find("MilitaryLeft").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                break;
        }
    }

    public void AddInfluence()
    {
        if (!PayCost(Player.Authority, INFLU_COST))
            return; //Не хватило денег

        Country.AddInfluence(Player.Authority, 1);

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

        Country.AddMilitary(Player.Authority, 1);
        SnapToCountry();

        VQueue.AddRolex(GetMySideVideoType(), VideoQueue.V_PRIO_NULL, VideoQueue.V_PUPPER_MIL_ADDED, Country, true, mMonthCount);
    }

    public void OrganizeMeeting()
    {
        if (!PayCost(Player.Authority, RIOT_COST))
            return; //Не хватило денег

    }

    public void OrganizeParade()
    {
        if (!PayCost(Player.Authority, PARADE_COST))
            return; //Не хватило денег

    }

    //Смена власти
    //
    public void ChangeGovernment()
    {
    }

    //Ежемесячное обновление информации
    void NextMonth()
    {
        mMonthCount++;
        if (mMonthCount == 0) return;   //перый месяц не считаем

        GameObject Countries = GameObject.Find("Countries");
        for (int idx = 0; idx < Countries.transform.childCount; idx++)
        {
            CountryScript Country = Countries.transform.GetChild(idx).GetComponent<CountryScript>();

            //Уменьшаем дискаунтеры
            if (Country.DiscounterRusInfl > 0) Country.DiscounterRusInfl--;
            if (Country.DiscounterRusMeeting > 0) Country.DiscounterRusMeeting--;
            if (Country.DiscounterRusParade > 0) Country.DiscounterRusParade--;
            if (Country.DiscounterRusSpy > 0) Country.DiscounterRusSpy--;
            if (Country.DiscounterUsaInfl > 0) Country.DiscounterUsaInfl--;
            if (Country.DiscounterUsaMeeting > 0) Country.DiscounterUsaMeeting--;
            if (Country.DiscounterUsaParade > 0) Country.DiscounterUsaParade--;
            if (Country.DiscounterUsaSpy > 0) Country.DiscounterUsaSpy--;

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

            ////Боевые действия
            //if (Country.OppForce > 0)
            //{
            //    int r = Random.Range(0, 100);
            //    if (r < 33)
            //        continue;   //ничего не произошло

            //    if (Country.GovForce > 0)
            //    {
            //        if (r < 66)
            //            Country.GovForce--;
            //        else
            //            Country.OppForce--;
            //    }

            //    if (Country.GovForce == 0)  //революция
            //    {
            //        Country.GovForce = Country.OppForce;
            //        Country.OppForce = 0;

            //        ChangeGovernment();
            //    }

            //}
        }
    }

    //Ежегодное обновление информации
    void NewYear()
    {
    }

    //Окончание игры и показ окна, говорящего об этом.
    void StopGame()
    {
    }

    //Обновление информации в верхнем меню
    void ShowHighWinInfo()
    {
        string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        int m = mMonthCount % 12;
        int y = mMonthCount / 12;
        string CurrentDate = months[m] + " " + (1950 + y);

        UpMenu.Find("Date").GetComponent<Text>().text = CurrentDate;
        UpMenu.Find("USScore").GetComponent<Text>().text = GameObject.Find("AmerPlayer").GetComponent<PlayerScript>().Score.ToString("f0");
        UpMenu.Find("USBudget").GetComponent<Text>().text = GameObject.Find("AmerPlayer").GetComponent<PlayerScript>().Budget.ToString("f0");
        UpMenu.Find("SovScore").GetComponent<Text>().text = GameObject.Find("SovPlayer").GetComponent<PlayerScript>().Score.ToString("f0");
        UpMenu.Find("SovBudget").GetComponent<Text>().text = GameObject.Find("SovPlayer").GetComponent<PlayerScript>().Budget.ToString("f0");
    }

    public bool PayCost(Authority Aut, float Money)
    {
        PlayerScript Player;

        switch (Aut)
        {
            case Authority.Amer:
                Player = transform.FindChild("AmerPlayer").GetComponent<PlayerScript>();
                break;
            case Authority.Soviet:
                Player = transform.FindChild("SovPlayer").GetComponent<PlayerScript>();
                break;
            default:
                return false;
                break;
        }

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
    public void SetInfo(string InfoText, string CountryName = "")
    {
        DownMenu.Find("Info").GetComponent<Text>().text = InfoText;
        DownMenu.Find("InfoCountry").GetComponent<Text>().text = CountryName;
    }

    // определить локальный тип видеоролика
    public int GetMySideVideoType()
    {
        return Player.Authority == Authority.Amer ? VideoQueue.V_TYPE_USA : VideoQueue.V_TYPE_USSR;
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

