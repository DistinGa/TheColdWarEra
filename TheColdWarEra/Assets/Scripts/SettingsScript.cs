using UnityEngine;
using System.Collections;

public class SettingsScript : MonoBehaviour {
    public static SettingsScript Settings;
    public bool mVideo; // tru-используем avi-видео, fals-используем картинки

    // Use this for initialization
    void Start () {
        Settings = this;

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
