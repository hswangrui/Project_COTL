using System.Collections.Generic;
using UnityEngine;

public class SoundOnStateChange : BaseMonoBehaviour
{
	public List<SoundOnStateData> Sounds = new List<SoundOnStateData>();

	public List<SoundOnStateData> OnHitSounds = new List<SoundOnStateData>();

	public List<SoundOnStateData> OnDieSounds = new List<SoundOnStateData>();

	private StateMachine.State cs;

	private StateMachine _state;

	private Health health;

	private StateMachine.State CurrentState
	{
		set
		{
			if (cs != value)
			{
				foreach (SoundOnStateData sound in Sounds)
				{
					AudioManager.Instance.StopLoop(sound.LoopedSound);
					if ((sound.State == value && sound.position == SoundOnStateData.Position.Beginning) || (sound.State == cs && sound.position == SoundOnStateData.Position.End))
					{
						AudioManager.Instance.PlayOneShot(sound.AudioSourcePath, base.transform.position);
					}
					else if (sound.State == value && sound.position == SoundOnStateData.Position.Loop)
					{
						sound.LoopedSound = AudioManager.Instance.CreateLoop(sound.AudioSourcePath, base.gameObject, true);
					}
				}
			}
			cs = value;
		}
	}

	private StateMachine state
	{
		get
		{
			if (_state == null)
			{
				_state = GetComponent<StateMachine>();
			}
			return _state;
		}
	}

	private void OnEnable()
	{
		health = GetComponent<Health>();
		health.OnHit += OnHit;
		health.OnDie += OnDie;
	}

	private void OnDisable()
	{
		DisableLoops();
		health.OnHit -= OnHit;
		health.OnDie -= OnDie;
	}

	private void DisableLoops()
	{
		foreach (SoundOnStateData sound in Sounds)
		{
			if (sound.position == SoundOnStateData.Position.Loop)
			{
				AudioManager.Instance.StopLoop(sound.LoopedSound);
			}
		}
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (OnHitSounds.Count > 0)
		{
			AudioManager.Instance.PlayOneShot(OnHitSounds[Random.Range(0, OnHitSounds.Count)].AudioSourcePath);
		}
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		DisableLoops();
		if (OnDieSounds.Count > 0)
		{
			AudioManager.Instance.PlayOneShot(OnHitSounds[Random.Range(0, OnHitSounds.Count)].AudioSourcePath);
		}
	}

	private void Update()
	{
		if (state != null)
		{
			CurrentState = state.CURRENT_STATE;
		}
	}
}
