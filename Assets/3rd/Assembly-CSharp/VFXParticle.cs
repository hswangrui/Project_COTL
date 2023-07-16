using System;
using System.Collections.Generic;
using FMOD.Studio;
using Sirenix.Utilities;

using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ParticleSystem))]
public class VFXParticle : VFXObject
{
	[Serializable]
	public class ParticleEvent
	{
		public UnityEvent Event;

		public float Time;

		public bool TriggersOnce;

		[NonSerialized]
		public float TriggeredAtTime = -1f;

		public void Init()
		{
			TriggeredAtTime = -1f;
		}
	}

	[SerializeField]
	private ParticleSystem _particleSystem;

	[SerializeField]
	private List<ParticleEvent> _events;

	public string loopedSoundSFX;

	private EventInstance loopedSound;

	private bool createdSFX;

	public override void Init()
	{
		if (!base.Initialized)
		{
			if (!_particleSystem)
			{
				_particleSystem = GetComponent<ParticleSystem>();
			}
			ParticleSystem.MainModule main = _particleSystem.main;
			main.playOnAwake = false;
			main.stopAction = ParticleSystemStopAction.Callback;
		}
		if (!loopedSoundSFX.IsNullOrWhitespace())
		{
			loopedSound = AudioManager.Instance.CreateLoop(loopedSoundSFX, base.gameObject, true);
			createdSFX = true;
		}
		base.Init();
	}

	public override void PlayVFX(float addEmissionDelay = 0f)
	{
		if (_particleSystem.isPlaying)
		{
			_particleSystem.Stop();
		}
		foreach (ParticleEvent @event in _events)
		{
			@event.Init();
		}
		if (!loopedSoundSFX.IsNullOrWhitespace())
		{
			if (!createdSFX)
			{
				loopedSound = AudioManager.Instance.CreateLoop(loopedSoundSFX, base.gameObject, true);
			}
			else
			{
				AudioManager.Instance.PlayLoop(loopedSound);
			}
		}
		base.PlayVFX(addEmissionDelay);
	}

	protected override void Emit()
	{
		_particleSystem.Play();
		base.Emit();
	}

	public override void StopVFX()
	{
		_particleSystem.Stop();
		TriggerStopEvent();
		if (!loopedSoundSFX.IsNullOrWhitespace())
		{
			AudioManager.Instance.StopLoop(loopedSound);
		}
	}

	public override void CancelVFX()
	{
		_particleSystem.Stop();
		base.CancelVFX();
		if (!loopedSoundSFX.IsNullOrWhitespace())
		{
			AudioManager.Instance.StopLoop(loopedSound);
		}
	}

	private void OnDisable()
	{
		if (!loopedSoundSFX.IsNullOrWhitespace())
		{
			AudioManager.Instance.StopLoop(loopedSound);
		}
	}

	private void OnDestroy()
	{
		if (!loopedSoundSFX.IsNullOrWhitespace())
		{
			AudioManager.Instance.StopLoop(loopedSound);
		}
	}

	private void LateUpdate()
	{
		if (!_particleSystem || !_particleSystem.isPlaying)
		{
			return;
		}
		foreach (ParticleEvent @event in _events)
		{
			float num = _particleSystem.time % _particleSystem.main.duration;
			if (!@event.TriggersOnce && num >= @event.TriggeredAtTime)
			{
				@event.TriggeredAtTime = -1f;
			}
			if (!(@event.TriggeredAtTime >= 0f) && !(@event.Time <= num))
			{
				@event.Event.Invoke();
				@event.TriggeredAtTime = num;
			}
		}
	}

	private void OnParticleSystemStopped()
	{
		base.CancelVFX();
		if (!loopedSoundSFX.IsNullOrWhitespace())
		{
			AudioManager.Instance.StopLoop(loopedSound);
		}
	}
}
