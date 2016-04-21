using UnityEngine;
using System.Collections;

public class GameManagerScript : MonoBehaviour {
    public static GameManagerScript GM;

    public GameObject[] Menus;
    public RectTransform StatLists;

    public GameObject Marker;    //Маркер указывающий на страну, с которой работаем.

    // Use this for initialization
    void Start () {
        GM = this;
	}
	
	// Update is called once per frame
	void Update () {
	
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