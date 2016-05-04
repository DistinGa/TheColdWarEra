using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour {
    public static GameManagerScript GM;
    public PlayerScript Player;

    private Camera MainCamera;
    private RectTransform DownMenu;
    private CountryScript Country;  //Выбранная в данный момент страна

    public GameObject[] Menus;
    public RectTransform StatLists;
    [Space(10)]
    public GameObject Marker;    //Маркер указывающий на страну, с которой работаем.
    [Space(10)]
    public Sprite SignUSA;
    public Sprite SignSU;

    // Use this for initialization
    void Start () {
        GM = this;
        MainCamera = FindObjectOfType<Camera>();
        DownMenu = GameObject.Find("DownMenu").GetComponent<RectTransform>();

        MainCamera.GetComponent<CameraScript>().SetNewPosition(Player.MainCountry.GetComponent<CountryScript>().Capital);
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

    public void SnapToCountry(Vector2 PointerPosition)
    {
        SnapToCountry(MainCamera.ScreenToWorldPoint(PointerPosition) - new Vector3(0, 0, MainCamera.transform.position.z));
    }

    public void SnapToCountry(Vector3 MarkerPosition)
    {
        Marker.transform.position = MarkerPosition;

        Country = Physics2D.OverlapPoint(MarkerPosition).GetComponent<CountryScript>();
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
                    DownMenu.Find("MilitaryRight_n").GetComponent<Image>().fillAmount = Country.NForce * 0.1f;
                    DownMenu.Find("MilitaryLeft").GetComponent<Image>().fillAmount = Country.OppForce * 0.1f;
                }
                else
                {
                    DownMenu.Find("MilitaryLeft_n").GetComponent<Image>().fillAmount = Country.NForce * 0.1f;
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

