using UnityEngine;

public class Music : MonoBehaviour {

	public AudioClip[] tracks;
	private AudioSource _source;
	private int _lastClipIndex = -1;

	void Start () {
		_source = GetComponent<AudioSource>();
	}

	void Update () {
		if(!_source.isPlaying)
			PlayRandomMusic();
	}

	/// <summary>
	///   <para>Plays a random track from the lost.</para>
	/// </summary>
	void PlayRandomMusic() {
		var nextClipIndex = _lastClipIndex;
		while (tracks.Length > 1 && nextClipIndex == _lastClipIndex) {
			nextClipIndex = Random.Range(0, tracks.Length);
		}
		_lastClipIndex = nextClipIndex;
		_source.clip = tracks[nextClipIndex];
		_source.Play();
	}
}
