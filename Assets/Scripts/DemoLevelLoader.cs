using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;

public class DemoLevelLoader : MonoBehaviour {
	void Update() {
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			SceneManager.LoadScene("demo-footage");
		}
		if (SceneManager.GetActiveScene().name == "demo-footage" && (Input.GetMouseButtonDown(0) ||Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))) {
			SceneManager.LoadScene("game-2p");
		}
		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			SceneManager.LoadScene("game-2p");
		}
		if (Input.GetKeyDown(KeyCode.Alpha3)) {
			SceneManager.LoadScene("game-3p");
		}
		if (Input.GetKeyDown(KeyCode.Alpha4)) {
			SceneManager.LoadScene("game-4p");
		}
		if (Input.GetKeyDown(KeyCode.Alpha5)) {
			SceneManager.LoadScene("game-5p");
		}
		if (Input.GetKeyDown(KeyCode.Alpha6)) {
			SceneManager.LoadScene("game-6p");
		}
	}
}