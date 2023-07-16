using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using src.Extensions;
using UnityEngine;

public class NotificationCentre : BaseMonoBehaviour
{
	public enum NotificationType
	{
		Died,
		Starving,
		DiedFromStarvation,
		LeaveCult,
		DiedFromIllness,
		RelationshipFriend,
		RelationshipLover,
		RelationshipEnemy,
		LevellingUp,
		RelationshipWasKilledBy,
		BecomeDissenter,
		StopBeingDissenter,
		Hungry,
		BecomeIll,
		NoLongerIll,
		NoLongerStarving,
		Brainwashed,
		NoLongerBrainwashed,
		ReadyToLevelUp,
		LeaveCultUnhappy,
		Exhausted,
		Tired,
		Dynamic_Starving,
		Dynamic_Homeless,
		ResearchComplete,
		BuildComplete,
		AteRottenFood,
		Homeless,
		BecomeUnwell,
		BloodMoon3,
		BloodMoon2,
		BloodMoon1,
		NewUpgradePoint,
		SacrificeFollower,
		NewRecruit,
		TraitAdded,
		TraitRemoved,
		WeaponDamaged,
		WeaponDestroyed,
		CurseDamaged,
		CurseDestroyed,
		NoWorkers,
		MurderedByYou,
		FastComplete,
		FoodBecomeRotten,
		LowFaithDonation,
		BecomeOld,
		DiedFromOldAge,
		LevelUp,
		GainedHeart,
		GainedStrength,
		QuestComplete,
		Ascended,
		HolidayComplete,
		WorkThroughNightRitualComplete,
		ConstructionRitualComplete,
		EnlightenmentRitualComplete,
		FishingRitualComplete,
		KilledInAFightPit,
		QuestFailed,
		LostRespect,
		FirePitBegan,
		FeastTableBegan,
		LevelUpCentreScreen,
		DemonConverted,
		DemonPreserved,
		ConsumeFollower,
		ZombieKilledFollower,
		ZombieSpawned,
		FaithEnforcerAssigned,
		TaxEnforcerAssigned,
		SacrificedAwayFromCult,
		Dynamic_Sick,
		Disciple,
		UpgradeRitualReady,
		FaithUp,
		FaithUpDoubleArrow,
		FaithDown,
		FaithDownDoubleArrow,
		None,
		DiedFromDeadlyMeal,
		RitualHoliday,
		RitualWorkThroughNight,
		RitualFasterBuilding,
		RitualFast,
		RitualFishing,
		RitualBrainwashing,
		RitualEnlightenment,
		EnemiesStronger,
		Dynamic_Dissenter,
		RitualHalloween
	}

	private const float kShowDuration = 0.5f;

	private const float kHideDuration = 0.5f;

	public static bool NotificationsEnabled = true;

	public static NotificationCentre Instance;

	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	private RectTransform _notificationContainer;

	[Header("Templates - Standard")]
	[SerializeField]
	private NotificationGeneric _genericNotificationTemplate;

	[SerializeField]
	private NotificationTwitch _twitchNotificationTemplate;

	[SerializeField]
	private NotificationItem _itemNotificationTemplate;

	[SerializeField]
	private NotificationFaith _faithNotificationTemplate;

	[SerializeField]
	private NotificationFollower _followerNotificationTemplate;

	[SerializeField]
	private NotificationRelationship _relationshipNotificationTemplate;

	[SerializeField]
	private NotificationHelpHinder _helpHinderNotificationTemplate;

	public static List<NotificationBase> Notifications = new List<NotificationBase>();

	private List<string> notificationsThisFrame = new List<string>();

	private Vector2 _onScreenPosition;

	private Vector2 _offScreenPosition;

	private void Awake()
	{
		_onScreenPosition = (_offScreenPosition = new Vector2(0f, -30f));
		_offScreenPosition.x = 0f - _rectTransform.sizeDelta.x - 200f;
	//	TwitchManager.OnNotificationReceived += OnTwitchNotificationReceived;
	}

	private void OnDestroy()
	{
		//TwitchManager.OnNotificationReceived -= OnTwitchNotificationReceived;
	}

	private void OnEnable()
	{
		Instance = this;
		Inventory.OnItemAddedToInventory += ItemAddedToInventory;
		Inventory.OnItemRemovedFromInventory += ItemAddedToInventory;
		FollowerBrainStats.OnLevelUp = (FollowerBrainStats.StatChangedEvent)Delegate.Combine(FollowerBrainStats.OnLevelUp, new FollowerBrainStats.StatChangedEvent(OnFollowerLevelUp));
		FollowerBrainInfo.OnReadyToLevelUp = (FollowerBrainStats.StatChangedEvent)Delegate.Combine(FollowerBrainInfo.OnReadyToLevelUp, new FollowerBrainStats.StatChangedEvent(OnReadyToLevelUp));
		FollowerTask_EatMeal.OnEatRottenFood = (Action<int>)Delegate.Combine(FollowerTask_EatMeal.OnEatRottenFood, new Action<int>(OnEatRottenFood));
		FollowerTask_Chat.OnChangeRelationship = (Action<FollowerInfo, FollowerInfo, IDAndRelationship.RelationshipState>)Delegate.Combine(FollowerTask_Chat.OnChangeRelationship, new Action<FollowerInfo, FollowerInfo, IDAndRelationship.RelationshipState>(OnChangeRelationship));
		PlayerWeapon.WeaponBroken = (Action)Delegate.Combine(PlayerWeapon.WeaponBroken, new Action(WeaponBroken));
		PlayerWeapon.WeaponDamaged = (Action)Delegate.Combine(PlayerWeapon.WeaponDamaged, new Action(WeaponDamaged));
		PlayerSpells.CurseDamaged = (Action)Delegate.Combine(PlayerSpells.CurseDamaged, new Action(CurseBroken));
		PlayerSpells.CurseBroken = (Action)Delegate.Combine(PlayerSpells.CurseBroken, new Action(CurseDamaged));
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		Inventory.OnItemAddedToInventory -= ItemAddedToInventory;
		Inventory.OnItemRemovedFromInventory -= ItemAddedToInventory;
		FollowerBrainStats.OnLevelUp = (FollowerBrainStats.StatChangedEvent)Delegate.Remove(FollowerBrainStats.OnLevelUp, new FollowerBrainStats.StatChangedEvent(OnFollowerLevelUp));
		FollowerBrainInfo.OnReadyToLevelUp = (FollowerBrainStats.StatChangedEvent)Delegate.Remove(FollowerBrainInfo.OnReadyToLevelUp, new FollowerBrainStats.StatChangedEvent(OnReadyToLevelUp));
		FollowerTask_EatMeal.OnEatRottenFood = (Action<int>)Delegate.Remove(FollowerTask_EatMeal.OnEatRottenFood, new Action<int>(OnEatRottenFood));
		FollowerTask_Chat.OnChangeRelationship = (Action<FollowerInfo, FollowerInfo, IDAndRelationship.RelationshipState>)Delegate.Remove(FollowerTask_Chat.OnChangeRelationship, new Action<FollowerInfo, FollowerInfo, IDAndRelationship.RelationshipState>(OnChangeRelationship));
		PlayerWeapon.WeaponBroken = (Action)Delegate.Remove(PlayerWeapon.WeaponBroken, new Action(WeaponBroken));
		PlayerWeapon.WeaponDamaged = (Action)Delegate.Remove(PlayerWeapon.WeaponDamaged, new Action(WeaponDamaged));
		PlayerSpells.CurseDamaged = (Action)Delegate.Remove(PlayerSpells.CurseDamaged, new Action(CurseBroken));
		PlayerSpells.CurseBroken = (Action)Delegate.Remove(PlayerSpells.CurseBroken, new Action(CurseDamaged));
	}

	public void Show(bool instant = false)
	{
		_rectTransform.DOKill();
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
		_rectTransform.DOKill();
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

	private void CurseBroken()
	{
		NotificationCentreScreen.Play(NotificationType.CurseDestroyed);
	}

	private void CurseDamaged()
	{
		NotificationCentreScreen.Play(NotificationType.CurseDamaged);
	}

	private void WeaponBroken()
	{
		NotificationCentreScreen.Play(NotificationType.WeaponDestroyed);
	}

	private void WeaponDamaged()
	{
		NotificationCentreScreen.Play(NotificationType.WeaponDamaged);
	}

	private void OnReadyToLevelUp(int brainID, float newValue, float oldValue, float change)
	{
		FollowerBrain followerBrain = FollowerBrain.FindBrainByID(brainID);
		PlayFollowerNotification(NotificationType.ReadyToLevelUp, followerBrain.Info, NotificationFollower.Animation.Happy);
	}

	private void OnFollowerLevelUp(int brainID, float newValue, float oldValue, float change)
	{
		FollowerBrain followerBrain = FollowerBrain.FindBrainByID(brainID);
		PlayFollowerNotification(NotificationType.LevellingUp, followerBrain.Info, NotificationFollower.Animation.Happy);
	}

	private void ItemAddedToInventory(InventoryItem.ITEM_TYPE itemType, int delta)
	{
		if (!CheckToShowHUD(itemType))
		{
			return;
		}
		foreach (NotificationBase notification in Notifications)
		{
			NotificationItem notificationItem;
			if ((object)(notificationItem = notification as NotificationItem) != null && notificationItem.ItemType == itemType)
			{
				notificationItem.UpdateDelta(delta);
				return;
			}
		}
		PlayItemNotification(itemType, delta);
	}

	public bool CheckToShowHUD(InventoryItem.ITEM_TYPE type)
	{
		if (type == InventoryItem.ITEM_TYPE.SEEDS || type == InventoryItem.ITEM_TYPE.INGREDIENTS)
		{
			return false;
		}
		return true;
	}

	private void OnRestStateChanged(int brainID, FollowerStatState newState, FollowerStatState oldState)
	{
		FollowerBrain followerBrain = FollowerBrain.FindBrainByID(brainID);
		if (newState == FollowerStatState.Urgent)
		{
			PlayFollowerNotification(NotificationType.Exhausted, followerBrain.Info, NotificationFollower.Animation.Tired);
		}
	}

	private void OnChangeRelationship(FollowerInfo f1, FollowerInfo f2, IDAndRelationship.RelationshipState state)
	{
		PlayRelationshipNotification(NotificationType.RelationshipFriend, f1, NotificationFollower.Animation.Happy, f2, NotificationFollower.Animation.Happy);
	}

	private void OnEatRottenFood(int brainID)
	{
		FollowerBrain followerBrain = FollowerBrain.FindBrainByID(brainID);
		PlayFollowerNotification(NotificationType.AteRottenFood, followerBrain.Info, NotificationFollower.Animation.Sick);
	}

	public void PlayItemNotification(InventoryItem.ITEM_TYPE itemType, int delta)
	{
		if (NotificationsEnabled)
		{
			string item = itemType.ToString();
			if (!notificationsThisFrame.Contains(item))
			{
				NotificationItem notificationItem = _itemNotificationTemplate.Instantiate(_notificationContainer, false);
				notificationItem.Configure(itemType, delta);
				Notifications.Add(notificationItem);
				notificationsThisFrame.Add(item);
			}
		}
	}

	public void PlayGenericNotification(string locKey)
	{
		if (NotificationsEnabled && !notificationsThisFrame.Contains(locKey))
		{
			NotificationGeneric notificationGeneric = _genericNotificationTemplate.Instantiate(_notificationContainer, false);
			notificationGeneric.Configure(locKey);
			Notifications.Add(notificationGeneric);
			FinalizedNotification finalizedNotification = new FinalizedNotification
			{
				LocKey = locKey
			};
			DataManager.Instance.AddToNotificationHistory(finalizedNotification);
			notificationsThisFrame.Add(locKey);
		}
	}

	public void PlayGenericNotification(NotificationType type)
	{
		if (NotificationsEnabled)
		{
			string item = type.ToString();
			if (!notificationsThisFrame.Contains(item))
			{
				NotificationGeneric notificationGeneric = _genericNotificationTemplate.Instantiate(_notificationContainer, false);
				notificationGeneric.Configure(type, GetFlair(type));
				Notifications.Add(notificationGeneric);
				FinalizedNotification finalizedNotification = new FinalizedNotification
				{
					LocKey = GetLocKey(type)
				};
				DataManager.Instance.AddToNotificationHistory(finalizedNotification);
				notificationsThisFrame.Add(item);
			}
		}
	}

	public void PlayGenericNotificationNonLocalizedParams(string locKey, params string[] nonLocalizedParameters)
	{
		if (NotificationsEnabled && !notificationsThisFrame.Contains(locKey))
		{
			NotificationGeneric notificationGeneric = _genericNotificationTemplate.Instantiate(_notificationContainer, false);
			notificationGeneric.ConfigureNonLocalizedParams(locKey, nonLocalizedParameters);
			Notifications.Add(notificationGeneric);
			FinalizedNotification finalizedNotification = new FinalizedNotification
			{
				LocKey = locKey,
				NonLocalisedParameters = nonLocalizedParameters
			};
			DataManager.Instance.AddToNotificationHistory(finalizedNotification);
			notificationsThisFrame.Add(locKey);
		}
	}

	public void PlayGenericNotificationLocalizedParams(string locKey, params string[] localizedParameters)
	{
		if (NotificationsEnabled && !notificationsThisFrame.Contains(locKey))
		{
			NotificationGeneric notificationGeneric = _genericNotificationTemplate.Instantiate(_notificationContainer, false);
			notificationGeneric.ConfigureLocalizedParams(locKey, localizedParameters);
			Notifications.Add(notificationGeneric);
			FinalizedNotification finalizedNotification = new FinalizedNotification
			{
				LocKey = locKey,
				LocalisedParameters = localizedParameters
			};
			DataManager.Instance.AddToNotificationHistory(finalizedNotification);
			notificationsThisFrame.Add(locKey);
		}
	}

	public void PlayTwitchNotification(string locKey, params string[] localizedParameters)
	{
		if (NotificationsEnabled && !notificationsThisFrame.Contains(locKey))
		{
			NotificationTwitch notificationTwitch = _twitchNotificationTemplate.Instantiate(_notificationContainer, false);
			notificationTwitch.ConfigureLocalizedParams(locKey, localizedParameters);
			Notifications.Add(notificationTwitch);
			FinalizedNotification finalizedNotification = new FinalizedNotification
			{
				LocKey = locKey,
				LocalisedParameters = localizedParameters
			};
			DataManager.Instance.AddToNotificationHistory(finalizedNotification);
			notificationsThisFrame.Add(locKey);
		}
	}

	public void PlayHelpHinderNotification(string locKey)
	{
		if (NotificationsEnabled && !notificationsThisFrame.Contains(locKey))
		{
			NotificationHelpHinder notificationHelpHinder = _helpHinderNotificationTemplate.Instantiate(_notificationContainer, false);
			notificationHelpHinder.ConfigureLocalizedParams(locKey);
			Notifications.Add(notificationHelpHinder);
			FinalizedNotification finalizedNotification = new FinalizedNotification
			{
				LocKey = locKey
			};
			DataManager.Instance.AddToNotificationHistory(finalizedNotification);
			notificationsThisFrame.Add(locKey);
		}
	}

	public void PlayFaithNotification(string locKey, float faithDelta, NotificationBase.Flair flair, int followerID = -1, params string[] args)
	{
		if (NotificationsEnabled)
		{
			string item = string.Format("{0}{1}{2}", locKey, faithDelta, followerID);
			if (!notificationsThisFrame.Contains(item))
			{
				FollowerInfo infoByID = FollowerInfo.GetInfoByID(followerID, true);
				NotificationFaith notificationFaith = _faithNotificationTemplate.Instantiate(_notificationContainer, false);
				notificationFaith.Configure(locKey, faithDelta, infoByID, true, flair, args);
				Notifications.Add(notificationFaith);
				FinalizedFaithNotification finalizedNotification = new FinalizedFaithNotification
				{
					LocKey = locKey,
					FaithDelta = faithDelta,
					followerInfoSnapshot = ((infoByID != null) ? new FollowerInfoSnapshot(infoByID) : null),
					LocalisedParameters = args
				};
				DataManager.Instance.AddToNotificationHistory(finalizedNotification);
				notificationsThisFrame.Add(item);
			}
		}
	}

	public void PlayFollowerNotification(NotificationType type, FollowerBrainInfo info, NotificationFollower.Animation followerAnimation)
	{
		if (NotificationsEnabled)
		{
			string item = string.Format(type.ToString(), info.GetHashCode());
			if (!notificationsThisFrame.Contains(item))
			{
				FollowerInfo infoByID = FollowerInfo.GetInfoByID(info.ID);
				NotificationFollower notificationFollower = _followerNotificationTemplate.Instantiate(_notificationContainer, false);
				notificationFollower.Configure(type, FollowerInfo.GetInfoByID(info.ID), followerAnimation, GetFlair(type));
				Notifications.Add(notificationFollower);
				FinalizedFollowerNotification finalizedNotification = new FinalizedFollowerNotification
				{
					LocKey = GetLocKey(type),
					followerInfoSnapshot = new FollowerInfoSnapshot(infoByID),
					Animation = followerAnimation
				};
				DataManager.Instance.AddToNotificationHistory(finalizedNotification);
				notificationsThisFrame.Add(item);
			}
		}
	}

	public void PlayRelationshipNotification(NotificationType type, FollowerInfo followerInfo, NotificationFollower.Animation followerAnimation, FollowerInfo otherFollowerInfo, NotificationFollower.Animation otherAnimation)
	{
		if (NotificationsEnabled)
		{
			string item = followerInfo.GetHashCode().ToString() + otherFollowerInfo.GetHashCode();
			if (!notificationsThisFrame.Contains(item))
			{
				NotificationRelationship notificationRelationship = _relationshipNotificationTemplate.Instantiate(_notificationContainer, false);
				notificationRelationship.Configure(type, followerInfo, otherFollowerInfo, followerAnimation, otherAnimation, GetFlair(type));
				Notifications.Add(notificationRelationship);
				FinalizedRelationshipNotification finalizedNotification = new FinalizedRelationshipNotification
				{
					LocKey = string.Format("Notifications/Relationship/{0}", type),
					followerInfoSnapshotA = new FollowerInfoSnapshot(followerInfo),
					followerInfoSnapshotB = new FollowerInfoSnapshot(otherFollowerInfo),
					FollowerAnimationA = followerAnimation,
					FollowerAnimationB = otherAnimation
				};
				DataManager.Instance.AddToNotificationHistory(finalizedNotification);
				notificationsThisFrame.Add(item);
			}
		}
	}

	public static string GetLocKey(NotificationType notificationType)
	{
		switch (notificationType)
		{
		case NotificationType.Died:
			return "Notifications/Follower/Died";
		case NotificationType.KilledInAFightPit:
			return "Notifications/Follower/KilledInAFightPit";
		case NotificationType.Starving:
			return "Notifications/Follower/Starving";
		case NotificationType.DiedFromStarvation:
			return "Notifications/Follower/DiedFromStarvation";
		case NotificationType.DiedFromIllness:
			return "Notifications/Follower/DiedFromIllness";
		case NotificationType.BecomeDissenter:
			return "Notifications/Follower/BecomeDissenter";
		case NotificationType.BecomeIll:
			return "Notifications/Follower/BecomeIll";
		case NotificationType.NoLongerIll:
			return "Notifications/Follower/NoLongerIll";
		case NotificationType.BecomeOld:
			return "Notifications/Follower/BecomeOld";
		case NotificationType.DiedFromOldAge:
			return "Notifications/Follower/DiedFromOldAge";
		case NotificationType.BecomeUnwell:
			return "Notifications/Follower/BecomeUnwell";
		case NotificationType.NoLongerStarving:
			return "Notifications/Follower/NoLongerStarving";
		case NotificationType.ResearchComplete:
			return "Notifications/NewBuildingUnlocked";
		case NotificationType.BuildComplete:
			return "Notifications/BuildComplete";
		case NotificationType.AteRottenFood:
			return "Notifications/Follower/AteRottenFood";
		case NotificationType.NewUpgradePoint:
			return "Notifications/NewUpgradePoint";
		case NotificationType.NewRecruit:
			return "Notifications/NewRecruit";
		case NotificationType.HolidayComplete:
			return "Notifications/HolidayComplete";
		case NotificationType.FoodBecomeRotten:
			return "Notifications/FoodBecomeRotten";
		case NotificationType.LowFaithDonation:
			return "Notifications/LowFaithDonation";
		case NotificationType.UpgradeRitualReady:
			return "Notifications/UpgradeRitualReady";
		case NotificationType.QuestComplete:
			return "Notifications/QuestComplete";
		case NotificationType.QuestFailed:
			return "Notifications/QuestFailed";
		case NotificationType.LostRespect:
			return "Notifications/LostRespect";
		case NotificationType.DemonConverted:
			return "Notifications/Follower/DemonConverted";
		case NotificationType.DemonPreserved:
			return "Notifications/Follower/DemonPreserved";
		case NotificationType.FaithEnforcerAssigned:
			return "Notifications/Follower/FaithEnforcerAssigned";
		case NotificationType.TaxEnforcerAssigned:
			return "Notifications/Follower/TaxEnforcerAssigned";
		case NotificationType.SacrificedAwayFromCult:
			return "Notifications/Follower/SacrificedAwayFromCult";
		default:
			return "MISSING LOCALISATION: " + notificationType;
		}
	}

	public static NotificationBase.Flair GetFlair(NotificationType type)
	{
		switch (type)
		{
		case NotificationType.Died:
		case NotificationType.DiedFromStarvation:
		case NotificationType.DiedFromIllness:
		case NotificationType.DiedFromOldAge:
		case NotificationType.DiedFromDeadlyMeal:
			return NotificationBase.Flair.Negative;
		default:
			return NotificationBase.Flair.None;
		}
	}

	private void LateUpdate()
	{
		notificationsThisFrame.Clear();
	}

	private void OnTwitchNotificationReceived(string viewerDisplayName, string notificationType)
	{
		//if (notificationType == "TOTEM_CONTRIBUTION" && !TwitchTotem.Deactivated)
		//{
		//	PlayTwitchNotification(string.Format(LocalizationManager.GetTranslation("Notifications/Twitch/TotemContribution"), viewerDisplayName));
		//}
	}
}
