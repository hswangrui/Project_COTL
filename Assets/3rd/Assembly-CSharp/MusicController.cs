using System.Collections;
using UnityEngine;

public class MusicController : BaseMonoBehaviour
{
	private static MusicController Instance;

	private AudioSource _audioSource;

	private AudioSource audioSource
	{
		get
		{
			if (_audioSource == null)
			{
				_audioSource = GetComponent<AudioSource>();
			}
			return _audioSource;
		}
	}

	public static void Play(AudioClip audioClip)
	{
		if (Instance == null)
		{
			Instance = (Object.Instantiate(Resources.Load("MMAudio/Music Controller")) as GameObject).GetComponent<MusicController>();
		}
		Instance.play(audioClip);
	}

	private void play(AudioClip audioClip)
	{
		Instance.audioSource.clip = audioClip;
		Instance.audioSource.loop = true;
		Instance.audioSource.Play();
	}

	public static void FadeOutAndStop()
	{
		Instance.fadeOutAndStop();
	}

	private void fadeOutAndStop()
	{
		StopAllCoroutines();
		StartCoroutine(DoFadeOutAndStop());
	}

	private IEnumerator DoFadeOutAndStop()
	{
		while ((audioSource.volume -= Time.deltaTime) > 0f)
		{
			yield return null;
		}
		audioSource.volume = 0f;
		audioSource.Stop();
	}
}
