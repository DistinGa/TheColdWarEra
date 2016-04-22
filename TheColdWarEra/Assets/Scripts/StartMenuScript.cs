using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuScript : MonoBehaviour {
    public Animator Animator;

	public void Exit() {
        Application.Quit();
	}

    public void LoadScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    public void InvokeStart()
    {
        Animator.Play(0);
        Invoke(("StartGame"), 5f);
    }

    public void StartGame()
    {
        LoadScene("GameScene");
    }
}
