using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;
using World;

public class DemoLevelLoader : MonoBehaviour {
	void Update() {
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			TileController.Reset();
			SceneManager.LoadScene("demo-footage");
		}
		if (SceneManager.GetActiveScene().name == "demo-footage" && (Input.GetMouseButtonDown(0) ||Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))) {
			TileController.Reset();
			SceneManager.LoadScene("game-2p");
		}
		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			TileController.Reset();
			SceneManager.LoadScene("game-2p");
		}
		if (Input.GetKeyDown(KeyCode.Alpha3)) {
			TileController.Reset();
			SceneManager.LoadScene("game-3p");
		}
		if (Input.GetKeyDown(KeyCode.Alpha4)) {
			TileController.Reset();
			SceneManager.LoadScene("game-4p");
		}
		if (Input.GetKeyDown(KeyCode.Alpha5)) {
			TileController.Reset();
			SceneManager.LoadScene("game-5p");
		}
		if (Input.GetKeyDown(KeyCode.Alpha6)) {
			TileController.Reset();
			SceneManager.LoadScene("game-6p");
		}
	}
}