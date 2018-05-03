using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class TechToggle : MonoBehaviour {
    Toggle tg;

	void Start ()
    {
        tg = SpaceRace.Instance.CurToggles.Find("Toggle" + (transform.GetSiblingIndex() + 1).ToString()).GetComponent<Toggle>();
	}

    public void OnMouseUpAsButton()
    {
        tg.isOn = true;
    }

    public void OnMouseEnter()
    {
        tg.transform.Find("MouseOver").gameObject.SetActive(true);
        //transform.Find("Checkmark").gameObject.SetActive(true);
        //GetComponent<Toggle>().isOn = true;
    }

    void OnMouseExit()
    {
        tg.transform.Find("MouseOver").gameObject.SetActive(false);
        //transform.Find("Checkmark").gameObject.SetActive(false);
    }
}
