using UnityEngine;
using System.Collections;

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


    // Use this for initialization
    void Start()
    {
        SetAuthority();
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
            if (NInf == 100)
                return; //Куда уж больше?

            AmInf += Amount;

            //Распределяем "минус" по другим влияниям.
            NInf -= Amount; //Сначала отнимаем от нейтрального влияния
            if(NInf < 0)    //Если нейтрального влияния не хватило, отнимаем от влияния соперника.
            {
                SovInf += NInf; //NInf отрицательное
                NInf = 0;
                if (SovInf < 0) //Если и влияния соперника не хватило, значит была попытка добавить слишком большое влияние.
                {
                    SovInf = 0;
                    AmInf = 100;
                }
            }
        }

        if (Inf == Authority.Soviet)
        {
            if (SovInf == 100)
                return; //Куда уж больше?

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
}
