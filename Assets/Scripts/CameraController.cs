using UnityEngine;
using PostFX;

public class CameraController : MonoBehaviour {
	public float panSpeed = 1f;
	public float scrollSpeed = 1f;

	public float zoomMin = 1f;
	public float zoomMax = 10f;

	public float angleRange = 20f;

	private float _angleMin;
	public GameObject focus;

	private Vector3 _targetPosition;
	private Vector3 _targetPositionFocus;
	private Quaternion _targetRotation;
	private Quaternion _targetRotationFocus;

	private AudioSource _windSound;

	void Start() {
		_angleMin = transform.localRotation.eulerAngles.x - angleRange;
		_windSound = GetComponent<AudioSource>();
	}

	void OnEnable()
	{
		PlayerController.OnPlayerDone += SaveCameraPosition;
		GameManager.OnTurnStart += MoveToPlayer;
	}

	void OnDisable()
	{
		PlayerController.OnPlayerDone -= SaveCameraPosition;
		GameManager.OnTurnStart -= MoveToPlayer;
	}

	void SaveCameraPosition(PlayerController player) {
		player.cameraPosition = _targetPosition;
		player.cameraPositionFocus = _targetPositionFocus;
	}

	void MoveToPlayer(PlayerController player) {
		if (player.IsLocal()) {
			MoveTo(player.cameraPosition, player.cameraPositionFocus);
		}
	}

	void MoveTo(Vector3 position, Vector3 positionFocus) {
		_targetPosition = position;
		_targetPositionFocus = positionFocus;
	}

	void Update () {


		var pos = _targetPosition;
		var posFocus = _targetPositionFocus;
		var rot = _targetRotation.eulerAngles;
		var rotFocus = _targetRotationFocus.eulerAngles;
		float panH = 0;
		float panV = 0;

		float scroll = Input.GetAxis("Mouse ScrollWheel");
		pos.y -= scroll * 5f * scrollSpeed;
		pos.y = Mathf.Clamp(pos.y, zoomMin, zoomMax);
		var relativePanSpeed = panSpeed / 3 * (pos.y / zoomMax);
		var factor = (pos.y - zoomMin) / (zoomMax - zoomMin);
		GetComponent<TiltShift>().Radius = 2f - 2f * factor;
		GetComponent<TiltShift>().Offset = (Input.mousePosition.y - Screen.height / 2) / (Screen.height / 2) * -1;
		_windSound.volume = factor;
		var angle = _angleMin + angleRange * factor;
		rot.x = angle;

		if (Input.GetAxis("Vertical") > 0 || Input.mousePosition.y >= Screen.height - 1) {
			panV = relativePanSpeed;
		}
		if (Input.GetAxis("Horizontal") > 0 || Input.mousePosition.x >= Screen.width - 1) {
			panH = relativePanSpeed;
		}
		if (Input.GetAxis("Vertical") < 0 || Input.mousePosition.y <= 1) {
			panV = relativePanSpeed * -1;
		}
		if (Input.GetAxis("Horizontal") < 0 || Input.mousePosition.x <= 1) {
			panH = relativePanSpeed * -1;
		}
		/*if (Input.GetButton("Rotate")) {
			rotFocus.y += Input.GetAxis("Mouse X");
		}*/

		if (Mathf.Abs(posFocus.x + panH) > GameManager.instance.world.Bounds().x) {
			panH = 0;
		}
		if (Mathf.Abs(posFocus.z + panV) > GameManager.instance.world.Bounds().y) {
			panV = 0;
		}

		posFocus.x += panH;
		posFocus.z += panV;

		_targetRotation = Quaternion.Euler(rot);
		_targetRotationFocus = Quaternion.Euler(rotFocus);
		_targetPosition = pos;
		_targetPositionFocus = posFocus;

		transform.localRotation = Quaternion.Lerp(transform.localRotation, _targetRotation, 10.0f * Time.deltaTime);
		//Focus.transform.localRotation = Quaternion.Lerp(Focus.transform.localRotation, _targetRotationFocus, 10.0f * Time.deltaTime);

		transform.transform.localPosition = Vector3.Lerp(transform.localPosition, _targetPosition, 10.0f * Time.deltaTime);
		focus.transform.localPosition = Vector3.Lerp(focus.transform.localPosition, _targetPositionFocus, 10.0f * Time.deltaTime);

	}
}
