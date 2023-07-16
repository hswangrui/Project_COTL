using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public class HourGlass : BaseMonoBehaviour
{
	private enum State
	{
		idle,
		turning,
		active,
		timeup
	}

	public SkeletonAnimation SpineAnimation;

	private TrackEntry Track;

	private float SpineDuration;

	public Transform SpawnPosition;

	public List<GameObject> Doors = new List<GameObject>();

	public List<SimpleSFX> DoorsSFX = new List<SimpleSFX>();

	private State _CurrentState;

	private State CurrentState
	{
		get
		{
			return _CurrentState;
		}
		set
		{
			if (_CurrentState != value)
			{
				switch (value)
				{
				case State.idle:
					SpineAnimation.AnimationState.SetAnimation(0, "idle", true);
					break;
				case State.turning:
					SpineAnimation.AnimationState.SetAnimation(0, "turn", false);
					break;
				case State.timeup:
					SpineAnimation.AnimationState.SetAnimation(0, "time-up", true);
					break;
				}
			}
			_CurrentState = value;
		}
	}

	private void Start()
	{
		CurrentState = State.idle;
	}

	public void TurnHouseglass()
	{
		StartCoroutine(OpenDoors());
	}

	private IEnumerator DoTurn()
	{
		CurrentState = State.turning;
		yield return new WaitForSeconds(1.5f);
		HUD_Timer.Timer = 120f;
		HUD_Timer.TimerRunning = true;
		StartCoroutine(DoActive());
		StartCoroutine(OpenDoors());
	}

	private IEnumerator OpenDoors()
	{
		yield return new WaitForSeconds(1f);
		float Progress = 0f;
		float Duration = 3f;
		foreach (SimpleSFX item in DoorsSFX)
		{
			if (item != null)
			{
				item.Play("stone_door_sliding");
			}
		}
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			CameraManager.shakeCamera(Random.Range(0.15f, 0.2f), Random.Range(0, 360));
			foreach (GameObject door in Doors)
			{
				Vector3 localPosition = door.transform.localPosition;
				localPosition.z = 2f * (Progress / Duration);
				door.transform.localPosition = localPosition;
			}
			yield return null;
		}
		foreach (GameObject door2 in Doors)
		{
			Collider2D componentInChildren = door2.GetComponentInChildren<Collider2D>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = false;
			}
		}
	}

	private IEnumerator DoActive()
	{
		CurrentState = State.active;
		while (CurrentState == State.active)
		{
			Track = SpineAnimation.AnimationState.SetAnimation(0, "countdown", false);
			SpineDuration = SpineAnimation.Skeleton.Data.FindAnimation("countdown").Duration;
			SpineAnimation.timeScale = 0f;
			Track.Animation.Apply(SpineAnimation.Skeleton, 0f, SpineDuration * HUD_Timer.Progress, false, null, 1f, MixBlend.Replace, MixDirection.In);
			if (HUD_Timer.IsTimeUp)
			{
				SpineAnimation.timeScale = 1f;
				CurrentState = State.timeup;
				break;
			}
			yield return null;
		}
	}
}
