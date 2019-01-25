using System.Collections;
using System.Collections.Generic;
using QuickOutline;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager instance;
	private List<PlayerController> _players;
	private int _currentPlayerIndex;
	private int _roundCount; //A round is done when every player has made his move
	private Hashtable _protocol;

	public Camera cam;

	public int winningScore = 100;

	[Header("Map")]
	public WorldController world;
	public GameObject tilePrototype;


	public delegate void PlayersChanged(List<PlayerController> players);
	public static event PlayersChanged OnPlayersChanged;


	public delegate void RoundStart(int roundCount);
	public static event RoundStart OnRoundStart;

	public delegate void TurnStart(PlayerController player);
	public static event TurnStart OnTurnStart;

	[Header("Cursor and highlighting")]
	public Outline cursor;
	public Color highlightHoverColor;
	public Color highlightClickColor;
	public Color highlightNeutralColor;
	public Color highlightNegativeColor;

	public void ClosePanels() {
		UIManager.ClosePanel();
	}

	void Start () {
		instance = null;

		if (instance == null) {
			instance = this;
		}
		else if (instance != this) {
			Destroy(gameObject);
		}
		cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
		instance._players = GetPlayers();
		instance.world = Instantiate(world);
	}

	void StartGame() {
		ShufflePlayers();
		GiveStartingInventory();
		instance._roundCount = 1;
		if (OnRoundStart != null) OnRoundStart(instance._roundCount);
		if (OnTurnStart != null) OnTurnStart(GetCurrentPlayer());
	}

	public void GiveStartingInventory() {
		foreach (var player in _players) {
			player.inventory.Give(100, Inventory.Resource.Food);
			player.inventory.Give(100, Inventory.Resource.Wood);
			player.inventory.Give(100, Inventory.Resource.Iron);
			player.inventory.Give(100, Inventory.Resource.Stone);
		}
	}

	void OnEnable()
	{
		PlayerController.OnPlayerDone += NextPlayer;
		WorldController.OnReady += StartGame;
	}

	void OnDisable()
	{
		PlayerController.OnPlayerDone -= NextPlayer;
		WorldController.OnReady -= StartGame;
	}

	public void ShufflePlayers() {
		// Fisher-Yates shuffle
		// Source (modified): http://answers.unity.com/answers/948088/view.html
		var n = instance._players.Count;
		for (var i = 0; i < n; i++) {
			var r = i + (int)(Random.value * (n - i));
			var temp = instance._players[r];
			instance._players[r] = instance._players[i];
			instance._players[i] = temp;
		}
		if (OnPlayersChanged != null) OnPlayersChanged(instance._players);
	}

	public void NextPlayer(PlayerController playerController) {
		if (playerController.score >= winningScore) {
			UIManager.instance.ShowPlayerWon(playerController);
			return;
		}
		instance._currentPlayerIndex++;
		if (instance._currentPlayerIndex >= instance._players.Count) {
			instance._currentPlayerIndex = 0;
			instance._roundCount++;
			if (OnRoundStart != null) OnRoundStart(instance._roundCount);
		}
		if (OnPlayersChanged != null) OnPlayersChanged(instance._players);
		if (OnTurnStart != null) OnTurnStart(GetCurrentPlayer());
	}

	public PlayerController GetCurrentPlayer() {
		return instance._players[_currentPlayerIndex];
	}

	public List<PlayerController> GetPlayers() {
		if (instance._players != null && instance._players.Count > 0) return instance._players;
		var playerGameObjects = GameObject.FindGameObjectsWithTag("Player");
		var players = new List<PlayerController>();
		foreach (var player in playerGameObjects) {
			var p = player.GetComponent<PlayerController>();
			players.Add(p);
		}
		return players;
	}

	public int GetCurrentRound() {
		return instance._roundCount;
	}

	public void CheckWinningConditions() {
		List<PlayerController> playersLeft = new List<PlayerController>();
		foreach (var player in GetPlayers()) {
			if (player.score >= winningScore) {
				UIManager.instance.ShowPlayerWon(player);
			}
			if (!player.defeated) playersLeft.Add(player);
		}
		if (playersLeft.Count <= 1) {
			UIManager.instance.ShowPlayerWon(playersLeft[0]);
		}
	}
}