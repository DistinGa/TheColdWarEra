using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ClockScript : MonoBehaviour {
    [SerializeField]
    Image curDecade, curYear, curMonth;
    [SerializeField]
    Sprite[] sprDecades, sprYears, sprMonths;

    public void ShowDate(int monthCount)
    {
        int d = (monthCount / 120) % 5;
        int y = (monthCount / 12) % 10;
        int m = monthCount % 12;

        curDecade.sprite = sprDecades[d];
        curYear.sprite = sprYears[y];
        curMonth.sprite = sprMonths[m];
    }
}
