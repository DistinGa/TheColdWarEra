using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitScript : MonoBehaviour {

	public void LoadStartMenu() {
        SceneManager.LoadScene("StartMenu");
    }
}
