using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using MMTools;
using Spine;
using Spine.Unity;
using Unify;
using UnityEngine;

public class FollowerRecruit : Interaction
{
	public delegate void FollowerEventDelegate(FollowerInfo info);

	public GameObject Menu;

	public GameObject MenuRecruit;

	public Follower Follower;

	public GameObject CameraBone;

	public AudioClip SacrificeSting;

	public interaction_FollowerInteraction FollowerInteraction;

	[SerializeField]
	private SkeletonAnimation portalSpine;

	public Action StatueCallback;

	public bool triggered;

	private const float triggerDistance = 10f;

	public static FollowerEventDelegate OnFollowerRecruited;

	private SkeletonAnimationLODManager skeletonAnimationLODManager;

	private string dString;

	private bool Activating;

	public GameObject FollowerRoleMenu;

	public BiomeLightingSettings LightingSettings;

	public OverrideLightingProperties overrideLightingProperties;

	private List<MeshRenderer> FollowersTurnedOff = new List<MeshRenderer>();

	public static Action OnNewRecruit;

	public bool RecruitOnComplete = true;

	private void Start()
	{
		UpdateLocalisation();
		IgnoreTutorial = true;
		Follower.State.CURRENT_STATE = StateMachine.State.Idle;
		ActivateDistance = 2f;
		Interactable = false;
		SecondaryInteractable = false;
		Follower.Brain.Info.Outfit = FollowerOutfitType.Rags;
		Follower.SetOutfit(FollowerOutfitType.Rags, false);
		Follower.Spine.AnimationState.Event += FollowerEvent;
		UIFollowerName componentInChildren = GetComponentInChildren<UIFollowerName>();
		if ((object)componentInChildren != null)
		{
			componentInChildren.Hide(false);
		}
		if (TryGetComponent<SkeletonAnimationLODManager>(out skeletonAnimationLODManager))
		{
			skeletonAnimationLODManager.DisableLODManager(true);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (triggered && !Interactable)
		{
			StartCoroutine(DelayedInteractable());
		}
	}

	public void SpawnAnim(bool pause = false)
	{
		Follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		Follower.SetBodyAnimation("spawn-in-base", false);
		Follower.AddBodyAnimation("pray", true, 0f);
		if (Follower.Brain._directInfoAccess.StartingCursedState == Thought.OldAge)
		{
			Follower.SetOutfit(FollowerOutfitType.Old, false);
		}
		else if (Follower.Brain._directInfoAccess.StartingCursedState == Thought.Ill)
		{
			Follower.SetFaceAnimation("Emotions/emotion-sick", true);
		}
		else if (Follower.Brain._directInfoAccess.StartingCursedState == Thought.BecomeStarving)
		{
			Follower.SetFaceAnimation("Emotions/emotion-unhappy", true);
		}
		else if (Follower.Brain._directInfoAccess.StartingCursedState == Thought.Dissenter)
		{
			Follower.SetFaceAnimation("Emotions/emotion-dissenter", true);
			Follower.SetFaceAnimation("Emotions/emotion-dissenter", true);
			Follower.SetOutfit(FollowerOutfitType.Rags, false, Follower.Brain._directInfoAccess.StartingCursedState);
		}
		if (pause)
		{
			Follower.Spine.AnimationState.TimeScale = 0f;
			return;
		}
		portalSpine.gameObject.SetActive(true);
		GameManager.GetInstance().StartCoroutine(ShowPortal());
		AudioManager.Instance.PlayOneShot("event:/followers/teleport_to_base", Follower.gameObject);
		Follower.Spine.AnimationState.TimeScale = 1f;
	}

	private IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator ShowPortal()
	{
		while (portalSpine.AnimationState == null)
		{
			yield return null;
		}
		portalSpine.AnimationState.SetAnimation(0, "spawn-in-base", false);
	}

	private IEnumerator DelayedInteractable()
	{
		yield return new WaitForSeconds(2f);
		Interactable = true;
		SecondaryInteractable = true;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		dString = ScriptLocalization.Interactions.Indoctrinate;
	}

	public void ManualTriggerAnimateIn()
	{
		Follower.Spine.gameObject.SetActive(true);
		SpawnAnim();
		StartCoroutine(DelayedInteractable());
		triggered = true;
	}

	protected override void Update()
	{
		base.Update();
		if (!triggered && (bool)PlayerFarming.Instance && Vector3.Distance(PlayerFarming.Instance.transform.position, base.transform.position) < 10f && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.InActive)
		{
			Follower.Spine.gameObject.SetActive(true);
			SpawnAnim();
			StartCoroutine(DelayedInteractable());
			triggered = true;
		}
	}

	public override void GetLabel()
	{
		base.Label = ((!Interactable || Activating || !triggered) ? "" : dString);
	}

	public override void GetSecondaryLabel()
	{
		base.SecondaryLabel = "";
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Activating = true;
		PlayerFarming.Instance.unitObject.speed = 0f;
		StartCoroutine(FrameDelay(delegate
		{
			DoRecruit(state, true);
			BiomeConstants.Instance.DepthOfFieldTween(1.5f, 4.5f, 10f, 1f, 145f);
			SimulationManager.Pause();
		}));
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		base.OnSecondaryInteract(state);
		Activating = true;
		PlayerFarming.Instance.GoToAndStop(base.transform.position + ((state.transform.position.x < base.transform.position.x) ? new Vector3(-1.5f, -0.5f) : new Vector3(1.5f, -0.5f)), base.gameObject, false, false, delegate
		{
			DoRecruit(state, false);
		});
	}

	private void CallbackClose()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	public void DoRecruit(StateMachine state, bool customise, bool newRecruit = true)
	{
		base.state = state;
		GameManager.GetInstance().CamFollowTarget.SnappyMovement = true;
		AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("FIRST_FOLLOWER"));
		if (DataManager.Instance.Followers.Count >= 4)
		{
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("GAIN_FIVE_FOLLOWERS"));
		}
		if (DataManager.Instance.Followers.Count >= 9)
		{
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("TEN_FOLLOWERS"));
		}
		if (DataManager.Instance.Followers.Count >= 19)
		{
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("TWENTY_FOLLOWERS"));
		}
		if (newRecruit)
		{
			foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
			{
				if (allBrain != Follower.Brain && allBrain.Location == Follower.Brain.Location)
				{
					allBrain.AddThought(Thought.CultHasNewRecruit);
				}
			}
		}
		DataManager.Instance.GameOverEnabled = true;
		if (DataManager.Instance.InGameOver)
		{
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.GameOver);
			DataManager.Instance.InGameOver = false;
			DataManager.Instance.DisplayGameOverWarning = false;
			DataManager.Instance.GameOver = false;
		}
		FollowerEventDelegate onFollowerRecruited = OnFollowerRecruited;
		if (onFollowerRecruited != null)
		{
			onFollowerRecruited(Follower.Brain._directInfoAccess);
		}
		CompleteCallBack(FollowerRole.Worker, customise);
	}

	private void CompleteCallBack(FollowerRole FollowerRole, bool customise)
	{
		PlayerFarming.Instance.GoToAndStop(base.transform.position + new Vector3(-1.5f, -0.1f), base.gameObject, false, false, delegate
		{
			PlayerFarming.Instance.transform.position = base.transform.position + new Vector3(-1.5f, -0.1f);
		});
		Follower.Brain.Info.FollowerRole = FollowerRole;
		List<ConversationEntry> list = new List<ConversationEntry>();
		List<string> list2 = new List<string> { "Conversation_NPC/FollowerSpawn/Line1", "Conversation_NPC/FollowerSpawn/Line2" };
		ConversationEntry conversationEntry = new ConversationEntry(base.gameObject, list2[UnityEngine.Random.Range(0, list2.Count)]);
		conversationEntry.soundPath = "event:/dialogue/followers/general_talk";
		conversationEntry.pitchValue = Follower.Brain._directInfoAccess.follower_pitch;
		conversationEntry.vibratoValue = Follower.Brain._directInfoAccess.follower_vibrato;
		conversationEntry.followerID = Follower.Brain.Info.ID;
		conversationEntry.SetZoom = true;
		conversationEntry.Zoom = 4f;
		conversationEntry.Offset = new Vector3(0f, -0.5f, 0f);
		conversationEntry.CharacterName = "<color=yellow>" + Follower.Brain.Info.Name + "</color>";
		list.Add(conversationEntry);
		if (Follower.Brain.Info.ID == FollowerManager.DeathCatID && !DataManager.Instance.LeaderFollowersRecruited.Contains(Follower.Brain.Info.ID))
		{
			DataManager.Instance.LeaderFollowersRecruited.Add(Follower.Brain.Info.ID);
		}
		if (Follower.Brain.Info.ID == FollowerManager.LeshyID || Follower.Brain.Info.ID == FollowerManager.HeketID || Follower.Brain.Info.ID == FollowerManager.KallamarID || Follower.Brain.Info.ID == FollowerManager.ShamuraID)
		{
			if (!DataManager.Instance.LeaderFollowersRecruited.Contains(Follower.Brain.Info.ID))
			{
				DataManager.Instance.LeaderFollowersRecruited.Add(Follower.Brain.Info.ID);
			}
			list.Clear();
			string termToSpeak = "Conversation_NPC/FollowerSpawn/Leshy/0";
			string soundPath = "event:/dialogue/followers/boss/fol_leshy";
			if (Follower.Brain.Info.ID == FollowerManager.HeketID)
			{
				termToSpeak = "Conversation_NPC/FollowerSpawn/Heket/0";
				soundPath = "event:/dialogue/followers/boss/fol_heket";
			}
			else if (Follower.Brain.Info.ID == FollowerManager.KallamarID)
			{
				termToSpeak = "Conversation_NPC/FollowerSpawn/Kallamar/0";
				soundPath = "event:/dialogue/followers/boss/fol_kallamar";
			}
			else if (Follower.Brain.Info.ID == FollowerManager.ShamuraID)
			{
				termToSpeak = "Conversation_NPC/FollowerSpawn/Shamura/0";
				soundPath = "event:/dialogue/followers/boss/fol_shamura";
			}
			ConversationEntry conversationEntry2 = ConversationEntry.Clone(conversationEntry);
			conversationEntry.TermToSpeak = termToSpeak;
			conversationEntry.followerID = Follower.Brain.Info.ID;
			conversationEntry.soundPath = soundPath;
			int num = 0;
			while (true)
			{
				string termToSpeak2 = ConversationEntry.Clone(conversationEntry).TermToSpeak;
				int num2 = ++num;
				if (LocalizationManager.GetTermData(termToSpeak2.Replace("0", num2.ToString())) == null)
				{
					break;
				}
				conversationEntry2 = ConversationEntry.Clone(conversationEntry);
				conversationEntry2.TermToSpeak = conversationEntry2.TermToSpeak.Replace("0", num.ToString());
				list.Add(conversationEntry2);
			}
		}
		GameManager.GetInstance().CamFollowTarget.ZoomSpeedConversation = 1f;
		GameManager.GetInstance().CamFollowTarget.MaxZoomInConversation = 100f;
		MMConversation.Play(new ConversationObject(list, null, delegate
		{
			StartCoroutine(SimpleNewRecruitRoutine(true));
		}), false);
	}

	private IEnumerator SimpleNewRecruitRoutine(bool customise)
	{
		GameManager.GetInstance().OnConversationNext(CameraBone, 4f);
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(base.transform.position, Follower.transform.position);
		yield return new WaitForSeconds(0.3f);
		if (customise)
		{
			FollowersTurnedOff.Clear();
			foreach (Follower item in FollowerManager.ActiveLocationFollowers())
			{
				SkeletonAnimation spine = item.Spine;
				if (spine.gameObject.activeSelf && Vector3.Distance(spine.transform.position, base.transform.position) < 1f && spine.transform.position.y < base.transform.position.y)
				{
					Debug.Log("Turning off gameobject: " + spine.name);
					MeshRenderer component = spine.gameObject.GetComponent<MeshRenderer>();
					component.enabled = false;
					FollowersTurnedOff.Add(component);
				}
			}
			GameManager.GetInstance().CameraSetOffset(new Vector3(-2f, 0f, 0f));
			UIFollowerIndoctrinationMenuController indoctrinationMenuInstance = MonoSingleton<UIManager>.Instance.ShowIndoctrinationMenu(Follower);
			UIFollowerIndoctrinationMenuController uIFollowerIndoctrinationMenuController = indoctrinationMenuInstance;
			uIFollowerIndoctrinationMenuController.OnIndoctrinationCompleted = (Action)Delegate.Combine(uIFollowerIndoctrinationMenuController.OnIndoctrinationCompleted, (Action)delegate
			{
				StartCoroutine(CharacterSetupCallback());
			});
			UIFollowerIndoctrinationMenuController uIFollowerIndoctrinationMenuController2 = indoctrinationMenuInstance;
			uIFollowerIndoctrinationMenuController2.OnShown = (Action)Delegate.Combine(uIFollowerIndoctrinationMenuController2.OnShown, (Action)delegate
			{
				LightingManager.Instance.inOverride = true;
				LightingSettings.overrideLightingProperties = overrideLightingProperties;
				LightingManager.Instance.overrideSettings = LightingSettings;
				LightingManager.Instance.transitionDurationMultiplier = 0f;
				LightingManager.Instance.UpdateLighting(true);
			});
			UIFollowerIndoctrinationMenuController uIFollowerIndoctrinationMenuController3 = indoctrinationMenuInstance;
			uIFollowerIndoctrinationMenuController3.OnHide = (Action)Delegate.Combine(uIFollowerIndoctrinationMenuController3.OnHide, (Action)delegate
			{
				foreach (MeshRenderer item2 in FollowersTurnedOff)
				{
					item2.enabled = true;
				}
				FollowersTurnedOff.Clear();
				LightingManager.Instance.inOverride = false;
				LightingManager.Instance.overrideSettings = null;
				LightingManager.Instance.transitionDurationMultiplier = 1f;
				LightingManager.Instance.lerpActive = false;
				LightingManager.Instance.UpdateLighting(true);
			});
			UIFollowerIndoctrinationMenuController uIFollowerIndoctrinationMenuController4 = indoctrinationMenuInstance;
			uIFollowerIndoctrinationMenuController4.OnHidden = (Action)Delegate.Combine(uIFollowerIndoctrinationMenuController4.OnHidden, (Action)delegate
			{
				indoctrinationMenuInstance = null;
			});
		}
		else
		{
			StartCoroutine(CharacterSetupCallback());
		}
	}

	private void FollowerEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "indoctrinated" && Follower.Brain._directInfoAccess.StartingCursedState != Thought.OldAge)
		{
			AudioManager.Instance.PlayOneShot("event:/Stings/Choir_Short", base.gameObject);
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.gameObject);
			CameraManager.instance.ShakeCameraForDuration(0.25f, 1f, 0.33f);
			MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
			Follower.Brain.Info.Outfit = FollowerOutfitType.Follower;
			Follower.SetOutfit(FollowerOutfitType.Follower, false);
			Follower.Spine.AnimationState.Event -= FollowerEvent;
		}
	}

	private IEnumerator CharacterSetupCallback()
	{
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("recruit", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(base.transform.position, Follower.transform.position);
		yield return new WaitForSeconds(0.5f);
		AudioManager.Instance.PlayOneShot("event:/dialogue/followers/positive_acknowledge", Follower.gameObject);
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir_mid", PlayerFarming.Instance.gameObject);
		Follower.SetBodyAnimation("Indoctrinate/indoctrinate-finish", false);
		yield return new WaitForSeconds(4f);
		SimpleSpineAnimator simpleAnimator = Follower.SimpleAnimator;
		if ((object)simpleAnimator != null)
		{
			simpleAnimator.ResetAnimationsToDefaults();
		}
		if (RecruitOnComplete)
		{
			FollowerManager.RecruitFollower(this, false);
		}
		yield return new WaitForEndOfFrame();
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
		yield return null;
		if (Follower.Brain.Stats.Thoughts.Count <= 0)
		{
			float value = UnityEngine.Random.value;
			if (value <= 0.2f)
			{
				Follower.Brain.AddThought(Thought.EnthusiasticNewRecruit);
			}
			else if (value > 0.2f && value < 0.8f)
			{
				Follower.Brain.AddThought(Thought.HappyNewRecruit);
			}
			else if (value >= 0.8f)
			{
				Follower.Brain.AddThought(Thought.UnenthusiasticNewRecruit);
			}
		}
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			ThoughtData thought = allBrain.GetThought(Thought.Fasting);
			if (thought != null)
			{
				Follower.Brain.AddThought(thought.Clone());
			}
		}
		Debug.Log("RecruitOnComplete:  " + RecruitOnComplete);
		if (RecruitOnComplete)
		{
			Debug.Log("OnNewRecruit?.Invoke();");
			FollowerInteraction.enabled = true;
			FollowerInteraction.SelectTask(state, false, false);
			Action onNewRecruit = OnNewRecruit;
			if (onNewRecruit != null)
			{
				onNewRecruit();
			}
			if (DataManager.Instance.FirstTimeInDungeon)
			{
				ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.GetNewFollowersFromDungeon);
			}
			FollowerManager.SpawnExistingRecruits(BiomeBaseManager.Instance.RecruitSpawnLocation.transform.position);
			CultFaithManager.AddThought(Thought.Cult_NewFolllower, -1, 1f);
		}
		if (Follower.Brain.HasTrait(FollowerTrait.TraitType.NaturallySkeptical))
		{
			CultFaithManager.AddThought(Thought.Cult_NewRecruitSkeptical, -1, 1f);
		}
		if (Follower.Brain.HasTrait(FollowerTrait.TraitType.NaturallyObedient))
		{
			CultFaithManager.AddThought(Thought.Cult_NewRecruitObedient, -1, 1f);
		}
		if (DataManager.Instance.LeaderFollowersRecruited.Count >= 5)
		{
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("ALL_LEADER_FOLLOWERS"));
			Debug.Log("ACHIEVEMENT GOT : ALL_LEADER_FOLLOWERS");
		}
		OnRecruitFinished();
		UnityEngine.Object.Destroy(this);
	}

	public void ContinueRecruit()
	{
		Debug.Log("ContinueRecruit");
		StartCoroutine(ContinueRecruitRoutine());
	}

	private IEnumerator ContinueRecruitRoutine()
	{
		Debug.Log("ContinueRecruitRoutine ");
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationNext(CameraBone, 4f);
		Follower.Brain.Info.Outfit = FollowerOutfitType.Follower;
		Follower.SetOutfit(FollowerOutfitType.Follower, false);
		Follower.SimpleAnimator.ResetAnimationsToDefaults();
		Follower.SetBodyAnimation("recruit-end", false);
		Follower.AddBodyAnimation("idle", true, 0f);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("recruit-end", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
		if (RecruitOnComplete)
		{
			FollowerManager.RecruitFollower(this, false);
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		Action statueCallback = StatueCallback;
		if (statueCallback != null)
		{
			statueCallback();
		}
		OnRecruitFinished();
		UnityEngine.Object.Destroy(this);
	}

	public void InstantRecruit(bool followPlayer = false)
	{
		StartCoroutine(InstantRecruitRoutine(followPlayer));
	}

	private IEnumerator InstantRecruitRoutine(bool followPlayer)
	{
		Follower.SimpleAnimator.ResetAnimationsToDefaults();
		Follower.Brain.Info.Outfit = FollowerOutfitType.Follower;
		Follower.SetOutfit(FollowerOutfitType.Follower, false);
		FollowerManager.RecruitFollower(this, followPlayer);
		yield return new WaitForSeconds(1f);
		OnRecruitFinished();
		UnityEngine.Object.Destroy(this);
	}

	private void OnRecruitFinished()
	{
		if (skeletonAnimationLODManager != null)
		{
			skeletonAnimationLODManager.DisableLODManager(false);
		}
		//TwitchFollowers.SendFollowers(delegate
		//{
		//	TwitchFollowers.SendFollowerInformation(Follower.Brain._directInfoAccess);
		//});
	}

	private void CallbackSacrifice()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CarryBone.gameObject, 8f);
		PlayerFarming.Instance.GoToAndStop(ChurchFollowerManager.Instance.AltarPosition.gameObject, base.gameObject, false, false, ContinueSacrifice);
	}

	private void ContinueSacrifice()
	{
		StartCoroutine(ChurchFollowerManager.Instance.DoSacrificeRoutine(this, Follower.Brain.Info.ID, CompleteSacrifice));
	}

	private void CompleteSacrifice()
	{
		GameManager.GetInstance().OnConversationEnd();
		Follower.SimpleAnimator.ResetAnimationsToDefaults();
		FollowerManager.RemoveRecruit(Follower.Brain.Info.ID);
		state.CURRENT_STATE = StateMachine.State.Idle;
		ChurchFollowerManager.Instance.ExitAllFollowers();
	}
}
