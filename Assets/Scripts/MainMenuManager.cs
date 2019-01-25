using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenuManager : MonoBehaviour {
	public Canvas canvas;
	public GameObject backdrop;
	public VideoPlayer demoVideo;
	public AudioSource backgroundMusic;

	public void Play() {
		SceneManager.LoadScene("world");
	}

	public void Demo() {
		canvas.gameObject.SetActive(false);
		backdrop.gameObject.SetActive(false);
		backgroundMusic.Stop();
		backgroundMusic.time = 0f;
		demoVideo.Play();
		demoVideo.gameObject.SetActive(true);
	}

	public void Quit() {
		Debug.Log("Quit");
		Application.Quit();
	}

	void Update () {
		if (demoVideo.gameObject.activeSelf && demoVideo.time >= demoVideo.clip.length || Input.anyKey) {
			backgroundMusic.Play();
			demoVideo.gameObject.SetActive(false);
			canvas.gameObject.SetActive(true);
			backdrop.gameObject.SetActive(true);
		}
		backdrop.transform.Rotate(Vector3.up, 10f * Time.deltaTime);
	}
}