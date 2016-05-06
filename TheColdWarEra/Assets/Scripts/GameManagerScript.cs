using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour {
    public static GameManagerScript GM;
    public PlayerScript Player;

    private Camera MainCamera;
    private RectTransform DownMenu;
    private RectTransform UpMenu;
    private CountryScript Country;  //Выбранная в данный момент страна

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
    void Start () {
        GM = this;
        MainCamera = FindObjectOfType<Camera>();
        DownMenu = GameObject.Find("DownMenu").GetComponent<RectTransform>();
        UpMenu = GameObject.Find("UpMenu").GetComponent<RectTransform>();
        Marker.GetComponent<SpriteRenderer>().sprite = Player.GetComponent<PlayerScript>().SprMarker;

        MainCamera.GetComponent<CameraScript>().SetNewPosition(Player.MyCountry.GetComponent<CountryScript>().Capital);
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

    public void LoadScene(string SceneName) {
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
        DownMenu.Find("Support").GetComponent<Text>().text = Country.Support.ToString();
        DownMenu.Find("Riots").GetComponent<Text>().text = (100 - Country.Support).ToString();
        DownMenu.Find("Budget").GetComponent<Text>().text = Player.Budget.ToString();
        DownMenu.Find("InfAmer").GetComponent<Text>().text = Country.AmInf.ToString();
        DownMenu.Find("InfNeutral").GetComponent<Text>().text = Country.NInf.ToString();
        DownMenu.Find("InfSoviet").GetComponent<Text>().text = Country.SovInf.ToString();

        DownMenu.Find("SpyLeft").GetComponent<Image>().fillAmount = Country.CIA * 0.2f;
        DownMenu.Find("SpyRight").GetComponent<Image>().fillAmount = Country.KGB * 0.2f;

        ShowMilitary();
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
                    DownMenu.Find("MilitaryLeft").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                else
                {
                    DownMenu.Find("MilitaryLeft_n").GetComponent<Image>().fillAmount = Country.GovForce * 0.1f;
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
        Country.AddInfluence(Player.Authority, 1);

        DownMenu.Find("InfAmer").GetComponent<Text>().text = Country.AmInf.ToString();
        DownMenu.Find("InfNeutral").GetComponent<Text>().text = Country.NInf.ToString();
        DownMenu.Find("InfSoviet").GetComponent<Text>().text = Country.SovInf.ToString();
    }

    public void AddSpy()
    {
        Country.AddSpy(Player.Authority, 1);
        DownMenu.Find("SpyLeft").GetComponent<Image>().fillAmount = Country.CIA * 0.2f;
        DownMenu.Find("SpyRight").GetComponent<Image>().fillAmount = Country.KGB * 0.2f;
    }

    public void AddMilitary()
    {
        Country.AddMilitary(Player.Authority, 1);
        ShowMilitary();
    }

    //Окончание игры и показ окна, говорящего об этом.
    void StopGame()
    {
    }

    //Ежемесячное обновление информации
    void NextMonth()
    {
        mMonthCount++;

        GameObject Countries = GameObject.Find("Countries");
        for (int idx = 0; idx < Countries.transform.childCount; idx++)
        {
            CountryScript Country = Countries.transform.GetChild(idx).GetComponent<CountryScript>();
            //Если влияние соответствует правительству, поддержка увеличивается.
            if ((Country.Authority == Authority.Amer && Country.AmInf > 50) || (Country.Authority == Authority.Soviet && Country.SovInf > 50))
            {
                Country.Support += SUPPORT_GROW;
                Country.Support = Mathf.Max(Country.Support, 100f);
            }

            //Если влияние не соответствует правительству, растёт оппозиция.
            if ((Country.Authority == Authority.Amer && Country.SovInf > 50) ||
                (Country.Authority == Authority.Soviet && Country.AmInf > 50) ||
                (Country.Authority == Authority.Neutral && (Country.SovInf + Country.AmInf) > 50))
            {
                Country.Support -= OPPO_GROW;
                Country.Support = Mathf.Min(Country.Support, 0);
            }

            //Боевые действия
            if (Country.OppForce > 0)
            {
                int r = Random.Range(0, 100);
                if (r < 33)
                    continue;   //ничего не произошло

                if (Country.GovForce > 0)
                {
                    if (r < 66)
                        Country.GovForce--;
                    else
                        Country.OppForce--;
                }

                if (Country.GovForce == 0)  //революция
                {
                    Country.GovForce = Country.OppForce;
                    Country.OppForce = 0;

                    ChangeGovernment();
                }

            }
        }
    }

    //Ежегодное обновление информации
    void NewYear()
    {
    }

    //Смена власти
    //
    void ChangeGovernment()
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
        UpMenu.Find("USScore").GetComponent<Text>().text = GameObject.Find("AmerPlayer").GetComponent<PlayerScript>().Score.ToString();
        UpMenu.Find("USBudget").GetComponent<Text>().text = GameObject.Find("AmerPlayer").GetComponent<PlayerScript>().Budget.ToString();
        UpMenu.Find("SovScore").GetComponent<Text>().text = GameObject.Find("SovPlayer").GetComponent<PlayerScript>().Score.ToString();
        UpMenu.Find("SovBudget").GetComponent<Text>().text = GameObject.Find("SovPlayer").GetComponent<PlayerScript>().Budget.ToString();
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

