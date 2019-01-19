using System.Collections.Generic;
using Players;
using UnityEngine;
using UnityEngine.UI;
using World;
using Image = UnityEngine.UI.Image;

public class UIManager : MonoBehaviour {

	public static UIManager instance;

	public Font regularFont;
	public Font boldFont;

	[Header("References")]
	public Text roundNumber;
	public GameObject playerList;
	public Text foodAmount;
	public Text woodAmount;
	public Text ironAmount;
	public Text stoneAmount;
	public Text foodLimit;
	public Text woodLimit;
	public Text ironLimit;
	public Text stoneLimit;
	public GameObject entityIndicator;
	public GameObject entityIndicatorNumber;
	public Text entityIndicatorNumberText;
	public GameObject entityIcon;

	[Header("Audio")]
	public AudioSource audioSource;

	public AudioClip soundPlop;
	public AudioClip soundError;
	public AudioClip soundUp;
	public AudioClip soundDown;
	public AudioClip soundBash;
	public AudioClip soundWhoosh;

	[Header("Splash Screen")]
	public GameObject splashScreen;

	[Header("Blueprints")]
	public GameObject playerItem;
	public RMF_RadialMenu radialMenu;
	public Tooltip tooltip;

	private static GameObject _currentPanel;

	public void ShowTooltip(string title, string description, int food, int wood, int iron, int stone, string additional) {
		tooltip.titleText.text = title;
		tooltip.descriptionText.text = description;
		tooltip.transform.SetAsLastSibling();
		var inventory = GameManager.instance.GetCurrentPlayer().inventory;
		if (food > 0) {
			tooltip.costFood.SetActive(true);
			var t = tooltip.costFood.GetComponentInChildren<Text>();
			t.text = food.ToString();
			if (inventory.Get(Inventory.Resource.Food) < food) {
				t.color = Color.red;
			}
			else {
				t.color = Color.white;
			}
		}
		else tooltip.costFood.SetActive(false);
		if (wood > 0) {
			tooltip.costWood.SetActive(true);
			var t = tooltip.costWood.GetComponentInChildren<Text>();
			t.text = wood.ToString();
			if (inventory.Get(Inventory.Resource.Wood) < wood) {
				t.color = Color.red;
			}
			else {
				t.color = Color.white;
			}
		}
		else tooltip.costWood.SetActive(false);
		if (iron > 0) {
			tooltip.costIron.SetActive(true);
			var t = tooltip.costIron.GetComponentInChildren<Text>();
			t.text = iron.ToString();
			if (inventory.Get(Inventory.Resource.Iron) < iron) {
				t.color = Color.red;
			}
			else {
				t.color = Color.white;
			}
		}
		else tooltip.costIron.SetActive(false);
		if (stone > 0) {
			tooltip.costStone.SetActive(true);
			var t = tooltip.costStone.GetComponentInChildren<Text>();
			t.text = stone.ToString();
			if (inventory.Get(Inventory.Resource.Stone) < stone) {
				t.color = Color.red;
			}
			else {
				t.color = Color.white;
			}
		}
		else tooltip.costStone.SetActive(false);
		if (additional.Length > 0) {
			tooltip.additional.gameObject.SetActive(true);
			tooltip.additional.text = additional;
		}
		else tooltip.additional.gameObject.SetActive(false);
		tooltip.gameObject.SetActive(true);
	}

	public void ShowTooltip(string title, string description, int food, int wood, int iron, int stone) {
		ShowTooltip(title, description, food, wood, iron, stone, "");
	}


	public void HideTooltip() {
		tooltip.gameObject.SetActive(false);
	}

	public void PlayUISound(AudioClip sound) {
		audioSource.spatialBlend = 0f;
		audioSource.pitch = 1f;
		audioSource.transform.position = GameManager.instance.cam.transform.position;
		audioSource.PlayOneShot(sound);
	}

	public void PlayUISound(AudioClip sound, float pitch) {
		audioSource.pitch = pitch;
		audioSource.spatialBlend = 0f;
		audioSource.transform.position = GameManager.instance.cam.transform.position;
		audioSource.PlayOneShot(sound);
	}

	public void PlayTileSound(TileController tile, AudioClip sound) {
		audioSource.pitch = 1f;
		audioSource.spatialBlend = 1f;
		audioSource.transform.position = tile.transform.position;
		audioSource.PlayOneShot(sound);
	}

	public void ShowPlayerWon(PlayerController player) {
		splashScreen.SetActive(true);
		splashScreen.GetComponentInChildren<Text>().text = player.name + " won!";
	}

	public static GameObject SetCurrentPanel(GameObject panel) {
		if (GameManager.instance.GetCurrentPlayer().CarriesEntities() || TileController.PreviewModeActive()) return null;
		if (_currentPanel != null) {
			Destroy(_currentPanel);
			if (panel == null) {
				instance.PlayUISound(instance.soundWhoosh, .75f);
				return _currentPanel = null;
			}
		}
		if (panel == null) {
			return _currentPanel = null;
		}
		instance.PlayUISound(instance.soundWhoosh);
		_currentPanel = Instantiate(panel, GameObject.FindWithTag("UI").transform);
		_currentPanel.transform.position = Input.mousePosition;
		return _currentPanel;
	}

	public static void ClosePanel() {
		instance.tooltip.gameObject.SetActive(false);
		SetCurrentPanel(null);
	}


	void Start () {
		Destroy(instance);
		if (instance == null) {
			instance = this;
		}
		else if (instance != this) {
			Destroy(gameObject);
		}
	}

	void OnEnable()
	{
		GameManager.OnPlayersChanged += UpdatePlayers;
		GameManager.OnRoundStart += UpdateRoundDisplay;
		GameManager.OnTurnStart += UpdateInventory;
		GameManager.OnTurnStart += UpdateEntityIndicator;
		Inventory.OnInventoryChanged += UpdateInventory;
		PlayerController.OnEntitiesChanged += UpdateEntityIndicator;
	}

	void OnDisable()
	{
		GameManager.OnPlayersChanged -= UpdatePlayers;
		GameManager.OnRoundStart -= UpdateRoundDisplay;
		GameManager.OnTurnStart += UpdateInventory;
		GameManager.OnTurnStart += UpdateEntityIndicator;
		Inventory.OnInventoryChanged -= UpdateInventory;
		PlayerController.OnEntitiesChanged -= UpdateEntityIndicator;
	}

	void Update() {
		entityIndicator.transform.position = Input.mousePosition;
	}

	public void UpdateEntityIndicator(PlayerController player) {
		if (player.CarriesEntities()) {
			foreach (var material in entityIcon.GetComponent<Renderer>().materials) {
				if (material.name.StartsWith("Player")) {
					material.color = player.color;
					break;
				}
			}
			if (player.CarriedEntitiyCount() > 1) {
				entityIndicatorNumberText.text = player.CarriedEntitiyCount().ToString();
				entityIndicatorNumber.gameObject.SetActive(true);
			}
			else {
				entityIndicatorNumber.gameObject.SetActive(false);
			}
			entityIndicator.gameObject.SetActive(true);
		}
		else {
			if (entityIndicator == null) return;
			entityIndicator.gameObject.SetActive(false);
		}
	}

	public void UpdateInventory(Inventory inventory) {
		foodAmount.text = inventory.Get(Inventory.Resource.Food).ToString();
		woodAmount.text = inventory.Get(Inventory.Resource.Wood).ToString();
		ironAmount.text = inventory.Get(Inventory.Resource.Iron).ToString();
		stoneAmount.text = inventory.Get(Inventory.Resource.Stone).ToString();
		foodLimit.text = inventory.GetCapacity(Inventory.Resource.Food).ToString();
		woodLimit.text = inventory.GetCapacity(Inventory.Resource.Wood).ToString();
		ironLimit.text = inventory.GetCapacity(Inventory.Resource.Iron).ToString();
		stoneLimit.text = inventory.GetCapacity(Inventory.Resource.Stone).ToString();
	}

	public void UpdateInventory(PlayerController player) {
		UpdateInventory(player.inventory);
	}

	public void UpdatePlayers(List<PlayerController> players) {
		foreach (Transform player in playerList.transform) {
			Destroy(player.gameObject);
		}
		foreach (var player in players) {
			var item = Instantiate(playerItem);
			var playerName = item.GetComponentsInChildren<Text>()[0];
			var playerScore = item.GetComponentsInChildren<Text>()[1];
			playerName.text = player.name;
			playerScore.text = "("+player.score+")";
			var playerColoredBadge = item.gameObject.GetComponentInChildren<Image>();
			playerColoredBadge.color = player.color;
			item.transform.SetParent(playerList.transform);
			item.transform.localScale = Vector3.one;
			if (player.HasTurn()) {
				playerColoredBadge.rectTransform.position = playerColoredBadge.rectTransform.position;
				playerName.font = boldFont;
				playerScore.font = boldFont;
			}
			if (player.defeated) {
				var n = "";
				foreach (char c in player.name)
				{
					n = n + c + '\u0336';
				}
				playerName.text = n;
			}
		}
		GameManager.instance.CheckWinningConditions();
	}

	public void PlayerDone() {
		GameManager.instance.GetCurrentPlayer().Done();
	}

	public void UpdateRoundDisplay(int roundCount) {
		roundNumber.text = roundCount.ToString();
	}
}