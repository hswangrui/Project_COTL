using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using Map;
using MMBiomeGeneration;
using MMTools;
using Spine;
using Spine.Unity;
using Unify;
using UnityEngine;
using UnityEngine.Events;

public class DungeonLeaderMechanics : BaseMonoBehaviour
{
	[Serializable]
	private enum IntroType
	{
		None,
		EndCombat,
		MidCombat,
		BeforeCombat
	}

	[Serializable]
	private struct Conversation
	{
		public int SpawnedCharacterIndex;

		public string Line;
	}

	[Serializable]
	private struct Question
	{
		[TermsPopup("")]
		public string CharacterName;

		[TermsPopup("")]
		public string Line1;

		[Space]
		[TermsPopup("")]
		public string AnswerA;

		[TermsPopup("")]
		public string ResultA;

		public UnityEvent[] ResultACallbacks;

		[Space]
		[TermsPopup("")]
		public string AnswerB;

		[TermsPopup("")]
		public string ResultB;

		public UnityEvent[] ResultBCallbacks;
	}

	public static DungeonLeaderMechanics Instance;

	[SerializeField]
	private IntroType introType;

	[SerializeField]
	private Interaction_SimpleConversation conversation;

	[SerializeField]
	private SkeletonAnimation leaderSpine;

	[SerializeField]
	private GameObject cameraTarget;

	[SerializeField]
	private GameObject goopFloorParticle;

	[SerializeField]
	private Material leaderOldMaterial;

	[SerializeField]
	private Material leaderNewMaterial;

	[SerializeField]
	private DungeonLeaderMechanics[] otherLeaders;

	[Space]
	[SerializeField]
	private Vector2 randomTime;

	[SerializeField]
	private ColliderEvents distortionObject;

	[Space]
	[SerializeField]
	private GameObject podiumHighlight;

	[SerializeField]
	private ColliderEvents middleCollider;

	[Space]
	[SerializeField]
	private float maxScreenShake;

	[SerializeField]
	private float shakeDuration;

	[SerializeField]
	private AnimationCurve shakeCurve;

	[SerializeField]
	private AnimationCurve controllerRumbleCurve;

	[SerializeField]
	private bool spawnsFollower;

	[SerializeField]
	private bool hasQuestion;

	[SerializeField]
	private bool hasEnemyRounds;

	[SerializeField]
	private string idle = "idle";

	[SerializeField]
	private string enter = "enter";

	[SerializeField]
	private string talk = "talk";

	[Space]
	[SerializeField]
	private EnemyRoundsBase enemyRounds;

	[Space]
	[SerializeField]
	private bool isCombatFollower;

	[Space]
	[SerializeField]
	private int spawnAmount;

	[SerializeField]
	private Conversation[] spawnedConvo;

	[Space]
	[SerializeField]
	private Question question;

	[SerializeField]
	private UnityEvent resultAFinishedCallback;

	[SerializeField]
	private UnityEvent resultBFinishedCallback;

	[Space]
	[SerializeField]
	private bool requireConditions;

	[SerializeField]
	private List<BiomeGenerator.VariableAndCondition> conditionalVariables = new List<BiomeGenerator.VariableAndCondition>();

	[SerializeField]
	private int dungeonNumber = -1;

	[SerializeField]
	private Interaction_SimpleConversation alternateConversation;

	[Space]
	[SerializeField]
	private UnityEvent awakeCallback;

	[SerializeField]
	private UnityEvent callback;

	private bool introStarted;

	private bool waiting;

	private int followersDead;

	private float midCombatTimestamp = -1f;

	private bool addedFakeHealth;

	private bool fadedRed;

	private bool completed;

	private bool playerAnimsPlayed;

	[SerializeField]
	private LightingManagerVolume lightOverride;

	private List<FollowerManager.SpawnedFollower> spawnedFollowers = new List<FollowerManager.SpawnedFollower>();

	private List<Material> modifiedMaterials = new List<Material>();

	private Material followerMaterial;

	private static readonly int LeaderEncounterColorBoost = Shader.PropertyToID("_LeaderEncounterColorBoost");

	private bool endCombat
	{
		get
		{
			return introType == IntroType.EndCombat;
		}
	}

	private bool midCombat
	{
		get
		{
			return introType == IntroType.MidCombat;
		}
	}

	private bool beforeCombat
	{
		get
		{
			return introType == IntroType.BeforeCombat;
		}
	}

	private void Awake()
	{
		UnityEvent unityEvent = awakeCallback;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
		if (lightOverride != null)
		{
			lightOverride.gameObject.SetActive(false);
		}
		if ((bool)conversation && introType != 0)
		{
			conversation.Interactable = false;
		}
		if ((bool)distortionObject)
		{
			distortionObject.OnTriggerEnterEvent += DamageEnemies;
			if (UnifyManager.platform != UnifyManager.Platform.Standalone)
			{
				distortionObject.Performance = true;
			}
		}
		if ((bool)middleCollider)
		{
			middleCollider.OnTriggerEnterEvent += PodiumIntro;
		}
		if (requireConditions)
		{
			bool flag = true;
			foreach (BiomeGenerator.VariableAndCondition conditionalVariable in conditionalVariables)
			{
				if (DataManager.Instance.GetVariable(conditionalVariable.Variable) != conditionalVariable.Condition)
				{
					flag = false;
				}
			}
			if (!flag)
			{
				base.gameObject.SetActive(false);
			}
		}
		if (dungeonNumber != -1 && ((dungeonNumber == 1 && DataManager.Instance.ShownDungeon1FinalLeaderEncounter) || (dungeonNumber == 2 && DataManager.Instance.ShownDungeon2FinalLeaderEncounter) || (dungeonNumber == 3 && DataManager.Instance.ShownDungeon3FinalLeaderEncounter) || (dungeonNumber == 4 && DataManager.Instance.ShownDungeon4FinalLeaderEncounter)))
		{
			base.gameObject.SetActive(false);
		}
		leaderSpine.CustomMaterialOverride.Add(leaderOldMaterial, leaderNewMaterial);
		PlayerFarming.OnGoToAndStopBegin += GoToAndStopBegin;
	}

	private void Start()
	{
		Transform transform = ((BiomeGenerator.Instance.CurrentRoom != null) ? BiomeGenerator.Instance.CurrentRoom.generateRoom.transform : BiomeGenerator.Instance.transform);
		BreakableSpiderNest[] componentsInChildren = transform.GetComponentsInChildren<BreakableSpiderNest>();
		if (componentsInChildren.Length != 0)
		{
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				componentsInChildren[num].GetComponent<Health>().DealDamage(float.MaxValue, base.gameObject, base.transform.position);
			}
		}
		SpiderNest[] componentsInChildren2 = transform.GetComponentsInChildren<SpiderNest>();
		if (componentsInChildren2.Length != 0)
		{
			for (int num2 = componentsInChildren2.Length - 1; num2 >= 0; num2--)
			{
				UnityEngine.Object.Destroy(componentsInChildren2[num2].gameObject);
			}
		}
		TrapCharger[] componentsInChildren3 = transform.GetComponentsInChildren<TrapCharger>();
		if (componentsInChildren3.Length != 0)
		{
			for (int num3 = componentsInChildren3.Length - 1; num3 >= 0; num3--)
			{
				UnityEngine.Object.Destroy(componentsInChildren3[num3].gameObject);
			}
		}
		TrapSpikes[] componentsInChildren4 = transform.GetComponentsInChildren<TrapSpikes>();
		if (componentsInChildren4.Length != 0)
		{
			for (int num4 = componentsInChildren4.Length - 1; num4 >= 0; num4--)
			{
				UnityEngine.Object.Destroy(componentsInChildren4[num4].ParentToDestroy);
			}
		}
		TrapProjectileCross[] componentsInChildren5 = transform.GetComponentsInChildren<TrapProjectileCross>();
		if (componentsInChildren5.Length != 0)
		{
			for (int num5 = componentsInChildren5.Length - 1; num5 >= 0; num5--)
			{
				UnityEngine.Object.Destroy(componentsInChildren5[num5].gameObject);
			}
		}
		TrapRockFall[] componentsInChildren6 = transform.GetComponentsInChildren<TrapRockFall>();
		if (componentsInChildren6.Length != 0)
		{
			for (int num6 = componentsInChildren6.Length - 1; num6 >= 0; num6--)
			{
				UnityEngine.Object.Destroy(componentsInChildren6[num6].gameObject);
			}
		}
	}

	private void OnEnable()
	{
		if (introType == IntroType.None && !GameManager.RoomActive && BiomeGenerator.Instance != null && BiomeGenerator.Instance.CurrentRoom != null && BiomeGenerator.Instance.CurrentRoom.generateRoom != null && !BiomeGenerator.Instance.CurrentRoom.Completed)
		{
			BiomeGenerator.Instance.CurrentRoom.generateRoom.roomMusicID = SoundConstants.RoomID.CultLeaderAmbience;
		}
		if (completed)
		{
			Hide();
		}
		if ((bool)conversation)
		{
			conversation.OnInteraction += OnInteraction;
		}
		Instance = this;
	}

	private void OnDestroy()
	{
		PlayerFarming.OnGoToAndStopBegin -= GoToAndStopBegin;
		foreach (FollowerManager.SpawnedFollower spawnedFollower in spawnedFollowers)
		{
			if (spawnedFollower.Follower != null)
			{
				SimFollower simFollower = FollowerManager.FindSimFollowerByID(spawnedFollower.FollowerBrain.Info.ID);
				if (simFollower != null)
				{
					FollowerManager.SimFollowersAtLocation(FollowerLocation.Base).Add(simFollower);
				}
			}
			FollowerManager.CleanUpCopyFollower(spawnedFollower);
		}
		spawnedFollowers.Clear();
	}

	private void OnDisable()
	{
		if ((bool)conversation)
		{
			conversation.OnInteraction -= OnInteraction;
		}
		if (introStarted)
		{
			base.gameObject.SetActive(false);
		}
		foreach (Material modifiedMaterial in modifiedMaterials)
		{
			modifiedMaterial.SetColor(UnitObject.LeaderEncounterColorBoost, new Color(0f, 0f, 0f, 0f));
		}
		modifiedMaterials.Clear();
		Instance = null;
	}

	public void Hide()
	{
		SkeletonAnimation skeletonAnimation = leaderSpine;
		if ((object)skeletonAnimation != null)
		{
			skeletonAnimation.gameObject.SetActive(false);
		}
		GameObject obj = goopFloorParticle;
		if ((object)obj != null)
		{
			obj.gameObject.SetActive(false);
		}
		DungeonLeaderMechanics[] array = otherLeaders;
		foreach (DungeonLeaderMechanics dungeonLeaderMechanics in array)
		{
			if ((bool)dungeonLeaderMechanics)
			{
				dungeonLeaderMechanics.Hide();
			}
		}
	}

	private void OnInteraction(StateMachine state)
	{
		GameManager.GetInstance().StartCoroutine(SetPlayerAnimations());
	}

	private void GoToAndStopBegin(Vector3 targetPosition)
	{
		if (!base.gameObject.activeInHierarchy || (!introStarted && !MMConversation.isPlaying))
		{
			return;
		}
		if ((bool)conversation)
		{
			conversation.MovePlayerToListenPosition = false;
		}
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.state.LockStateChanges = true;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "floating-boss-start", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "floating-boss-loop", true, 0f);
		PlayerFarming.Instance.state.LockStateChanges = true;
		PlayerFarming.Instance.IdleOnEnd = false;
		PlayerFarming.Instance.transform.DOMove(targetPosition, 2f).SetSpeedBased().SetEase(Ease.InOutSine)
			.OnComplete(delegate
			{
				PlayerFarming.Instance.IdleOnEnd = true;
				PlayerFarming.Instance.EndGoToAndStop();
				StartCoroutine(FrameDelay(delegate
				{
					PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
					PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "floating-boss-loop", true);
				}));
			});
		PlayerFarming.OnGoToAndStopBegin -= GoToAndStopBegin;
	}

	private IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		callback();
	}

	private IEnumerator SetPlayerAnimations()
	{
		if (playerAnimsPlayed)
		{
			yield break;
		}
		playerAnimsPlayed = true;
		yield return new WaitForEndOfFrame();
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.state.LockStateChanges = true;
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "floating-boss-loop", true, 0f);
		yield return new WaitForSeconds(3.3f);
		while (MMConversation.CURRENT_CONVERSATION != null || !conversation.Interactable || waiting)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.transform.DOKill();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("floating-boss-land", 0, false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle-up", true, 0f);
		yield return new WaitForSeconds(1.76f);
		if (LetterBox.IsPlaying)
		{
			while (MMConversation.CURRENT_CONVERSATION != null || waiting)
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.5f);
			while (MMConversation.CURRENT_CONVERSATION != null || waiting)
			{
				yield return null;
			}
		}
		PlayerFarming.Instance.EndGoToAndStop();
		PlayerFarming.Instance.state.LockStateChanges = false;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
	}

	public void EndConversation()
	{
		GameManager.GetInstance().OnConversationEnd();
	}

	private void Update()
	{
		if (!introStarted && GameManager.RoomActive && !RoomLockController.DoorsOpen)
		{
			if (introType == IntroType.EndCombat)
			{
				if (!addedFakeHealth && introType == IntroType.EndCombat)
				{
					Health.team2.Add(null);
					Interaction_Chest instance = Interaction_Chest.Instance;
					if ((object)instance != null)
					{
						instance.AddEnemy(null);
					}
					addedFakeHealth = true;
				}
				if (Health.team2.Count <= 0 || (Health.team2.Count == 1 && Health.team2[0] == null))
				{
					introStarted = false;
					StartCoroutine(DrawnOutIntroIE());
				}
				else
				{
					for (int num = Health.team2.Count - 1; num >= 0; num--)
					{
						if (Health.team2[num] == null)
						{
							Health.team2.RemoveAt(num);
						}
					}
					Health.team2.Add(null);
				}
			}
			else if (introType == IntroType.MidCombat)
			{
				if (midCombatTimestamp == -1f)
				{
					midCombatTimestamp = Time.time + UnityEngine.Random.Range(randomTime.x, randomTime.y);
				}
				else if (midCombatTimestamp != -1f && Time.time > midCombatTimestamp)
				{
					introStarted = false;
					StartCoroutine(InstantSpawnIntroIE());
				}
			}
			else if (introType == IntroType.None)
			{
				if (!addedFakeHealth)
				{
					Health.team2.Add(null);
					Interaction_Chest instance2 = Interaction_Chest.Instance;
					if ((object)instance2 != null)
					{
						instance2.AddEnemy(null);
					}
					addedFakeHealth = true;
				}
				if (!fadedRed)
				{
					DeviceLightingManager.TransitionLighting(Color.white, Color.red, 2f, DeviceLightingManager.F_KEYS);
					fadedRed = true;
					FadeRedIn();
				}
			}
		}
		if (!GameManager.RoomActive && Interaction_Chest.Instance != null && introType == IntroType.MidCombat)
		{
			Health.team2.Add(null);
			Interaction_Chest instance3 = Interaction_Chest.Instance;
			if ((object)instance3 != null)
			{
				instance3.AddEnemy(null);
			}
		}
	}

	private IEnumerator DrawnOutIntroIE()
	{
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.CultLeaderAmbience);
		AudioManager.Instance.SetMusicCombatState(false);
		introStarted = true;
		yield return new WaitForSeconds(0.5f);
		List<Health> breakables = Health.neutralTeam;
		bool conversationActive = false;
		DeviceLightingManager.TransitionLighting(Color.white, Color.red, shakeDuration / 1.5f, DeviceLightingManager.F_KEYS, Ease.InSine);
		float lastBreakTime = 1f;
		float t = 0f;
		while (t < shakeDuration)
		{
			float num = t / shakeDuration;
			CameraManager.shakeCamera(maxScreenShake * shakeCurve.Evaluate(num));
			FadeRedIn();
			MMVibrate.RumbleContinuous(controllerRumbleCurve.Evaluate(num), controllerRumbleCurve.Evaluate(num) * 2f);
			if (num > 0.6f && !conversationActive)
			{
				conversationActive = true;
				GameManager.GetInstance().OnConversationNew(false);
				GameManager.GetInstance().OnConversationNext(cameraTarget);
				if (goopFloorParticle != null)
				{
					ShowGoop();
				}
			}
			if (num > 0.75f && !conversation.Interactable)
			{
				while (PlayerFarming.Instance.state.CURRENT_STATE != 0 && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.Moving && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.InActive && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.Attacking && (PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.CustomAnimation || !(PlayerFarming.Instance.Spine.AnimationState.GetCurrent(0).Animation.Name == "floating-boss-loop")))
				{
					yield return null;
				}
				foreach (ConversationEntry entry in conversation.Entries)
				{
					entry.SetZoom = true;
					entry.Zoom = 11f;
				}
				TrapPoison.RemoveAllPoison();
				conversation.Interactable = true;
				conversation.OnInteract(PlayerFarming.Instance.state);
				if (leaderSpine.AnimationState != null)
				{
					leaderSpine.AnimationState.AddAnimation(0, talk, true, 0f);
				}
				conversation.Callback.AddListener(delegate
				{
					if (goopFloorParticle != null)
					{
						SimpleSpineDeactivateAfterPlay component2 = goopFloorParticle.GetComponent<SimpleSpineDeactivateAfterPlay>();
						component2.Init = false;
						component2.Animation = "leader-stop";
					}
					if (!hasEnemyRounds)
					{
						RoomLockController.RoomCompleted();
						FadeRedAway();
					}
					else if (!spawnsFollower && !hasQuestion)
					{
						Health.team2.Clear();
						BeginEnemyRounds();
					}
				});
			}
			if (t > lastBreakTime && UnityEngine.Random.Range(0f, 1f) > 0.6f && breakables.Count > 0)
			{
				Health health = breakables[UnityEngine.Random.Range(0, breakables.Count)];
				DropLootOnDeath component = health.GetComponent<DropLootOnDeath>();
				if ((bool)component)
				{
					component.LootToDrop = InventoryItem.ITEM_TYPE.NONE;
				}
				health.ImpactOnHit = false;
				health.ScreenshakeOnDie = false;
				health.ScreenshakeOnHit = false;
				health.DealDamage(health.totalHP, base.gameObject, base.transform.position);
				lastBreakTime = t + UnityEngine.Random.Range(0f, 1f);
			}
			t += Time.deltaTime;
			yield return null;
		}
		MMVibrate.StopRumble();
		Health.team2.Clear();
	}

	private IEnumerator InstantSpawnIntroIE()
	{
		DeviceLightingManager.TransitionLighting(Color.white, Color.red, 2f, DeviceLightingManager.F_KEYS);
		FadeRedIn();
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.CultLeaderAmbience);
		AudioManager.Instance.SetMusicCombatState(false);
		introStarted = true;
		if (goopFloorParticle != null)
		{
			ShowGoop();
		}
		yield return new WaitForSeconds(0.75f);
		SpawnDistortionObject();
		yield return new WaitForSeconds(0.75f);
		leaderSpine.timeScale = 5f;
		leaderSpine.AnimationState.SetAnimation(0, enter, false);
		leaderSpine.AnimationState.AddAnimation(0, idle, true, 0f);
		yield return new WaitForSeconds(0.75f);
		leaderSpine.timeScale = 1f;
		TrapPoison.RemoveAllPoison();
		yield return new WaitForSeconds(1f);
		while (PlayerFarming.Instance.state.CURRENT_STATE != 0 && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.Moving && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.InActive)
		{
			yield return null;
		}
		if (!conversation)
		{
			yield break;
		}
		foreach (ConversationEntry entry in conversation.Entries)
		{
			entry.SetZoom = true;
			entry.Zoom = 11f;
		}
		conversation.Interactable = true;
		conversation.OnInteract(PlayerFarming.Instance.state);
		leaderSpine.AnimationState.AddAnimation(0, talk, true, 0f);
		conversation.Callback.AddListener(delegate
		{
			Health.team2.Remove(null);
			if (!hasEnemyRounds)
			{
				RoomLockController.RoomCompleted();
				FadeRedAway();
			}
		});
	}

	private void PodiumIntro(Collider2D collider)
	{
		if (!introStarted && !RoomLockController.DoorsOpen)
		{
			StartCoroutine(PodiumIntroIE());
		}
	}

	private IEnumerator PodiumIntroIE()
	{
		if (introStarted || !GameManager.RoomActive)
		{
			yield break;
		}
		DeviceLightingManager.TransitionLighting(Color.white, Color.red, 2f, DeviceLightingManager.F_KEYS);
		FadeRedIn();
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.CultLeaderAmbience);
		AudioManager.Instance.SetMusicCombatState(false);
		introStarted = true;
		if ((bool)podiumHighlight)
		{
			podiumHighlight.gameObject.SetActive(true);
		}
		if (podiumHighlight == null && (bool)conversation)
		{
			middleCollider.transform.position = conversation.ListenPosition;
		}
		while (PlayerFarming.Instance == null || (PlayerFarming.Instance.state.CURRENT_STATE != 0 && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.Moving && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.InActive))
		{
			yield return null;
		}
		if (goopFloorParticle != null)
		{
			ShowGoop();
		}
		yield return new WaitForSeconds(0.75f);
		SpawnDistortionObject();
		yield return new WaitForSeconds(0.75f);
		leaderSpine.timeScale = 5f;
		leaderSpine.AnimationState.SetAnimation(0, enter, false);
		leaderSpine.AnimationState.AddAnimation(0, idle, true, 0f);
		yield return new WaitForSeconds(0.75f);
		leaderSpine.timeScale = 1f;
		TrapPoison.RemoveAllPoison();
		for (int i = 0; i < otherLeaders.Length; i++)
		{
			SpawnOtherLeader(i);
		}
		yield return new WaitForSeconds((!podiumHighlight) ? 1 : 3);
		if (middleCollider != null && (PlayerFarming.Instance.Spine.AnimationState.GetCurrent(0).Animation.Name != "floating-boss-start" || PlayerFarming.Instance.Spine.AnimationState.GetCurrent(0).Animation.Name != "floating-boss-loop"))
		{
			GoToAndStopBegin(middleCollider.transform.position);
		}
		GameManager.GetInstance().StartCoroutine(SetPlayerAnimations());
		if (!conversation)
		{
			yield break;
		}
		foreach (ConversationEntry entry in conversation.Entries)
		{
			entry.SetZoom = true;
			entry.Zoom = 11f;
		}
		conversation.Interactable = true;
		conversation.OnInteract(PlayerFarming.Instance.state);
		leaderSpine.AnimationState.AddAnimation(0, talk, true, 0f);
		conversation.Callback.AddListener(delegate
		{
			Health.team2.Remove(null);
			if (!hasEnemyRounds && !hasQuestion)
			{
				RoomLockController.RoomCompleted();
				FadeRedAway();
			}
		});
	}

	private void DamageEnemies(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != Health.Team.PlayerTeam)
		{
			component.ImpactOnHit = false;
			component.ScreenshakeOnDie = false;
			component.ScreenshakeOnHit = false;
			component.invincible = false;
			component.untouchable = false;
			component.DealDamage(component.totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Poison);
		}
	}

	public void SpawnOtherLeader(int index)
	{
		otherLeaders[index].StartCoroutine(otherLeaders[index].InstantSpawnIntroIE());
	}

	public void ShowGoop()
	{
		if (!goopFloorParticle.gameObject.activeSelf)
		{
			goopFloorParticle.gameObject.SetActive(true);
			Spine.AnimationState animationState = goopFloorParticle.GetComponent<SkeletonAnimation>().AnimationState;
			if (animationState != null)
			{
				animationState.AddAnimation(0, "leader-loop", true, 0f);
			}
			AudioManager.Instance.PlayOneShot("event:/enemy/teleport_appear", base.transform.position);
			AudioManager.Instance.PlayOneShot("event:/enemy/summoned", base.transform.position);
		}
		else
		{
			StartCoroutine(HideGoopDelayIE());
		}
	}

	private IEnumerator HideGoopDelayIE()
	{
		yield return new WaitForSeconds(2f);
		goopFloorParticle.GetComponent<SimpleSpineDeactivateAfterPlay>().enabled = true;
		AudioManager.Instance.PlayOneShot("event:/enemy/teleport_away", base.transform.position);
	}

	public void FadeRedAway()
	{
		DeviceLightingManager.StopAll();
		DeviceLightingManager.TransitionLighting(Color.red, Color.white, 1f, DeviceLightingManager.F_KEYS);
		GameManager.GetInstance().WaitForSeconds(1f, delegate
		{
			DeviceLightingManager.UpdateLocation();
		});
		GameManager.GetInstance().SetDitherTween(0f);
		if (lightOverride != null)
		{
			lightOverride.gameObject.SetActive(false);
		}
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.StandardAmbience);
		BiomeGenerator.Instance.CurrentRoom.generateRoom.roomMusicID = SoundConstants.RoomID.StandardAmbience;
		if ((bool)Interaction_Chest.Instance)
		{
			GameManager.GetInstance().RemoveFromCamera(Interaction_Chest.Instance.gameObject);
		}
	}

	private void FadeRedIn()
	{
		if (lightOverride != null)
		{
			lightOverride.gameObject.SetActive(true);
		}
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.CultLeaderAmbience);
		BiomeGenerator.Instance.CurrentRoom.generateRoom.roomMusicID = SoundConstants.RoomID.StandardAmbience;
	}

	private void SpawnDistortionObject()
	{
		if (!distortionObject)
		{
			return;
		}
		for (int num = Health.team2.Count - 1; num >= 0; num--)
		{
			if (Health.team2[num] != null && Health.team2[num].invincible)
			{
				Health.team2[num].invincible = false;
			}
		}
		distortionObject.SetActive(true);
		distortionObject.transform.DOScale(25f, 3.5f).SetEase(Ease.Linear).OnComplete(delegate
		{
			try
			{
				TrapPoison.RemoveAllPoison();
				UnityEngine.Object.Destroy(distortionObject.gameObject);
			}
			catch
			{
				Debug.Log("Unable to Detroy distortionObject.gameObject");
			}
		});
		for (int num2 = EnemySpider.EnemySpiders.Count - 1; num2 >= 0; num2--)
		{
			if ((bool)EnemySpider.EnemySpiders[num2] && EnemySpider.EnemySpiders[num2] != this)
			{
				SpawnEnemyOnDeath component = EnemySpider.EnemySpiders[num2].GetComponent<SpawnEnemyOnDeath>();
				if ((bool)component)
				{
					component.Amount = 0;
				}
				EnemySpider.EnemySpiders[num2].health.enabled = true;
				EnemySpider.EnemySpiders[num2].health.DealDamage(EnemySpider.EnemySpiders[num2].health.totalHP, base.gameObject, EnemySpider.EnemySpiders[num2].transform.position);
			}
		}
		StartCoroutine(KillAll());
	}

	private IEnumerator KillAll()
	{
		yield return new WaitForSeconds(0.25f);
		List<Health> list = new List<Health>(Health.team2);
		foreach (Health item in list)
		{
			if (item != null)
			{
				item.invincible = false;
				item.enabled = true;
				item.DealDamage(float.PositiveInfinity, item.gameObject, Vector3.zero, false, Health.AttackTypes.Projectile);
				yield return new WaitForSeconds(0.05f);
			}
		}
	}

	public void BeginEnemyRounds()
	{
		StartCoroutine(BeginEnemyRoundsIE());
	}

	private IEnumerator BeginEnemyRoundsIE()
	{
		UnityEvent unityEvent = callback;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
		completed = true;
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.CultLeaderAmbience);
		yield return new WaitForSeconds(2f);
		GameManager.GetInstance().SetDitherTween(2f, 3f);
		Debug.Log("Set offering combat music ID");
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.OfferingCombat);
		AudioManager.Instance.SetMusicCombatState();
		while (PlayerFarming.Instance.state.CURRENT_STATE != 0 && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.Moving)
		{
			yield return null;
		}
		enemyRounds.BeginCombat(false, delegate
		{
			RoomLockController.RoomCompleted(true);
			Interaction_Chest instance = Interaction_Chest.Instance;
			if ((object)instance != null)
			{
				instance.Reveal();
			}
			FadeRedAway();
			for (int num = Health.team2.Count - 1; num >= 0; num--)
			{
				if (Health.team2[num] != null)
				{
					Health.team2[num].invincible = false;
					Health.team2[num].DealDamage(Health.team2[num].totalHP, base.gameObject, base.transform.position);
				}
			}
		});
		enemyRounds.OnEnemySpawned += delegate(UnitObject enemy)
		{
			MeshRenderer[] componentsInChildren = enemy.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer meshRenderer in componentsInChildren)
			{
				if (meshRenderer != null && meshRenderer.sharedMaterial != null && !modifiedMaterials.Contains(meshRenderer.sharedMaterial))
				{
					modifiedMaterials.Add(meshRenderer.sharedMaterial);
				}
			}
			MeshRenderer component = enemy.GetComponent<MeshRenderer>();
			if (component != null && component.sharedMaterial != null && !modifiedMaterials.Contains(component.sharedMaterial))
			{
				modifiedMaterials.Add(component.sharedMaterial);
			}
		};
	}

	public void Bow()
	{
		StartCoroutine(BowIE());
	}

	private IEnumerator BowIE()
	{
		waiting = true;
		PlayerFarming.Instance.state.LookAngle = 90f;
		PlayerFarming.Instance.state.facingAngle = 90f;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		TrackEntry trackEntry = PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "pray", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle-up", true, 0f);
		yield return new WaitForSeconds(trackEntry.Animation.Duration);
		CultFaithManager.AddThought(Thought.Cult_LostRespect, -1, 1f);
		waiting = false;
	}

	public void AskQuestion()
	{
		StartCoroutine(AskQuestionIE());
	}

	private IEnumerator AskQuestionIE()
	{
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		List<ConversationEntry> list = new List<ConversationEntry>
		{
			new ConversationEntry(base.gameObject, question.Line1)
		};
		list[0].CharacterName = question.CharacterName;
		list[0].Offset = new Vector3(0f, 3f, 0f);
		list[0].SetZoom = true;
		list[0].Zoom = 12f;
		if (question.CharacterName == "NAMES/CultLeaders/Dungeon2")
		{
			list[0].soundPath = "event:/dialogue/dun2_cult_leader_heket/standard_heket";
		}
		else if (question.CharacterName == "NAMES/CultLeaders/Dungeon4")
		{
			list[0].soundPath = "event:/dialogue/dun4_cult_leader_shamura/standard_shamura";
		}
		List<MMTools.Response> responses = new List<MMTools.Response>
		{
			new MMTools.Response(question.AnswerA, delegate
			{
				StartCoroutine(ResponseIE(true, question));
			}, question.AnswerA),
			new MMTools.Response(question.AnswerB, delegate
			{
				StartCoroutine(ResponseIE(false, question));
			}, question.AnswerB)
		};
		MMConversation.Play(new ConversationObject(list, responses, null), false);
		yield return new WaitForEndOfFrame();
	}

	private IEnumerator ResponseIE(bool responseWasA, Question question)
	{
		yield return null;
		if (responseWasA)
		{
			yield return StartCoroutine(ACallbackIE());
		}
		else
		{
			yield return StartCoroutine(BCallbackIE());
		}
		List<ConversationEntry> list = new List<ConversationEntry>
		{
			new ConversationEntry(base.gameObject, responseWasA ? question.ResultA : question.ResultB)
		};
		list[0].CharacterName = question.CharacterName;
		list[0].Offset = new Vector3(0f, 3f, 0f);
		list[0].SetZoom = true;
		list[0].Zoom = 12f;
		if (question.CharacterName == "NAMES/CultLeaders/Dungeon2")
		{
			list[0].soundPath = "event:/dialogue/dun2_cult_leader_heket/standard_heket";
		}
		else if (question.CharacterName == "NAMES/CultLeaders/Dungeon4")
		{
			list[0].soundPath = "event:/dialogue/dun4_cult_leader_shamura/standard_shamura";
		}
		MMConversation.Play(new ConversationObject(list, null, null), false);
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		if (responseWasA)
		{
			UnityEvent unityEvent = resultAFinishedCallback;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
		}
		else
		{
			UnityEvent unityEvent2 = resultBFinishedCallback;
			if (unityEvent2 != null)
			{
				unityEvent2.Invoke();
			}
		}
	}

	private IEnumerator ACallbackIE()
	{
		UnityEvent[] resultACallbacks = question.ResultACallbacks;
		foreach (UnityEvent obj in resultACallbacks)
		{
			if (obj != null)
			{
				obj.Invoke();
			}
			while (waiting)
			{
				yield return null;
			}
		}
	}

	private IEnumerator BCallbackIE()
	{
		UnityEvent[] resultBCallbacks = question.ResultBCallbacks;
		foreach (UnityEvent obj in resultBCallbacks)
		{
			if (obj != null)
			{
				obj.Invoke();
			}
			while (waiting)
			{
				yield return null;
			}
		}
	}

	public void ConvertAllMapNodes()
	{
		StartCoroutine(ConvertAllMapNodesIE());
	}

	private IEnumerator ConvertAllMapNodesIE()
	{
		if (MapManager.Instance.CurrentLayer < MapGenerator.Nodes.Count - 1 || MapGenerator.Nodes.Count == 0)
		{
			UIAdventureMapOverlayController adventureMapOverlayController = MapManager.Instance.ShowMap(true);
			while (adventureMapOverlayController.IsShowing)
			{
				yield return null;
			}
			yield return adventureMapOverlayController.ConvertAllNodesToCombatNodes();
			MapManager.Instance.CloseMap();
			while (adventureMapOverlayController.IsHiding)
			{
				yield return null;
			}
			RoomLockController.RoomCompleted();
			FadeRedAway();
			UnityEvent unityEvent = callback;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			completed = true;
		}
		else
		{
			BeginEnemyRounds();
		}
	}

	public void ConvertMiniBossNodeToBossNode()
	{
		StartCoroutine(ConvertMiniBossNodeToBossNodeIE());
	}

	private IEnumerator ConvertMiniBossNodeToBossNodeIE()
	{
		UIAdventureMapOverlayController adventureMapOverlayController = MapManager.Instance.ShowMap(true);
		while (adventureMapOverlayController.IsShowing)
		{
			yield return null;
		}
		yield return adventureMapOverlayController.ConvertMiniBossNodeToBossNode();
		MapManager.Instance.CloseMap();
		while (adventureMapOverlayController.IsHiding)
		{
			yield return null;
		}
		FadeRedAway();
		UnityEvent unityEvent = callback;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
	}

	public void SpawnInCombatFollower()
	{
		StartCoroutine(SpawnInCombatFollowerIE());
	}

	private IEnumerator SpawnInCombatFollowerIE()
	{
		if ((bool)distortionObject)
		{
			distortionObject.gameObject.SetActive(false);
		}
		waiting = true;
		List<FollowerInfo> possibleFollowers = new List<FollowerInfo>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (follower.CursedState == Thought.None && !FollowerManager.FollowerLocked(follower.ID))
			{
				possibleFollowers.Add(follower);
			}
		}
		if (possibleFollowers.Count > 0)
		{
			yield return new WaitForEndOfFrame();
			GameManager.GetInstance().OnConversationNew();
			Vector3[] spawnPositions = new Vector3[2]
			{
				base.transform.position + Vector3.right * 2f,
				base.transform.position - Vector3.right * 2f
			};
			for (int i = 0; i < spawnAmount; i++)
			{
				if (possibleFollowers.Count == 0)
				{
					break;
				}
				FollowerInfo followerInfo = possibleFollowers[UnityEngine.Random.Range(0, possibleFollowers.Count)];
				FollowerManager.SpawnedFollower spawnedFollower = SpawnFollower(followerInfo, spawnPositions[i]);
				FollowerManager.RetireSimFollowerByID(spawnedFollower.FollowerBrain.Info.ID);
				Health component = spawnedFollower.Follower.GetComponent<Health>();
				if ((bool)component)
				{
					component.invincible = true;
				}
				spawnedFollower.Follower.OverridingEmotions = true;
				SetFollowerOutfit(spawnedFollower, Thought.Dissenter);
				spawnedFollowers.Add(spawnedFollower);
				possibleFollowers.Remove(followerInfo);
				GameManager.GetInstance().OnConversationNext(spawnedFollower.Follower.gameObject, 5f);
				yield return new WaitForSeconds(1f);
			}
			List<ConversationEntry> list = new List<ConversationEntry>();
			for (int j = 0; j < spawnedConvo.Length; j++)
			{
				list.Add(new ConversationEntry(spawnedFollowers[0].Follower.gameObject, spawnedConvo[j].Line));
				list[j].CharacterName = spawnedFollowers[0].FollowerBrain.Info.Name;
				list[j].Offset = new Vector3(0f, 1f, 0f);
				list[j].SetZoom = true;
				list[j].Zoom = 5f;
			}
			MMConversation.Play(new ConversationObject(list, null, null));
			while (MMConversation.CURRENT_CONVERSATION != null)
			{
				yield return null;
			}
			waiting = false;
			while (PlayerFarming.Instance.state.CURRENT_STATE != 0 && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.Moving)
			{
				yield return null;
			}
			if (spawnedFollowers[0].Follower != null)
			{
				followerMaterial = spawnedFollowers[0].Follower.Spine.gameObject.GetComponent<MeshRenderer>().sharedMaterial;
			}
			foreach (FollowerManager.SpawnedFollower spawnedFollower2 in spawnedFollowers)
			{
				spawnedFollower2.Follower.GetComponent<EnemyFollower>().enabled = true;
				spawnedFollower2.Follower.GetComponent<Health>().invincible = false;
				spawnedFollower2.Follower.GetComponent<Health>().OnDie += FollowerDied;
			}
			GameManager.GetInstance().OnConversationEnd();
			UnityEvent unityEvent = callback;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			completed = true;
		}
		else
		{
			BeginEnemyRounds();
		}
		waiting = false;
	}

	private void SetFollowerOutfit(FollowerManager.SpawnedFollower spawnedFollower, Thought cursedState)
	{
		Skin skin = new Skin("New Skin");
		Skin skin2 = spawnedFollower.Follower.Spine.Skeleton.Data.FindSkin(spawnedFollower.FollowerBrain._directInfoAccess.SkinName);
		if (skin2 != null)
		{
			skin.AddSkin(skin2);
		}
		else
		{
			skin.AddSkin(spawnedFollower.Follower.Spine.Skeleton.Data.FindSkin("Cat"));
			spawnedFollower.FollowerBrain._directInfoAccess.SkinName = "Cat";
		}
		string outfitSkinName = spawnedFollower.Follower.Outfit.GetOutfitSkinName(spawnedFollower.FollowerBrain._directInfoAccess.Outfit);
		if (!string.IsNullOrEmpty(outfitSkinName))
		{
			skin.AddSkin(spawnedFollower.Follower.Spine.skeleton.Data.FindSkin(outfitSkinName));
		}
		if (cursedState == Thought.Dissenter)
		{
			skin.AddSkin(spawnedFollower.Follower.Spine.skeleton.Data.FindSkin("Other/Dissenter"));
			spawnedFollower.Follower.Spine.AnimationState.SetAnimation(0, "Emotions/emotion-dissenter", true);
		}
		spawnedFollower.Follower.Spine.Skeleton.SetSkin(skin);
		spawnedFollower.Follower.Spine.skeleton.SetSlotsToSetupPose();
		WorshipperData.SkinAndData colourData = WorshipperData.Instance.GetColourData(spawnedFollower.FollowerBrain._directInfoAccess.SkinName);
		if (colourData == null)
		{
			return;
		}
		foreach (WorshipperData.SlotAndColor slotAndColour in colourData.SlotAndColours[Mathf.Clamp(spawnedFollower.FollowerBrain._directInfoAccess.SkinColour, 0, colourData.SlotAndColours.Count - 1)].SlotAndColours)
		{
			Slot slot = spawnedFollower.Follower.Spine.skeleton.FindSlot(slotAndColour.Slot);
			if (slot != null)
			{
				slot.SetColor(slotAndColour.color);
			}
		}
	}

	private void FollowerDied(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		foreach (FollowerManager.SpawnedFollower spawnedFollower in spawnedFollowers)
		{
			if (spawnedFollower.Follower != null && spawnedFollower.Follower.GetComponent<Health>() == Victim)
			{
				FollowerBrain.AllBrains.Add(spawnedFollower.FollowerBrain);
				FollowerManager.FollowerDie(spawnedFollower.FollowerBrain.Info.ID, NotificationCentre.NotificationType.Died);
				break;
			}
		}
		followersDead++;
		if (followersDead >= spawnAmount)
		{
			if (followerMaterial != null)
			{
				followerMaterial.SetColor(LeaderEncounterColorBoost, new Color(0f, 0f, 0f, 0f));
			}
			FadeRedAway();
			Interaction_Chest instance = Interaction_Chest.Instance;
			if ((object)instance != null)
			{
				instance.Reveal();
			}
			RoomLockController.RoomCompleted(true);
		}
	}

	public void MakeRandomFollowerIll()
	{
		StartCoroutine(MakeRandomFollowerIllIE());
	}

	private IEnumerator MakeRandomFollowerIllIE()
	{
		waiting = true;
		List<FollowerInfo> possibleFollowers = new List<FollowerInfo>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (follower.Illness <= 0f && follower.CursedState == Thought.None && !FollowerManager.FollowerLocked(follower.ID))
			{
				possibleFollowers.Add(follower);
			}
		}
		if (possibleFollowers.Count > 0)
		{
			yield return new WaitForEndOfFrame();
			FollowerManager.SpawnedFollower spawnedFollower = SpawnFollower(possibleFollowers[UnityEngine.Random.Range(0, possibleFollowers.Count)], base.transform.position + Vector3.up * -1f);
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(spawnedFollower.Follower.gameObject, 12f);
			yield return new WaitForSeconds(0.5f);
			List<ConversationEntry> list = new List<ConversationEntry>();
			list.Add(new ConversationEntry(spawnedFollower.Follower.gameObject, "Conversation_NPC/Story/Dungeon3/Leader1/3"));
			list[0].Offset = new Vector3(0f, 0f, -1f);
			list[0].CharacterName = spawnedFollower.FollowerBrain.Info.Name;
			MMConversation.Play(new ConversationObject(list, null, null), false);
			while (MMConversation.isPlaying)
			{
				yield return null;
			}
			yield return StartCoroutine(MakeFollowerIll(spawnedFollower, 0.9f));
			GameManager.GetInstance().OnConversationEnd();
			UnityEvent unityEvent = callback;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			completed = true;
			RoomLockController.RoomCompleted();
			FadeRedAway();
		}
		else
		{
			BeginEnemyRounds();
		}
		waiting = false;
	}

	private IEnumerator MakeFollowerIll(FollowerManager.SpawnedFollower spawnedFollower, float delay)
	{
		waiting = true;
		spawnedFollower.Follower.Spine.AnimationState.AddAnimation(1, "Reactions/react-feared", false, 0f);
		spawnedFollower.Follower.Spine.AnimationState.AddAnimation(1, "Conversations/idle-hate", false, 0f);
		yield return new WaitForSeconds(delay);
		yield return new WaitForSeconds(0.5f);
		for (int i = 0; i < UnityEngine.Random.Range(4, 7); i++)
		{
			SoulCustomTarget.Create(spawnedFollower.Follower.gameObject, base.transform.position + new Vector3(0f, 0f, -0.5f) + UnityEngine.Random.insideUnitSphere * 0.5f, Color.green, null, 0.2f, 100f);
			yield return new WaitForSeconds(0.05f);
		}
		yield return new WaitForSeconds(0.5f);
		float offset = UnityEngine.Random.Range(0f, 0.3f);
		TrackEntry e = spawnedFollower.Follower.Spine.AnimationState.SetAnimation(1, "Sick/chunder", false);
		e.TrackTime = offset;
		TrackEntry track = spawnedFollower.Follower.Spine.AnimationState.AddAnimation(1, "spawn-out-angry", false, 0f);
		yield return new WaitForSeconds(0.5f);
		spawnedFollower.FollowerBrain.MakeSick();
		DataManager.Instance.LastFollowerToBecomeIll = -1f;
		spawnedFollower.Follower.ShowAllFollowerIcons();
		FollowerExhaustionWarning componentInChildren = spawnedFollower.Follower.GetComponentInChildren<FollowerExhaustionWarning>(true);
		if (componentInChildren != null)
		{
			componentInChildren.Hide();
		}
		FollowerReeducationWarning componentInChildren2 = spawnedFollower.Follower.GetComponentInChildren<FollowerReeducationWarning>(true);
		if (componentInChildren2 != null)
		{
			componentInChildren2.Hide();
		}
		FollowerStarvingWarning componentInChildren3 = spawnedFollower.Follower.GetComponentInChildren<FollowerStarvingWarning>(true);
		if ((bool)componentInChildren3)
		{
			componentInChildren3.Hide();
		}
		FollowerIllnessWarning followerIllnessWarning = spawnedFollower.Follower.GetComponentInChildren<FollowerIllnessWarning>(true);
		if (followerIllnessWarning != null)
		{
			followerIllnessWarning.Show();
		}
		yield return new WaitForSeconds(e.Animation.Duration - offset - 0.5f);
		if (followerIllnessWarning != null)
		{
			followerIllnessWarning.Hide();
		}
		yield return new WaitForSeconds(track.Animation.Duration);
		spawnedFollower.Follower.gameObject.SetActive(false);
		FollowerBrain.RemoveBrain(spawnedFollower.FollowerFakeBrain.Info.ID);
		waiting = false;
	}

	public void MakeAllFollowersIll()
	{
		StartCoroutine(MakeAllFollowersIllIE());
	}

	private IEnumerator MakeAllFollowersIllIE()
	{
		waiting = true;
		List<FollowerInfo> possibleFollowers = new List<FollowerInfo>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (follower.Illness <= 0f && follower.CursedState == Thought.None && possibleFollowers.Count < 6 && !FollowerManager.FollowerLocked(follower.ID))
			{
				possibleFollowers.Add(follower);
			}
		}
		if (possibleFollowers.Count > 0)
		{
			yield return new WaitForEndOfFrame();
			GameManager.GetInstance().OnConversationNew();
			int total = Mathf.Min(possibleFollowers.Count, 5);
			float angleBetween = 180f / (float)(total - 1);
			float startingAngle = -180f;
			Mathf.Max(Mathf.Clamp(total / 2, 2, 4), 3f);
			float delay = 1f / (float)possibleFollowers.Count;
			for (int i = 0; i < total; i++)
			{
				FollowerManager.SpawnedFollower spawnedFollower = SpawnFollower(possibleFollowers[i], base.transform.position + Vector3.right * 2f);
				GameManager.GetInstance().AddToCamera(spawnedFollower.Follower.gameObject);
				Vector3 vector = base.transform.position + (Vector3)Utils.DegreeToVector2(startingAngle) * 3.5f;
				spawnedFollower.Follower.transform.position = vector;
				spawnedFollower.Follower.State.LookAngle = (spawnedFollower.Follower.State.facingAngle = Utils.GetAngle(vector, base.transform.position));
				StartCoroutine(MakeFollowerIll(spawnedFollower, 1f - delay * (float)(i + 1) + 0.5f));
				startingAngle = Mathf.Repeat(startingAngle + angleBetween, 360f);
				yield return new WaitForSeconds(delay);
			}
			yield return new WaitForSeconds(5f);
			GameManager.GetInstance().RemoveAllFromCamera();
			GameManager.GetInstance().OnConversationEnd();
			UnityEvent unityEvent = callback;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			completed = true;
			RoomLockController.RoomCompleted();
			FadeRedAway();
		}
		else
		{
			BeginEnemyRounds();
		}
		waiting = false;
	}

	public void MakeRandomFollowerStarving()
	{
		StartCoroutine(MakeRandomFollowerStarvingIE());
	}

	private IEnumerator MakeRandomFollowerStarvingIE()
	{
		waiting = true;
		List<FollowerInfo> possibleFollowers = new List<FollowerInfo>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (follower.Satiation > 30f && follower.CursedState == Thought.None && !FollowerManager.FollowerLocked(follower.ID) && !FollowerBrain.GetOrCreateBrain(follower).HasTrait(FollowerTrait.TraitType.DontStarve))
			{
				possibleFollowers.Add(follower);
			}
		}
		if (possibleFollowers.Count > 0)
		{
			yield return new WaitForEndOfFrame();
			FollowerManager.SpawnedFollower spawnedFollower = SpawnFollower(possibleFollowers[UnityEngine.Random.Range(0, possibleFollowers.Count)], base.transform.position + Vector3.up * -1f);
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(spawnedFollower.Follower.gameObject, 12f);
			yield return new WaitForSeconds(0.5f);
			List<ConversationEntry> list = new List<ConversationEntry>();
			list.Add(new ConversationEntry(spawnedFollower.Follower.gameObject, "Conversation_NPC/Story/Dungeon2/Leader1/3"));
			list[0].Offset = new Vector3(0f, 0f, -1f);
			list[0].CharacterName = spawnedFollower.FollowerBrain.Info.Name;
			MMConversation.Play(new ConversationObject(list, null, null), false);
			while (MMConversation.isPlaying)
			{
				yield return null;
			}
			yield return StartCoroutine(MakeFollowerStarving(spawnedFollower, 0.9f));
			GameManager.GetInstance().OnConversationEnd();
			UnityEvent unityEvent = callback;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			completed = true;
			RoomLockController.RoomCompleted();
			FadeRedAway();
		}
		else
		{
			BeginEnemyRounds();
		}
		waiting = false;
	}

	private IEnumerator MakeFollowerStarving(FollowerManager.SpawnedFollower spawnedFollower, float delay)
	{
		waiting = true;
		spawnedFollower.Follower.Spine.AnimationState.AddAnimation(1, "Reactions/react-feared", false, 0f);
		spawnedFollower.Follower.Spine.AnimationState.AddAnimation(1, "Conversations/idle-hate", false, 0f);
		yield return new WaitForSeconds(delay);
		float offset = UnityEngine.Random.Range(0f, 0.3f);
		TrackEntry e = spawnedFollower.Follower.Spine.AnimationState.SetAnimation(1, "Hungry/get-hungry", false);
		e.TrackTime = offset;
		TrackEntry track = spawnedFollower.Follower.Spine.AnimationState.AddAnimation(1, "spawn-out-angry", false, 0f);
		for (int i = 0; i < UnityEngine.Random.Range(4, 7); i++)
		{
			SoulCustomTarget.Create(base.gameObject, spawnedFollower.Follower.transform.position + new Vector3(0f, 0f, -0.5f) + UnityEngine.Random.insideUnitSphere * 0.5f, Color.red, null, 0.2f, 100f);
			yield return new WaitForSeconds(0.05f);
		}
		NotificationCentre.NotificationsEnabled = false;
		spawnedFollower.FollowerBrain.MakeStarve();
		spawnedFollower.Follower.ShowAllFollowerIcons();
		spawnedFollower.Follower.ShowAllFollowerIcons();
		FollowerExhaustionWarning componentInChildren = spawnedFollower.Follower.GetComponentInChildren<FollowerExhaustionWarning>(true);
		if (componentInChildren != null)
		{
			componentInChildren.Hide();
		}
		FollowerReeducationWarning componentInChildren2 = spawnedFollower.Follower.GetComponentInChildren<FollowerReeducationWarning>(true);
		if (componentInChildren2 != null)
		{
			componentInChildren2.Hide();
		}
		FollowerIllnessWarning componentInChildren3 = spawnedFollower.Follower.GetComponentInChildren<FollowerIllnessWarning>(true);
		if ((bool)componentInChildren3)
		{
			componentInChildren3.Hide();
		}
		FollowerStarvingWarning followerStarvingWArning = spawnedFollower.Follower.GetComponentInChildren<FollowerStarvingWarning>(true);
		if ((bool)followerStarvingWArning)
		{
			followerStarvingWArning.Show();
		}
		NotificationCentre.NotificationsEnabled = true;
		NotificationCentre.Instance.PlayFollowerNotification(NotificationCentre.NotificationType.Starving, spawnedFollower.FollowerBrain.Info, NotificationFollower.Animation.Unhappy);
		yield return new WaitForSeconds(e.Animation.Duration - offset - 0.5f);
		if ((bool)followerStarvingWArning)
		{
			followerStarvingWArning.Hide();
		}
		yield return new WaitForSeconds(track.Animation.Duration);
		spawnedFollower.Follower.gameObject.SetActive(false);
		FollowerBrain.RemoveBrain(spawnedFollower.FollowerFakeBrain.Info.ID);
		waiting = false;
	}

	public void MakeAllFollowersStarving()
	{
		StartCoroutine(MakeAllFollowersStarvingIE());
	}

	private IEnumerator MakeAllFollowersStarvingIE()
	{
		waiting = true;
		List<FollowerInfo> possibleFollowers = new List<FollowerInfo>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (follower.Satiation > 25f && follower.CursedState == Thought.None && !FollowerManager.FollowerLocked(follower.ID) && !FollowerBrain.GetOrCreateBrain(follower).HasTrait(FollowerTrait.TraitType.DontStarve))
			{
				possibleFollowers.Add(follower);
			}
		}
		if (possibleFollowers.Count > 0)
		{
			yield return new WaitForEndOfFrame();
			GameManager.GetInstance().OnConversationNew();
			int total = Mathf.Min(possibleFollowers.Count, 5);
			float angleBetween = 180f / (float)(total - 1);
			float startingAngle = -180f;
			float distanceBetween = Mathf.Max(Mathf.Clamp(total / 2, 2, 4), 3f);
			float delay = 1f / (float)possibleFollowers.Count;
			for (int i = 0; i < total; i++)
			{
				FollowerManager.SpawnedFollower spawnedFollower = SpawnFollower(possibleFollowers[i], base.transform.position + Vector3.right * 2f);
				GameManager.GetInstance().AddToCamera(spawnedFollower.Follower.gameObject);
				Vector3 vector = base.transform.position + (Vector3)Utils.DegreeToVector2(startingAngle) * distanceBetween;
				vector.y -= 1f;
				spawnedFollower.Follower.transform.position = vector;
				spawnedFollower.Follower.State.LookAngle = (spawnedFollower.Follower.State.facingAngle = Utils.GetAngle(vector, base.transform.position));
				StartCoroutine(MakeFollowerStarving(spawnedFollower, 1f - delay * (float)(i + 1) + 0.5f));
				startingAngle = Mathf.Repeat(startingAngle + angleBetween, 360f);
				yield return new WaitForSeconds(delay);
			}
			yield return new WaitForSeconds(5f);
			GameManager.GetInstance().RemoveAllFromCamera();
			GameManager.GetInstance().OnConversationEnd();
			UnityEvent unityEvent = callback;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			completed = true;
			RoomLockController.RoomCompleted();
			FadeRedAway();
		}
		else
		{
			BeginEnemyRounds();
		}
		waiting = false;
	}

	private FollowerManager.SpawnedFollower SpawnFollower(FollowerInfo followerInfo, Vector3 position)
	{
		FollowerManager.SpawnedFollower result = FollowerManager.SpawnCopyFollower(isCombatFollower ? FollowerManager.CombatFollowerPrefab : FollowerManager.FollowerPrefab, followerInfo, position, base.transform.parent, BiomeGenerator.Instance.DungeonLocation);
		result.Follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		result.Follower.Spine.AnimationState.SetAnimation(1, "spawn-in", false);
		result.Follower.Spine.AnimationState.AddAnimation(1, "Reactions/react-worried1", true, 0f);
		result.Follower.Spine.AnimationState.AddAnimation(1, "Conversations/idle-hate", true, 0f);
		return result;
	}

	public void AlternateDungeon1Leader3()
	{
		if ((bool)alternateConversation && (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_2) || DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_3)))
		{
			for (int num = otherLeaders.Length - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(otherLeaders[num].gameObject);
			}
			otherLeaders = new DungeonLeaderMechanics[0];
			UnityEngine.Object.Destroy(conversation);
			alternateConversation.gameObject.SetActive(true);
			conversation = alternateConversation;
			podiumHighlight = null;
		}
	}

	public void AlternateDungeon2Leader1()
	{
		if ((bool)alternateConversation && !DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1))
		{
			UnityEngine.Object.Destroy(conversation);
			alternateConversation.gameObject.SetActive(true);
			conversation = alternateConversation;
			podiumHighlight = null;
		}
	}

	public void AlternateDungeon2Leader2()
	{
		if ((bool)alternateConversation && (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_3) || DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_4) || !DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1)))
		{
			for (int num = otherLeaders.Length - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(otherLeaders[num].gameObject);
			}
			otherLeaders = new DungeonLeaderMechanics[0];
			UnityEngine.Object.Destroy(conversation);
			alternateConversation.gameObject.SetActive(true);
			conversation = alternateConversation;
			podiumHighlight = null;
		}
	}

	public void AlternateDungeon2Leader4()
	{
		if ((bool)alternateConversation && !DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1))
		{
			UnityEngine.Object.Destroy(conversation);
			alternateConversation.gameObject.SetActive(true);
			conversation = alternateConversation;
			podiumHighlight = null;
		}
	}

	public void AlternateDungeon2Leader5()
	{
		if (!DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1))
		{
			conversation.Entries.RemoveAt(0);
		}
	}

	public void AlternateDungeon3Leader3()
	{
		if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_4))
		{
			conversation.Entries.RemoveAt(2);
		}
	}

	public void AlternateDungeon3Leader4()
	{
		if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_4))
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void Dungeon2Encounter1()
	{
		List<FollowerInfo> list = new List<FollowerInfo>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (follower.Satiation > 30f && follower.CursedState == Thought.None)
			{
				list.Add(follower);
			}
		}
		if (list.Count > 0)
		{
			ConversationEntry conversationEntry = new ConversationEntry(cameraTarget, "Conversation_NPC/Story/Dungeon2/Leader1/2");
			conversationEntry.CharacterName = ScriptLocalization.NAMES_CultLeaders.Dungeon2;
			conversation.Entries.Add(conversationEntry);
		}
	}

	public void Dungeon2Encounter3()
	{
		List<FollowerInfo> list = new List<FollowerInfo>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (follower.Satiation > 30f && follower.CursedState == Thought.None)
			{
				list.Add(follower);
			}
		}
		if (list.Count > 0)
		{
			ConversationEntry conversationEntry = new ConversationEntry(cameraTarget, "Conversation_NPC/Story/Dungeon2/Leader3/2");
			conversationEntry.CharacterName = ScriptLocalization.NAMES_CultLeaders.Dungeon2;
			conversation.Entries.Add(conversationEntry);
		}
	}

	public void Dungeon3Encounter1()
	{
		List<FollowerInfo> list = new List<FollowerInfo>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (follower.Illness <= 0f && follower.CursedState == Thought.None)
			{
				list.Add(follower);
			}
		}
		if (list.Count > 0)
		{
			ConversationEntry conversationEntry = new ConversationEntry(cameraTarget, "Conversation_NPC/Story/Dungeon3/Leader1/2");
			conversationEntry.CharacterName = ScriptLocalization.NAMES_CultLeaders.Dungeon3;
			conversation.Entries.Add(conversationEntry);
		}
	}

	public void Dungeon3Encounter2()
	{
		List<FollowerInfo> list = new List<FollowerInfo>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (follower.Illness <= 0f && follower.CursedState == Thought.None)
			{
				list.Add(follower);
			}
		}
		if (list.Count > 0)
		{
			ConversationEntry conversationEntry = new ConversationEntry(cameraTarget, "Conversation_NPC/Story/Dungeon3/Leader2/1");
			conversationEntry.CharacterName = ScriptLocalization.NAMES_CultLeaders.Dungeon3;
			conversation.Entries.Add(conversationEntry);
		}
	}

	public void Dungeon4Encounter1()
	{
		List<FollowerInfo> list = new List<FollowerInfo>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (follower.CursedState == Thought.None)
			{
				list.Add(follower);
			}
		}
		if (list.Count > 0)
		{
			ConversationEntry conversationEntry = new ConversationEntry(cameraTarget, "Conversation_NPC/Story/Dungeon4/Leader1/3");
			conversationEntry.CharacterName = ScriptLocalization.NAMES_CultLeaders.Dungeon4;
			conversation.Entries.Add(conversationEntry);
		}
	}

	public void ShownFinalLeaderEncounter()
	{
		if (dungeonNumber == 1)
		{
			DataManager.Instance.ShownDungeon1FinalLeaderEncounter = true;
		}
		else if (dungeonNumber == 2)
		{
			DataManager.Instance.ShownDungeon2FinalLeaderEncounter = true;
		}
		else if (dungeonNumber == 3)
		{
			DataManager.Instance.ShownDungeon3FinalLeaderEncounter = true;
		}
		else if (dungeonNumber == 4)
		{
			DataManager.Instance.ShownDungeon4FinalLeaderEncounter = true;
		}
	}
}
