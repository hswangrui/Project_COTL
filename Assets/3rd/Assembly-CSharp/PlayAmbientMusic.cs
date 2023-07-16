using UnityEngine;

public class PlayAmbientMusic : BaseMonoBehaviour
{
	public AudioClip Music;

	public float fadeIn = 1f;

	private void Start()
	{
		AmbientMusicController.PlayTrack(Music, fadeIn);
	}
}
