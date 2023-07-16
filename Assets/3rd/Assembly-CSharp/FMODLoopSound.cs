using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using WebSocketSharp;

public class FMODLoopSound : BaseMonoBehaviour
{
	public EventInstance LoopedSound;

	[EventRef]
	public string AudioSourcePath = string.Empty;

	public int ParameterSetOnStart;

	public string ParameterToSetOnStart = "";

	public bool SetParameter;

	public string ParameterToSet = "";

	public float ParameterSet;

	public bool PlayOnStart = true;

	public bool TriggerOn;

	public bool TriggeredOn;

	public bool isMusic;

	private bool LoopStarted;

	public float Distance;

	public float MaxDistance = 20f;

	private void OnEnable()
	{
		if (LoopStarted)
		{
			PlayLoop();
		}
		LoopStarted = false;
	}

	private void Update()
	{
		if (!LoopStarted)
		{
			return;
		}
		if (PlayerFarming.Instance == null)
		{
			AudioManager.Instance.StopLoop(LoopedSound);
		}
		else
		{
			if (!SetParameter)
			{
				return;
			}
			if (!TriggerOn)
			{
				Distance = base.gameObject.transform.position.y - PlayerFarming.Instance.transform.position.y;
				ParameterSet = Distance / MaxDistance + 1f;
				AudioManager.Instance.SetEventInstanceParameter(LoopedSound, ParameterToSet, ParameterSet);
				return;
			}
			Distance = Vector3.Distance(PlayerFarming.Instance.transform.position, base.transform.position);
			if (Distance < MaxDistance && !TriggeredOn)
			{
				if (isMusic)
				{
					AudioManager.Instance.SetMusicRoomID(1, ParameterToSet);
				}
				else
				{
					AudioManager.Instance.SetEventInstanceParameter(LoopedSound, ParameterToSet, 1f);
				}
				TriggeredOn = true;
			}
		}
	}

	private void Start()
	{
		if (PlayOnStart)
		{
			PlayLoop();
		}
	}

	private IEnumerator WaitForPlayer()
	{
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		AudioManager.Instance.PlayMusic(AudioSourcePath);
		if (!ParameterToSetOnStart.IsNullOrEmpty())
		{
			AudioManager.Instance.SetMusicRoomID(ParameterSetOnStart, ParameterToSetOnStart);
		}
		LoopStarted = true;
	}

	private IEnumerator WaitForPlayerLoop()
	{
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		LoopedSound = AudioManager.Instance.CreateLoop(AudioSourcePath, base.gameObject, true);
		if (!ParameterToSetOnStart.IsNullOrEmpty())
		{
			AudioManager.Instance.SetEventInstanceParameter(LoopedSound, ParameterToSetOnStart, ParameterSetOnStart);
		}
		LoopStarted = true;
	}

	public void PlayLoop()
	{
		StopLoop();
		if (!AudioSourcePath.IsNullOrEmpty())
		{
			if (isMusic)
			{
				StartCoroutine(WaitForPlayer());
			}
			else
			{
				StartCoroutine(WaitForPlayerLoop());
			}
		}
	}

	public void StopLoop()
	{
		AudioManager.Instance.StopLoop(LoopedSound);
		LoopStarted = false;
	}

	private void OnDisable()
	{
		StopLoop();
	}

	private void OnDestroy()
	{
		StopLoop();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, MaxDistance);
	}
}
