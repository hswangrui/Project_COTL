using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MMBiomeGeneration;
using src.Extensions;
using Unify;
using UnityEngine;

namespace Lamb.UI
{
	public class UIDynamicNotificationCenter : BaseMonoBehaviour
	{
		private const float kShowDuration = 0.5f;

		private const float kHideDuration = 0.5f;

		public static UIDynamicNotificationCenter Instance;

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private RectTransform _dynamicNotificationContainer;

		[Header("Templates")]
		[SerializeField]
		private NotificationDynamicGeneric _genericDynamicNotificationTemplate;

		[SerializeField]
		private NotificationDynamicCursed _cursedDynamicNotificationTemplate;

		[SerializeField]
		private NotificationDynamicRitual _ritualDynamicNotificationTemplate;

		public static List<NotificationDynamicBase> NotificationsDynamic = new List<NotificationDynamicBase>();

		public static DynamicNotification_StarvingFollower StarvingFollowerData;

		public static DynamicNotification_HomelessFollower HomelessFollowerData;

		public static DynamicNotification_SickFolllower SickFollowerData;

		public static DynamicNotification_ExhaustedFolllower ExhaustedFollowerData;

		public static DynamicNotification_DissentingFolllower DissentingFollowerData;

		private static DynamicNotification_RitualActive HolidayRitualData;

		private static DynamicNotification_RitualActive WorkThroughNightRitualData;

		private static DynamicNotification_RitualActive FastRitualData;

		private static DynamicNotification_RitualActive FishingRitualData;

		private static DynamicNotification_RitualActive BrainwashingRitualData;

		private static DynamicNotification_RitualActive EnlightenmentRitualData;

		private static DynamicNotification_RitualActive HalloweenRitualData;

		private Vector2 _onScreenPosition;

		private Vector2 _offScreenPosition;

		private bool _initialized;

		public static List<DynamicNotificationData> DynamicNotifications { get; private set; } = new List<DynamicNotificationData>();


		private void Awake()
		{
			_onScreenPosition = (_offScreenPosition = _rectTransform.anchoredPosition);
			_offScreenPosition.x = 0f - _rectTransform.sizeDelta.x - 50f;
			if (BiomeGenerator.Instance != null)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public static void Reset()
		{
			NotificationsDynamic.Clear();
			DynamicNotifications.Clear();
			StarvingFollowerData = null;
			HomelessFollowerData = null;
			SickFollowerData = null;
			ExhaustedFollowerData = null;
			DissentingFollowerData = null;
			HolidayRitualData = null;
			WorkThroughNightRitualData = null;
			FastRitualData = null;
			FishingRitualData = null;
			BrainwashingRitualData = null;
			EnlightenmentRitualData = null;
			HalloweenRitualData = null;
		}

		private void Start()
		{
			FollowerBrainStats.OnExhaustionStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnExhaustionStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnExhaustedStateChanged));
			FollowerBrainStats.OnIllnessStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnIllnessStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnIllnessStateChanged));
			FollowerBrainStats.OnStarvationStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnStarvationStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnStarvationStateChanged));
			FollowerBrainStats.OnReeducationStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Combine(FollowerBrainStats.OnReeducationStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnReeducationStateChanged));
			FollowerBrain.OnBrainAdded = (Action<int>)Delegate.Combine(FollowerBrain.OnBrainAdded, new Action<int>(OnBrainAdded));
			FollowerBrain.OnBrainRemoved = (Action<int>)Delegate.Combine(FollowerBrain.OnBrainRemoved, new Action<int>(OnBrainRemoved));
			FollowerManager.OnFollowerAdded = (FollowerManager.FollowerChanged)Delegate.Combine(FollowerManager.OnFollowerAdded, new FollowerManager.FollowerChanged(OnFollowerAdded));
			FollowerManager.OnFollowerRemoved = (FollowerManager.FollowerChanged)Delegate.Combine(FollowerManager.OnFollowerRemoved, new FollowerManager.FollowerChanged(OnFollowerRemoved));
			FollowerManager.OnFollowerDie = (FollowerManager.FollowerGoneEvent)Delegate.Combine(FollowerManager.OnFollowerDie, new FollowerManager.FollowerGoneEvent(OnFollowerDie));
			FollowerManager.OnFollowerLeave = (FollowerManager.FollowerGoneEvent)Delegate.Combine(FollowerManager.OnFollowerLeave, new FollowerManager.FollowerGoneEvent(OnFollowerDie));
			LocationManager.OnFollowersSpawned = (Action)Delegate.Combine(LocationManager.OnFollowersSpawned, new Action(OnFollowersSpawned));
			TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
			StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
			StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructureAdded));
			StructureManager.OnStructureUpgraded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureUpgraded, new StructureManager.StructureChanged(OnStructureAdded));
			StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Combine(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
			Structures_Bed.OnBedCollapsedStatic = (Structures_Bed.BedEvent)Delegate.Combine(Structures_Bed.OnBedCollapsedStatic, new Structures_Bed.BedEvent(OnBedModified));
			Structures_Bed.OnBedRebuiltStatic = (Structures_Bed.BedEvent)Delegate.Combine(Structures_Bed.OnBedRebuiltStatic, new Structures_Bed.BedEvent(OnBedModified));
			if (SaveAndLoad.Loaded)
			{
				InitializeRitualData();
			}
			else
			{
				SaveAndLoad.OnLoadComplete = (Action)Delegate.Combine(SaveAndLoad.OnLoadComplete, new Action(InitializeRitualData));
			}
			RitualWorkHoliday.OnHolidayBegan = (Action)Delegate.Combine(RitualWorkHoliday.OnHolidayBegan, new Action(OnHolidayRitualBegan));
			RitualWorkThroughNight.OnWorkThroughNightBegan = (Action)Delegate.Combine(RitualWorkThroughNight.OnWorkThroughNightBegan, new Action(OnWorkThroughNightRitualBegan));
			RitualFast.OnRitualFastingBegan = (Action)Delegate.Combine(RitualFast.OnRitualFastingBegan, new Action(OnFastRitualBegan));
			RitualFishing.OnFishingRitualBegan = (Action)Delegate.Combine(RitualFishing.OnFishingRitualBegan, new Action(OnFishingRitualBegan));
			RitualBrainwashing.OnBrainwashingRitualBegan = (Action)Delegate.Combine(RitualBrainwashing.OnBrainwashingRitualBegan, new Action(OnBrainwashingRitualBegan));
			RitualElightenment.OnEnlightenmentRitualBegan = (Action)Delegate.Combine(RitualElightenment.OnEnlightenmentRitualBegan, new Action(OnEnlightenmentRitualBegan));
			RitualHalloween.OnHalloweenRitualBegan = (Action)Delegate.Combine(RitualHalloween.OnHalloweenRitualBegan, new Action(OnHalloweenRitualBegan));
		}

		private void OnDestroy()
		{
			FollowerBrainStats.OnExhaustionStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnExhaustionStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnExhaustedStateChanged));
			FollowerBrainStats.OnIllnessStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnIllnessStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnIllnessStateChanged));
			FollowerBrainStats.OnStarvationStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnStarvationStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnStarvationStateChanged));
			FollowerBrainStats.OnReeducationStateChanged = (FollowerBrainStats.StatStateChangedEvent)Delegate.Remove(FollowerBrainStats.OnReeducationStateChanged, new FollowerBrainStats.StatStateChangedEvent(OnReeducationStateChanged));
			FollowerBrain.OnBrainAdded = (Action<int>)Delegate.Remove(FollowerBrain.OnBrainAdded, new Action<int>(OnBrainAdded));
			FollowerBrain.OnBrainRemoved = (Action<int>)Delegate.Remove(FollowerBrain.OnBrainRemoved, new Action<int>(OnBrainRemoved));
			FollowerManager.OnFollowerDie = (FollowerManager.FollowerGoneEvent)Delegate.Remove(FollowerManager.OnFollowerDie, new FollowerManager.FollowerGoneEvent(OnFollowerDie));
			FollowerManager.OnFollowerAdded = (FollowerManager.FollowerChanged)Delegate.Remove(FollowerManager.OnFollowerAdded, new FollowerManager.FollowerChanged(OnFollowerAdded));
			FollowerManager.OnFollowerRemoved = (FollowerManager.FollowerChanged)Delegate.Remove(FollowerManager.OnFollowerRemoved, new FollowerManager.FollowerChanged(OnFollowerRemoved));
			FollowerManager.OnFollowerLeave = (FollowerManager.FollowerGoneEvent)Delegate.Remove(FollowerManager.OnFollowerLeave, new FollowerManager.FollowerGoneEvent(OnFollowerDie));
			LocationManager.OnFollowersSpawned = (Action)Delegate.Remove(LocationManager.OnFollowersSpawned, new Action(OnFollowersSpawned));
			TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
			StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
			StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructureAdded));
			StructureManager.OnStructureUpgraded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureUpgraded, new StructureManager.StructureChanged(OnStructureAdded));
			StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Remove(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
			Structures_Bed.OnBedCollapsedStatic = (Structures_Bed.BedEvent)Delegate.Remove(Structures_Bed.OnBedCollapsedStatic, new Structures_Bed.BedEvent(OnBedModified));
			Structures_Bed.OnBedRebuiltStatic = (Structures_Bed.BedEvent)Delegate.Remove(Structures_Bed.OnBedRebuiltStatic, new Structures_Bed.BedEvent(OnBedModified));
			RitualWorkHoliday.OnHolidayBegan = (Action)Delegate.Remove(RitualWorkHoliday.OnHolidayBegan, new Action(OnHolidayRitualBegan));
			RitualWorkThroughNight.OnWorkThroughNightBegan = (Action)Delegate.Remove(RitualWorkThroughNight.OnWorkThroughNightBegan, new Action(OnWorkThroughNightRitualBegan));
			RitualFast.OnRitualFastingBegan = (Action)Delegate.Remove(RitualFast.OnRitualFastingBegan, new Action(OnFastRitualBegan));
			RitualFishing.OnFishingRitualBegan = (Action)Delegate.Remove(RitualFishing.OnFishingRitualBegan, new Action(OnFishingRitualBegan));
			RitualBrainwashing.OnBrainwashingRitualBegan = (Action)Delegate.Remove(RitualBrainwashing.OnBrainwashingRitualBegan, new Action(OnBrainwashingRitualBegan));
			RitualElightenment.OnEnlightenmentRitualBegan = (Action)Delegate.Remove(RitualElightenment.OnEnlightenmentRitualBegan, new Action(OnEnlightenmentRitualBegan));
			RitualHalloween.OnHalloweenRitualBegan = (Action)Delegate.Remove(RitualHalloween.OnHalloweenRitualBegan, new Action(OnHalloweenRitualBegan));
			SaveAndLoad.OnLoadComplete = (Action)Delegate.Remove(SaveAndLoad.OnLoadComplete, new Action(InitializeRitualData));
		}

		private void OnEnable()
		{
			Instance = this;
			if (StarvingFollowerData == null)
			{
				StarvingFollowerData = AddDynamicData(new DynamicNotification_StarvingFollower());
			}
			OnStarvingFollowersNotification();
			StarvingFollowerData.UpdateFollowers();
			if (HomelessFollowerData == null)
			{
				HomelessFollowerData = AddDynamicData(new DynamicNotification_HomelessFollower());
			}
			if (SickFollowerData == null)
			{
				SickFollowerData = AddDynamicData(new DynamicNotification_SickFolllower());
			}
			OnSickFollowersNotification();
			SickFollowerData.UpdateFollowers();
			if (DissentingFollowerData == null)
			{
				DissentingFollowerData = AddDynamicData(new DynamicNotification_DissentingFolllower());
			}
			OnDissentingFollowersNotification();
			DissentingFollowerData.UpdateFollowers();
			if (ExhaustedFollowerData == null)
			{
				ExhaustedFollowerData = AddDynamicData(new DynamicNotification_ExhaustedFolllower());
			}
			OnExhaustedFollowersNotification();
			ExhaustedFollowerData.UpdateFollowers();
			if (!_initialized)
			{
				foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
				{
					CheckNewBrain(allBrain);
				}
				_initialized = true;
			}
			HomelessFollowerData.CheckFollowerCount();
			HomelessFollowerData.OnStructuresPlaced();
			OnHomelessFollowersNotification();
		}

		private void OnDisable()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}

		public void Show(bool instant = false)
		{
			StopAllCoroutines();
			if (instant)
			{
				_rectTransform.anchoredPosition = _onScreenPosition;
			}
			else
			{
				StartCoroutine(DoShow());
			}
		}

		private IEnumerator DoShow()
		{
			_rectTransform.DOKill();
			_rectTransform.DOAnchorPos(_onScreenPosition, 0.5f).SetEase(Ease.InOutSine).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.5f);
		}

		public void Hide(bool instant = false)
		{
			StopAllCoroutines();
			if (instant)
			{
				_rectTransform.anchoredPosition = _offScreenPosition;
			}
			else
			{
				StartCoroutine(DoHide());
			}
		}

		private IEnumerator DoHide()
		{
			_rectTransform.DOKill();
			_rectTransform.DOAnchorPos(_offScreenPosition, 0.5f).SetEase(Ease.InOutSine).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.5f);
		}

		private void InitializeRitualData()
		{
			if (HolidayRitualData == null)
			{
				HolidayRitualData = AddDynamicData(new DynamicNotification_RitualActive(UpgradeSystem.Type.Ritual_Holiday));
			}
			OnHolidayRitualBegan();
			if (WorkThroughNightRitualData == null)
			{
				WorkThroughNightRitualData = AddDynamicData(new DynamicNotification_RitualActive(UpgradeSystem.Type.Ritual_WorkThroughNight));
			}
			OnWorkThroughNightRitualBegan();
			if (FastRitualData == null)
			{
				FastRitualData = AddDynamicData(new DynamicNotification_RitualActive(UpgradeSystem.Type.Ritual_Fast));
			}
			OnFastRitualBegan();
			if (FishingRitualData == null)
			{
				FishingRitualData = AddDynamicData(new DynamicNotification_RitualActive(UpgradeSystem.Type.Ritual_FishingRitual));
			}
			OnFishingRitualBegan();
			if (BrainwashingRitualData == null)
			{
				BrainwashingRitualData = AddDynamicData(new DynamicNotification_RitualActive(UpgradeSystem.Type.Ritual_Brainwashing));
			}
			OnBrainwashingRitualBegan();
			if (EnlightenmentRitualData == null)
			{
				EnlightenmentRitualData = AddDynamicData(new DynamicNotification_RitualActive(UpgradeSystem.Type.Ritual_Enlightenment));
			}
			OnEnlightenmentRitualBegan();
			if (HalloweenRitualData == null)
			{
				HalloweenRitualData = AddDynamicData(new DynamicNotification_RitualActive(UpgradeSystem.Type.Ritual_Halloween));
			}
			OnHalloweenRitualBegan();
		}

		private T AddDynamicData<T>(T data) where T : DynamicNotificationData
		{
			if (!DynamicNotifications.Contains(data))
			{
				DynamicNotifications.Add(data);
			}
			return data;
		}

		private void OnHolidayRitualBegan()
		{
			PlayDynamic(HolidayRitualData, _ritualDynamicNotificationTemplate);
		}

		private void OnWorkThroughNightRitualBegan()
		{
			PlayDynamic(WorkThroughNightRitualData, _ritualDynamicNotificationTemplate);
		}

		private void OnFastRitualBegan()
		{
			PlayDynamic(FastRitualData, _ritualDynamicNotificationTemplate);
		}

		private void OnFishingRitualBegan()
		{
			PlayDynamic(FishingRitualData, _ritualDynamicNotificationTemplate);
		}

		private void OnBrainwashingRitualBegan()
		{
			PlayDynamic(BrainwashingRitualData, _ritualDynamicNotificationTemplate);
		}

		private void OnEnlightenmentRitualBegan()
		{
			PlayDynamic(EnlightenmentRitualData, _ritualDynamicNotificationTemplate);
		}

		private void OnHalloweenRitualBegan()
		{
			PlayDynamic(HalloweenRitualData, _ritualDynamicNotificationTemplate);
		}

		private void OnStarvingFollowersNotification()
		{
			PlayDynamic(StarvingFollowerData, _cursedDynamicNotificationTemplate);
		}

		private void OnSickFollowersNotification()
		{
			PlayDynamic(SickFollowerData, _cursedDynamicNotificationTemplate);
		}

		private void OnExhaustedFollowersNotification()
		{
			PlayDynamic(ExhaustedFollowerData, _cursedDynamicNotificationTemplate);
		}

		private void OnHomelessFollowersNotification()
		{
			PlayDynamic(HomelessFollowerData, _genericDynamicNotificationTemplate);
		}

		private void OnDissentingFollowersNotification()
		{
			PlayDynamic(DissentingFollowerData, _cursedDynamicNotificationTemplate);
		}

		private void OnFollowersSpawned()
		{
			HomelessFollowerData.CheckFollowerCount();
			if (HomelessFollowerData.TotalCount > 0f)
			{
				PlayDynamic(HomelessFollowerData, _genericDynamicNotificationTemplate);
			}
		}

		private void OnNewPhaseStarted()
		{
			if (DataManager.Instance.OnboardedHomelessAtNight && TimeManager.IsNight)
			{
				DataManager.Instance.OnboardedHomeless = true;
				DataManager.Instance.OnboardedHomelessAtNight = false;
			}
			HomelessFollowerData.CheckFollowerCount();
			if (HomelessFollowerData.TotalCount > 0f)
			{
				PlayDynamic(HomelessFollowerData, _genericDynamicNotificationTemplate);
			}
		}

		private void OnFollowerRemoved(int ID)
		{
			HomelessFollowerData.CheckFollowerCount();
			if (HomelessFollowerData.TotalCount > 0f)
			{
				PlayDynamic(HomelessFollowerData, _genericDynamicNotificationTemplate);
			}
		}

		private void OnFollowerAdded(int ID)
		{
			HomelessFollowerData.CheckFollowerCount();
			if (HomelessFollowerData.TotalCount > 0f)
			{
				PlayDynamic(HomelessFollowerData, _genericDynamicNotificationTemplate);
			}
		}

		private void OnStructureAdded(StructuresData structure)
		{
			HomelessFollowerData.OnStructureAdded(structure);
			if (HomelessFollowerData.TotalCount > 0f)
			{
				PlayDynamic(HomelessFollowerData, _genericDynamicNotificationTemplate);
			}
		}

		private void OnStructuresPlaced()
		{
			HomelessFollowerData.OnStructuresPlaced();
			if (HomelessFollowerData.TotalCount > 0f)
			{
				PlayDynamic(HomelessFollowerData, _genericDynamicNotificationTemplate);
			}
		}

		private void OnBedModified()
		{
			HomelessFollowerData.OnStructuresPlaced();
			if (HomelessFollowerData.TotalCount > 0f)
			{
				PlayDynamic(HomelessFollowerData, _genericDynamicNotificationTemplate);
			}
		}

		private void OnStarvationStateChanged(int brainID, FollowerStatState newState, FollowerStatState oldState)
		{
			FollowerBrain followerBrain = FollowerBrain.FindBrainByID(brainID);
			if (oldState == FollowerStatState.Off && newState == FollowerStatState.On)
			{
				StarvingFollowerData.AddFollower(followerBrain);
				PlayDynamic(StarvingFollowerData, _cursedDynamicNotificationTemplate);
			}
			else if (oldState == FollowerStatState.On && newState == FollowerStatState.Off)
			{
				StarvingFollowerData.RemoveFollower(followerBrain.Info.ID);
			}
		}

		private void OnExhaustedStateChanged(int brainID, FollowerStatState newState, FollowerStatState oldState)
		{
			FollowerBrain followerBrain = FollowerBrain.FindBrainByID(brainID);
			if (oldState == FollowerStatState.Off && newState == FollowerStatState.On)
			{
				ExhaustedFollowerData.AddFollower(followerBrain);
				PlayDynamic(ExhaustedFollowerData, _genericDynamicNotificationTemplate);
			}
			else if (oldState == FollowerStatState.On && newState == FollowerStatState.Off)
			{
				ExhaustedFollowerData.RemoveFollower(followerBrain.Info.ID);
			}
		}

		private void OnIllnessStateChanged(int brainID, FollowerStatState newState, FollowerStatState oldState)
		{
			FollowerBrain followerBrain = FollowerBrain.FindBrainByID(brainID);
			if (oldState == FollowerStatState.Off && newState == FollowerStatState.On)
			{
				SickFollowerData.AddFollower(followerBrain);
				PlayDynamic(SickFollowerData, _cursedDynamicNotificationTemplate);
			}
			else if (oldState == FollowerStatState.On && newState == FollowerStatState.Off)
			{
				SickFollowerData.RemoveFollower(followerBrain.Info.ID);
			}
		}

		private void OnReeducationStateChanged(int brainID, FollowerStatState newState, FollowerStatState oldState)
		{
			FollowerBrain followerBrain = FollowerBrain.FindBrainByID(brainID);
			if (oldState == FollowerStatState.Off && newState == FollowerStatState.On)
			{
				DissentingFollowerData.AddFollower(followerBrain);
				PlayDynamic(DissentingFollowerData, _cursedDynamicNotificationTemplate);
			}
			else if (oldState == FollowerStatState.On && newState == FollowerStatState.Off)
			{
				DissentingFollowerData.RemoveFollower(followerBrain.Info.ID);
			}
		}

		private void OnFollowerDie(int brainID, NotificationCentre.NotificationType deathNotificationType)
		{
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("FIRST_DEATH"));
			FollowerBrain.FindBrainByID(brainID);
			SickFollowerData.RemoveFollower(brainID);
			HomelessFollowerData.CheckFollowerCount();
			StarvingFollowerData.RemoveFollower(brainID);
			ExhaustedFollowerData.RemoveFollower(brainID);
			DissentingFollowerData.RemoveFollower(brainID);
			Debug.Log("Notifications - OnFollowerDie");
		}

		private void OnBrainAdded(int brainID)
		{
			FollowerBrain brain = FollowerBrain.FindBrainByID(brainID);
			CheckNewBrain(brain);
		}

		private void OnBrainRemoved(int brainID)
		{
			StarvingFollowerData.RemoveFollower(brainID);
			HomelessFollowerData.CheckFollowerCount();
			if (HomelessFollowerData.TotalCount > 0f)
			{
				PlayDynamic(HomelessFollowerData, _genericDynamicNotificationTemplate);
			}
			DissentingFollowerData.RemoveFollower(brainID);
			SickFollowerData.RemoveFollower(brainID);
		}

		private void PlayDynamic(DynamicNotificationData data, NotificationDynamicBase prefab)
		{
			if (!NotificationCentre.NotificationsEnabled || data.IsEmpty)
			{
				return;
			}
			foreach (NotificationDynamicBase item in NotificationsDynamic)
			{
				if (!item.Closing && item.Data.Type == data.Type)
				{
					return;
				}
			}
			NotificationDynamicBase notificationDynamicBase = GameObjectExtensions.Instantiate(prefab, _dynamicNotificationContainer);
			notificationDynamicBase.Configure(data);
			NotificationsDynamic.Add(notificationDynamicBase);
		}

		private void CheckNewBrain(FollowerBrain brain)
		{
			if (brain.Info.CursedState == Thought.BecomeStarving)
			{
				StarvingFollowerData.AddFollower(brain);
				PlayDynamic(StarvingFollowerData, _cursedDynamicNotificationTemplate);
			}
			if (brain.Info.CursedState == Thought.Ill)
			{
				SickFollowerData.AddFollower(brain);
				PlayDynamic(SickFollowerData, _cursedDynamicNotificationTemplate);
			}
			if (brain.Stats.Exhaustion > 0f)
			{
				ExhaustedFollowerData.AddFollower(brain);
				PlayDynamic(ExhaustedFollowerData, _genericDynamicNotificationTemplate);
			}
			HomelessFollowerData.CheckFollowerCount();
			if (HomelessFollowerData.TotalCount > 0f)
			{
				PlayDynamic(HomelessFollowerData, _genericDynamicNotificationTemplate);
			}
			if (brain.Info.CursedState == Thought.Dissenter)
			{
				DissentingFollowerData.AddFollower(brain);
				PlayDynamic(DissentingFollowerData, _cursedDynamicNotificationTemplate);
			}
		}
	}
}
