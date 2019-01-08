using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {

	public Text titleText;
	public Text descriptionText;
	public Text additional;
	public GameObject costFood;
	public GameObject costWood;
	public GameObject costIron;
	public GameObject costStone;

	void LateUpdate() {
		var newPosition = Input.mousePosition + new Vector3(10f, 10f, 10f);
		newPosition.y = Mathf.Clamp(newPosition.y, 320, Screen.height);
		newPosition.x = Mathf.Clamp(newPosition.x, 256, Screen.width);
		transform.position = newPosition;
	}
}
