using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitScript : MonoBehaviour {

	public void LoadStartMenu() {
        Destroy(SettingsScript.Settings.gameObject);
        SceneManager.LoadScene("StartMenu");
    }
}
