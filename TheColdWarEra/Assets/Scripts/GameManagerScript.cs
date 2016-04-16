using UnityEngine;
using System.Collections;

public class GameManagerScript : MonoBehaviour {
    public GameObject TechMenu;

    public bool TechMenuOn;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ToggleTechMenu()
    {
        TechMenuOn = !TechMenuOn;
        TechMenu.SetActive(TechMenuOn);
    }
}
