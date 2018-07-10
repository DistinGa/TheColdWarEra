
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpaceRace : MonoBehaviour
{
    const string Builded = "Background", notAvailable = "Background_notbuild", available = "Background_next";
    public static SpaceRace Instance;

    public Text txtName;
    public Text Description;
    public Text txtInf;
    public Text Price;
    [Space(10)]
    public Transform Heads;
    public Sprite HeadLight, HeadDark;
    public GameObject LightScreen, DarkScreen;
    [Space(10)]

    public Sprite sprLocInf;    //Плашка с надписью "increase alliance influence"
    public Sprite sprGlobInf;   //Плашка с надписью "increase global influence"

    //public Sprite BlueIcon;
    //public Sprite RedIcon;
    //public Sprite GreenIcon;
    //public Sprite BlueIconOp;
    //public Sprite RedIconOp;
    //public Sprite GreenIconOp;

    public const int TechCount = 41;

    int CurTechIndex;
    bool initPouseState;
    Button LaunchButton;
    Transform curToggles = null;

    [SerializeField]
    Technology[] Techs = new Technology[TechCount]; //элементов в массиве на 1 больше, чем технологий, чтобы была возможность нумеровать их с единицы (чтобы использовать исходный код с минимальными изменениями)

    public Transform CurToggles
    {
        get
        {
            if (curToggles == null)
                Start();

            return curToggles;
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (SettingsScript.Settings.playerSelected == Authority.Amer)
        {
            LightScreen.SetActive(true);
            DarkScreen.SetActive(false);
            LaunchButton = LightScreen.transform.Find("LaunchButtonPlate/btnLaunch").GetComponent<Button>();
        }
        else
        {
            LightScreen.SetActive(false);
            DarkScreen.SetActive(true);
            LaunchButton = DarkScreen.transform.Find("LaunchButtonPlate/btnLaunch").GetComponent<Button>();
        }

        curToggles = GetComponentInChildren<ToggleGroup>().transform;
        curToggles.Find("Toggle1").GetComponent<Toggle>().isOn = true;
    }

    public void OnEnable()
    {
        initPouseState = GameManagerScript.GM.IsPoused;
        GameManagerScript.GM.IsPoused = true;
        CameraScript.Camera.setOverMenu = true;

        PaintTechButtons();
        //transform.Find("Toggles/Toggle1").GetComponent<Toggle>().isOn = true;
    }

    public void OnDisable()
    {
        GameManagerScript.GM.IsPoused = initPouseState;
        CameraScript.Camera.setOverMenu = false;
    }

    //Установка соответствующих спрайтов на кнопки технологий
    private void PaintTechButtons()
    {
        Image BackImage;
        GameManagerScript GM = GameManagerScript.GM;

        Image tmpHead;
        for (int i = 1; i < 41; i++)
        {
            tmpHead = Heads.GetChild(i - 1).GetComponent<Image>();
            tmpHead.enabled = GM.GetOpponentTo(GM.Player).GetTechStatus(i);
            if (GM.GetOpponentTo(GM.Player).GetTechStatus(i))
                tmpHead.sprite = SettingsScript.Settings.playerSelected == Authority.Amer ? HeadDark : HeadLight;  //технология открыта оппонентом

            BackImage = CurToggles.Find("Toggle" + i.ToString()).GetComponent<Toggle>().targetGraphic.GetComponent<Image>();
            if (GM.Player.GetTechStatus(i))
            {//технология открыта
                //BackImage.sprite = transform.Find("Toggles/Toggle" + i.ToString() + Builded).GetComponent<Image>().sprite;
                BackImage.sprite = BackImage.transform.Find(Builded).GetComponent<Image>().sprite;

                //if (GM.GetOpponentTo(GM.Player).GetTechStatus(i))
                //    BackImage.sprite = BlueIconOp;  //технология открыта оппонентом
            }
            else
            {
                if (GM.Player.GetTechStatus(Techs[i].mPrevTechNumber))
                {//технология доступна
                    //BackImage.sprite = transform.Find("Toggles/Toggle" + i.ToString() + available).GetComponent<Image>().sprite;
                    BackImage.sprite = BackImage.transform.Find(available).GetComponent<Image>().sprite;

                    //if (GM.GetOpponentTo(GM.Player).GetTechStatus(i))
                    //    BackImage.sprite = GreenIconOp;  //технология открыта оппонентом
                }
                else
                {//технология недоступна
                    //BackImage.sprite = transform.Find("Toggles/Toggle" + i.ToString() + notAvailable).GetComponent<Image>().sprite;
                    BackImage.sprite = BackImage.transform.Find(notAvailable).GetComponent<Image>().sprite;

                    //if (GM.GetOpponentTo(GM.Player).GetTechStatus(i))
                    //    BackImage.sprite = RedIconOp;  //технология открыта оппонентом
                }
            }
        }
    }

    //Заполнение отображаемых полей при нажатии кнопки технологии
    public void ShowTechInfo(int ind)
    {
        GameManagerScript GM = GameManagerScript.GM;
        CurTechIndex = ind;
        SoundManager.SM.PlaySound("sound/click2");

        //Кнопка запуска доступна, если выбранная технология ещё не открыта и доступна
        LaunchButton.interactable = (!GM.Player.GetTechStatus(CurTechIndex) && GM.Player.GetTechStatus(Techs[CurTechIndex].mPrevTechNumber));

        if (GM.Player.Authority == Authority.Amer)
        {
            txtName.text = Techs[ind].mUsaTechName;
            Description.text = Techs[ind].mUsaDescr;
        }
        else
        {
            txtName.text = Techs[ind].mRusTechName;
            Description.text = Techs[ind].mRusDescr;
        }

        Price.text = "Cost: " + Techs[ind].mCost.ToString() + " gold";

        string strInf;
        //Технологии из нижнего ряда добавляют влияние только в союзных странах, технологии из вертикальных веток - во всех странах.
        if (ind < 16)
        {
            //Нижний ряд
            if (SettingsScript.Settings.playerSelected == Authority.Amer)
                strInf = "Good locations influence +";
            else
                strInf = "Evil locations influence +";
            //Если технология не открыта оппонентом, показываем прибавку влияния при первом открытии
            if(!GM.GetOpponentTo(GM.Player).GetTechStatus(ind))
                txtInf.text = strInf + Techs[ind].mLocalInfl_1.ToString();
            else
                txtInf.text = strInf + Techs[ind].mLocalInfl.ToString();
        }
        else
        {
            //Вертикальные ветки технологий
            strInf = "All locations influence +";

            //Если технология не открыта оппонентом, показываем прибавку влияния при первом открытии
            if (!GM.GetOpponentTo(GM.Player).GetTechStatus(ind))
                txtInf.text = strInf + Techs[ind].mGlobalInfl_1.ToString();
            else
                txtInf.text = strInf + Techs[ind].mGlobalInfl.ToString();
        }
    }

    public void SetTechno(int TechInd, string mUsaDescr, string mRusDescr, int mCost, int mLocalInfl, int mGlobalInfl, int mLocalInfl_1, int mGlobalInfl_1)
    {
        Techs[TechInd].mUsaDescr = mUsaDescr;
        Techs[TechInd].mRusDescr = mRusDescr;
        Techs[TechInd].mCost = mCost;
        Techs[TechInd].mLocalInfl = mLocalInfl;
        Techs[TechInd].mGlobalInfl = mGlobalInfl;
        Techs[TechInd].mLocalInfl_1 = mLocalInfl_1;
        Techs[TechInd].mGlobalInfl_1 = mGlobalInfl_1;
    }

    //Запуск технологии по кнопке
    public void Launch()
    {
        GameManagerScript GM = GameManagerScript.GM;
        if (LaunchTech(GM.Player, CurTechIndex))
        {
            SoundManager.SM.PlaySound("sound/launch");
            PaintTechButtons();
            //Кнопка запуска доступна, если выбранная технология ещё не открыта и доступна
            LaunchButton.interactable = (!GM.Player.GetTechStatus(CurTechIndex) && GM.Player.GetTechStatus(Techs[CurTechIndex].mPrevTechNumber));
        }
    }

    //Запуск технологии
    //Player - игрок, запускающий технологию
    //TechInd - индекс технологии
    public bool LaunchTech(PlayerScript Player, int TechInd)
    {
        GameManagerScript GM = GameManagerScript.GM;
        //Если технология недоступна, запрещаем её запуск
        if (!Player.GetTechStatus(Techs[TechInd].mPrevTechNumber))
            return false;

        //Если не хватает денег, запрет
        if (!GM.PayCost(Player, Techs[TechInd].mCost))
            return false;

        int InfAmount = 0;
        //Если игрок первый, кто открывает данную технологию, бонус к влиянию повышенный
        if (GM.GetOpponentTo(Player).GetTechStatus(TechInd))
            InfAmount = Mathf.Max(Techs[TechInd].mLocalInfl, Techs[TechInd].mGlobalInfl);
        else
            InfAmount = Mathf.Max(Techs[TechInd].mLocalInfl_1, Techs[TechInd].mGlobalInfl_1);

        //Увеличение влияния в странах
        if (TechInd < 16)
            GM.AddInfluenceInCountries(Player.Authority, Player.Authority, InfAmount);  //увеличение в союзных странах
        else
            GM.AddInfluenceInCountries(Authority.Neutral, Player.Authority, InfAmount); //Во всех нейтральных странах

        //Отметка открытой технологии
        Player.SetTechStatus(TechInd);

#if !myDEBUG
        //Steam achievments
        if (GM.Player == Player)
        {
            if (TechInd == 19)
                SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_0");
            if (TechInd == 25)
                SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_1");
            if (TechInd == 39)
                SteamManager.UnLockAchievment("NEW_ACHIEVEMENT_1_4");
        }
#endif

        // показать видео
        GM.VQueue.AddRolex(VideoQueue.V_TYPE_GLOB, VideoQueue.V_PRIO_NULL,
                                         VideoQueue.V_PUPPER_TECHNOLOW_START + TechInd - 1,
                                         Player.MyCountry);

        return true;
    }

    //Возвращает номер предыдущей технологии
    public int GetPrevTechNumber(int ind)
    {
        return Techs[ind].mPrevTechNumber;
    }

    [System.Serializable]
    class Technology
    {
        public int mPrevTechNumber = 0; // предыдущая технология
        public Sprite mUsaSprite = null, mRusSprite = null;
        public string mUsaTechName, mRusTechName; // текстовое описание
        [TextArea(2, 5)]
        public string mUsaDescr, mRusDescr; // текстовое описание
        public int mCost; // стоимость технологии
        public int mLocalInfl, mGlobalInfl, mLocalInfl_1, mGlobalInfl_1; // % прироста лок-глоб infl, и если открыли первыми
    }
}
