using UnityEngine;
using UnityEngine.UI;

public class StatsLineScript : MonoBehaviour {
    public Image imgInf;
    public RectTransform InfShadow;
    public Transform ZeroPos;
    public GameObject InfGlow;
    public Sprite Inf0;
    public Sprite Inf100;
    public Sprite InfCommon;

    [Space(10)]
    public Image Opposition;
    public GameObject OppGlow;
    public Image Eyes;
    public Sprite RedEyes, GreenEyes;

    public Image Score;

    [Space(10)]
    public Image CountryFrame;
    public Image CountryName;
    public Sprite DarkFrame;
    public Sprite LightFrame;

    public Authority ListAuthority;

    CountryScript Country;

    public void Init(CountryScript c, Sprite ScoreSprite)
    {
        Country = c;

        //Влияние
        int inf = c.GetInfluense(ListAuthority);
        InfGlow.SetActive(inf >= GameManagerScript.GM.INSTALL_PUPPER_INFLU);

        if (inf == 0)
        {
            imgInf.sprite = Inf0;
            InfShadow.gameObject.SetActive(false);
        }
        else if (inf == 100)
        {
            imgInf.sprite = Inf100;
            InfShadow.gameObject.SetActive(false);
        }
        else
        {
            imgInf.sprite = InfCommon;
            InfShadow.gameObject.SetActive(true);
            InfShadow.position = Vector3.Lerp(imgInf.transform.position, ZeroPos.position, inf * 0.01f);
        }

        //Оппозиция
        Opposition.fillAmount = (c.Support * 0.01f);
        OppGlow.SetActive(c.Support <= (100 - GameManagerScript.GM.INSTALL_PUPPER_OPPO));
        if (c.Support == float.Epsilon)
        {
            Eyes.sprite = RedEyes;
            Eyes.enabled = true;
        }
        else if (c.Support - 100f == float.Epsilon)
        {
            Eyes.sprite = GreenEyes;
            Eyes.enabled = true;
        }
        else
            Eyes.enabled = false;

        //Очки стран учитываем, только для своих стран
        if (ListAuthority == c.Authority)
        {
            Score.sprite = ScoreSprite;
            Score.enabled = true;
        }
        else
        {
            Score.enabled = false;
        }

        //Название
        CountryName.sprite = c.GetStatsNameSprite();
        switch (c.Authority)
        {
            case Authority.Neutral:
                CountryFrame.enabled = false;
                break;
            case Authority.Amer:
                CountryFrame.sprite = LightFrame;
                CountryFrame.enabled = true;
                break;
            case Authority.Soviet:
                CountryFrame.sprite = DarkFrame;
                CountryFrame.enabled = true;
                break;
        }
    }

    //Позиционирование на страну, выбранную в списке
    public void ToCountry()
    {
        Camera.main.GetComponent<CameraScript>().SetNewPosition(Country.Capital);
    }
}
