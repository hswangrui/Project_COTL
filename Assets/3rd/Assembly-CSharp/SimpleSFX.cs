using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSFX : BaseMonoBehaviour
{
	[Serializable]
	public class SFXAndTag
	{
		public string Tag;

		public List<AudioClip> audioClips = new List<AudioClip>();

		public bool Looping;

		public SFXAndTag(AudioClip c, string t)
		{
			audioClips.Add(c);
			Tag = t;
		}

		public SFXAndTag(List<AudioClip> c, string t)
		{
			audioClips = c;
			Tag = t;
		}
	}

	public float MinDistance = 5f;

	public float MaxDistance = 30f;

	public List<SFXAndTag> Sounds = new List<SFXAndTag>();

	public AudioSource _audioSource;

	public SFXAndTag PlayOnAwake;

	private AudioSource audioSource
	{
		get
		{
			if (_audioSource == null)
			{
				_audioSource = base.gameObject.AddComponent<AudioSource>();
				_audioSource.playOnAwake = false;
				_audioSource.spread = 360f;
				_audioSource.spatialBlend = 1f;
				_audioSource.minDistance = MinDistance;
				_audioSource.maxDistance = MaxDistance;
			}
			return _audioSource;
		}
	}

	private void Start()
	{
		if (PlayOnAwake != null)
		{
			Play(PlayOnAwake.Tag);
		}
	}

	public void AddSound(AudioClip clip, string tag)
	{
		Sounds.Add(new SFXAndTag(clip, tag));
	}

	public void Play(string Tag)
	{
		foreach (SFXAndTag sound in Sounds)
		{
			if (sound.Tag == Tag)
			{
				audioSource.clip = sound.audioClips[UnityEngine.Random.Range(0, sound.audioClips.Count)];
				audioSource.loop = sound.Looping;
				if (sound.Looping)
				{
					audioSource.Play();
				}
				else
				{
					audioSource.PlayOneShot(sound.audioClips[UnityEngine.Random.Range(0, sound.audioClips.Count)]);
				}
				break;
			}
		}
	}

	private void Update()
	{
		audioSource.pitch = Time.timeScale;
	}
}
