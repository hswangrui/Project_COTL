using System;
using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using I2.Loc;
using MMBiomeGeneration;
using Spine.Unity;
using UnityEngine;
using WebSocketSharp;

public class Interaction_Follower : Interaction
{
	private Villager_Info v_i;

	protected WorshipperInfoManager wim;

	private StateMachine RecruitState;

	public StateMachine StateMachine;

	public bool ForceSpecificSkin;

	[SpineSkin("", "", true, false, false)]
	public string ForceSkin = "";

	[SerializeField]
	private SkeletonAnimation portalSpine;

	public ParticleSystem recruitParticles;

	public SkeletonAnimation skeletonAnimation;

	public FollowerInfo followerInfo;

	private string q1Title = "Interactions/FollowerSpawn/Convert/Title";

	private string q1Description = "Interactions/FollowerSpawn/Convert/Description";

	private string q2Title = "Interactions/FollowerSpawn/Consume/Title";

	private string q2Description = "Interactions/FollowerSpawn/Consume/Description";

	private string skin;

	public Action followerInfoAssigned;

	private Objectives_FindFollower findFollowerObjective;

	private string sRescue;

	[HideInInspector]
	public int Cost;

	protected bool Activated;

	private int Souls;

	private StateMachine CompanionState;

	public GameObject ConversionBone;

	private EventInstance receiveLoop;

	protected virtual void Start()
	{
		RecruitState = GetComponent<StateMachine>();
		skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
		UpdateLocalisation();
		StateMachine = GetComponent<StateMachine>();
		skin = (ForceSpecificSkin ? ForceSkin : "");
		if (!ForceSpecificSkin)
		{
			skin = DataManager.GetRandomLockedSkin();
			if (skin.IsNullOrEmpty())
			{
				skin = DataManager.GetRandomSkin();
			}
		}
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			if (objective.Type == Objectives.TYPES.FIND_FOLLOWER && ((Objectives_FindFollower)objective).TargetLocation == BiomeGenerator.Instance.DungeonLocation)
			{
				findFollowerObjective = (Objectives_FindFollower)objective;
				break;
			}
		}
		followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base, skin);
		if (followerInfo.SkinName == "Giraffe")
		{
			followerInfo.Name = LocalizationManager.GetTranslation("FollowerNames/Sparkles");
		}
		if (findFollowerObjective != null)
		{
			followerInfo.SkinName = findFollowerObjective.FollowerSkin;
			if (!followerInfo.SkinName.Contains("Boss"))
			{
				followerInfo.SkinCharacter = WorshipperData.Instance.GetSkinIndexFromName(followerInfo.SkinName.StripNumbers());
			}
			else
			{
				followerInfo.SkinCharacter = WorshipperData.Instance.GetSkinIndexFromName(followerInfo.SkinName);
			}
			followerInfo.Name = findFollowerObjective.TargetFollowerName;
			skin = followerInfo.SkinName;
		}
		v_i = Villager_Info.NewCharacter(skin);
		wim = GetComponent<WorshipperInfoManager>();
		wim.SetV_I(v_i);
		ActivateDistance = 3f;
		Action action = followerInfoAssigned;
		if (action != null)
		{
			action();
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sRescue = ScriptLocalization.Interactions.Rescue;
	}

	public override void GetLabel()
	{
		base.Label = ((Activated || !Interactable) ? "" : sRescue);
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activated)
		{
			EndIndicateHighlighted();
			base.state = state;
			Activated = true;
			GameManager.GetInstance().OnConversationNew(false);
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
			PlayerFarming.Instance.GoToAndStop(base.transform.position + Vector3.left * 1.5f, base.gameObject, false, false, delegate
			{
				StartCoroutine(FollowerChoiceIE());
			});
		}
	}

	private IEnumerator GiveSouls()
	{
		yield return new WaitForSeconds(1.5f);
		Souls = Cost;
		while (--Souls >= 0)
		{
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", state.transform.position);
			ResourceCustomTarget.Create(ConversionBone, state.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
			yield return new WaitForSeconds(0.2f);
		}
		Inventory.ChangeItemQuantity(20, Cost);
	}

	private IEnumerator PositionPlayer()
	{
		yield return new WaitForSeconds(0.25f);
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		Vector3 TargetPosition = base.transform.position + ((state.transform.position.x < base.transform.position.x) ? Vector3.left : Vector3.right) * 1.5f;
		while (Vector3.Distance(state.transform.position, TargetPosition) > 0.1f)
		{
			state.transform.position = Vector3.Lerp(state.transform.position, TargetPosition, 2f * Time.deltaTime);
			yield return null;
		}
	}

	private IEnumerator ConvertToWarrior()
	{
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(ConversionBone, 4f);
		GameManager.GetInstance().AddPlayerToCamera();
		state.GetComponent<HealthPlayer>().untouchable = true;
		yield return new WaitForSeconds(0.25f);
		SimpleSpineAnimator playerAnimator = state.GetComponentInChildren<SimpleSpineAnimator>();
		playerAnimator.Animate("floating", 0, true);
		yield return new WaitForSeconds(1f);
		CameraManager.shakeCamera(0.5f, UnityEngine.Random.Range(0, 360));
		if (!DataManager.GetFollowerSkinUnlocked(skin))
		{
			DataManager.SetFollowerSkinUnlocked(skin);
		}
		FollowerManager.CreateNewRecruit(followerInfo, NotificationCentre.NotificationType.NewRecruit);
		RoomLockController.RoomCompleted();
		Thought thought = ((UnityEngine.Random.value < 0.7f) ? Thought.GratefulRecued : ((!(UnityEngine.Random.value <= 0.3f)) ? Thought.InstantBelieverRescued : Thought.ResentfulRescued));
		ThoughtData data = FollowerThoughts.GetData(thought);
		data.Init();
		followerInfo.Thoughts.Add(data);
		playerAnimator.Animate("floating-land", 0, false);
		playerAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(2f);
		GameManager.GetInstance().OnConversationEnd();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator FollowerChoiceIE()
	{
		Interactable = false;
		GameManager.GetInstance().OnConversationNext(ConversionBone, 4f);
		GameManager.GetInstance().AddPlayerToCamera();
		if (true)
		{
			yield return StartCoroutine(ConvertIE());
		}
		else
		{
			yield return StartCoroutine(ConsumefollowerRoutine());
		}
		RoomLockController.RoomCompleted();
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(1f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator ConvertIE()
	{
		AudioManager.Instance.PlayOneShot("event:/followers/rescue", base.gameObject);
		skeletonAnimation.AnimationState.SetAnimation(0, "convert-short", false);
		AudioManager.Instance.PlayOneShot("event:/dialogue/followers/positive_acknowledge", base.gameObject);
		recruitParticles.Play();
		portalSpine.gameObject.SetActive(true);
		portalSpine.AnimationState.SetAnimation(0, "convert-short", false);
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_start", PlayerFarming.Instance.gameObject);
		receiveLoop = AudioManager.Instance.CreateLoop("event:/player/receive_animation_loop", PlayerFarming.Instance.gameObject, true);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		float duration = PlayerFarming.Instance.simpleSpineAnimator.Animate("specials/special-activate-long", 0, true).Animation.Duration;
		CameraManager.shakeCamera(0.5f, UnityEngine.Random.Range(0, 360));
		yield return new WaitForSeconds(duration - 1f);
		FollowerManager.CreateNewRecruit(followerInfo, NotificationCentre.NotificationType.NewRecruit);
		DataManager.SetFollowerSkinUnlocked(followerInfo.SkinName);
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_end", PlayerFarming.Instance.gameObject);
		receiveLoop.stop(STOP_MODE.ALLOWFADEOUT);
		if (findFollowerObjective != null)
		{
			findFollowerObjective.Complete();
			ObjectiveManager.UpdateObjective(findFollowerObjective);
			StoryData followerStoryData = Quests.GetFollowerStoryData(findFollowerObjective.Follower);
			if (followerStoryData != null)
			{
				foreach (StoryDataItem item in Quests.GetChildStoryDataItemsFromStoryDataItem(followerStoryData.EntryStoryItem))
				{
					item.TargetFollowerID_1 = followerInfo.ID;
				}
			}
		}
		float value = UnityEngine.Random.value;
		Thought thought = Thought.None;
		if (value < 0.7f)
		{
			value = UnityEngine.Random.value;
			if (value <= 0.3f)
			{
				thought = Thought.HappyConvert;
			}
			else if (value > 0.3f && value < 0.6f)
			{
				thought = Thought.GratefulConvert;
			}
			else if (value >= 0.6f)
			{
				thought = Thought.SkepticalConvert;
			}
		}
		else
		{
			value = UnityEngine.Random.value;
			thought = ((!(value <= 0.3f) || DataManager.Instance.Followers.Count <= 0) ? Thought.InstantBelieverConvert : Thought.ResentfulConvert);
		}
		ThoughtData data = FollowerThoughts.GetData(thought);
		data.Init();
		followerInfo.Thoughts.Add(data);
	}

	private IEnumerator ConsumefollowerRoutine()
	{
		GameManager.GetInstance().OnConversationNext(base.gameObject, 5f);
		GameManager.GetInstance().AddPlayerToCamera();
		Vector3 vector = ((state.transform.position.x < base.transform.position.x) ? Vector3.left : Vector3.right);
		Vector3 targetPosition = base.transform.position + vector * 2f;
		PlayerFarming.Instance.GoToAndStop(targetPosition);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("sacrifice", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/dialogue/followers/negative_acknowledge", base.gameObject);
		skeletonAnimation.AnimationState.SetAnimation(0, "sacrifice", false);
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 6f, 2f).SetEase(Ease.InOutSine);
		yield return new WaitForSeconds(3.2333333f);
		int i = 0;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= 50)
			{
				break;
			}
			BlackSoul blackSoul = InventoryItem.SpawnBlackSoul(1, base.transform.position + Vector3.back);
			if (blackSoul != null)
			{
				blackSoul.SetAngle(270 + UnityEngine.Random.Range(-90, 90), new Vector2(2f, 4f));
			}
			yield return new WaitForSeconds(0.01f);
		}
		yield return new WaitForSeconds(1f);
	}
}
