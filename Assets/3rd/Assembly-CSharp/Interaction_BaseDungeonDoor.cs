using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using MMTools;
using Spine;
using Spine.Unity;
using src.Extensions;
using UnityEngine;

public class Interaction_BaseDungeonDoor : Interaction
{
	public int FollowerCount = 5;

	public int SacrificeFollowerLevel = -1;

	public GameObject RitualPosition;

	public GameObject RitualReceiveDevotionPosition;

	public FollowerLocation Location;

	public string SceneName;

	public BoxCollider2D CollideForDoor;

	public BoxCollider2D BlockingCollider;

	public SimpleSetCamera SimpleSetCamera;

	public SkeletonAnimation DoorSpine;

	public GameObject Lights;

	public GameObject portal;

	public GameObject DoorLights;

	public GameObject DoorToMove;

	private Vector3 OpenDoorPosition = new Vector3(0f, -2.5f, 4f);

	[SerializeField]
	private ParticleSystem doorSmokeParticleSystem;

	[SerializeField]
	private SkeletonRendererCustomMaterials spineMaterialOverride;

	public GameObject RitualLighting;

	[SerializeField]
	private GameObject BeholderEyeStatues;

	[SerializeField]
	private GameObject BeholderEyeStatues_2;

	private bool Used;

	public GameObject doorLightSource;

	public SpriteRenderer doorInnerBlack;

	private string SRequires;

	private string SOpenDoor;

	[TermsPopup("")]
	public string PlaceName;

	public Color PlaceColor;

	private string PlaceString;

	private List<FollowerBrain> brains;

	private bool HaveFollowers;

	private bool Blocking;

	private List<FollowerManager.SpawnedFollower> spawnedFollowers = new List<FollowerManager.SpawnedFollower>();

	private int NumGivingDevotion;

	public EventInstance LoopedSound;

	private EventInstance loopedInstanceOutro;

	public bool Unlocked { get; private set; }

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		ActivateDistance = 2f;
		Used = false;
		Lights.SetActive(false);
		spineMaterialOverride.enabled = false;
		Unlocked = DataManager.Instance.UnlockedDungeonDoor.Contains(Location);
		if (Unlocked && !Blocking)
		{
			DoorToMove.transform.localPosition = OpenDoorPosition;
			DoorToMove.SetActive(false);
		}
		else
		{
			DoorToMove.transform.localPosition = Vector3.zero;
		}
		OpenDoor();
		int value = DataManager.Instance.GetDungeonLayer(Location) - 1;
		DoorSpine.AnimationState.SetAnimation(0, Mathf.Clamp(value, 0, 3).ToString(), false);
		if (DataManager.Instance.DeathCatBeaten && DataManager.Instance.OnboardedLayer2 && !DataManager.Instance.DungeonCompleted(Location, true))
		{
			DoorSpine.AnimationState.SetAnimation(0, DataManager.GetGodTearNotches(Location).ToString(), true);
		}
		else if (DataManager.Instance.BossesCompleted.Contains(Location))
		{
			DoorSpine.AnimationState.SetAnimation(0, "beaten", true);
		}
		else if (DataManager.Instance.BossesEncountered.Contains(Location))
		{
			DoorSpine.AnimationState.SetAnimation(0, "4", true);
		}
		DoorLights.SetActive(GetFollowerCount());
		if (Location == FollowerLocation.Dungeon1_1 && !DataManager.Instance.IntroDoor1)
		{
			DoorLights.SetActive(false);
		}
		CheckBeholders();
	}

	private void CheckBeholders()
	{
		BeholderEyeStatues.SetActive(false);
		BeholderEyeStatues_2.SetActive(false);
		switch (Location)
		{
		case FollowerLocation.Dungeon1_1:
			if (DataManager.Instance.CheckKilledBosses("Boss Beholder 1_P2"))
			{
				BeholderEyeStatues_2.SetActive(true);
			}
			else if (DataManager.Instance.CheckKilledBosses("Boss Beholder 1"))
			{
				BeholderEyeStatues.SetActive(true);
			}
			break;
		case FollowerLocation.Dungeon1_2:
			if (DataManager.Instance.CheckKilledBosses("Boss Beholder 2_P2"))
			{
				BeholderEyeStatues_2.SetActive(true);
			}
			else if (DataManager.Instance.CheckKilledBosses("Boss Beholder 2"))
			{
				BeholderEyeStatues.SetActive(true);
			}
			break;
		case FollowerLocation.Dungeon1_3:
			if (DataManager.Instance.CheckKilledBosses("Boss Beholder 3_P2"))
			{
				BeholderEyeStatues_2.SetActive(true);
			}
			else if (DataManager.Instance.CheckKilledBosses("Boss Beholder 3"))
			{
				BeholderEyeStatues.SetActive(true);
			}
			break;
		case FollowerLocation.Dungeon1_4:
			if (DataManager.Instance.CheckKilledBosses("Boss Beholder 4_P2"))
			{
				BeholderEyeStatues_2.SetActive(true);
			}
			else if (DataManager.Instance.CheckKilledBosses("Boss Beholder 4"))
			{
				BeholderEyeStatues.SetActive(true);
			}
			break;
		}
	}

	private void Start()
	{
		OnEnableInteraction();
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		SRequires = ScriptLocalization.Interactions.Requires;
		SOpenDoor = ScriptLocalization.Interactions.OpenDoor;
		PlaceString = LocalizationManager.GetTranslation(PlaceName);
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		if (!Unlocked)
		{
			MonoSingleton<Indicator>.Instance.ShowTopInfo("<sprite name=\"img_SwirleyLeft\"> " + PlaceString.Colour(PlaceColor) + " <sprite name=\"img_SwirleyRight\">");
		}
	}

	public override void OnBecomeNotCurrent()
	{
		base.OnBecomeNotCurrent();
		MonoSingleton<Indicator>.Instance.HideTopInfo();
	}

	public bool GetFollowerCount()
	{
		brains = new List<FollowerBrain>(FollowerBrain.AllBrains);
		for (int num = brains.Count - 1; num >= 0; num--)
		{
			if (DataManager.Instance.Followers_Dead.Contains(brains[num]._directInfoAccess))
			{
				brains.RemoveAt(num);
			}
		}
		return brains.Count >= FollowerCount;
	}

	public override void GetLabel()
	{
		if (Unlocked)
		{
			base.Label = "<sprite name=\"img_SwirleyLeft\"> " + PlaceString.Colour(PlaceColor) + " <sprite name=\"img_SwirleyRight\">";
			Interactable = false;
			return;
		}
		Interactable = true;
		HaveFollowers = GetFollowerCount();
		base.Label = (HaveFollowers ? (SOpenDoor + " | " + FollowerCount + " " + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.FOLLOWERS)) : (SRequires + "<color=red> " + DataManager.Instance.Followers.Count + "</color> / " + FollowerCount + " " + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.FOLLOWERS)));
		if (SacrificeFollowerLevel == -1)
		{
			return;
		}
		if (HaveFollowers)
		{
			if (TimeManager.TotalElapsedGameTime - DataManager.Instance.OpenedDoorTimestamp >= 1200f)
			{
				string arg = "<color=#FFD201>" + ScriptLocalization.Interactions.Level + " " + SacrificeFollowerLevel + "</color><sprite name=\"icon_Ascend\">";
				base.Label = string.Format(LocalizationManager.GetTranslation("Interactions/SacrificeFollowerToOpen"), arg) + string.Format(" | ({0}/{1} {2})", DataManager.Instance.Followers.Count, FollowerCount, FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.FOLLOWERS));
			}
			else
			{
				Interactable = false;
				base.Label = ScriptLocalization.Interactions.Recharging;
			}
		}
		else
		{
			Interactable = false;
		}
	}

	public override void IndicateHighlighted()
	{
		if (!Unlocked)
		{
			base.IndicateHighlighted();
		}
	}

	private void OpenDoor()
	{
		if (Unlocked && !Blocking)
		{
			CollideForDoor.enabled = true;
			BlockingCollider.enabled = false;
			Lights.SetActive(true);
		}
		else
		{
			CollideForDoor.enabled = false;
			BlockingCollider.enabled = true;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (SacrificeFollowerLevel != -1)
		{
			if (FollowerBrain.AllBrains.Count > 0)
			{
				base.OnInteract(state);
				PlayerFarming.Instance.GoToAndStop(base.transform.position + new Vector3(-1f, -2.5f), base.gameObject, false, false, delegate
				{
					StartCoroutine(SacrificeFollowerRoutine());
				});
			}
			else
			{
				IndicateHighlighted();
				AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.transform.position);
				MonoSingleton<Indicator>.Instance.PlayShake();
			}
		}
		else if (HaveFollowers)
		{
			StartCoroutine(DoRitualRoutine());
		}
		else
		{
			IndicateHighlighted();
			AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.transform.position);
			MonoSingleton<Indicator>.Instance.PlayShake();
		}
	}

	private IEnumerator SacrificeFollowerRoutine()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject);
		yield return new WaitForSeconds(0.5f);
		List<FollowerBrain> list = new List<FollowerBrain>();
		foreach (FollowerBrain item in FollowerBrain.AllAvailableFollowerBrains())
		{
			if (item.Info.XPLevel >= SacrificeFollowerLevel)
			{
				list.Add(item);
			}
		}
		UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
	//	followerSelectInstance.VotingType = TwitchVoting.VotingType.SACRIFICE_TO_DOOR;
		followerSelectInstance.Show(list);
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance;
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/select_follower");
			StartCoroutine(SpawnFollower(followerInfo.ID));
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectInstance;
		uIFollowerSelectMenuController2.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController2.OnHidden, (Action)delegate
		{
			followerSelectInstance = null;
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController3 = followerSelectInstance;
		uIFollowerSelectMenuController3.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController3.OnCancel, (Action)delegate
		{
			GameManager.GetInstance().OnConversationEnd();
		});
	}

	private IEnumerator SpawnFollower(int ID)
	{
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		DataManager.Instance.OpenedDoorTimestamp = TimeManager.TotalElapsedGameTime;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject);
		yield return new WaitForSeconds(1f);
		FollowerManager.FindFollowerInfo(ID).Outfit = FollowerOutfitType.Worker;
		FollowerManager.SpawnedFollower spawnedFollower = FollowerManager.SpawnCopyFollower(FollowerManager.FindFollowerInfo(ID), base.transform.position + Vector3.down / 3f + Vector3.right / 1.8f, base.transform.parent, PlayerFarming.Location);
		spawnedFollower.Follower.gameObject.SetActive(false);
		portal.gameObject.SetActive(true);
		GameManager.GetInstance().OnConversationNext(portal.gameObject, 7f);
		yield return new WaitForSeconds(0.5f);
		spawnedFollower.Follower.gameObject.SetActive(true);
		spawnedFollower.Follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		spawnedFollower.Follower.Spine.AnimationState.SetAnimation(0, "sacrifice-door", false);
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", spawnedFollower.Follower.gameObject);
		spawnedFollower.Follower.Spine.AnimationState.Event += HandleAnimationStateEvent;
		FollowerManager.RemoveFollowerBrain(ID);
		yield return new WaitForSeconds(2f);
		UIManager.PlayAudio("event:/Stings/thenight_sacrifice_followers");
		float Progress = 0f;
		float Duration = 6.75f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			GameManager.GetInstance().CameraSetTargetZoom(Mathf.Lerp(9f, 4f, Mathf.SmoothStep(0f, 1f, Progress / Duration)));
			CameraManager.shakeCamera(0.3f + 0.5f * (Progress / Duration));
			yield return null;
		}
		CameraManager.instance.ShakeCameraForDuration(0.5f, 0.7f, 0.3f);
		spawnedFollower.Follower.Spine.AnimationState.Event -= HandleAnimationStateEvent;
		FollowerManager.CleanUpCopyFollower(spawnedFollower);
		SimpleSetCamera.Play();
		yield return StartCoroutine(OpenDoorRoutine());
		GameManager.GetInstance().OnConversationEnd();
	}

	private void HandleAnimationStateEvent(TrackEntry trackentry, Spine.Event e)
	{
		Debug.Log(e.Data.Name.Colour(Color.yellow));
		string text = e.Data.Name;
		if (text == "door-sacrifice")
		{
			UIManager.PlayAudio("event:/rituals/door_sacrifice");
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		for (int num = spawnedFollowers.Count - 1; num >= 0; num--)
		{
			FollowerManager.CleanUpCopyFollower(spawnedFollowers[num]);
		}
	}

	public void Play()
	{
		StartCoroutine(OpenDoorRoutine());
	}

	public void Block()
	{
		Debug.Log("BLOCK ME!");
		Blocking = true;
		DoorToMove.transform.DOLocalMove(Vector3.zero, 1f).SetEase(Ease.OutSine);
		Unlocked = false;
		OpenDoor();
	}

	public void Unblock()
	{
		Blocking = false;
		if (DataManager.Instance.UnlockedDungeonDoor.Contains(Location))
		{
			DoorToMove.transform.DOLocalMove(OpenDoorPosition, 1f).SetEase(Ease.OutSine);
			Unlocked = true;
			OpenDoor();
		}
	}

	private IEnumerator DoRitualRoutine()
	{
		spineMaterialOverride.enabled = true;
		yield return null;
		SimulationManager.Pause();
		bool Waiting = true;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject);
		PlayerFarming.Instance.GoToAndStop(RitualPosition.transform.position + new Vector3(0f, -2f), RitualPosition, false, false, delegate
		{
			PlayerFarming.Instance.transform.position = RitualPosition.transform.position + new Vector3(0f, -2f);
			Waiting = false;
		});
		yield return new WaitForSeconds(1f);
		List<FollowerBrain> brains = new List<FollowerBrain>(FollowerBrain.AllBrains);
		for (int num = brains.Count - 1; num >= 0; num--)
		{
			if (DataManager.Instance.Followers_Dead.Contains(brains[num]._directInfoAccess) || FollowerManager.FollowerLocked(brains[num].Info.ID))
			{
				brains.RemoveAt(num);
			}
		}
		yield return new WaitForSeconds(1f);
		for (int i = 0; i < brains.Count; i++)
		{
			FollowerManager.SpawnedFollower spawnedFollower2 = FollowerManager.SpawnCopyFollower(brains[i]._directInfoAccess, RitualPosition.transform.position, base.transform.parent, PlayerFarming.Location);
			spawnedFollowers.Add(spawnedFollower2);
			spawnedFollower2.Follower.transform.position = GetFollowerPosition(i, brains.Count);
			spawnedFollower2.Follower.State.facingAngle = ((spawnedFollower2.Follower.transform.position.x > 0f) ? 180 : 0);
			spawnedFollower2.Follower.State.LookAngle = spawnedFollower2.Follower.State.facingAngle;
			spawnedFollower2.Follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
			spawnedFollower2.Follower.SetFaceAnimation("Emotions/emotion-happy", true);
			if ((bool)spawnedFollower2.Follower.GetComponentInChildren<ShadowLockToGround>())
			{
				spawnedFollower2.Follower.GetComponentInChildren<ShadowLockToGround>().enabled = false;
			}
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", spawnedFollower2.Follower.gameObject);
			spawnedFollower2.Follower.TimedAnimation("spawn-in", 7f / 15f, delegate
			{
				spawnedFollower2.Follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
				spawnedFollower2.Follower.SetBodyAnimation("dance", true);
			});
			yield return new WaitForSeconds(0.05f);
			if (spawnedFollower2.Follower.SkeletonAnimationLODManager != null)
			{
				spawnedFollower2.Follower.SkeletonAnimationLODManager.DisableLODManager(true);
			}
		}
		while (Waiting)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNext(CrownStatueController.Instance.CameraPosition, 10f);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/door-ritual", 0, false);
		AudioManager.Instance.PlayOneShot("event:/Stings/white_eyes", PlayerFarming.Instance.gameObject);
		RitualLighting.SetActive(true);
		BiomeConstants.Instance.ImpactFrameForDuration();
		LoopedSound = AudioManager.Instance.CreateLoop("event:/door/eye_beam_door_open", true);
		MMVibrate.RumbleContinuous(0f, 2f);
		CameraManager.instance.ShakeCameraForDuration(0.6f, 0.7f, 2f);
		BiomeConstants.Instance.ChromaticAbberationTween(1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 0.75f);
		PlayerFarming.Instance.Spine.AnimationState.Event += HandleEvent;
		yield return new WaitForSeconds(1f);
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 6f, 5f).SetEase(Ease.InOutSine);
		NumGivingDevotion = 0;
		foreach (FollowerManager.SpawnedFollower spawnedFollower3 in spawnedFollowers)
		{
			NumGivingDevotion++;
			spawnedFollower3.Follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
			spawnedFollower3.Follower.SetBodyAnimation("worship", true);
			StartCoroutine(SpawnSouls(spawnedFollower3.Follower.Spine.transform.position));
			yield return new WaitForSeconds(0.1f);
		}
		while (NumGivingDevotion > 0)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		PlayerFarming.Instance.Spine.AnimationState.Event -= HandleEvent;
		MMVibrate.StopRumble();
		RitualLighting.SetActive(false);
		BiomeConstants.Instance.ChromaticAbberationTween(1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		AudioManager.Instance.StopLoop(LoopedSound);
		yield return StartCoroutine(OpenDoorRoutine());
		yield return new WaitForSeconds(1f);
		foreach (FollowerManager.SpawnedFollower spawnedFollower in spawnedFollowers)
		{
			StartCoroutine(PlaySoundDelay(spawnedFollower.Follower.gameObject));
			spawnedFollower.Follower.TimedAnimation("spawn-out", 13f / 15f, delegate
			{
				FollowerManager.CleanUpCopyFollower(spawnedFollower);
				if (spawnedFollower.Follower.SkeletonAnimationLODManager != null)
				{
					spawnedFollower.Follower.SkeletonAnimationLODManager.DisableLODManager(false);
				}
			});
			yield return new WaitForSeconds(0.1f);
		}
		spawnedFollowers.Clear();
	}

	private IEnumerator PlaySoundDelay(GameObject spawnedFollower)
	{
		yield return new WaitForSeconds(17f / 30f);
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", spawnedFollower);
	}

	private IEnumerator SpawnSouls(Vector3 Position)
	{
		float delay = 0.5f;
		float Count = 8f;
		for (int i = 0; (float)i < Count; i++)
		{
			float num = (float)i / Count;
			SoulCustomTargetLerp.Create(RitualReceiveDevotionPosition.gameObject, Position + Vector3.forward * 2f + Vector3.up, 0.5f, Color.red);
			yield return new WaitForSeconds(delay - 0.2f * num);
		}
		NumGivingDevotion--;
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
		else if (e.Data.Name == "sfxTrigger")
		{
			AudioManager.Instance.CreateLoop("event:/Stings/lamb_ascension", PlayerFarming.Instance.gameObject, true);
		}
	}

	private Vector3 GetFollowerPosition(int index, int total)
	{
		float num;
		float f;
		if (total <= 12)
		{
			num = 3f;
			f = (float)index * (360f / (float)total) * ((float)Math.PI / 180f);
			return RitualPosition.transform.position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
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
		return RitualPosition.transform.position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
	}

	public void FadeDoorLight()
	{
		DoorLights.SetActive(true);
		SpriteRenderer component = DoorLights.GetComponent<SpriteRenderer>();
		Color color = component.color;
		color.a = 0f;
		component.color = color;
		component.DOFade(1f, 2f);
		DataManager.Instance.IntroDoor1 = true;
	}

	public IEnumerator OpenDoorRoutine()
	{
		if (!LetterBox.IsPlaying)
		{
			GameManager.GetInstance().OnConversationNew();
			SimpleSetCamera.Play();
		}
		if (!DataManager.Instance.UnlockedDungeonDoor.Contains(Location))
		{
			DataManager.Instance.UnlockedDungeonDoor.Add(Location);
		}
		AudioManager.Instance.PlayOneShot("event:/door/door_unlock", base.gameObject);
		yield return new WaitForSeconds(1f);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.3f);
		yield return new WaitForSeconds(0.2f);
		AudioManager.Instance.PlayOneShot("event:/door/door_lower", base.gameObject);
		doorSmokeParticleSystem.Play();
		float Progress = 0f;
		float Duration = 3f;
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, Duration);
		Vector3 StartingPosition = DoorToMove.transform.position;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			DoorToMove.transform.position = Vector3.Lerp(StartingPosition, StartingPosition + OpenDoorPosition, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/door/door_done", base.gameObject);
		doorSmokeParticleSystem.Stop();
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.3f);
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
		SimpleSetCamera.Reset();
		spineMaterialOverride.enabled = false;
		Unlocked = true;
		OpenDoor();
		Lights.SetActive(true);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player" && !Used)
		{
			AudioManager.Instance.StopCurrentMusic();
			AudioManager.Instance.StopCurrentAtmos();
			AudioManager.Instance.PlayOneShot("event:/Stings/boss_door_complete");
			AudioManager.Instance.PlayOneShot("event:/ui/map_location_appear");
			PlayerFarming.Instance.GetBlackSoul(Mathf.RoundToInt(FaithAmmo.Total - FaithAmmo.Ammo), false);
			Used = true;
			MMTransition.StopCurrentTransition();
			GetFloor(Location);
			MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, SceneName, 1f, "", FadeSave);
			GameManager.GetInstance().OnConversationNew();
		}
	}

	public static void GetFloor(FollowerLocation Location)
	{
		DataManager.LocationAndLayer locationAndLayer = DataManager.LocationAndLayer.ContainsLocation(Location, DataManager.Instance.CachePreviousRun);
		int num = 0;
		int num2 = 4;
		num = DataManager.Instance.GetDungeonLayer(Location);
		bool flag = num >= num2 || DataManager.Instance.DungeonCompleted(Location);
		if (GameManager.Layer2)
		{
			num = DataManager.GetGodTearNotches(Location) + 1;
		}
		DataManager.Instance.DungeonBossFight = num >= num2 && !DataManager.Instance.DungeonCompleted(Location, GameManager.Layer2);
		if (flag)
		{
			num = DataManager.RandomSeed.Next(1, num2 + 1);
			if (locationAndLayer != null)
			{
				while (num == locationAndLayer.Layer)
				{
					num = DataManager.RandomSeed.Next(1, num2 + 1);
				}
			}
		}
		GameManager.DungeonUseAllLayers = flag;
		if (flag)
		{
			GameManager.CurrentDungeonLayer = 4;
		}
		else
		{
			GameManager.NextDungeonLayer(num);
		}
		GameManager.NewRun("", false, Location);
		if (locationAndLayer != null)
		{
			locationAndLayer.Layer = num;
			Debug.Log("Now set cached layer to: " + locationAndLayer.Layer);
		}
		else
		{
			DataManager.Instance.CachePreviousRun.Add(new DataManager.LocationAndLayer(Location, num));
		}
	}

	private void FadeSave()
	{
		SaveAndLoad.Save();
	}
}
