using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AmbientMusicController : BaseMonoBehaviour
{
	[Serializable]
	public class Track
	{
		public string Tag = "";

		public bool Permenant;

		public bool DontFade;

		public float Volume = 1f;

		private AudioSource _audioSource;

		public AudioClip preAudioClip;

		public AudioClip audioClip;

		public List<Track> Layers = new List<Track>();

		private Coroutine cFadeIn;

		private Coroutine cDoPreRoll;

		private Coroutine cFadeOut;

		[HideInInspector]
		public bool isPlaying
		{
			get
			{
				if (audioSource != null)
				{
					return audioSource.isPlaying;
				}
				return false;
			}
		}

		public AudioSource audioSource
		{
			get
			{
				if (_audioSource == null)
				{
					_audioSource = gameObject.gameObject.AddComponent<AudioSource>();
					_audioSource.dopplerLevel = 0f;
					_audioSource.outputAudioMixerGroup = _Instance.audioMixerGroup;
					_audioSource.playOnAwake = false;
					_audioSource.loop = true;
				}
				return _audioSource;
			}
			set
			{
				_audioSource = value;
			}
		}

		private AmbientMusicController gameObject
		{
			get
			{
				return Instance;
			}
		}

		public Track(AudioClip preAudioClip, AudioClip audioClip)
		{
			this.audioClip = audioClip;
			this.preAudioClip = preAudioClip;
		}

		public void MatchTimeToTrack(Track OtherTrack)
		{
			audioSource.time = OtherTrack.audioSource.time % audioSource.time;
		}

		public void SetVolume(float Volume)
		{
			audioSource.volume = Volume;
		}

		public static Track CreateAndPlay(AudioClip preAudioClip, AudioClip audioClip, float FadeIn, bool Loop)
		{
			Track track = new Track(preAudioClip, audioClip);
			track.Play(true, FadeIn, Loop);
			track.Permenant = false;
			track.gameObject.Tracks.Add(track);
			return track;
		}

		public static Track Create(AudioClip preAudioClip, AudioClip audioClip, bool Permenant)
		{
			Track track = new Track(preAudioClip, audioClip);
			track.Permenant = Permenant;
			track.gameObject.Tracks.Add(track);
			return track;
		}

		public void Play(bool FadeOthers, float FadeIn, bool Loop = true)
		{
			if (FadeOthers)
			{
				FadeAll(this);
			}
			if (cFadeIn != null)
			{
				gameObject.StopCoroutine(cFadeIn);
			}
			cFadeIn = gameObject.StartCoroutine(DoFadeIn(FadeIn));
			if (preAudioClip != null)
			{
				if (cDoPreRoll != null)
				{
					gameObject.StopCoroutine(cDoPreRoll);
				}
				cDoPreRoll = gameObject.StartCoroutine(DoPreRoll());
			}
			else if (audioSource.clip == null || !audioSource.isPlaying)
			{
				audioSource.clip = audioClip;
				audioSource.loop = Loop;
				audioSource.Play();
			}
		}

		public void PlayLayer(int Index)
		{
			if (Layers[Index].audioSource != null && audioSource != null)
			{
				Layers[Index].audioSource.time = audioSource.time % Layers[Index].audioSource.time;
			}
			Layers[Index].Play(false, 0f);
		}

		public void Stop()
		{
			audioSource.Stop();
			foreach (Track layer in Layers)
			{
				layer.audioSource.Stop();
			}
		}

		public void StopLayer(int Index, float Fadeout)
		{
			Layers[Index].FadeOut(Fadeout, false);
		}

		private IEnumerator DoPreRoll()
		{
			audioSource.clip = preAudioClip;
			audioSource.Play();
			yield return new WaitForSeconds(preAudioClip.length);
			audioSource.clip = audioClip;
			audioSource.Play();
		}

		private IEnumerator DoFadeIn(float Duration)
		{
			while ((audioSource.volume += Time.deltaTime * (1f / Duration)) < Volume)
			{
				yield return null;
			}
			audioSource.volume = Volume;
		}

		public void FadeOut(float Duration, bool FadeLayers)
		{
			if (cFadeOut != null)
			{
				Instance.StopCoroutine(cFadeOut);
			}
			cFadeOut = Instance.StartCoroutine(DoFadeOut(Duration));
			if (!FadeLayers)
			{
				return;
			}
			foreach (Track layer in Layers)
			{
				layer.FadeOut(Duration, false);
			}
		}

		private IEnumerator DoFadeOut(float Duration)
		{
			while ((audioSource.volume -= Time.deltaTime * (1f / Duration)) > 0f)
			{
				yield return null;
			}
			audioSource.volume = 0f;
			if (!Permenant)
			{
				audioSource.Stop();
				audioSource.clip = null;
				Instance.Tracks.Remove(this);
				UnityEngine.Object.Destroy(audioSource);
			}
		}
	}

	public AudioMixerGroup audioMixerGroup;

	public List<Track> Tracks = new List<Track>();

	public Track CustomCombatTrack;

	public static bool AmbientIsPlaying;

	private static AmbientMusicController _Instance;

	public Track AmbientTrack
	{
		get
		{
			return Tracks[0];
		}
	}

	public Track DefaultCombatTrack
	{
		get
		{
			return Tracks[1];
		}
	}

	public Track NatureTrack
	{
		get
		{
			return Tracks[2];
		}
	}

	public static AmbientMusicController Instance
	{
		get
		{
			if (_Instance == null)
			{
				GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("MMAudio/Ambient Music Controller")) as GameObject;
				_Instance = obj.GetComponent<AmbientMusicController>();
				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			return _Instance;
		}
	}

	public static void PlayAmbient(float FadeIn)
	{
		Instance.AmbientTrack.Play(true, FadeIn);
		AmbientIsPlaying = true;
	}

	public static void PlayNature(float FadeIn)
	{
		Instance.NatureTrack.Play(false, FadeIn);
	}

	public static void PlayAmbientLayerByTag(string tag)
	{
		Debug.Log("AddAmbientLayerByTrack");
		foreach (Track layer in Instance.AmbientTrack.Layers)
		{
			if (layer.Tag == tag)
			{
				layer.Play(false, 0f);
			}
		}
	}

	public static void PlayAmbientCombat()
	{
		Instance.AmbientTrack.PlayLayer(0);
	}

	public static void StopAmbientCombat()
	{
		Instance.AmbientTrack.StopLayer(0, 5f);
	}

	public static void PlayAmbientSnake()
	{
		Instance.AmbientTrack.PlayLayer(1);
	}

	public static void StopAmbientSnake()
	{
		Instance.AmbientTrack.StopLayer(1, 2f);
	}

	public static void StopAll()
	{
		Instance.StopAllCoroutines();
		foreach (Track track in Instance.Tracks)
		{
			track.Stop();
		}
	}

	public static void PlayTrack(AudioClip clip, float FadeIn, bool Loop = true)
	{
		Track.CreateAndPlay(null, clip, FadeIn, Loop);
		AmbientIsPlaying = false;
	}

	public static void StopTrackAndResturnToAmbient()
	{
		PlayAmbient(3f);
	}

	public static void PlayCombat()
	{
		Instance.DefaultCombatTrack.Play(true, 0f);
		AmbientIsPlaying = false;
	}

	public static void PlayCombat(AudioClip PreRoll, AudioClip CombatMusic)
	{
		Instance.CustomCombatTrack = Track.CreateAndPlay(PreRoll, CombatMusic, 0f, true);
		AmbientIsPlaying = false;
	}

	public static void StopCombat()
	{
		PlayAmbient(4f);
	}

	public static void FadeAll(Track Exception)
	{
		Instance.StopAllCoroutines();
		foreach (Track track in Instance.Tracks)
		{
			if (track != Exception && !track.DontFade)
			{
				track.FadeOut(1f, true);
			}
		}
	}
}
