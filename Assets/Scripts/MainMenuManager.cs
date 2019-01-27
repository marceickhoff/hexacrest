using UnityEngine;
using UnityEngine.SceneManagement;
using World;

public class MainMenuManager : MonoBehaviour {
	public GameObject backdrop;

	public void Play() {
		TileController.Reset();
		SceneManager.LoadScene("world");
	}

	public void Settings() {
		//TODO
	}

	public void Quit() {
		Debug.Log("Quit");
		Application.Quit();
	}

	void Update () {
		backdrop.transform.Rotate(Vector3.up, 10f * Time.deltaTime);
	}
}