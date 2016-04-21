using UnityEngine;
using System.Collections;

public class CountryScript : MonoBehaviour {
    public Sprite SprAmerican;
    public Sprite SprSoviet;
    public Sprite SprNeutral;

    [Space(10)]
    public string Name;
    public Region Region;
    public Authority Authority;
    public int Score;
    public int Support;
    [Space(10)]
    public int SovInf;
    public int AmInf;
    public int NInf;
    [Space(10)]
    public int GovForce;
    public int OppForce;
    public int NForce;
    [Space(10)]
    public int KGB;
    public int CIA;


	// Use this for initialization
	void Start () {
        SetAuthority();
    }
	
	public void SetAuthority() {
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

    public Transform Capital
    {
        get {return transform.FindChild("Capital"); }
    }
}
