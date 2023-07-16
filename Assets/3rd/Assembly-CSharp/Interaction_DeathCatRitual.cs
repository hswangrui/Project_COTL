using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using I2.Loc;
using MMBiomeGeneration;
using MMTools;
using Spine;
using UnityEngine;

public class Interaction_DeathCatRitual : Interaction
{
	[SerializeField]
	private Transform playerPosition;

	[SerializeField]
	private Transform spawnPosition;

	[SerializeField]
	private GameObject RitualLighting;

	[SerializeField]
	private GameObject LightingOverride;

	[SerializeField]
	private GameObject DappleLight;

	private List<FollowerManager.SpawnedFollower> spawnedFollowers = new List<FollowerManager.SpawnedFollower>();

	private int TargetFollowerCount = 20;

	private bool EnoughFollowers;

	private EventInstance loopedInstanceOutro;

	private EventInstance loopedInstance;

	private void Awake()
	{
		EnoughFollowers = DataManager.Instance.Followers.Count >= TargetFollowerCount;
		if (EnoughFollowers)
		{
			InitializeFollowers();
		}
	}

	private void InitializeFollowers()
	{
		List<FollowerBrain> list = new List<FollowerBrain>(FollowerBrain.AllBrains);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (DataManager.Instance.Followers_Dead.Contains(list[num]._directInfoAccess))
			{
				list.RemoveAt(num);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			FollowerManager.SpawnedFollower item = FollowerManager.SpawnCopyFollower(list[i]._directInfoAccess, spawnPosition.position, base.transform.parent, PlayerFarming.Location);
			item.Follower.gameObject.SetActive(false);
			spawnedFollowers.Add(item);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		for (int num = spawnedFollowers.Count - 1; num >= 0; num--)
		{
			FollowerManager.CleanUpCopyFollower(spawnedFollowers[num]);
		}
		spawnedFollowers.Clear();
	}

	public override void GetLabel()
	{
		if (DataManager.Instance.DeathCatBeaten)
		{
			base.Label = "";
			return;
		}
		base.GetLabel();
		EnoughFollowers = DataManager.Instance.Followers.Count >= TargetFollowerCount;
		HoldToInteract = EnoughFollowers;
		base.Label = (EnoughFollowers ? (ScriptLocalization.Interactions.PerformRitual + " | " + DataManager.Instance.Followers.Count + " / " + TargetFollowerCount + " " + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.FOLLOWERS)) : (ScriptLocalization.Interactions.Requires + "<color=red> " + DataManager.Instance.Followers.Count + " / " + TargetFollowerCount + " " + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.FOLLOWERS)));
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (!EnoughFollowers)
		{
			MonoSingleton<Indicator>.Instance.PlayShake();
		}
		else
		{
			StartCoroutine(RitualIE());
		}
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "Spin")
		{
			Debug.Log("Spin sfx");
			CameraManager.shakeCamera(0.1f, Utils.GetAngle(PlayerFarming.Instance.gameObject.transform.position, base.transform.position));
			MMVibrate.Haptic(MMVibrate.HapticTypes.SoftImpact, false, true, this);
			loopedInstanceOutro = AudioManager.Instance.CreateLoop("event:/player/jump_spin_float", PlayerFarming.Instance.gameObject, true);
		}
		else if (e.Data.Name == "whiteEyes")
		{
			AudioManager.Instance.PlayOneShot("event:/Stings/white_eyes", PlayerFarming.Instance.gameObject);
		}
	}

	private IEnumerator RitualIE()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject);
		bool waiting = true;
		PlayerFarming.Instance.GoToAndStop(playerPosition.position, base.gameObject, false, false, delegate
		{
			PlayerFarming.Instance.state.transform.DOMove(playerPosition.position, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
			waiting = false;
		});
		while (waiting)
		{
			yield return null;
		}
		SimulationManager.Pause();
		for (int num = spawnedFollowers.Count - 1; num >= 0; num--)
		{
			if (DataManager.Instance.Followers_Dead.Contains(spawnedFollowers[num].FollowerBrain._directInfoAccess))
			{
				FollowerManager.CleanUpCopyFollower(spawnedFollowers[num]);
				spawnedFollowers.RemoveAt(num);
			}
		}
		yield return new WaitForSeconds(1f);
		loopedInstance = AudioManager.Instance.CreateLoop("event:/followers/warp_in_pre_deathcat", PlayerFarming.Instance.gameObject, true);
		for (int i = 0; i < spawnedFollowers.Count; i++)
		{
			FollowerManager.SpawnedFollower spawnedFollower = spawnedFollowers[i];
			spawnedFollower.Follower.gameObject.SetActive(true);
			spawnedFollower.Follower.transform.position = GetFollowerPosition(i, spawnedFollowers.Count);
			spawnedFollower.Follower.transform.localScale = new Vector3((!(PlayerFarming.Instance.transform.position.x > spawnedFollower.Follower.transform.position.x)) ? 1 : (-1), 1f, 1f);
			spawnedFollower.Follower.State.LookAngle = spawnedFollower.Follower.State.facingAngle;
			spawnedFollower.Follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
			spawnedFollower.Follower.SimpleAnimator.enabled = false;
			spawnedFollower.Follower.SetFaceAnimation("Emotions/emotion-happy", true);
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", spawnedFollower.Follower.gameObject);
			AudioManager.Instance.PlayOneShot("event:/material/footstep_water", spawnedFollower.Follower.gameObject);
			spawnedFollower.Follower.TimedAnimation("spawn-in", 7f / 15f, delegate
			{
				spawnedFollower.Follower.AddBodyAnimation("worship", true, 0f);
			});
			yield return new WaitForSeconds(0.05f);
		}
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/final-ritual-start", 0, false);
		AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower", PlayerFarming.Instance.transform.position);
		AudioManager.Instance.PlayOneShot("event:/sermon/sermon_speech_bubble", PlayerFarming.Instance.transform.position);
		PlayerFarming.Instance.Spine.AnimationState.Event += HandleEvent;
		yield return new WaitForSeconds(0.5f);
		foreach (FollowerManager.SpawnedFollower spawnedFollower2 in spawnedFollowers)
		{
			StartCoroutine(SpawnSouls(spawnedFollower2.Follower.transform.position));
			yield return new WaitForSeconds(0.1f);
		}
		AudioManager.Instance.PlayOneShot("event:/rituals/blood_sacrifice", PlayerFarming.Instance.transform.position);
		RitualLighting.SetActive(true);
		LightingOverride.SetActive(false);
		BiomeConstants.Instance.ImpactFrameForDuration();
		DappleLight.SetActive(false);
		MMVibrate.RumbleContinuous(0f, 1f);
		CameraManager.instance.ShakeCameraForDuration(0.6f, 1f, 4.5f);
		BiomeConstants.Instance.ChromaticAbberationTween(1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 0.75f);
		yield return new WaitForSeconds(4.5f);
		BiomeConstants.Instance.ChromaticAbberationTween(1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		MMVibrate.StopRumble();
		MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.WhiteFade, MMTransition.NO_SCENE, 5f, "", null);
		yield return new WaitForSeconds(2f);
		for (int num2 = spawnedFollowers.Count - 1; num2 >= 0; num2--)
		{
			FollowerManager.CleanUpCopyFollower(spawnedFollowers[num2]);
		}
		spawnedFollowers.Clear();
		GameManager.GetInstance().OnConversationEnd();
		BiomeGenerator.ChangeRoom(0, 1);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		AudioManager.Instance.SetMusicRoomID(0, "deathcat_room_id");
		yield return new WaitForSeconds(4.75f);
		PlayerFarming.Instance.Spine.AnimationState.Event -= HandleEvent;
		AudioManager.Instance.StopLoop(loopedInstanceOutro);
	}

	public void Skip()
	{
		StartCoroutine(SkipRoutine());
	}

	private IEnumerator SkipRoutine()
	{
		MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.WhiteFade, MMTransition.NO_SCENE, 5f, "", null);
		AudioManager.Instance.SetMusicRoomID(0, "deathcat_room_id");
		yield return new WaitForSeconds(2f);
		BiomeGenerator.ChangeRoom(0, 1);
	}

	private Vector3 GetFollowerPosition(int index, int total)
	{
		float num;
		float f;
		if (total <= TargetFollowerCount)
		{
			num = 3f;
			f = (float)index * (360f / (float)total) * ((float)Math.PI / 180f);
			return playerPosition.position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
		}
		int num2 = 8;
		if (index < num2)
		{
			num = 3f;
			f = (float)index * (360f / (float)Mathf.Min(total, num2)) * ((float)Math.PI / 180f);
		}
		else
		{
			num = 4f;
			f = (float)(index - num2) * (360f / (float)(total - num2)) * ((float)Math.PI / 180f);
		}
		return playerPosition.position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
	}

	private IEnumerator SpawnSouls(Vector3 Position)
	{
		float delay = 0.5f;
		float Count = 5f;
		for (int i = 0; (float)i < Count; i++)
		{
			float num = (float)i / Count;
			SoulCustomTargetLerp.Create(PlayerFarming.Instance.CrownBone.gameObject, Position, 0.5f, Color.red);
			yield return new WaitForSeconds(delay - 0.2f * num);
		}
	}
}
