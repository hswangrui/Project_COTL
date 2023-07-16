using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MMTools;
using Pathfinding;
using Spine;
using Spine.Unity;
using UnityEditor.U2D.Path;
using UnityEngine;

public class Follower : BaseMonoBehaviour
{
	public enum ComplaintType
	{
		Hunger,
		Homeless,
		Sick,
		ReadyToLevelUp,
		NeedBetterHouse,
		FirstTimeSpeakingToPlayer,
		Grateful,
		None,
		GiveQuest,
		CompletedQuest,
		FailedQuest,
		GiveOnboarding,
		ShowTwitchMessage
	}

	public static List<Follower> Followers = new List<Follower>();

	public GameObject Container;

	public GameObject CameraBone;

	public GameObject PropagandaIcon;

	public const float DEFAULT_MAX_SPEED = 2.25f;

	public const float COMPLAINT_COOLDOWN_GAME_MINUTES = 120f;

	public const float SHARED_COMPLAINT_COOLDOWN_GAME_MINUTES = 5f;

	public static float LastComplaintFromAnyFollowerTime;

	public GameObject UIWorshipperStatsPrefab;

	public ParticleSystem ParticleSystem;

	public GameObject BlessedTodayIcon;

	public Seeker Seeker;

	public StateMachine State;

	public SkeletonAnimation Spine;

	public SimpleSpineAnimator SimpleAnimator;

	public Health Health;

	public WorshipperBubble WorshipperBubble;

	public Transform ChainConnection;

	public interaction_FollowerInteraction Interaction_FollowerInteraction;

	public UIFollowerPrayingProgress UIFollowerPrayingProgress;

	public FollowerRadialProgressBar FollowerRadialProgress;

	public GameObject CompletedQuestIcon;

	public GameObject FollowerWarningIcons;

	public GameObject IllnessAura;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string AnimIdle;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string AnimHoodUp;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string AnimHoodDown;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string AnimWalking;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string AnimPray;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string AnimWorship;

	private float _timedActionTimer;

	private Action _timedAction;

	private bool _dying;

	private List<Vector3> _currentPath;

	private int _currentWaypoint;

	private Vector3 _startPos;

	private float _t;

	private Vector3 _destPos;

	private float _speed;

	private float _stoppingDistance = 0.1f;

	private Action _onPathComplete;

	private bool _isResumed;

	public Material NormalMaterial;

	public Material BW_Material;

	private SimpleBark simpleBark;

	private bool showingBark;

	public string strCurrentTask = "";

	private FollowerAdorationUI adorationUI;

	private string cachedBarkMessage = "";

	private SkeletonAnimationLODManager skeletonAnimationLODManager;

	private static readonly int LeaderEncounterColorBoost = Shader.PropertyToID("_LeaderEncounterColorBoost");

	public Action OnFollowerBrainAssigned;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string TestAnimation;

	public Action OnUpgradeDiscipleRoutineComplete;

	private RaycastHit LockToGroundHit;

	private Vector3 LockToGroundPosition;

	private Vector3 LockToGroundNewPosition;

	private bool WasDistanceCalculatedOnThisFrame;

	private bool IsUpdateRequired = true;

	private const float MOVEMENT_DELTA = 1f;

	private Vector3 lastUpdatedPosition = Vector3.zero;

	private const float DELAY_BETWEEN_POSITION_CHECK = 0.3f;

	private float lastPositionUpdateTimer;

	private float delayBetweenPositionUpdateSpread;

	private Tween completedQuestIconTween;

	private bool UseDeltaTime = true;

	public FollowerBrain Brain { get; private set; }

	public FollowerOutfit Outfit { get; private set; }

	public bool IsPaused
	{
		get
		{
			return !_isResumed;
		}
	}

	public bool UseUnscaledTime { get; set; }

	public bool OverridingEmotions { get; set; }

	public bool OverridingOutfit { get; set; }

	public HatType CurrentHat { get; private set; }

	public FollowerAdorationUI AdorationUI
	{
		get
		{
			return adorationUI;
		}
	}

	public SkeletonAnimationLODManager SkeletonAnimationLODManager
	{
		get
		{
			return skeletonAnimationLODManager;
		}
	}

	public void PlayParticles()
	{
		ParticleSystem.Play();
	}

	private void Start()
	{
		if ((bool)CultFaithManager.Instance)
		{
			CultFaithManager.Instance.OnThoughtModified += OnThoughtModified;
		}
		adorationUI = GetComponentInChildren<FollowerAdorationUI>(true);
		BlessedTodayIcon.gameObject.SetActive(false);
		simpleBark = GetComponent<SimpleBark>();
		if (simpleBark != null)
		{
			simpleBark.enabled = false;
		}
		foreach (FollowerPet.FollowerPetType pet in Brain._directInfoAccess.Pets)
		{
			FollowerPet.Create(pet, this);
		}
		Brain.OnCursedStateRemoved += Brain_OnCursedStateRemoved;
		if (PlayerFarming.Location == FollowerLocation.Base)
		{
			UIFollowerName componentInChildren = GetComponentInChildren<UIFollowerName>(true);
			if ((object)componentInChildren != null)
			{
				componentInChildren.Show(false);
			}
		}
	}

	private void OnDestroy()
	{
		if ((bool)CultFaithManager.Instance)
		{
			CultFaithManager.Instance.OnThoughtModified -= OnThoughtModified;
		}
		foreach (FollowerPet followerPet in FollowerPet.FollowerPets)
		{
			FollowerPet followerPet2 = followerPet;
			for (int num = FollowerPet.FollowerPets.Count - 1; num >= 0; num--)
			{
				if (FollowerPet.FollowerPets[num].Follower == this)
				{
					UnityEngine.Object.Destroy(FollowerPet.FollowerPets[num].gameObject);
				}
			}
		}
		Brain.OnCursedStateRemoved -= Brain_OnCursedStateRemoved;
	}

	private void OnEnable()
	{
		Followers.Add(this);
		if (Spine != null && Spine.AnimationState != null)
		{
			Spine.AnimationState.Start += SetEmotionAnimation;
		}
		Color value = new Color(0f, 0f, 0f, 0f);
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (meshRenderer != null && meshRenderer.sharedMaterial != null)
			{
				meshRenderer.sharedMaterial.SetColor(LeaderEncounterColorBoost, value);
			}
		}
		lastUpdatedPosition = base.transform.position;
		delayBetweenPositionUpdateSpread = 0.3f + UnityEngine.Random.Range(0f, 0.3f);
	}

	private void OnDisable()
	{
		Followers.Remove(this);
		Pause();
		if(Spine)
			Spine.AnimationState.Start -= SetEmotionAnimation;
	}

	public void Init(FollowerBrain brain, FollowerOutfit outfit)
	{
		Brain = brain;
		Outfit = outfit;
		if ((!DataManager.Instance.DLC_Cultist_Pack && DataManager.CultistDLCSkins.Contains(Brain.Info.SkinName.StripNumbers())) || (!DataManager.Instance.DLC_Heretic_Pack && DataManager.HereticDLCSkins.Contains(Brain.Info.SkinName.StripNumbers())))
		{
			WorshipperData.SkinAndData skinAndData = WorshipperData.Instance.Characters[brain.Info.SkinCharacter];
			brain.Info.SkinColour = skinAndData.SlotAndColours.RandomIndex();
			brain.Info.SkinCharacter = WorshipperData.Instance.GetRandomAvailableSkin(true, true);
			string skinNameFromIndex = WorshipperData.Instance.GetSkinNameFromIndex(brain.Info.SkinCharacter);
			brain.Info.SkinVariation = WorshipperData.Instance.GetColourData(skinNameFromIndex).Skin.RandomIndex();
			brain.Info.SkinName = skinNameFromIndex;
		}
		Outfit.SetOutfit(Spine, false);
		Spine.AnimationState.Start += SetEmotionAnimation;
		if ((bool)SimpleAnimator)
		{
			SimpleAnimator.AnimationTrack = 1;
		}
		Action onFollowerBrainAssigned = OnFollowerBrainAssigned;
		if (onFollowerBrainAssigned != null)
		{
			onFollowerBrainAssigned();
		}
		Brain.SavedFollowerTaskDestination = Vector3.zero;
		if (brain.Location == FollowerLocation.Base && DataManager.Instance.CompletedQuestFollowerIDs.Contains(brain.Info.ID))
		{
			foreach (ObjectivesData objective in DataManager.Instance.Objectives)
			{
				if (objective is Objectives_TalkToFollower && objective.Follower == brain.Info.ID && string.IsNullOrEmpty(((Objectives_TalkToFollower)objective).ResponseTerm))
				{
					ShowCompletedQuestIcon(true);
					return;
				}
			}
			DataManager.Instance.CompletedQuestFollowerIDs.Remove(brain.Info.ID);
		}
		if (!FollowerManager.FollowerLocked(Brain.Info.ID))
		{
			if (Brain.ThoughtExists(Thought.PropogandaSpeakers))
			{
				PropagandaIcon.gameObject.SetActive(true);
			}
			if (!Brain.ThoughtExists(Thought.PropogandaSpeakers))
			{
				PropagandaIcon.SetActive(false);
			}
		}
	}

	private void PlayAnimation()
	{
		Brain.CompleteCurrentTask();
		Brain.HardSwapToTask(new FollowerTask_ManualControl());
		SetBodyAnimation(TestAnimation, true);
	}

	private void MakeDissenter()
	{
		Brain.MakeDissenter();
	}

	private void MakeSick()
	{
		Brain.MakeSick();
	}

	private void TestUpgrade()
	{
		Brain.AddAdoration(FollowerBrain.AdorationActions.Gift, null);
	}

	private IEnumerator InstantUpgradeToDisciple(Action OnBecomeDiscipleComplete)
	{
		PlayParticles();
		SetOutfit(FollowerOutfitType.Follower, false);
		yield return null;
		if (OnBecomeDiscipleComplete != null)
		{
			OnBecomeDiscipleComplete();
		}
	}

	public IEnumerator UpgradeToDiscipleRoutine()
	{
		Debug.Log("Upgrade to disciple routine");
		if (Brain.CurrentTaskType != FollowerTaskType.ManualControl && Brain.CurrentTaskType != FollowerTaskType.AttendTeaching)
		{
			Brain.CompleteCurrentTask();
			Brain.HardSwapToTask(new FollowerTask_ManualControl());
		}
		TimedAnimation("level-up", 1.5f);
		yield return new WaitForSecondsRealtime(2f / 3f);
		BiomeConstants.Instance.EmitHeartPickUpVFX(CameraBone.transform.position, 0f, "black", "burst_big", false);
		AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/hearts_appear", base.gameObject.transform.position);
		yield return new WaitForSecondsRealtime(1f);
		if (Brain.CurrentTaskType != FollowerTaskType.AttendTeaching)
		{
			Brain.CompleteCurrentTask();
		}
		Debug.Log("LEVELED UP");
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.Disciple);
		adorationUI.SetObjects();
	//	TwitchFollowers.SendFollowerInformation(Brain._directInfoAccess);
	}

	public void Resume()
	{
		if (!_isResumed)
		{
			_isResumed = true;
			Seeker seeker = Seeker;
			seeker.pathCallback = (OnPathDelegate)Delegate.Combine(seeker.pathCallback, new OnPathDelegate(OnCheckSafeSpawn));
			Vector3 safeSpawnCheckPosition = LocationManager.LocationManagers[Brain.Location].SafeSpawnCheckPosition;
			Seeker.StartPath(base.transform.position, safeSpawnCheckPosition);
		}
	}

	private void OnCheckSafeSpawn(Path path)
	{
		Seeker seeker = Seeker;
		seeker.pathCallback = (OnPathDelegate)Delegate.Remove(seeker.pathCallback, new OnPathDelegate(OnCheckSafeSpawn));
		ABPath aBPath = (ABPath)path;
		if (path.error || (Vector3.Distance(aBPath.originalEndPoint, aBPath.endPoint) > 1.5f && path.duration > 0f))
		{
			base.transform.position = ((path.error || path.duration <= 0f) ? Brain.LastPosition : aBPath.originalEndPoint);
		}
		Seeker seeker2 = Seeker;
		seeker2.pathCallback = (OnPathDelegate)Delegate.Combine(seeker2.pathCallback, new OnPathDelegate(StartPath));
		FollowerBrainStats.OnFearLoveChanged = (FollowerBrainStats.StatChangedEvent)Delegate.Combine(FollowerBrainStats.OnFearLoveChanged, new FollowerBrainStats.StatChangedEvent(OnFearLoveChanged));
		FollowerBrain brain = Brain;
		brain.OnStateChanged = (Action<FollowerState, FollowerState>)Delegate.Combine(brain.OnStateChanged, new Action<FollowerState, FollowerState>(OnStateChanged));
		FollowerBrain brain2 = Brain;
		brain2.OnTaskChanged = (Action<FollowerTask, FollowerTask>)Delegate.Combine(brain2.OnTaskChanged, new Action<FollowerTask, FollowerTask>(OnTaskChanged));
		if (Brain.CurrentTask != null)
		{
			OnTaskChanged(Brain.CurrentTask, null);
			if (Brain.CurrentTask != null)
			{
				OnFollowerTaskStateChanged(FollowerTaskState.None, Brain.CurrentTask.State);
			}
		}
		if (Brain.CurrentState != null)
		{
			OnStateChanged(Brain.CurrentState, null);
		}
	}

	public void HideAllFollowerIcons()
	{
		if ((bool)FollowerWarningIcons)
		{
			FollowerWarningIcons.SetActive(false);
		}
		if ((bool)CompletedQuestIcon)
		{
			CompletedQuestIcon.SetActive(false);
		}
		FollowerAdorationUI componentInChildren = GetComponentInChildren<FollowerAdorationUI>(true);
		if ((object)componentInChildren != null)
		{
			componentInChildren.Hide();
		}
		UIFollowerName componentInChildren2 = GetComponentInChildren<UIFollowerName>(true);
		if ((object)componentInChildren2 != null)
		{
			componentInChildren2.Hide(false);
		}
		BlessedTodayIcon.gameObject.SetActive(false);
		PropagandaIcon.SetActive(false);
	}

	public void ShowAllFollowerIcons(bool excludeLoyaltyBar = true)
	{
		if ((bool)FollowerWarningIcons)
		{
			FollowerWarningIcons.SetActive(true);
		}
		if ((bool)CompletedQuestIcon)
		{
			CompletedQuestIcon.SetActive(DataManager.Instance.CompletedQuestFollowerIDs.Contains(Brain.Info.ID));
		}
		if (!excludeLoyaltyBar)
		{
			FollowerAdorationUI componentInChildren = GetComponentInChildren<FollowerAdorationUI>(true);
			if ((object)componentInChildren != null)
			{
				componentInChildren.Show();
			}
		}
		UIFollowerName componentInChildren2 = GetComponentInChildren<UIFollowerName>();
		if ((object)componentInChildren2 != null)
		{
			componentInChildren2.Show();
		}
	}

	public void StartTeleportToTransitionPosition()
	{
		if (Brain.CurrentTask != null)
		{
			Vector3 destination = Brain.CurrentTask.GetDestination(this);
			if (Brain.CurrentTask != null && (Brain.CurrentTask.State == FollowerTaskState.None || Brain.CurrentTask.State == FollowerTaskState.GoingTo))
			{
				Seeker seeker = Seeker;
				seeker.pathCallback = (OnPathDelegate)Delegate.Combine(seeker.pathCallback, new OnPathDelegate(EndTeleportToTransitionPosition));
				Vector3 lastPosition = Brain.LastPosition;
				Seeker.StartPath(lastPosition, destination);
				return;
			}
			base.transform.position = destination;
			Resume();
			if (Brain.CurrentTask != null && Brain.CurrentTask.State == FollowerTaskState.Doing)
			{
				OnFollowerTaskStateChanged(FollowerTaskState.None, Brain.CurrentTask.State);
			}
		}
		else
		{
			base.transform.position = Brain.LastPosition;
			Resume();
		}
	}

	public void EndTeleportToTransitionPosition(Path p)
	{
		Seeker seeker = Seeker;
		seeker.pathCallback = (OnPathDelegate)Delegate.Remove(seeker.pathCallback, new OnPathDelegate(EndTeleportToTransitionPosition));
		if (!p.error)
		{
			List<Vector3> vectorPath = p.vectorPath;
			int num = (vectorPath.Count - 1) / 2;
			Vector3 position = ((vectorPath.Count % 2 != 0) ? vectorPath[num] : ((vectorPath[num] + vectorPath[num + 1]) / 2f));
			base.transform.position = position;
		}
		Resume();
	}

	public void Pause()
	{
		if (!_isResumed)
		{
			return;
		}
		_isResumed = false;
		Seeker.CancelCurrentPathRequest();
		Seeker seeker = Seeker;
		seeker.pathCallback = (OnPathDelegate)Delegate.Remove(seeker.pathCallback, new OnPathDelegate(StartPath));
		Seeker seeker2 = Seeker;
		seeker2.pathCallback = (OnPathDelegate)Delegate.Remove(seeker2.pathCallback, new OnPathDelegate(OnCheckSafeSpawn));
		Seeker seeker3 = Seeker;
		seeker3.pathCallback = (OnPathDelegate)Delegate.Remove(seeker3.pathCallback, new OnPathDelegate(EndTeleportToTransitionPosition));
		ClearPath();
		FollowerBrainStats.OnFearLoveChanged = (FollowerBrainStats.StatChangedEvent)Delegate.Remove(FollowerBrainStats.OnFearLoveChanged, new FollowerBrainStats.StatChangedEvent(OnFearLoveChanged));
		if (Brain != null)
		{
			FollowerBrain brain = Brain;
			brain.OnStateChanged = (Action<FollowerState, FollowerState>)Delegate.Remove(brain.OnStateChanged, new Action<FollowerState, FollowerState>(OnStateChanged));
			FollowerBrain brain2 = Brain;
			brain2.OnTaskChanged = (Action<FollowerTask, FollowerTask>)Delegate.Remove(brain2.OnTaskChanged, new Action<FollowerTask, FollowerTask>(OnTaskChanged));
			if (Brain.CurrentTask != null)
			{
				Brain.CurrentTask.Cleanup(this);
				OnTaskChanged(null, Brain.CurrentTask);
			}
			if (Brain.CurrentState != null)
			{
				OnStateChanged(null, Brain.CurrentState);
			}
		}
	}

	private void SetHealth()
	{
		Health.HP = Brain.Stats.HP;
		Health.totalHP = Brain.Stats.MaxHP;
	}

	private void Update()
	{
		lastPositionUpdateTimer -= Time.deltaTime;
		strCurrentTask = ((Brain != null && Brain.CurrentTask != null) ? Brain.CurrentTask.Type.ToString() : "");
		if (State.CURRENT_STATE == StateMachine.State.TimedAction)
		{
			_timedActionTimer -= ((UseDeltaTime && !UseUnscaledTime) ? Time.deltaTime : Time.unscaledDeltaTime);
			if (_timedActionTimer <= 0f)
			{
				if (State.CURRENT_STATE == StateMachine.State.TimedAction)
				{
					State.CURRENT_STATE = StateMachine.State.Idle;
				}
				if (_timedAction != null)
				{
					Action timedAction = _timedAction;
					_timedAction = null;
					timedAction();
				}
			}
		}
		else
		{
			UpdateMovement();
		}
		if (PlayerFarming.Location != 0 && !BlessedTodayIcon.gameObject.activeSelf && (Brain.Stats.ReceivedBlessing || Brain.Stats.Intimidated || Brain.Stats.Inspired))
		{
			BlessedTodayIcon.gameObject.SetActive(true);
			BlessedTodayIcon.transform.localScale = Vector3.zero;
            DG.Tweening.Sequence s = DOTween.Sequence();
			s.AppendInterval(2f);
			s.Append(BlessedTodayIcon.transform.DOScale(1.25f, 0.5f).SetEase(Ease.OutBack));
		}
		else if (BlessedTodayIcon.gameObject.activeSelf && !Brain.Stats.ReceivedBlessing && !Brain.Stats.Intimidated && !Brain.Stats.Inspired)
		{
			BlessedTodayIcon.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).OnComplete(delegate
			{
				BlessedTodayIcon.gameObject.SetActive(false);
			});
		}
		if (showingBark && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) > 7.5f)
		{
			HideBark();
		}
	}

	private void LateUpdate()
	{
		if (PlayerFarming.Location != FollowerLocation.Base || Time.timeScale == 0f)
		{
			return;
		}
		LockToGroundPosition = base.transform.position + Vector3.back * 3f;
		if (Physics.Raycast(LockToGroundPosition, Vector3.forward, out LockToGroundHit, 4f))
		{
			if (LockToGroundHit.collider.gameObject.GetComponent<MeshCollider>() != null)
			{
				LockToGroundNewPosition = base.transform.position;
				LockToGroundNewPosition.z = LockToGroundHit.point.z;
				base.transform.position = LockToGroundNewPosition;
			}
		}
		else
		{
			LockToGroundNewPosition = base.transform.position;
			LockToGroundNewPosition.z = 0f;
			base.transform.position = LockToGroundNewPosition;
		}
		Brain.LastPosition = base.transform.position;
		WasDistanceCalculatedOnThisFrame = false;
	}

	public void Tick(float deltaGameTime)
	{
		if (Brain == null || _dying)
		{
			return;
		}
		if ((Brain.CurrentTask == null || !(Brain.CurrentTask is FollowerTask_MissionaryComplete)) && Brain._directInfoAccess.MissionaryFinished && DataManager.Instance.Followers_OnMissionary_IDs.Contains(Brain.Info.ID))
		{
			Brain.HardSwapToTask(new FollowerTask_MissionaryComplete());
			if (Brain.CurrentTask != null)
			{
				base.transform.position = Brain.CurrentTask.GetDestination(this);
			}
		}
		else if ((Brain.CurrentTask == null || (!(Brain.CurrentTask is FollowerTask_OnMissionary) && !(Brain.CurrentTask is FollowerTask_ChangeLocation))) && !Brain._directInfoAccess.MissionaryFinished && (Brain.Location == FollowerLocation.Missionary || (Brain.Location == FollowerLocation.Base && DataManager.Instance.Followers_OnMissionary_IDs.Contains(Brain.Info.ID))))
		{
			FollowerTask followerTask = new FollowerTask_OnMissionary();
			followerTask.AnimateOutFromLocation = false;
			Brain.HardSwapToTask(followerTask);
			FollowerTask currentTask = Brain.CurrentTask;
			if (currentTask != null)
			{
				currentTask.Arrive();
			}
		}
		else if ((Brain.CurrentTask == null || (!(Brain.CurrentTask is FollowerTask_IsDemon) && !(Brain.CurrentTask is FollowerTask_ChangeLocation))) && (Brain.Location == FollowerLocation.Demon || (Brain.Location == FollowerLocation.Base && DataManager.Instance.Followers_Demons_IDs.Contains(Brain.Info.ID))))
		{
			Brain.HardSwapToTask(new FollowerTask_IsDemon());
			FollowerTask currentTask2 = Brain.CurrentTask;
			if (currentTask2 != null)
			{
				currentTask2.Arrive();
			}
		}
		else
		{
			if ((Brain.CurrentTask == null || !(Brain.CurrentTask is FollowerTask_Imprisoned)) && DataManager.Instance.Followers_Imprisoned_IDs.Contains(Brain.Info.ID))
			{
				List<StructureBrain> allStructuresOfType = StructureManager.GetAllStructuresOfType(StructureBrain.TYPES.PRISON);
				foreach (StructureBrain item in allStructuresOfType)
				{
					if (item == null || item.Data == null || item.Data.FollowerID != Brain.Info.ID)
					{
						continue;
					}
					Brain.HardSwapToTask(new FollowerTask_Imprisoned(item.Data.ID));
					ClearPath();
					base.transform.position = Brain.CurrentTask.GetDestination(this);
					Brain.LastPosition = base.transform.position;
					Brain.CurrentTask.Arrive();
					goto IL_071e;
				}
				foreach (StructureBrain item2 in allStructuresOfType)
				{
					if (item2 == null || item2.Data == null || item2.Data.FollowerID != -1)
					{
						continue;
					}
					item2.Data.FollowerID = Brain.Info.ID;
					Brain.HardSwapToTask(new FollowerTask_Imprisoned(item2.Data.ID));
					ClearPath();
					base.transform.position = Brain.CurrentTask.GetDestination(this);
					Brain.LastPosition = base.transform.position;
					Brain.CurrentTask.Arrive();
					goto IL_071e;
				}
				DataManager.Instance.Followers_Imprisoned_IDs.Remove(Brain.Info.ID);
			}
			if (State.CURRENT_STATE != StateMachine.State.TimedAction && Brain.Location == FollowerLocation.Base)
			{
				Vector3 a = Brain.LastPosition;
				if (base.transform != null && this != null)
				{
					a = base.transform.position;
				}
				if (Brain.CurrentTask == null || (!Brain.CurrentTask.BlockTaskChanges && !Brain.CurrentTask.BlockReactTasks))
				{
					if (Brain.CurrentTask != null && Brain.Location == FollowerLocation.Base && !(Brain.CurrentTask is FollowerTask_GetAttention) && DataManager.Instance.CurrentOnboardingFollowerID == Brain.Info.ID && !FollowerManager.FollowerLocked(Brain.Info.ID) && Brain.Info.CursedState == Thought.None && Brain.Stats.Adoration < Brain.Stats.MAX_ADORATION)
					{
						Brain.HardSwapToTask(new FollowerTask_GetAttention(ComplaintType.GiveOnboarding, false));
					}
					else if (Brain.CurrentTask != null && Brain.Location == FollowerLocation.Base && !(Brain.CurrentTask is FollowerTask_GetAttention) && !string.IsNullOrEmpty(cachedBarkMessage) && !FollowerManager.FollowerLocked(Brain.Info.ID))
					{
						Brain.HardSwapToTask(new FollowerTask_GetAttention(ComplaintType.ShowTwitchMessage, false));
					}
					else
					{
						if (CheckForUpdate())
						{
							List<Follower> list = FollowerManager.FollowersAtLocation(Brain.Location);
							int num = 0;
							while (num < list.Count)
							{
								if (!(list[num] != null) || !(list[num] != this) || !(list[num].transform != null) || !(Vector3.Distance(a, list[num].transform.position) < 6f) || FollowerManager.FollowerLocked(list[num].Brain.Info.ID) || !Brain.CheckForInteraction(list[num].Brain))
								{
									num++;
									continue;
								}
								goto IL_071e;
							}
						}
						Brain.SpeakersInRange = 0;
						if (CheckForUpdate())
						{
							List<Structure> structures = Structure.Structures;
							for (int i = 0; i < structures.Count; i++)
							{
								if (structures[i] != null && structures[i].Structure_Info != null)
								{
									float num2 = Vector3.Distance(a, structures[i].Structure_Info.Position);
									if (num2 < 8f && Brain.CheckForInteraction(structures[i], num2))
									{
										break;
									}
								}
							}
						}
					}
				}
			}
		}
		goto IL_071e;
		IL_071e:
		Brain.Tick(deltaGameTime);
		if (Time.frameCount % 5 == 0)
		{
			Brain.SpeakersInRange = 0;
			if (CheckForUpdate())
			{
				List<Structure> structures2 = Structure.Structures;
				for (int j = 0; j < structures2.Count; j++)
				{
					if (Brain.CheckForSpeakers(structures2[j]))
					{
						Brain.SpeakersInRange = 1;
						break;
					}
				}
			}
			if (Brain.SpeakersInRange > 0 && !FollowerManager.FollowerLocked(Brain.Info.ID) && Brain.Info.CursedState == Thought.None)
			{
				if (!Brain.ThoughtExists(Thought.PropogandaSpeakers))
				{
					Brain.AddThought(Thought.PropogandaSpeakers, true);
					if ((bool)PropagandaIcon)
					{
						PropagandaIcon.gameObject.SetActive(true);
						PropagandaIcon.transform.localScale = Vector3.zero;
						PropagandaIcon.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
					}
				}
			}
			else if (Brain.ThoughtExists(Thought.PropogandaSpeakers))
			{
				Brain.RemoveThought(Thought.PropogandaSpeakers, true);
				if ((bool)PropagandaIcon)
				{
					PropagandaIcon.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(delegate
					{
						PropagandaIcon.SetActive(false);
					});
				}
			}
		}
		if (lastPositionUpdateTimer <= 0f)
		{
			lastUpdatedPosition = base.transform.position;
			lastPositionUpdateTimer = delayBetweenPositionUpdateSpread;
		}
	}

	private bool CheckForUpdate()
	{
		if (WasDistanceCalculatedOnThisFrame)
		{
			return IsUpdateRequired;
		}
		IsUpdateRequired = Vector3.Distance(base.transform.position, lastUpdatedPosition) >= 1f && lastPositionUpdateTimer <= 0f;
		return IsUpdateRequired;
	}

	private bool CheckForInteractionWithPlayer()
	{
		return false;
	}

	private void OnStateChanged(FollowerState newState, FollowerState oldState)
	{
		if (oldState != null)
		{
			oldState.Cleanup(this);
		}
		if (newState != null)
		{
			newState.Setup(this);
		}
		SetEmotionAnimation();
		SetOverrideOutfit();
	}

	private void OnTaskChanged(FollowerTask newTask, FollowerTask oldTask)
	{
		if (oldTask != null)
		{
			oldTask.OnFollowerTaskStateChanged = (FollowerTask.FollowerTaskDelegate)Delegate.Remove(oldTask.OnFollowerTaskStateChanged, new FollowerTask.FollowerTaskDelegate(OnFollowerTaskStateChanged));
		}
		if (newTask != null)
		{
			newTask.Setup(this);
			if (oldTask == null || oldTask.Type != FollowerTaskType.ChangeLocation)
			{
				newTask.ClaimReservations();
			}
			newTask.OnFollowerTaskStateChanged = (FollowerTask.FollowerTaskDelegate)Delegate.Combine(newTask.OnFollowerTaskStateChanged, new FollowerTask.FollowerTaskDelegate(OnFollowerTaskStateChanged));
		}
		Interaction_FollowerInteraction.enabled = newTask == null || !newTask.DisablePickUpInteraction;
	}

	private void OnFollowerTaskStateChanged(FollowerTaskState oldState, FollowerTaskState newState)
	{
		if (Brain.CurrentTask != null)
		{
			switch (oldState)
			{
			case FollowerTaskState.Idle:
				Brain.CurrentTask.OnIdleEnd(this);
				break;
			case FollowerTaskState.GoingTo:
				Brain.CurrentTask.OnGoingToEnd(this);
				break;
			case FollowerTaskState.Doing:
				Brain.CurrentTask.OnDoingEnd(this);
				break;
			case FollowerTaskState.Finalising:
				Brain.CurrentTask.OnFinaliseEnd(this);
				break;
			case FollowerTaskState.Done:
				Debug.Log(string.Concat(oldState, "  ", newState));
				throw new ArgumentException("Should never change a Task state once it's Done! " + Brain.Info.Name);
			}
			switch (newState)
			{
			case FollowerTaskState.None:
				throw new ArgumentException("Should never change a Task state back to None!");
			case FollowerTaskState.Idle:
				Brain.CurrentTask.OnIdleBegin(this);
				break;
			case FollowerTaskState.GoingTo:
				Brain.CurrentTask.OnGoingToBegin(this);
				break;
			case FollowerTaskState.Doing:
				Brain.CurrentTask.OnDoingBegin(this);
				break;
			case FollowerTaskState.Finalising:
				Brain.CurrentTask.OnFinaliseBegin(this);
				break;
			case FollowerTaskState.Done:
				Brain.CurrentTask.Cleanup(this);
				State.CURRENT_STATE = StateMachine.State.Idle;
				ResetStateAnimations();
				break;
			case FollowerTaskState.WaitingForLocation:
				break;
			}
		}
	}

	public void ShowCompletedQuestIcon(bool show)
	{
		if (completedQuestIconTween != null && completedQuestIconTween.active)
		{
			completedQuestIconTween.Kill();
		}
		if (show)
		{
			CompletedQuestIcon.transform.localScale = Vector3.zero;
			CompletedQuestIcon.SetActive(true);
			completedQuestIconTween = CompletedQuestIcon.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
		}
		else
		{
			CompletedQuestIcon.transform.localScale = Vector3.one;
			completedQuestIconTween = CompletedQuestIcon.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack).OnComplete(delegate
			{
				CompletedQuestIcon.SetActive(false);
			});
		}
	}

	private void OnHit(GameObject attacker, Vector3 attackLocation, Health.AttackTypes attackType, bool FromBehind)
	{
		Brain.Stats.HP = Health.HP;
		Brain.Stats.Motivate(1);
	}

	private void OnDie(GameObject attacker, Vector3 attackLocation, Health victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		Brain.DiedOfIllness = true;
		Brain.CheckChangeTask();
	}

	private void OnFaithChanged(int followerID, float newValue, float oldValue, float change)
	{
		if (followerID == Brain.Info.ID)
		{
			bool flag = change > 0f;
			UITextPopUp.Create(string.Format("{0}{1}", flag ? "+" : "", change), flag ? Color.green : Color.red, base.gameObject, new Vector3(0f, 2f));
		}
	}

	private void OnFearLoveChanged(int followerID, float newValue, float oldValue, float change)
	{
		if (followerID == Brain.Info.ID)
		{
			bool flag = change > 0f;
			UITextPopUp.Create(string.Format("{0}{1}", flag ? "+" : "", change), flag ? Color.green : Color.red, base.gameObject, new Vector3(0f, 2f));
		}
	}

	public void DieWithAnimation(string animation, float duration, string deadAnimation = "dead", bool playAnimation = true, int dir = 1, NotificationCentre.NotificationType deathNotificationType = NotificationCentre.NotificationType.Died, Action<GameObject> callback = null, bool force = false)
	{
		TimedAnimation(animation, duration, delegate
		{
			Die(deathNotificationType, deadAnimation: deadAnimation, PlayAnimation: playAnimation, Dir: dir, callback: callback, force: force);
		});
		_dying = true;
	}

	public void Die(NotificationCentre.NotificationType deathNotificationType = NotificationCentre.NotificationType.Died, bool PlayAnimation = true, int Dir = 1, string deadAnimation = "dead", Action<GameObject> callback = null, bool force = false)
	{
		GameManager.GetInstance().StartCoroutine(FollowerDieIE(deathNotificationType, PlayAnimation, Dir, deadAnimation, callback, force));
	}

	private IEnumerator FollowerDieIE(NotificationCentre.NotificationType deathNotificationType = NotificationCentre.NotificationType.Died, bool PlayAnimation = true, int Dir = 1, string deadAnimation = "dead", Action<GameObject> callback = null, bool force = false)
	{
		while (!force && (PlayerFarming.Location != FollowerLocation.Base || LetterBox.IsPlaying || MMConversation.isPlaying || SimulationManager.IsPaused || Brain.Location != FollowerLocation.Base || !PlayerFarming.LongToPerformPlayerStates.Contains(PlayerFarming.Instance.state.CURRENT_STATE)))
		{
			yield return null;
		}
		if (deathNotificationType != NotificationCentre.NotificationType.Ascended)
		{
			if (deathNotificationType == NotificationCentre.NotificationType.DiedFromDeadlyMeal)
			{
				Brain.DiedFromDeadlyDish = true;
			}
			if (deathNotificationType != NotificationCentre.NotificationType.MurderedByYou)
			{
				SimulationManager.Pause();
				GameManager.GetInstance().OnConversationNew();
				GameManager.GetInstance().OnConversationNext(base.gameObject);
				string deathText = Brain._directInfoAccess.GetDeathText();
				if (!string.IsNullOrEmpty(deathText))
				{
					LetterBox.Instance.ShowSubtitle(deathText);
				}
				yield return new WaitForSeconds(0.7f);
				AudioManager.Instance.PlayOneShot("event:/Stings/church_bell", base.gameObject);
				yield return new WaitForSeconds(0.25f);
				SimulationManager.UnPause();
			}
			StructuresData structure = StructuresData.GetInfoByType(StructureBrain.TYPES.DEAD_WORSHIPPER, 0);
			structure.FollowerID = Brain.Info.ID;
			structure.Dir = Dir;
			StructureManager.BuildStructure(Brain.Location, structure, base.transform.position, Vector2Int.one, false, delegate(GameObject g)
			{
				DeadWorshipper component = g.GetComponent<DeadWorshipper>();
				if (component != null)
				{
					component.PlayAnimation = PlayAnimation;
					component.DeadAnimation = deadAnimation;
					component.Setup();
				}
				if (structure != null)
				{
					PlacementRegion.TileGridTile closestTileGridTileAtWorldPosition = PlacementRegion.Instance.GetClosestTileGridTileAtWorldPosition(structure.Position);
					if (closestTileGridTileAtWorldPosition != null)
					{
						component.Structure.Brain.AddToGrid(closestTileGridTileAtWorldPosition.Position);
					}
				}
				Action<GameObject> action = callback;
				if (action != null)
				{
					action(g);
				}
			});
		}
		if (Brain != null)
		{
			Dwelling.DwellingAndSlot dwellingAndSlot = Brain.GetDwellingAndSlot();
			if (dwellingAndSlot != null)
			{
				Dwelling dwellingByID = Dwelling.GetDwellingByID(dwellingAndSlot.ID);
				if (dwellingByID != null)
				{
					dwellingByID.SetBedImage(dwellingAndSlot.dwellingslot, Dwelling.SlotState.UNCLAIMED);
				}
			}
			Brain.Die(deathNotificationType);
		}
		UnityEngine.Object.Destroy(base.gameObject);
		if (deathNotificationType != NotificationCentre.NotificationType.Ascended && deathNotificationType != NotificationCentre.NotificationType.MurderedByYou)
		{
			yield return new WaitForSeconds(3f);
			GameManager.GetInstance().OnConversationEnd();
		}
	}

	public void Leave(NotificationCentre.NotificationType leaveNotificationType)
	{
		FollowerManager.FollowerLeave(Brain.Info.ID, leaveNotificationType);
		Brain.Leave(leaveNotificationType);
		if ((bool)base.gameObject)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void FacePosition(Vector3 positionToFace)
	{
		Spine.Skeleton.ScaleX = ((!(base.transform.position.x < positionToFace.x)) ? 1 : (-1));
	}

	public void ClearPath()
	{
		_currentPath = null;
		_currentWaypoint = 0;
		_speed = 0f;
		if (Seeker != null)
		{
			Seeker.CancelCurrentPathRequest();
		}
	}

	public void GoTo(Vector3 destination, Action onComplete)
	{
		ClearPath();
		if (AstarPath.active != null)
		{
			Seeker.StartPath(base.transform.position, destination);
		}
		else
		{
			SetBodyAnimation(AnimWalking, true);
			State.CURRENT_STATE = StateMachine.State.Moving;
			_currentPath = new List<Vector3>();
			_currentPath.Add(base.transform.position);
			_currentPath.Add(destination);
			_currentWaypoint = 1;
			_startPos = base.transform.position;
			_destPos = _currentPath[1];
		}
		_onPathComplete = onComplete;
	}

	public IEnumerator GoToRoutine(Vector3 destination)
	{
		bool pathComplete = false;
		GoTo(destination, delegate
		{
			pathComplete = true;
		});
		while (!pathComplete)
		{
			yield return null;
		}
	}

	public void StartPath(Path p)
	{
		if (!p.error)
		{
			if (State.CURRENT_STATE != StateMachine.State.Moving)
			{
				SetBodyAnimation(AnimWalking, true);
				State.CURRENT_STATE = StateMachine.State.Moving;
			}
			_currentPath = p.vectorPath;
			_currentWaypoint = 1;
			_startPos = base.transform.position;
			_destPos = _currentPath[1];
			_t = 0f;
		}
	}

	private void UpdateMovement()
	{
		float num = (UseUnscaledTime ? GameManager.FixedUnscaledDeltaTime : GameManager.FixedDeltaTime);
		if (_currentPath == null || _currentWaypoint >= _currentPath.Count || State.CURRENT_STATE != StateMachine.State.Moving)
		{
			_speed += (0f - _speed) / 4f * num;
		}
		else
		{
			State.facingAngle = Utils.GetAngle(base.transform.position, _currentPath[_currentWaypoint]);
			if (_t >= 1f)
			{
				_t = 0f;
				_currentWaypoint++;
				if (_currentWaypoint == _currentPath.Count)
				{
					ClearPath();
					SetBodyAnimation(AnimIdle, true);
					State.CURRENT_STATE = StateMachine.State.Idle;
					Action onPathComplete = _onPathComplete;
					_onPathComplete = null;
					if (onPathComplete != null)
					{
						onPathComplete();
					}
				}
				else
				{
					_startPos = base.transform.position;
					_destPos = _currentPath[_currentWaypoint];
				}
			}
			else if (Brain != null && Brain.CurrentState != null && _speed < Brain.CurrentState.MaxSpeed)
			{
				_speed += 1f * num;
				_speed = Mathf.Clamp(_speed, 0f, (Brain.Location == FollowerLocation.Church) ? 2.25f : Brain.CurrentState.MaxSpeed);
				_speed *= ((Brain.Info.Necklace == InventoryItem.ITEM_TYPE.Necklace_2) ? 1.25f : 1f);
			}
		}
		if (_speed > 0f)
		{
			float num2 = Vector3.Distance(_startPos, _destPos);
			float num3 = _speed / num2;
			FacePosition(_destPos);
			base.transform.position = Vector3.Lerp(_startPos, _destPos, _t);
			_t += num3 * (UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
			_t = Mathf.Clamp01(_t);
		}
	}

	public void HideStats()
	{
	}

	public void PickUp()
	{
		if (Brain.CurrentTaskType != FollowerTaskType.ManualControl)
		{
			Brain.HardSwapToTask(new FollowerTask_ManualControl());
		}
		Brain.CurrentTask.ClearDestination();
		State.CURRENT_STATE = StateMachine.State.InActive;
	}

	public void Drop()
	{
		Vector3 position = base.transform.position;
		position.z = 0f;
		base.transform.position = position;
		TimedAnimation("put-down", 0.3f, Dropped);
		HideStats();
	}

	public void Dropped()
	{
		Brain.CompleteCurrentTask();
		State.CURRENT_STATE = StateMachine.State.Idle;
	}

	public void ApplyEffect(string animation, Action effect, float Timer = 1f, bool useDeltaTime = true)
	{
		TimedAnimation(animation, Timer, delegate
		{
			effect();
		}, useDeltaTime);
		HideStats();
	}

	public void ResetStateAnimations()
	{
		if (Brain.CurrentState != null)
		{
			Brain.CurrentState.SetStateAnimations(this);
		}
		else
		{
			SimpleAnimator.ResetAnimationsToDefaults();
		}
	}

	public void SetFaceAnimation(string animName, bool loop)
	{
		if (Spine.AnimationName != animName)
		{
			Spine.AnimationState.SetAnimation(0, animName, loop);
		}
	}

	public float SetBodyAnimation(string animName, bool loop)
	{
		if (Spine.AnimationName != animName)
		{
			return Spine.AnimationState.SetAnimation(1, animName, loop).Animation.Duration;
		}
		if (Spine.AnimationState != null && Spine.AnimationState.Tracks != null && Spine.AnimationState.Tracks.Count > 0)
		{
			return Spine.AnimationState.Tracks.Items[0].Animation.Duration;
		}
		return 0f;
	}

	public void AddBodyAnimation(string animName, bool loop, float Delay)
	{
		Spine.AnimationState.AddAnimation(1, animName, loop, Delay);
	}

	private void SetEmotionAnimation(TrackEntry trackEntry)
	{
		if (trackEntry.TrackIndex == 1)
		{
			SetEmotionAnimation();
			SetOverrideOutfit();
		}
	}

	private void SetEmotionAnimation()
	{
		if (!OverridingEmotions)
		{
			if (Brain.Info.CursedState == Thought.Dissenter)
			{
				SetFaceAnimation("Emotions/emotion-dissenter", true);
			}
			else if (Brain.Stats.HasLevelledUp)
			{
				SetFaceAnimation("Emotions/emotion-enlightened", true);
			}
			else if (FollowerBrainStats.BrainWashed)
			{
				SetFaceAnimation("Emotions/emotion-brainwashed", true);
			}
			else if (Brain.Info.CursedState == Thought.Ill)
			{
				SetFaceAnimation("Emotions/emotion-sick", true);
			}
			else if (Brain.Stats.Rest <= 20f)
			{
				SetFaceAnimation("Emotions/emotion-tired", true);
			}
			else if (CultFaithManager.CurrentFaith >= 0f && CultFaithManager.CurrentFaith <= 25f)
			{
				SetFaceAnimation("Emotions/emotion-angry", true);
			}
			else if (CultFaithManager.CurrentFaith > 25f && CultFaithManager.CurrentFaith <= 40f)
			{
				SetFaceAnimation("Emotions/emotion-unhappy", true);
			}
			else if (CultFaithManager.CurrentFaith > 40f && CultFaithManager.CurrentFaith <= 80f)
			{
				SetFaceAnimation("Emotions/emotion-normal", true);
			}
			else if (CultFaithManager.CurrentFaith > 75f)
			{
				SetFaceAnimation("Emotions/emotion-happy", true);
			}
		}
	}

	public void SetHat(HatType hatType)
	{
		CurrentHat = hatType;
		SetOverrideOutfit(true);
	}

	private void SetOverrideOutfit(bool forceUpdate = false)
	{
		if (OverridingOutfit)
		{
			return;
		}
		if (Brain.CurrentTaskType != FollowerTaskType.Refinery)
		{
			if (Brain.Info.TaxEnforcer)
			{
				CurrentHat = HatType.TaxEnforcer;
			}
			else if (Brain.Info.FaithEnforcer)
			{
				CurrentHat = HatType.FaithEnforcer;
			}
		}
		if (Brain.CurrentTaskType != FollowerTaskType.MissionaryComplete && Brain.CurrentTaskType != FollowerTaskType.MissionaryInProgress && Brain.CurrentTaskType != FollowerTaskType.ChangeLocation)
		{
			if (Brain.Location == FollowerLocation.Base && FollowerBrainStats.IsHoliday && Outfit.CurrentOutfit != FollowerOutfitType.Holiday)
			{
				Outfit.SetOutfit(Spine, FollowerOutfitType.Holiday, Brain.Info.Necklace, false, Thought.None, CurrentHat);
			}
			else if (Brain.Location == FollowerLocation.Base && Outfit.CurrentOutfit != FollowerOutfitType.Old && Brain.Info.CursedState == Thought.OldAge)
			{
				Outfit.SetOutfit(Spine, FollowerOutfitType.Old, Brain.Info.Necklace, false, Thought.None, CurrentHat);
			}
			else if (Brain.Location == FollowerLocation.Base && Outfit.CurrentOutfit == FollowerOutfitType.Holiday && !FollowerBrainStats.IsHoliday)
			{
				Outfit.SetOutfit(Spine, FollowerOutfitType.Follower, Brain.Info.Necklace, false, Thought.None, CurrentHat);
			}
			else if (forceUpdate)
			{
				Outfit.SetOutfit(Spine, FollowerOutfitType.Follower, Brain.Info.Necklace, false, Thought.None, CurrentHat);
			}
		}
	}

	public void SetOutfit(FollowerOutfitType outfitType, bool hooded, Thought overrideCursedState = Thought.None)
	{
		Outfit.SetOutfit(Spine, outfitType, Brain.Info.Necklace, hooded, overrideCursedState, CurrentHat);
		Brain._directInfoAccess.Outfit = outfitType;
	}

	public void ButtonDie()
	{
		Die();
	}

	public void LeaveCult()
	{
		Brain.LeavingCult = true;
	}

	public void DebugSkinName()
	{
		Debug.Log(Brain.Info.SkinName);
	}

	public void GetAttentionTask()
	{
		Brain.HardSwapToTask(new FollowerTask_GetAttention(Brain.GetMostPressingComplaint()));
	}

	public void AddThought(Thought ThoughtType)
	{
		Brain.AddThought(ThoughtType);
	}

	public void RemoveCursedState(Thought ThoughtType)
	{
		Brain.RemoveCurseState(ThoughtType);
	}

	public void AddTrait(FollowerTrait.TraitType Trait)
	{
		Brain.AddTrait(Trait);
	}

	public void AddBathroom()
	{
		Brain.Stats.Bathroom += 10f;
	}

	public void CheckRole()
	{
		Debug.Log(Brain.Info.Name + "   " + Brain.Info.FollowerRole);
	}

	public void SetWorshipper()
	{
		Brain.Info.FollowerRole = FollowerRole.Worshipper;
		Brain.CompleteCurrentTask();
		SetOutfit(FollowerOutfitType.Follower, false);
	}

	public void SetMonk()
	{
		Brain.Info.FollowerRole = FollowerRole.Monk;
		Brain.CompleteCurrentTask();
		SetOutfit(FollowerOutfitType.Follower, false);
	}

	public void SetWorker()
	{
		Brain.Info.FollowerRole = FollowerRole.Worker;
		Brain.CompleteCurrentTask();
		SetOutfit(FollowerOutfitType.Follower, false);
	}

	public void HoodOn(string animation, bool snap)
	{
		if (snap)
		{
			Outfit.SetOutfit(Spine, Brain.Info.Outfit, Brain.Info.Necklace, true);
			SetBodyAnimation(animation, true);
		}
		else
		{
			GameManager.GetInstance().StartCoroutine(PutHoodOnRoutine(animation));
		}
	}

	public void HoodOff(string animation = "idle", bool snap = false, Action onComplete = null)
	{
		StartCoroutine(HoodOffWaitForEndOfFrame(animation, snap, onComplete));
	}

	private IEnumerator HoodOffWaitForEndOfFrame(string animation = "idle", bool snap = false, Action onComplete = null)
	{
		if (Outfit.IsHooded)
		{
			if (snap)
			{
				Outfit.SetOutfit(Spine, Brain.Info.Outfit, Brain.Info.Necklace, false, Thought.None, CurrentHat);
				SetBodyAnimation(animation, true);
				if (onComplete != null)
				{
					onComplete();
				}
			}
			else
			{
				yield return null;
				StartCoroutine(TakeHoodOffRoutine(animation, onComplete));
			}
		}
		else if (onComplete != null)
		{
			onComplete();
		}
	}

	public IEnumerator PutHoodOnRoutine(string animation)
	{
		float waitDuration2;
		if (!Outfit.IsHooded)
		{
			SetBodyAnimation(AnimHoodUp, false);
			AddBodyAnimation(animation, true, 0f);
			waitDuration2 = 19f / 30f;
			while (true)
			{
				float num;
				waitDuration2 = (num = waitDuration2 - Time.deltaTime);
				if (!(num > 0f))
				{
					break;
				}
				yield return null;
			}
			Outfit.SetOutfit(Spine, Brain.Info.Outfit, Brain.Info.Necklace, true, Thought.None, CurrentHat);
		}
		else
		{
			SetBodyAnimation(animation, true);
		}
		waitDuration2 = 1f / 3f;
		while (true)
		{
			float num;
			waitDuration2 = (num = waitDuration2 - Time.deltaTime);
			if (num > 0f)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	public IEnumerator TakeHoodOffRoutine(string animation = "idle", Action onComplete = null)
	{
		SetBodyAnimation(AnimHoodDown, false);
		AddBodyAnimation(animation, true, 0f);
		yield return new WaitForSecondsRealtime(0.5f);
		Outfit.SetOutfit(Spine, Brain.Info.Outfit, Brain.Info.Necklace, false, Thought.None, CurrentHat);
		yield return new WaitForSecondsRealtime(0.5f);
		if (onComplete != null)
		{
			onComplete();
		}
	}

	public void TimedAnimationWithHood(string animation, float timer, Action onComplete = null, bool Loop = true, bool UseDeltaTime = true)
	{
	}

	public void TimedAnimation(string animation, float timer, Action onComplete = null, bool Loop = true, bool useDeltaTime = true)
	{
		UseDeltaTime = useDeltaTime;
		Spine.UseDeltaTime = useDeltaTime;
		if (State.CURRENT_STATE == StateMachine.State.TimedAction)
		{
			_timedActionTimer = 0f;
			Action timedAction = _timedAction;
			if (timedAction != null)
			{
				timedAction();
			}
			_timedAction = null;
		}
		if (!_dying)
		{
			ClearPath();
			_timedActionTimer = timer;
			State.CURRENT_STATE = StateMachine.State.TimedAction;
			SetBodyAnimation(animation, Loop);
			_timedAction = onComplete;
		}
	}

	private void OnThoughtModified(Thought thought)
	{
		switch (thought)
		{
		case Thought.Brainwashed:
		case Thought.Dissenter:
		case Thought.Ill:
		case Thought.Holiday:
		case Thought.Cult_CureDissenter:
		case Thought.Cult_MushroomEncouraged_Trait:
			SetOutfit(Outfit.CurrentOutfit, Outfit.IsHooded);
			break;
		}
	}

	private void Brain_OnCursedStateRemoved()
	{
		SetOutfit(Outfit.CurrentOutfit, Outfit.IsHooded);
	}

	public void PlayerGetSoul(int devotion)
	{
		Brain.Stats.DevotionGiven += devotion;
		PlayerFarming.Instance.GetSoul(devotion);
	}

	public void HideBark()
	{
		simpleBark.Close();
		showingBark = false;
		ShowAllFollowerIcons();
	}

	public void ShowBarkMessageTest()
	{
		cachedBarkMessage = "Test test testerinooo";
	}

	public void ShowBarkMessage(string message)
	{
		cachedBarkMessage = message;
	}

	public void ShowTwitchMessage()
	{
		FollowerTask_GetAttention followerTask_GetAttention;
		if ((followerTask_GetAttention = Brain.CurrentTask as FollowerTask_GetAttention) != null)
		{
			followerTask_GetAttention.StopSpeechBubble(this);
		}
		Brain.HardSwapToTask(new FollowerTask_ManualControl());
		string anim = string.Format("Conversations/talk-nice{0}", UnityEngine.Random.Range(1, 4));
		if (cachedBarkMessage.Contains("love"))
		{
			anim = string.Format("Conversations/talk-love{0}", UnityEngine.Random.Range(1, 4));
		}
		else if (cachedBarkMessage.Contains("hate"))
		{
			anim = string.Format("Conversations/talk-hate{0}", UnityEngine.Random.Range(1, 4));
		}
		GameManager.GetInstance().WaitForSeconds(0.5f, delegate
		{
			SetBodyAnimation(anim, true);
		});
		GameManager.GetInstance().WaitForSeconds(5f, delegate
		{
			Brain.CompleteCurrentTask();
		});
		ShowBarkMessage();
	}

	private void ShowBarkMessage()
	{
		simpleBark.Entries = new List<ConversationEntry>(1)
		{
			new ConversationEntry(base.gameObject, cachedBarkMessage)
		};
		simpleBark.ActivateDistance = 0f;
		if (Spine.GetComponent<Renderer>().isVisible)
		{
			HideAllFollowerIcons();
			simpleBark.Entries[0].CharacterName = Brain.Info.Name;
			simpleBark.Renderer = Spine.GetComponent<Renderer>();
			simpleBark.enabled = true;
			simpleBark.Show();
			simpleBark.OnClose += delegate
			{
				showingBark = false;
				ShowAllFollowerIcons();
			};
			MMConversation.mmConversation.SpeechBubble.ForceOffset = true;
			MMConversation.mmConversation.SpeechBubble.ScreenOffset = -20f;
			MMConversation.mmConversation.SpeechBubble.transform.localScale = Vector3.one * 0.75f;
		}
		cachedBarkMessage = "";
		showingBark = true;
	}
}
