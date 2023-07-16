using UnityEngine;
using UnityEngine.Audio;

public class SFXController : BaseMonoBehaviour
{
	private static SFXController Instance;

	public AudioMixerGroup audioMixerGroup;

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

	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public static void Play(AudioClip audioClip)
	{
		if (Instance == null)
		{
			Instance = Object.Instantiate(Resources.Load("MMAudio/SFX Controller") as GameObject).GetComponent<SFXController>();
		}
		Instance.audioSource.clip = audioClip;
		Instance.audioSource.outputAudioMixerGroup = Instance.audioMixerGroup;
		Instance.audioSource.PlayOneShot(audioClip);
	}

	public static void Play(AudioClip audioClip, float pitch)
	{
		if (Instance == null)
		{
			Instance = Object.Instantiate(Resources.Load("MMAudio/SFX Controller") as GameObject).GetComponent<SFXController>();
		}
		Instance.audioSource.clip = audioClip;
		Instance.audioSource.pitch = pitch;
		Instance.audioSource.outputAudioMixerGroup = Instance.audioMixerGroup;
		Instance.audioSource.PlayOneShot(audioClip);
	}

	public static void Play(AudioClip audioClip, float pitch, float volume)
	{
		if (Instance == null)
		{
			Instance = Object.Instantiate(Resources.Load("MMAudio/SFX Controller") as GameObject).GetComponent<SFXController>();
		}
		Instance.audioSource.clip = audioClip;
		Instance.audioSource.pitch = pitch;
		Instance.audioSource.volume = volume;
		Instance.audioSource.outputAudioMixerGroup = Instance.audioMixerGroup;
		Instance.audioSource.PlayOneShot(audioClip);
	}
}
