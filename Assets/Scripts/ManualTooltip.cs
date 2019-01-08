using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManualTooltip : MonoBehaviour {

	public string title;
	[TextArea] public string description;
	[TextArea] public string additional;
	public int costFood;
	public int costWood;
	public int costIron;
	public int costStone;

	public void Show() {
		UIManager.instance.ShowTooltip(title, description, costFood, costWood, costIron, costStone, additional);
	}

	public void Hide() {
		UIManager.instance.HideTooltip();
	}
}
