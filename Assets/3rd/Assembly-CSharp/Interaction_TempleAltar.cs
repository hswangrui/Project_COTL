using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.AltarMenu;
using Lamb.UI.Rituals;
using Lamb.UI.SermonWheelOverlay;
using MMTools;
using Spine;
using Spine.Unity;
using src.Extensions;
using src.Rituals;
using src.UI.Overlays.TutorialOverlay;
using Unify;
using UnityEngine;

public class Interaction_TempleAltar : Interaction
{
	public static Interaction_TempleAltar Instance;

	private SermonController SermonController;

	[SerializeField]
	private Collider2D Collider;

	public ChurchFollowerManager ChurchFollowerManager;

	public GameObject RitualAvailable;

	public Animator RitualAvailableAnimator;

	public GameObject distortionObject;

	public GameObject SermonAvailableObject;

	public GameObject RitualAvailableObject;

	public GameObject Menu;

	public Sprite AltarEmpty;

	public Sprite SpriteOn;

	public Sprite SpriteOff;

	public Sprite SpriteNotPurchased;

	public Material SpriteOnMaterial;

	public Material SpriteOffMaterial;

	public SpriteRenderer spriteRenderer;

	public bool Activated;

	public GameObject Notification;

	public GameObject DoctrineXPPrefab;

	private UIDoctrineBar UIDoctrineBar;

	public static SermonsAndRituals.SermonRitualType CurrentType;

	public UIFollowerXP UIFollowerXPPrefab;

	public GameObject DoctrineUpgradePrefab;

	private bool firstChoice;

	private float barLocalXP;

	public Light RitualLighting;

	public SkeletonAnimation PortalEffect;

	private bool performedSermon;

	private string sPreachSermon;

	private string sAlreadyGivenSermon;

	private string sSacrifice;

	private string sRitual;

	private string sRequiresTemple2;

	private string sRequireMoreFollowers;

	private string sInteract;

	private bool initialInteraction = true;

	public SermonCategory SermonCategory;

	private bool SermonsStillAvailable = true;

	private EventInstance sermonLoop;

	private bool GivenAnswer;

	private int RewardLevel;

	public static DoctrineUpgradeSystem.DoctrineType DoctrineUnlockType;

	public GameObject RitualCameraPosition;

	private bool HasBuiltTemple2;

	public SimpleSetCamera CloseUpCamera;

	public SimpleSetCamera SimpleSetCamera;

	public SimpleSetCamera RitualCloseSetCamera;

	public GameObject FrontWall;

	private bool Activating;

	private Ritual CurrentRitual;

	private UpgradeSystem.Type RitualType;

	private int fleece;

	public void ResetSprite()
	{
		if (!(spriteRenderer == null))
		{
			if (DataManager.Instance.PreviousSermonDayIndex < TimeManager.CurrentDay)
			{
				spriteRenderer.sprite = SpriteOn;
			}
			else
			{
				spriteRenderer.sprite = SpriteOff;
			}
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		HasSecondaryInteraction = false;
		HasBuiltTemple2 = DataManager.Instance.HasBuiltTemple2;
		RitualAvailableAnimator.Play("Hidden");
		distortionObject.SetActive(false);
		ResetSprite();
	}

	private bool CheckCanAfford(UpgradeSystem.Type type)
	{
		List<StructuresData.ItemCost> cost = UpgradeSystem.GetCost(type);
		for (int i = 0; i < cost.Count; i++)
		{
			if (Inventory.GetItemQuantity((int)cost[i].CostItem) < cost[i].CostValue)
			{
				return false;
			}
		}
		return true;
	}

	private void Start()
	{
		Instance = this;
		UpdateLocalisation();
		ActivateDistance = 1.5f;
		Interactable = true;
		SecondaryInteractable = true;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sInteract = ScriptLocalization.Interactions.TempleAltar;
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : sInteract);
	}

	public override void OnInteract(StateMachine state)
	{
		if (Activated)
		{
			return;
		}
		base.OnInteract(state);
		MonoSingleton<UIManager>.Instance.ForceDisableSaving = true;
		Activated = true;
		base.HasChanged = true;
		OnBecomeNotCurrent();
		RitualAvailableAnimator.Play("Hidden");
		GameManager.GetInstance().OnConversationNew(false, false, true);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 8f);
		GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = true;
		CultFaithManager.Instance.BarController.UseQueuing = false;
		Collider.enabled = false;
		PlayerFarming.Instance.GoToAndStop(ChurchFollowerManager.Instance.AltarPosition.gameObject, base.gameObject, false, false, delegate
		{
			Collider.enabled = true;
			if (initialInteraction)
			{
				Ritual.FollowerToAttendSermon = Ritual.GetFollowersAvailableToAttendSermon();
				foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
				{
					ChurchFollowerManager.Instance.AddBrainToAudience(item);
				}
			}
			initialInteraction = false;
			StartCoroutine(DelayMenu());
		});
	}

	private IEnumerator DelayMenu()
	{
		spriteRenderer.sprite = AltarEmpty;
		AudioManager.Instance.PlayOneShot("event:/sermon/book_pickup", PlayerFarming.Instance.gameObject);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
		state.transform.DOMove(ChurchFollowerManager.Instance.AltarPosition.transform.position, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
		yield return new WaitForSeconds(0.2f);
		PlayerFarming.Instance.Spine.UseDeltaTime = false;
		Time.timeScale = 0f;
		MonoSingleton<UIManager>.Instance.ForceBlockMenus = true;
		UIAltarMenuController uIAltarMenuController = MonoSingleton<UIManager>.Instance.ShowAltarMenu();
		uIAltarMenuController.OnHidden = (Action)Delegate.Combine(uIAltarMenuController.OnHidden, (Action)delegate
		{
			PlayerFarming.Instance.Spine.UseDeltaTime = true;
		});
		uIAltarMenuController.OnSermonSelected = (Action)Delegate.Combine(uIAltarMenuController.OnSermonSelected, (Action)delegate
		{
			DoSermon();
		});
		uIAltarMenuController.OnRitualsSelected = (Action)Delegate.Combine(uIAltarMenuController.OnRitualsSelected, (Action)delegate
		{
			DoRitual();
		});
		uIAltarMenuController.OnPlayerUpgradesSelected = (Action)Delegate.Combine(uIAltarMenuController.OnPlayerUpgradesSelected, (Action)delegate
		{
			DoPlayerUpgrade();
		});
		uIAltarMenuController.OnDoctrineSelected = (Action)Delegate.Combine(uIAltarMenuController.OnDoctrineSelected, (Action)delegate
		{
			DoDoctrine();
		});
		uIAltarMenuController.OnCancel = (Action)Delegate.Combine(uIAltarMenuController.OnCancel, (Action)delegate
		{
			DoCancel();
		});
	}

	private void DoSermon()
	{
		MonoSingleton<UIManager>.Instance.ForceBlockMenus = false;
		if (DataManager.Instance.PreviousSermonDayIndex >= TimeManager.CurrentDay)
		{
			DoCancel();
			CloseAndSpeak("AlreadyGivenSermon");
			return;
		}
		if (DataManager.Instance.Followers.Count <= 0)
		{
			DoCancel();
			CloseAndSpeak("NoFollowers");
			return;
		}
		if (Ritual.FollowersAvailableToAttendSermon() <= 0)
		{
			DoCancel();
			CloseAndSpeak("NoFollowersAvailable");
			return;
		}
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		performedSermon = true;
		SermonController = GetComponent<SermonController>();
		SermonController.Play(state);
	}

	private void DoRitual()
	{
		MonoSingleton<UIManager>.Instance.ForceBlockMenus = false;
		if (Ritual.FollowersAvailableToAttendSermon() <= 0)
		{
			DoCancel();
			CloseAndSpeak("NoFollowers");
			return;
		}
		if (Ritual.FollowersAvailableToAttendSermon() <= 0)
		{
			DoCancel();
			CloseAndSpeak("NoFollowersAvailable");
			return;
		}
		GameManager.GetInstance().CameraSetOffset(Vector3.left * 2.25f);
		GameManager.GetInstance().OnConversationNext(state.gameObject, 6f);
		AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/start_ritual", PlayerFarming.Instance.gameObject);
		RitualAvailableAnimator.Play("Hidden");
		StartCoroutine(OpenRitualMenuRoutine());
		Activated = false;
	}

	private void DoDoctrine()
	{
		Time.timeScale = 0f;
		MonoSingleton<UIManager>.Instance.ForceBlockMenus = false;
		PlayerFarming.Instance.Spine.UseDeltaTime = false;
		Activated = false;
		UIDoctrineMenuController uIDoctrineMenuController = MonoSingleton<UIManager>.Instance.DoctrineMenuTemplate.Instantiate();
		uIDoctrineMenuController.Show();
		uIDoctrineMenuController.OnHide = (Action)Delegate.Combine(uIDoctrineMenuController.OnHide, (Action)delegate
		{
			GameManager.GetInstance().CameraSetOffset(Vector3.zero);
			OnInteract(state);
		});
	}

	private void DoPlayerUpgrade()
	{
		MonoSingleton<UIManager>.Instance.ForceBlockMenus = false;
		GameManager.GetInstance().CameraSetOffset(Vector3.left * 2.25f);
		GameManager.GetInstance().OnConversationNext(state.gameObject, 6f);
		AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/start_ritual", PlayerFarming.Instance.gameObject);
		RitualAvailableAnimator.Play("Hidden");
		StartCoroutine(OpenPlayerUpgradeRoutine());
		Activated = false;
	}

	private IEnumerator OpenPlayerUpgradeRoutine()
	{
		yield return new WaitForSeconds(0.5f);
		OpenPlayerUpgradeMenu();
	}

	private void OpenPlayerUpgradeMenu()
	{
		UIPlayerUpgradesMenuController playerUpgradesMenuInstance = MonoSingleton<UIManager>.Instance.ShowPlayerUpgradesMenu();
		UIPlayerUpgradesMenuController uIPlayerUpgradesMenuController = playerUpgradesMenuInstance;
		uIPlayerUpgradesMenuController.OnCancel = (Action)Delegate.Combine(uIPlayerUpgradesMenuController.OnCancel, (Action)delegate
		{
			GameManager.GetInstance().CameraSetOffset(Vector3.zero);
			OnInteract(state);
		});
		UIPlayerUpgradesMenuController uIPlayerUpgradesMenuController2 = playerUpgradesMenuInstance;
		uIPlayerUpgradesMenuController2.OnHidden = (Action)Delegate.Combine(uIPlayerUpgradesMenuController2.OnHidden, (Action)delegate
		{
			playerUpgradesMenuInstance = null;
		});
	}

	private void DoCancel()
	{
		MonoSingleton<UIManager>.Instance.ForceBlockMenus = false;
		MonoSingleton<UIManager>.Instance.ForceDisableSaving = false;
		SimulationManager.UnPause();
		AudioManager.Instance.StopLoop(sermonLoop);
		AudioManager.Instance.PlayOneShot("event:/sermon/book_put_down", PlayerFarming.Instance.gameObject);
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		Time.timeScale = 1f;
		PlayerFarming.Instance.Spine.UseDeltaTime = true;
		GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = false;
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		GameManager.GetInstance().OnConversationEnd();
		CultFaithManager.Instance.BarController.UseQueuing = false;
		initialInteraction = true;
		if (Ritual.FollowerToAttendSermon != null)
		{
			foreach (FollowerBrain f in Ritual.FollowerToAttendSermon)
			{
				if (f == null)
				{
					continue;
				}
				Follower follower = FollowerManager.FindFollowerByID(f.Info.ID);
				if ((bool)follower)
				{
					FollowerManager.FindFollowerByID(f.Info.ID).Spine.UseDeltaTime = true;
					follower.ShowAllFollowerIcons();
					follower.Spine.UseDeltaTime = true;
					follower.UseUnscaledTime = false;
				}
				if (performedSermon)
				{
					int num = 0;
					if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Temple_III))
					{
						num = 1;
					}
					if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Temple_IV))
					{
						num = 2;
					}
					for (int i = 0; i < num; i++)
					{
						AudioManager.Instance.PlayOneShot("event:/followers/pop_in", follower.transform.position);
						ResourceCustomTarget.Create(Interaction_DonationBox.Instance.gameObject, follower.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, delegate
						{
							Interaction_DonationBox.Instance.DepositCoin();
						});
					}
				}
				f.CompleteCurrentTask();
				StartCoroutine(DelayCallback(1f, delegate
				{
					if (f.CurrentTaskType == FollowerTaskType.AttendTeaching)
					{
						f.CompleteCurrentTask();
					}
				}));
			}
		}
		ResetSprite();
		ChurchFollowerManager.Instance.ClearAudienceBrains();
		if (Ritual.FollowerToAttendSermon != null)
		{
			Ritual.FollowerToAttendSermon.Clear();
		}
		performedSermon = false;
		Activated = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		ResetSprite();
	}

	private void CloseAndSpeak(string ConversationEntryTerm)
	{
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(base.gameObject, "FollowerInteractions/" + ConversationEntryTerm, "idle"));
		list[0].SpeakerIsPlayer = true;
		MMConversation.Play(new ConversationObject(list, null, null));
	}

	public IEnumerator FollowersEnterForSermonRoutine(bool shufflePosition = false)
	{
		if (Ritual.FollowerToAttendSermon == null || Ritual.FollowerToAttendSermon.Count <= 0)
		{
			Ritual.FollowerToAttendSermon = Ritual.GetFollowersAvailableToAttendSermon();
		}
		if (TimeManager.CurrentPhase == DayPhase.Night)
		{
			DataManager.Instance.WokeUpEveryoneDay = TimeManager.CurrentDay;
		}
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			Follower follower = FollowerManager.FindFollowerByID(item.Info.ID);
			if ((bool)follower)
			{
				item.ShouldReconsiderTask = false;
				follower.HideAllFollowerIcons();
				follower.Spine.UseDeltaTime = false;
				follower.UseUnscaledTime = true;
			}
			if (!(item.CurrentTask is FollowerTask_AttendTeaching) || shufflePosition)
			{
				if (item.CurrentTask != null)
				{
					item.CurrentTask.Abort();
				}
				item.HardSwapToTask(new FollowerTask_AttendTeaching());
				item.ShouldReconsiderTask = false;
				if (item.CurrentTaskType == FollowerTaskType.ChangeLocation)
				{
					item.CurrentTask.Arrive();
				}
				yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0.05f, 0.15f));
			}
		}
		float timer = 0f;
		while (!FollowersInPosition())
		{
			float num;
			timer = (num = timer + Time.deltaTime);
			if (num < 10f)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	private bool ReadyForTeaching()
	{
		bool result = true;
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			if (item.ShouldReconsiderTask || item.CurrentTaskType == FollowerTaskType.ChangeLocation || item.Location != PlayerFarming.Location || (item.CurrentTaskType == FollowerTaskType.AttendTeaching && item.CurrentTask.State != FollowerTaskState.GoingTo))
			{
				result = false;
			}
		}
		return result;
	}

	private bool FollowersInPosition()
	{
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			if (item.CurrentTaskType != FollowerTaskType.ChangeLocation && item.Location == PlayerFarming.Location && (item.CurrentTaskType != FollowerTaskType.AttendTeaching || item.CurrentTask.State == FollowerTaskState.Doing))
			{
				continue;
			}
			if (item.Location != PlayerFarming.Location)
			{
				item.HardSwapToTask(new FollowerTask_AttendTeaching());
				item.ShouldReconsiderTask = false;
				if (item.CurrentTaskType == FollowerTaskType.ChangeLocation)
				{
					item.CurrentTask.Arrive();
				}
			}
			return false;
		}
		return true;
	}

	private IEnumerator TeachSermonRoutine()
	{
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
		AudioManager.Instance.PlayOneShot("event:/sermon/start_sermon", PlayerFarming.Instance.gameObject);
		AudioManager.Instance.PlayOneShot("event:/building/building_bell_ring", PlayerFarming.Instance.gameObject);
		StartCoroutine(CentrePlayer());
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 12f);
		yield return StartCoroutine(FollowersEnterForSermonRoutine());
		SimulationManager.Pause();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 7f);
		GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, -0.5f));
		yield return new WaitForSeconds(0.5f);
		SermonsStillAvailable = false;
		if (DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory.Afterlife) < 4)
		{
			SermonsStillAvailable = true;
		}
		if (DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory.Food) < 4)
		{
			SermonsStillAvailable = true;
		}
		if (DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory.Possession) < 4)
		{
			SermonsStillAvailable = true;
		}
		if (DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory.WorkAndWorship) < 4)
		{
			SermonsStillAvailable = true;
		}
		if (DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory.LawAndOrder) < 4)
		{
			SermonsStillAvailable = true;
		}
		if (DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory.Special) < 3)
		{
			Debug.Log("A");
			SermonCategory = SermonCategory.Special;
			SermonsStillAvailable = true;
			StartCoroutine(GatherFollowers());
		}
		else if (SermonsStillAvailable)
		{
			AudioManager.Instance.PlayOneShot("event:/sermon/sermon_menu_appear", PlayerFarming.Instance.gameObject);
			SermonCategory finalisedCategory = SermonCategory.None;
			UISermonWheelController uISermonWheelController = MonoSingleton<UIManager>.Instance.SermonWheelTemplate.Instantiate();
			uISermonWheelController.OnItemChosen = (Action<SermonCategory>)Delegate.Combine(uISermonWheelController.OnItemChosen, (Action<SermonCategory>)delegate(SermonCategory chosenCategory)
			{
				Debug.Log(string.Format("Chose category {0}", chosenCategory).Colour(Color.yellow));
				finalisedCategory = chosenCategory;
			});
			uISermonWheelController.OnHide = (Action)Delegate.Combine(uISermonWheelController.OnHide, (Action)delegate
			{
				if (finalisedCategory == SermonCategory.None)
				{
					DoCancel();
					{
						foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
						{
							try
							{
								item.CurrentTask.End();
							}
							catch (Exception)
							{
								Debug.Log(item.Info.Name);
							}
						}
						return;
					}
				}
				SermonCategory = finalisedCategory;
				StartCoroutine(GatherFollowers());
				AudioManager.Instance.PlayOneShot("event:/sermon/select_sermon", PlayerFarming.Instance.gameObject);
			});
			uISermonWheelController.Show();
		}
		else
		{
			SermonCategory = SermonCategory.None;
			StartCoroutine(GatherFollowers());
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		AudioManager.Instance.StopLoop(sermonLoop);
	}

	private new void OnDestroy()
	{
		AudioManager.Instance.StopLoop(sermonLoop);
		Ritual.OnEnd = (Action<bool>)Delegate.Remove(Ritual.OnEnd, new Action<bool>(RitualOnEnd));
	}

	public void PulseDisplacementObject(Vector3 Position)
	{
		distortionObject.transform.position = Position;
		if (distortionObject.gameObject.activeSelf)
		{
			distortionObject.transform.localScale = Vector3.zero;
			distortionObject.transform.DORestart();
			distortionObject.transform.DOPlay();
			return;
		}
		distortionObject.SetActive(true);
		distortionObject.transform.localScale = Vector3.zero;
		distortionObject.transform.DOScale(9f, 0.9f).SetEase(Ease.Linear).OnComplete(delegate
		{
			distortionObject.SetActive(false);
		});
	}

	public IEnumerator GatherFollowers()
	{
		GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, 1f));
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 7f);
		PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
		PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
		PlayerFarming.Instance.Spine.Update(Time.deltaTime);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("sermons/sermon-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("sermons/sermon-loop", 0, true, 0f);
		sermonLoop = AudioManager.Instance.CreateLoop("event:/sermon/preach_loop", PlayerFarming.Instance.gameObject, true, false);
		yield return new WaitForSeconds(0.6f);
		PulseDisplacementObject(state.transform.position);
		yield return new WaitForSeconds(0.4f);
		ChurchFollowerManager.Instance.StartSermonEffect();
		bool askedQuestion = false;
		if (SermonsStillAvailable)
		{
			askedQuestion = true;
			GameObject gameObject = UnityEngine.Object.Instantiate(DoctrineXPPrefab, GameObject.FindWithTag("Canvas").transform);
			UIDoctrineBar = gameObject.GetComponent<UIDoctrineBar>();
			float xp = DoctrineUpgradeSystem.GetXPBySermon(SermonCategory);
			DoctrineUpgradeSystem.GetXPTargetBySermon(SermonCategory);
			float num = 1.5f;
			yield return StartCoroutine(UIDoctrineBar.Show(xp, SermonCategory));
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 11f);
			int studyIncrementCount = Mathf.FloorToInt(DataManager.Instance.TempleStudyXP / 0.1f);
			float delay2 = 1.5f / (float)studyIncrementCount;
			if (studyIncrementCount > 0)
			{
				DataManager.Instance.TempleStudyXP -= (float)studyIncrementCount * 0.1f;
				for (int i = 0; i < studyIncrementCount; i++)
				{
					xp += 0.1f;
					if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Temple_SermonEfficiency))
					{
						xp += 0.05f;
					}
					if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Temple_SermonEfficiencyII))
					{
						xp += 0.05f;
					}
					float target2 = DoctrineUpgradeSystem.GetXPTargetBySermon(SermonCategory);
					SoulCustomTarget.Create(base.gameObject, ChurchFollowerManager.Instance.StudySlots[UnityEngine.Random.Range(0, ChurchFollowerManager.Instance.StudySlots.Length)].transform.position, Color.white, delegate
					{
						IncrementXPBar();
					}, 0.2f, 500f);
					yield return new WaitForSeconds(delay2);
					if (xp >= target2)
					{
						yield return new WaitForSeconds(0.5f);
						yield return StartCoroutine(UIDoctrineBar.UpdateSecondBar(xp, 0.5f));
						AudioManager.Instance.PlayOneShot("event:/sermon/upgrade_menu_appear");
						yield return StartCoroutine(UIDoctrineBar.FlashBarRoutine(0.3f, 1f));
						yield return StartCoroutine(UIDoctrineBar.Hide());
						yield return StartCoroutine(AskQuestionRoutine(InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE));
						xp = 0f;
						if (DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory) >= 4)
						{
							break;
						}
						if (i < studyIncrementCount - 1)
						{
							yield return StartCoroutine(UIDoctrineBar.Show(0f, SermonCategory));
							barLocalXP = 0f;
						}
					}
				}
				yield return new WaitForSeconds(1f);
				yield return StartCoroutine(UIDoctrineBar.UpdateSecondBar(xp, 0.5f));
				yield return new WaitForSeconds(0.5f);
			}
			int followersCount = Ritual.FollowerToAttendSermon.Count;
			delay2 = 1.5f / (float)followersCount;
			barLocalXP = xp;
			int count = 0;
			do
			{
				xp += 0.1f;
				if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Temple_SermonEfficiency))
				{
					xp += 0.05f;
				}
				if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Temple_SermonEfficiencyII))
				{
					xp += 0.05f;
				}
				float target2 = DoctrineUpgradeSystem.GetXPTargetBySermon(SermonCategory);
				SoulCustomTarget.Create(base.gameObject, Ritual.FollowerToAttendSermon[count].LastPosition, Color.white, delegate
				{
					IncrementXPBar();
				}, 0.2f, 500f);
				yield return new WaitForSeconds(delay2);
				if (xp >= target2)
				{
					yield return new WaitForSeconds(0.5f);
					yield return StartCoroutine(UIDoctrineBar.UpdateSecondBar(xp, 0.5f));
					yield return StartCoroutine(UIDoctrineBar.FlashBarRoutine(0.3f, 1f));
					yield return StartCoroutine(UIDoctrineBar.Hide());
					yield return StartCoroutine(AskQuestionRoutine(InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE));
					xp = 0f;
					if (DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory) >= 4)
					{
						break;
					}
					if (count < followersCount - 1)
					{
						yield return StartCoroutine(UIDoctrineBar.Show(0f, SermonCategory));
						barLocalXP = 0f;
					}
				}
				count++;
			}
			while (count < followersCount);
			ChurchFollowerManager.Instance.EndSermonEffect();
			yield return new WaitForSeconds(0.5f);
			yield return StartCoroutine(UIDoctrineBar.UpdateSecondBar(xp, 0.5f));
			yield return new WaitForSeconds(0.5f);
			UnityEngine.Object.Destroy(UIDoctrineBar.gameObject);
			DoctrineUpgradeSystem.SetXPBySermon(SermonCategory, xp);
		}
		else
		{
			yield return new WaitForSeconds(1f);
			ChurchFollowerManager.Instance.EndSermonEffect();
		}
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("DELIVER_FIRST_SERMON"));
		PlayerFarming.Instance.simpleSpineAnimator.Animate("sermons/sermon-stop", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/sermon/end_sermon", PlayerFarming.Instance.gameObject);
		sermonLoop.stop(STOP_MODE.ALLOWFADEOUT);
		AudioManager.Instance.StopLoop(sermonLoop);
		yield return new WaitForSeconds(1f / 3f);
		ResetSprite();
		AudioManager.Instance.PlayOneShot("event:/sermon/book_put_down", PlayerFarming.Instance.gameObject);
		GameManager.GetInstance().OnConversationEnd();
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = false;
		DataManager.Instance.PreviousSermonDayIndex = TimeManager.CurrentDay;
		PlayerFarming.Instance.Spine.UseDeltaTime = true;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
			{
				if (DataManager.Instance.UnlockededASermon > 1 && SermonCategory != 0 && SermonCategory == DataManager.Instance.PreviousSermonCategory)
				{
					allBrain.AddThought(Thought.WatchedSermonSameAsYesterday);
				}
				else if (allBrain.HasTrait(FollowerTrait.TraitType.SermonEnthusiast))
				{
					allBrain.AddThought(Thought.WatchedSermonDevotee);
				}
				else
				{
					allBrain.AddThought(Thought.WatchedSermon);
				}
				allBrain.AddAdoration(FollowerBrain.AdorationActions.Sermon, null);
				StartCoroutine(DelayFollowerReaction(allBrain, UnityEngine.Random.Range(0.1f, 0.5f)));
				Follower follower = FollowerManager.FindFollowerByID(allBrain.Info.ID);
				if ((object)follower != null)
				{
					follower.ShowAllFollowerIcons(false);
				}
			}
		}
		DataManager.Instance.PreviousSermonCategory = SermonCategory;
		ResetSprite();
		yield return new WaitForSeconds(0.5f);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.GiveSermon);
		Activated = false;
		yield return new WaitForSeconds(1.5f);
		if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.SermonEnthusiast))
		{
			CultFaithManager.AddThought(Thought.Cult_SermonEnthusiast_Trait, -1, 1f);
		}
		else
		{
			CultFaithManager.AddThought(Thought.Cult_Sermon, -1, 1f);
		}
		if (DataManager.Instance.WokeUpEveryoneDay == TimeManager.CurrentDay && TimeManager.CurrentPhase == DayPhase.Night && !FollowerBrainStats.IsWorkThroughTheNight)
		{
			CultFaithManager.AddThought(Thought.Cult_WokeUpEveryone, -1, 1f);
		}
		if (!askedQuestion)
		{
			PlayerFarming.Instance.health.BlueHearts += 2f;
		}
	}

	private void IncrementXPBar()
	{
		barLocalXP += 0.1f;
		StartCoroutine(UIDoctrineBar.UpdateFirstBar(barLocalXP, 0.1f));
	}

	public IEnumerator AskQuestionRoutine(InventoryItem.ITEM_TYPE currency)
	{
		AudioManager.Instance.PlayOneShot("event:/sermon/sermon_speech_bubble", PlayerFarming.Instance.gameObject);
		GivenAnswer = false;
		if (currency == InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE)
		{
			RewardLevel = 5;
		}
		else
		{
			RewardLevel = DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory) + 1;
		}
		string termToSpeak = string.Concat("DoctrineUpgradeSystem/", SermonCategory, RewardLevel);
		if (SermonCategory == SermonCategory.Special || RewardLevel >= 5)
		{
			termToSpeak = " ";
		}
		Debug.Log(string.Concat("Text: DoctrineUpgradeSystem/", SermonCategory, RewardLevel));
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(PlayerFarming.Instance.CameraBone, termToSpeak));
		list[0].CharacterName = "DoctrineUpgradeSystem/DoctrinalDecision";
		List<DoctrineResponse> list2 = new List<DoctrineResponse>
		{
			new DoctrineResponse(SermonCategory, RewardLevel, true, delegate
			{
				Reply(true);
			}),
			new DoctrineResponse(SermonCategory, RewardLevel, false, delegate
			{
				Reply(false);
			})
		};
		if (DoctrineUpgradeSystem.GetSermonReward(SermonCategory, RewardLevel, false) == DoctrineUpgradeSystem.DoctrineType.None)
		{
			list2.RemoveAt(1);
		}
		if (SermonCategory == SermonCategory.Special)
		{
			list2 = new List<DoctrineResponse>
			{
				new DoctrineResponse(SermonCategory, RewardLevel, true, delegate
				{
					Reply(true);
				})
			};
		}
		MMConversation.Play(new ConversationObject(list, null, null, list2), false, false, false, true, true, false, false);
		while (!GivenAnswer)
		{
			yield return null;
		}
		DoctrineUnlockType = DoctrineUpgradeSystem.GetSermonReward(SermonCategory, RewardLevel, firstChoice);
		DoctrineUpgradeSystem.UnlockAbility(DoctrineUnlockType);
		if (currency != InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE)
		{
			DoctrineUpgradeSystem.SetLevelBySermon(SermonCategory, DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory) + 1);
		}
		HUD_Manager.Instance.Hide(true, 0);
	}

	private void Reply(bool FirstChoice)
	{
		firstChoice = FirstChoice;
		GivenAnswer = true;
		Debug.Log("CALLBACK! " + FirstChoice);
	}

	public IEnumerator DelayFollowerReaction(FollowerBrain brain, float Delay)
	{
		yield return new WaitForSecondsRealtime(Delay);
		Follower f = FollowerManager.FindFollowerByID(brain.Info.ID);
		if (f == null)
		{
			yield break;
		}
		f.HideAllFollowerIcons();
		f.TimedAnimation("Reactions/react-enlightened1", 1.5f, delegate
		{
			f.UseUnscaledTime = true;
			((FollowerTask_AttendTeaching)f.Brain.CurrentTask).StartAgain(f);
		}, false, false);
		float num = 0f;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			num += allBrain.Stats.Happiness;
		}
		if (num / (100f * (float)FollowerBrain.AllBrains.Count) <= 0.2f)
		{
			NotificationCentre.Instance.PlayGenericNotification(NotificationCentre.NotificationType.LowFaithDonation);
		}
	}

	public IEnumerator CentrePlayer()
	{
		state.facingAngle = 270f;
		float Progress = 0f;
		Vector3 StartPosition = state.transform.position;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < 0.25f)
			{
				state.transform.position = Vector3.Lerp(StartPosition, ChurchFollowerManager.Instance.AltarPosition.position, Mathf.SmoothStep(0f, 1f, Progress / 0.25f));
				yield return null;
				continue;
			}
			break;
		}
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		if (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Temple2) || !HasBuiltTemple2)
		{
			MonoSingleton<Indicator>.Instance.PlayShake();
		}
		else if (!Activating)
		{
			base.OnSecondaryInteract(state);
			Activating = true;
			OnBecomeNotCurrent();
			base.HasChanged = true;
			GameManager.GetInstance().OnConversationNew(false);
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 8f);
			GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = true;
			PlayerFarming.Instance.GoToAndStop(ChurchFollowerManager.Instance.AltarPosition.gameObject, base.gameObject, false, false, delegate
			{
				AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/start_ritual", PlayerFarming.Instance.gameObject);
				RitualAvailableAnimator.Play("Hidden");
				StartCoroutine(CentrePlayer());
				StartCoroutine(OpenRitualMenuRoutine());
			});
		}
	}

	private IEnumerator OpenRitualMenuRoutine()
	{
		yield return new WaitForSeconds(0.5f);
		OpenRitualMenu();
	}

	private void OpenRitualMenu()
	{
		UIRitualsMenuController uIRitualsMenuController = MonoSingleton<UIManager>.Instance.RitualsMenuTemplate.Instantiate();
		uIRitualsMenuController.Show();
		uIRitualsMenuController.OnRitualSelected = (Action<UpgradeSystem.Type>)Delegate.Combine(uIRitualsMenuController.OnRitualSelected, new Action<UpgradeSystem.Type>(PerformRitual));
		uIRitualsMenuController.OnCancel = (Action)Delegate.Combine(uIRitualsMenuController.OnCancel, (Action)delegate
		{
			GameManager.GetInstance().CameraSetOffset(Vector3.zero);
			OnInteract(state);
		});
	}

	public void PerformRitual(UpgradeSystem.Type RitualType)
	{
		this.RitualType = RitualType;
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		if (CurrentRitual != null)
		{
			UnityEngine.Object.Destroy(CurrentRitual);
		}
		Ritual.OnEnd = (Action<bool>)Delegate.Combine(Ritual.OnEnd, new Action<bool>(RitualOnEnd));
		Debug.Log("Perform ritual: " + RitualType);
		ObjectiveManager.SetRitualObjectivesFailLocked();
		switch (RitualType)
		{
		case UpgradeSystem.Type.Ritual_HeartsOfTheFaithful1:
		{
			RitualFlockOfTheFaithful ritualFlockOfTheFaithful = base.gameObject.AddComponent<RitualFlockOfTheFaithful>();
			ritualFlockOfTheFaithful.ritualLight = RitualLighting;
			ritualFlockOfTheFaithful.Play();
			CurrentRitual = ritualFlockOfTheFaithful;
			break;
		}
		case UpgradeSystem.Type.Ritual_UnlockCurse:
			StartCoroutine(DelayCallback(0.5f, delegate
			{
				(UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/UI Unlock Curse"), GameObject.FindWithTag("Canvas").transform) as GameObject).GetComponent<UIUnlockCurse>().Init(delegate
				{
					CurrentRitual = base.gameObject.AddComponent<RitualUnlockCurse>();
					CurrentRitual.Play();
				}, delegate
				{
					OpenRitualMenu();
				});
			}));
			break;
		case UpgradeSystem.Type.Ritual_UnlockWeapon:
			StartCoroutine(DelayCallback(0.5f, delegate
			{
				(UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/UI Unlock Weapon"), GameObject.FindWithTag("Canvas").transform) as GameObject).GetComponent<UIUnlockWeapon>().Init(delegate
				{
					CurrentRitual = base.gameObject.AddComponent<RitualUnlockWeapon>();
					CurrentRitual.Play();
				}, delegate
				{
					OpenRitualMenu();
				});
			}));
			break;
		case UpgradeSystem.Type.Ritual_Sacrifice:
		{
			RitualSacrifice ritualSacrifice = base.gameObject.AddComponent<RitualSacrifice>();
			ritualSacrifice.RitualLight = RitualLighting;
			ritualSacrifice.Play();
			CurrentRitual = ritualSacrifice;
			break;
		}
		case UpgradeSystem.Type.Ritual_ConsumeFollower:
		{
			RitualConsumeFollower ritualConsumeFollower = base.gameObject.AddComponent<RitualConsumeFollower>();
			ritualConsumeFollower.Play();
			CurrentRitual = ritualConsumeFollower;
			break;
		}
		case UpgradeSystem.Type.Ritual_FasterBuilding:
			CurrentRitual = base.gameObject.AddComponent<RitualFasterBuilding>();
			CurrentRitual.Play();
			break;
		case UpgradeSystem.Type.Ritual_FirePit:
			CurrentRitual = base.gameObject.AddComponent<RitualFirePit>();
			CurrentRitual.Play();
			break;
		case UpgradeSystem.Type.Ritual_Feast:
			CurrentRitual = base.gameObject.AddComponent<RitualFeast>();
			CurrentRitual.Play();
			break;
		case UpgradeSystem.Type.Ritual_Enlightenment:
			Debug.Log("ENLIGHTENMENT!");
			CurrentRitual = base.gameObject.AddComponent<RitualElightenment>();
			CurrentRitual.Play();
			break;
		case UpgradeSystem.Type.Ritual_Holiday:
			CurrentRitual = base.gameObject.AddComponent<RitualWorkHoliday>();
			CurrentRitual.Play();
			break;
		case UpgradeSystem.Type.Ritual_WorkThroughNight:
			CurrentRitual = base.gameObject.AddComponent<RitualWorkThroughNight>();
			CurrentRitual.Play();
			break;
		case UpgradeSystem.Type.Ritual_AlmsToPoor:
			CurrentRitual = base.gameObject.AddComponent<RitualAlmsToPoor>();
			CurrentRitual.Play();
			break;
		case UpgradeSystem.Type.Ritual_DonationRitual:
			CurrentRitual = base.gameObject.AddComponent<RitualDonation>();
			CurrentRitual.Play();
			break;
		case UpgradeSystem.Type.Ritual_Fast:
			CurrentRitual = base.gameObject.AddComponent<RitualFast>();
			CurrentRitual.Play();
			{
				foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
				{
					allBrain.Stats.Starvation = 0f;
					allBrain.Stats.Satiation = 100f;
					int num = UnityEngine.Random.Range(0, 10);
					if (Utils.Between(num, 0f, 6f))
					{
						allBrain.AddThought(Thought.Fasting);
					}
					else if (Utils.Between(num, 6f, 8f))
					{
						allBrain.AddThought(Thought.AngryAboutFasting);
					}
					else if (Utils.Between(num, 8f, 10f))
					{
						allBrain.AddThought(Thought.HappyAboutFasting);
					}
				}
				break;
			}
		case UpgradeSystem.Type.Ritual_FishingRitual:
			CurrentRitual = base.gameObject.AddComponent<RitualFishing>();
			CurrentRitual.Play();
			break;
		case UpgradeSystem.Type.Ritual_HarvestRitual:
			CurrentRitual = base.gameObject.AddComponent<RitualHarvest>();
			CurrentRitual.Play();
			break;
		case UpgradeSystem.Type.Ritual_Funeral:
		{
			RitualFuneral ritualFuneral = base.gameObject.AddComponent<RitualFuneral>();
			ritualFuneral.Play();
			CurrentRitual = ritualFuneral;
			break;
		}
		case UpgradeSystem.Type.Ritual_Ressurect:
		{
			RitualRessurect ritualRessurect = base.gameObject.AddComponent<RitualRessurect>();
			ritualRessurect.Play();
			ritualRessurect.RitualLight = RitualLighting;
			CurrentRitual = ritualRessurect;
			break;
		}
		case UpgradeSystem.Type.Ritual_Fightpit:
		{
			RitualFightpit ritualFightpit = base.gameObject.AddComponent<RitualFightpit>();
			ritualFightpit.Play();
			CurrentRitual = ritualFightpit;
			break;
		}
		case UpgradeSystem.Type.Ritual_Wedding:
		{
			RitualWedding ritualWedding = base.gameObject.AddComponent<RitualWedding>();
			ritualWedding.Play();
			CurrentRitual = ritualWedding;
			break;
		}
		case UpgradeSystem.Type.Ritual_Halloween:
		{
			RitualHalloween ritualHalloween = base.gameObject.AddComponent<RitualHalloween>();
			ritualHalloween.Play();
			CurrentRitual = ritualHalloween;
			break;
		}
		case UpgradeSystem.Type.Ritual_Reindoctrinate:
		{
			RitualReindoctrinate ritualReindoctrinate = base.gameObject.AddComponent<RitualReindoctrinate>();
			ritualReindoctrinate.Play();
			CurrentRitual = ritualReindoctrinate;
			break;
		}
		case UpgradeSystem.Type.Ritual_AssignTaxCollector:
		{
			RitualTaxEnforcer ritualTaxEnforcer = base.gameObject.AddComponent<RitualTaxEnforcer>();
			ritualTaxEnforcer.Play();
			CurrentRitual = ritualTaxEnforcer;
			break;
		}
		case UpgradeSystem.Type.Ritual_AssignFaithEnforcer:
		{
			RitualFaithEnforcer ritualFaithEnforcer = base.gameObject.AddComponent<RitualFaithEnforcer>();
			ritualFaithEnforcer.Play();
			CurrentRitual = ritualFaithEnforcer;
			break;
		}
		case UpgradeSystem.Type.Ritual_Brainwashing:
		{
			RitualBrainwashing ritualBrainwashing = base.gameObject.AddComponent<RitualBrainwashing>();
			ritualBrainwashing.Play();
			CurrentRitual = ritualBrainwashing;
			break;
		}
		case UpgradeSystem.Type.Ritual_Ascend:
		{
			RitualAscend ritualAscend = base.gameObject.AddComponent<RitualAscend>();
			ritualAscend.Play();
			CurrentRitual = ritualAscend;
			break;
		}
		}
	}

	private IEnumerator DelayCallback(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}

	private void UnlockHeartsCallback(bool cancelled)
	{
		Ritual.OnEnd = (Action<bool>)Delegate.Remove(Ritual.OnEnd, new Action<bool>(UnlockHeartsCallback));
		StartCoroutine(UnlockHeartsCallbackRoutine());
	}

	private IEnumerator UnlockHeartsCallbackRoutine()
	{
		yield return new WaitForSeconds(5f);
		GameManager.GetInstance().StartCoroutine(UpgradeSystem.ListOfUnlocksRoutine());
	}

	private void RitualOnEnd(bool cancelled)
	{
		if (!cancelled && RitualType != UpgradeSystem.Type.Ritual_HeartsOfTheFaithful1)
		{
			foreach (StructuresData.ItemCost item in UpgradeSystem.GetCost(RitualType))
			{
				Inventory.ChangeItemQuantity((int)item.CostItem, -item.CostValue);
			}
		}
		ResetSprite();
		Ritual.OnEnd = (Action<bool>)Delegate.Remove(Ritual.OnEnd, new Action<bool>(RitualOnEnd));
		UnityEngine.Object.Destroy(CurrentRitual);
		Activating = false;
		PlayerFarming.Instance.Spine.UseDeltaTime = true;
		if (cancelled)
		{
			return;
		}
		AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("FIRST_RITUAL"));
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.PerformAnyRitual);
		if (!DataManager.Instance.ShowLoyaltyBars)
		{
			GameManager.GetInstance().StartCoroutine(WaitForConversationToEnd(delegate
			{
				Onboarding.Instance.RatLoyalty.SetActive(true);
				Onboarding.Instance.RatLoyalty.GetComponent<Interaction_SimpleConversation>().Play();
			}));
		}
		switch (RitualType)
		{
		case UpgradeSystem.Type.Ritual_AssignTaxCollector:
			UpgradeSystem.AddCooldown(RitualType, 6000f);
			break;
		case UpgradeSystem.Type.Ritual_AssignFaithEnforcer:
			UpgradeSystem.AddCooldown(RitualType, 6000f);
			break;
		case UpgradeSystem.Type.Ritual_UnlockCurse:
			UpgradeSystem.AddCooldown(RitualType, 1200f);
			break;
		case UpgradeSystem.Type.Ritual_UnlockWeapon:
			UpgradeSystem.AddCooldown(RitualType, 1200f);
			break;
		case UpgradeSystem.Type.Ritual_Sacrifice:
			StartCoroutine(UnlockSacrifices());
			UpgradeSystem.AddCooldown(RitualType, 8400f);
			break;
		case UpgradeSystem.Type.Ritual_ConsumeFollower:
			UpgradeSystem.AddCooldown(RitualType, 1200f);
			break;
		case UpgradeSystem.Type.Ritual_FasterBuilding:
			UpgradeSystem.AddCooldown(RitualType, 6000f);
			break;
		case UpgradeSystem.Type.Ritual_Enlightenment:
			UpgradeSystem.AddCooldown(RitualType, 6000f);
			break;
		case UpgradeSystem.Type.Ritual_Holiday:
			UpgradeSystem.AddCooldown(RitualType, 6000f);
			UpgradeSystem.AddCooldown(UpgradeSystem.Type.Ritual_WorkThroughNight, 1200f);
			break;
		case UpgradeSystem.Type.Ritual_WorkThroughNight:
			UpgradeSystem.AddCooldown(RitualType, 6000f);
			UpgradeSystem.AddCooldown(UpgradeSystem.Type.Ritual_Holiday, 3600f);
			break;
		case UpgradeSystem.Type.Ritual_AlmsToPoor:
			UpgradeSystem.AddCooldown(RitualType, 6000f);
			break;
		case UpgradeSystem.Type.Ritual_FirePit:
			UpgradeSystem.AddCooldown(RitualType, 8400f);
			break;
		case UpgradeSystem.Type.Ritual_Ascend:
			UpgradeSystem.AddCooldown(RitualType, 6000f);
			break;
		case UpgradeSystem.Type.Ritual_Feast:
			UpgradeSystem.AddCooldown(RitualType, 6000f);
			break;
		case UpgradeSystem.Type.Ritual_Fast:
			UpgradeSystem.AddCooldown(RitualType, 12000f);
			UpgradeSystem.AddCooldown(UpgradeSystem.Type.Ritual_Feast, 6000f);
			break;
		case UpgradeSystem.Type.Ritual_DonationRitual:
			UpgradeSystem.AddCooldown(RitualType, 6000f);
			break;
		case UpgradeSystem.Type.Ritual_FishingRitual:
			UpgradeSystem.AddCooldown(RitualType, 6000f);
			break;
		case UpgradeSystem.Type.Ritual_HarvestRitual:
			UpgradeSystem.AddCooldown(RitualType, 6000f);
			break;
		case UpgradeSystem.Type.Ritual_Funeral:
			UpgradeSystem.AddCooldown(RitualType, 1200f);
			break;
		case UpgradeSystem.Type.Ritual_Ressurect:
			UpgradeSystem.AddCooldown(RitualType, 3600f);
			break;
		case UpgradeSystem.Type.Ritual_Fightpit:
			UpgradeSystem.AddCooldown(RitualType, 6000f);
			break;
		case UpgradeSystem.Type.Ritual_Wedding:
			UpgradeSystem.AddCooldown(RitualType, 3600f);
			break;
		case UpgradeSystem.Type.Ritual_Brainwashing:
			UpgradeSystem.AddCooldown(RitualType, 8400f);
			break;
		}
	}

	private IEnumerator WaitForConversationToEnd(Action callback)
	{
		while (LetterBox.IsPlaying)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		if (callback != null)
		{
			callback();
		}
	}

	private void Close()
	{
		ResetSprite();
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		GameManager.GetInstance().OnConversationEnd();
		Activating = false;
	}

	public void GetAbilityRoutine(UpgradeSystem.Type Type)
	{
		StartCoroutine(GetAbilityRoutineIE(Type));
	}

	private IEnumerator UnlockSacrifices()
	{
		yield return new WaitForSeconds(1f);
		if (DataManager.Instance.sacrificesCompleted == 0)
		{
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("FIRST_SACRIFICE"));
		}
		DataManager.Instance.sacrificesCompleted++;
		yield return new WaitForSeconds(1f);
		if (DataManager.Instance.sacrificesCompleted >= 10)
		{
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("SACRIFICE_FOLLOWERS"));
		}
		yield return null;
	}

	private IEnumerator GetAbilityRoutineIE(UpgradeSystem.Type Type)
	{
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "gameover-fast", true);
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_start", PlayerFarming.Instance.gameObject);
		EventInstance receiveLoop = AudioManager.Instance.CreateLoop("event:/player/receive_animation_loop", PlayerFarming.Instance.gameObject, true);
		float Progress = 0f;
		float Duration = 3.6666667f;
		float StartingZoom = GameManager.GetInstance().CamFollowTarget.distance;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration - 0.5f))
			{
				break;
			}
			GameManager.GetInstance().CameraSetZoom(Mathf.Lerp(StartingZoom, 4f, Progress / Duration));
			if (Time.frameCount % 10 == 0)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.CrownBone.gameObject, base.transform.position, Color.red, null, 0.2f);
			}
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 4f);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "specials/special-activate", false);
		yield return new WaitForSeconds(1f);
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_end", PlayerFarming.Instance.gameObject);
		receiveLoop.stop(STOP_MODE.ALLOWFADEOUT);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "idle", true);
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
		UITutorialOverlayController TutorialOverlay = null;
		switch (Type)
		{
		case UpgradeSystem.Type.Ability_BlackHeart:
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.DarknessWithin))
			{
				TutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.DarknessWithin);
			}
			break;
		case UpgradeSystem.Type.Ability_Resurrection:
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Resurrection))
			{
				TutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Resurrection);
			}
			break;
		case UpgradeSystem.Type.Ability_TeleportHome:
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Omnipresence))
			{
				TutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Omnipresence);
			}
			break;
		case UpgradeSystem.Type.Ability_Eat:
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.TheHunger))
			{
				TutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.TheHunger);
			}
			break;
		}
		while (TutorialOverlay != null)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.MonsterHeart);
		ResetSprite();
		OnInteract(state);
	}

	public void GetFleeceRoutine(int oldFleece, int newFleece)
	{
		StartCoroutine(GetFleeceRoutineIE(oldFleece, newFleece));
	}

	private IEnumerator GetFleeceRoutineIE(int oldFleece, int newFleece)
	{
		fleece = newFleece;
		SimpleSpineAnimator simpleSpineAnimator = PlayerFarming.Instance.simpleSpineAnimator;
		if ((object)simpleSpineAnimator != null)
		{
			simpleSpineAnimator.SetSkin("Lamb_" + oldFleece);
		}
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "unlock-fleece", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_start", PlayerFarming.Instance.gameObject);
		EventInstance receiveLoop = AudioManager.Instance.CreateLoop("event:/player/receive_animation_loop", PlayerFarming.Instance.gameObject, true);
		PlayerFarming.Instance.Spine.AnimationState.Event += AnimationState_Event;
		yield return new WaitForSeconds(3f);
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_end", PlayerFarming.Instance.gameObject);
		receiveLoop.stop(STOP_MODE.ALLOWFADEOUT);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.UnlockFleece);
		ResetSprite();
		OnInteract(state);
	}

	private void AnimationState_Event(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "change-skin")
		{
			PlayerFarming.Instance.Spine.AnimationState.Event -= AnimationState_Event;
			SimpleSpineAnimator simpleSpineAnimator = PlayerFarming.Instance.simpleSpineAnimator;
			if ((object)simpleSpineAnimator != null)
			{
				simpleSpineAnimator.SetSkin("Lamb_" + fleece);
			}
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
			AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/hearts_appear", base.gameObject);
			AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", base.gameObject.transform.position);
		}
	}
}
