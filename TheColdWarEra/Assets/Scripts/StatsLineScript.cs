using UnityEngine;
using UnityEngine.UI;

public class StatsLineScript : MonoBehaviour {
    public RectTransform Influence;
    public RectTransform Opposition;
    public RectTransform Score;
    public RectTransform Button;

    public Sprite Blue;
    public Sprite Red;
    public Sprite Grey;

    public Authority ListAuthority;
    public CountryScript Country;

    public void Init(CountryScript c)
    {
        Country = c;

        Image Image = GetComponent<Image>();

        Influence.GetComponent<Text>().text = (ListAuthority == Authority.Amer? c.AmInf: c.SovInf).ToString("f0");
        Opposition.GetComponent<Text>().text = (100f - c.Support).ToString("f0");
        //Очки стран учитываем, только для своих стран
        if(ListAuthority == c.Authority)
            Score.GetComponent<Text>().text = (c.Score).ToString("f0");
        else
            Score.GetComponent<Text>().text = "";

        Button.GetComponent<Text>().text = c.Name;

        switch (c.Authority)
        {
            case Authority.Neutral:
                Image.sprite = Grey;
                break;
            case Authority.Amer:
                Image.sprite = Blue;
                break;
            case Authority.Soviet:
                Image.sprite = Red;
                break;
        }

        //gameObject.SetActive(true);
    }

    //Позиционирование на страну, выбранную в списке
    public void ToCountry()
    {
        Camera.main.GetComponent<CameraScript>().SetNewPosition(Country.Capital);
    }
}
