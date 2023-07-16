using System.Collections.Generic;
using I2.Loc;
using Lamb.UI.FollowerSelect;
using src.Extensions;
using src.UINavigator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class CultMenu : UISubmenuBase
	{
		[Header("Cult Stats")]
		[SerializeField]
		private TextMeshProUGUI _cultNameText;

		[SerializeField]
		private TextMeshProUGUI _followersCount;

		[SerializeField]
		private TextMeshProUGUI _structuresCount;

		[SerializeField]
		private TextMeshProUGUI _deadFollowersCount;

		[SerializeField]
		private GameObject _divider;

		[SerializeField]
		private GameObject _faithContainer;

		[SerializeField]
		private Image _faithBar;

		[SerializeField]
		private GameObject _hungerContainer;

		[SerializeField]
		private Image _hungerBar;

		[SerializeField]
		private GameObject _sicknessContainer;

		[SerializeField]
		private Image _sicknessBar;

		[SerializeField]
		private MMButton _followerButton;

		[Header("Content")]
		[SerializeField]
		private GameObject _cultHeader;

		[SerializeField]
		private GameObject _statsheader;

		[SerializeField]
		private GameObject _cultContent;

		[SerializeField]
		private GameObject _followerContent;

		[SerializeField]
		private GameObject _notificationsHeader;

		[SerializeField]
		private RectTransform _notificationContent;

		[SerializeField]
		private RectTransform _notificationHistoryContent;

		[SerializeField]
		private GameObject _traitsHeader;

		[SerializeField]
		private GridLayoutGroup _cultTraitLayoutGroup;

		[SerializeField]
		private RectTransform _cultTraitContent;

		[SerializeField]
		private MMScrollRect _scrollRect;

		[SerializeField]
		private GameObject _noFollowerMessage;

		[Header("Templates")]
		[SerializeField]
		private NotificationDynamicGeneric _notificationDynamicTemplate;

		[SerializeField]
		private NotificationDynamicRitual _ritualNotificationDynamicTemplate;

		[SerializeField]
		private IndoctrinationTraitItem _traitItemTemplate;

		[SerializeField]
		private HistoricalNotificationGeneric _historicalNotificationGeneric;

		[SerializeField]
		private HistoricalNotificationFollower _historicalNotificationFollower;

		[SerializeField]
		private HistoricalNotificationFaith _historicalNotificationFaith;

		[SerializeField]
		private HistoricalNotificationItem _historicalNotificationItem;

		[SerializeField]
		private HistoricalNotificationRelationship _historicalNotificationRelationship;

		private List<IndoctrinationTraitItem> _traitItems = new List<IndoctrinationTraitItem>();

		private List<NotificationDynamicBase> _notificationItems = new List<NotificationDynamicBase>();

		private List<UIHistoricalNotification> _notificationHistoryItems = new List<UIHistoricalNotification>();

		public override void Awake()
		{
			base.Awake();
			_followerButton.onClick.AddListener(OnCultButtonPressed);
		}

		protected override void OnShowStarted()
		{
			_scrollRect.normalizedPosition = Vector2.one;
			_scrollRect.enabled = false;
			if (DataManager.Instance.Followers.Count + DataManager.Instance.Followers_Dead.Count == 0)
			{
				_followerButton.Interactable = false;
				MonoSingleton<UINavigatorNew>.Instance.Clear();
				_cultNameText.gameObject.SetActive(false);
				_statsheader.SetActive(false);
				_cultHeader.SetActive(false);
				_cultContent.SetActive(false);
				_notificationsHeader.SetActive(false);
				_notificationContent.gameObject.SetActive(false);
				_notificationHistoryContent.gameObject.SetActive(false);
				_traitsHeader.gameObject.SetActive(false);
				_cultTraitContent.gameObject.SetActive(false);
				_followerContent.SetActive(false);
				return;
			}
			_noFollowerMessage.gameObject.SetActive(false);
			if (string.IsNullOrEmpty(DataManager.Instance.CultName))
			{
				_cultNameText.text = ScriptLocalization.NAMES_Place.Cult;
			}
			else
			{
				_cultNameText.text = DataManager.Instance.CultName;
			}
			_followersCount.text = DataManager.Instance.Followers.Count.ToString();
			_structuresCount.text = StructureManager.GetTotalHomesCount().ToString();
			_deadFollowersCount.text = DataManager.Instance.Followers_Dead.Count.ToString();
			_divider.SetActive(DataManager.Instance.ShowCultFaith || DataManager.Instance.ShowCultIllness || DataManager.Instance.ShowCultHunger);
			_faithContainer.SetActive(DataManager.Instance.ShowCultFaith);
			_faithBar.fillAmount = CultFaithManager.CultFaithNormalised;
			_faithBar.color = StaticColors.ColorForThreshold(_faithBar.fillAmount);
			_sicknessContainer.SetActive(DataManager.Instance.ShowCultIllness);
			_sicknessBar.fillAmount = IllnessBar.IllnessNormalized;
			_sicknessBar.color = StaticColors.ColorForThreshold(_sicknessBar.fillAmount);
			_hungerContainer.SetActive(DataManager.Instance.ShowCultHunger);
			_hungerBar.fillAmount = HungerBar.HungerNormalized;
			_hungerBar.color = StaticColors.ColorForThreshold(_hungerBar.fillAmount);
			if (_traitItems.Count == 0)
			{
				foreach (FollowerTrait.TraitType cultTrait in DataManager.Instance.CultTraits)
				{
					IndoctrinationTraitItem indoctrinationTraitItem = GameObjectExtensions.Instantiate(_traitItemTemplate, _cultTraitContent);
					indoctrinationTraitItem.Configure(cultTrait);
					_traitItems.Add(indoctrinationTraitItem);
				}
				_traitsHeader.SetActive(_traitItems.Count > 0);
				_cultTraitContent.gameObject.SetActive(_traitItems.Count > 0);
			}
			if (_notificationItems.Count == 0)
			{
				foreach (DynamicNotificationData dynamicNotification in UIDynamicNotificationCenter.DynamicNotifications)
				{
					if (!dynamicNotification.IsEmpty)
					{
						NotificationDynamicBase notificationDynamicBase = null;
						DynamicNotificationData dynamicNotificationData = dynamicNotification;
						DynamicNotification_RitualActive dynamicNotification_RitualActive;
						notificationDynamicBase = ((dynamicNotificationData == null || (dynamicNotification_RitualActive = dynamicNotificationData as DynamicNotification_RitualActive) == null) ? ((NotificationDynamicBase)GameObjectExtensions.Instantiate(_notificationDynamicTemplate, _notificationContent)) : ((NotificationDynamicBase)GameObjectExtensions.Instantiate(_ritualNotificationDynamicTemplate, _notificationContent)));
						notificationDynamicBase.Configure(dynamicNotification);
						notificationDynamicBase.StopAllCoroutines();
						notificationDynamicBase.Container.localScale = Vector3.one;
						_notificationItems.Add(notificationDynamicBase);
					}
				}
				_notificationContent.gameObject.SetActive(_notificationItems.Count > 0);
			}
			if (_notificationHistoryItems.Count == 0)
			{
				foreach (FinalizedNotification item in DataManager.Instance.NotificationHistory)
				{
					FinalizedNotification finalizedNotification = item;
					if (finalizedNotification != null)
					{
						FinalizedFaithNotification finalizedFaithNotification;
						if ((finalizedFaithNotification = finalizedNotification as FinalizedFaithNotification) != null)
						{
							FinalizedFaithNotification finalizedNotification2 = finalizedFaithNotification;
							HistoricalNotificationFaith historicalNotificationFaith = GameObjectExtensions.Instantiate(_historicalNotificationFaith, _notificationHistoryContent);
							historicalNotificationFaith.Configure(finalizedNotification2);
							_notificationHistoryItems.Add(historicalNotificationFaith);
							continue;
						}
						FinalizedItemNotification finalizedItemNotification;
						if ((finalizedItemNotification = finalizedNotification as FinalizedItemNotification) != null)
						{
							FinalizedItemNotification finalizedNotification3 = finalizedItemNotification;
							HistoricalNotificationItem historicalNotificationItem = GameObjectExtensions.Instantiate(_historicalNotificationItem, _notificationHistoryContent);
							historicalNotificationItem.Configure(finalizedNotification3);
							_notificationHistoryItems.Add(historicalNotificationItem);
							continue;
						}
						FinalizedFollowerNotification finalizedFollowerNotification;
						if ((finalizedFollowerNotification = finalizedNotification as FinalizedFollowerNotification) != null)
						{
							FinalizedFollowerNotification finalizedNotification4 = finalizedFollowerNotification;
							HistoricalNotificationFollower historicalNotificationFollower = GameObjectExtensions.Instantiate(_historicalNotificationFollower, _notificationHistoryContent);
							historicalNotificationFollower.Configure(finalizedNotification4);
							_notificationHistoryItems.Add(historicalNotificationFollower);
							continue;
						}
						FinalizedRelationshipNotification finalizedRelationshipNotification;
						if ((finalizedRelationshipNotification = finalizedNotification as FinalizedRelationshipNotification) != null)
						{
							FinalizedRelationshipNotification finalizedNotification5 = finalizedRelationshipNotification;
							HistoricalNotificationRelationship historicalNotificationRelationship = GameObjectExtensions.Instantiate(_historicalNotificationRelationship, _notificationHistoryContent);
							historicalNotificationRelationship.Configure(finalizedNotification5);
							_notificationHistoryItems.Add(historicalNotificationRelationship);
							continue;
						}
					}
					HistoricalNotificationGeneric historicalNotificationGeneric = GameObjectExtensions.Instantiate(_historicalNotificationGeneric, _notificationHistoryContent);
					historicalNotificationGeneric.Configure(item);
					_notificationHistoryItems.Add(historicalNotificationGeneric);
				}
			}
			Navigation navigation = _followerButton.navigation;
			navigation.mode = Navigation.Mode.Explicit;
			if (_traitItems.Count > 0)
			{
				navigation.selectOnDown = _traitItems[0].Selectable;
			}
			else if (_notificationItems.Count > 0)
			{
				navigation.selectOnDown = _notificationItems[0].Selectable;
			}
			else if (_notificationHistoryItems.Count > 0)
			{
				navigation.selectOnDown = _notificationHistoryItems[0].Selectable;
			}
			_followerButton.navigation = navigation;
			if (_traitItems.Count > 0)
			{
				int constraintCount = _cultTraitLayoutGroup.constraintCount;
				int num = Mathf.FloorToInt((float)_traitItems.Count / (float)constraintCount);
				for (int i = 0; i < _traitItems.Count; i++)
				{
					Navigation navigation2 = _traitItems[i].Selectable.navigation;
					navigation2.mode = Navigation.Mode.Explicit;
					int num2 = Mathf.FloorToInt((float)i / (float)constraintCount);
					int num3 = i - num2 * constraintCount;
					if (num2 == 0)
					{
						navigation2.selectOnUp = _followerButton;
						if (i + constraintCount < _traitItems.Count)
						{
							navigation2.selectOnDown = _traitItems[i + constraintCount].Selectable;
						}
						else if (_notificationItems.Count > 0)
						{
							navigation2.selectOnDown = _notificationItems[0].Selectable;
						}
						else if (_notificationHistoryItems.Count > 0)
						{
							navigation2.selectOnDown = _notificationHistoryItems[0].Selectable;
						}
					}
					else if (num2 == num)
					{
						if (_notificationItems.Count > 0)
						{
							navigation2.selectOnDown = _notificationItems[0].Selectable;
						}
						else if (_notificationHistoryItems.Count > 0)
						{
							navigation2.selectOnDown = _notificationHistoryItems[0].Selectable;
						}
						else if (num3 < _notificationItems.Count)
						{
							navigation2.selectOnDown = _notificationItems[num3].Selectable;
						}
						else
						{
							navigation2.selectOnDown = _notificationItems.LastElement().Selectable;
						}
						navigation2.selectOnUp = _traitItems[i - constraintCount].Selectable;
					}
					else
					{
						navigation2.selectOnUp = _traitItems[i - constraintCount].Selectable;
						if (i + constraintCount < _traitItems.Count)
						{
							navigation2.selectOnDown = _traitItems[i + constraintCount].Selectable;
						}
						else
						{
							navigation2.selectOnDown = _traitItems.LastElement().Selectable;
						}
					}
					if (num3 > 0)
					{
						navigation2.selectOnLeft = _traitItems[i - 1].Selectable;
					}
					if (num3 < constraintCount && i + 1 < _traitItems.Count)
					{
						navigation2.selectOnRight = _traitItems[i + 1].Selectable;
					}
					_traitItems[i].Selectable.navigation = navigation2;
				}
			}
			if (_notificationItems.Count > 0)
			{
				for (int j = 0; j < _notificationItems.Count; j++)
				{
					Navigation navigation3 = _notificationItems[j].Selectable.navigation;
					navigation3.mode = Navigation.Mode.Explicit;
					if (_traitItems.Count == 0)
					{
						navigation3.selectOnUp = _followerButton;
					}
					else if (j < _traitItems.Count)
					{
						navigation3.selectOnUp = _traitItems[j].Selectable;
					}
					else
					{
						navigation3.selectOnUp = _traitItems.LastElement().Selectable;
					}
					if (_notificationHistoryItems.Count > 0)
					{
						navigation3.selectOnDown = _notificationHistoryItems[0].Selectable;
					}
					if (j > 0)
					{
						navigation3.selectOnLeft = _notificationItems[j - 1].Selectable;
					}
					if (j < _notificationItems.Count - 1)
					{
						navigation3.selectOnRight = _notificationItems[j + 1].Selectable;
					}
					_notificationItems[j].Selectable.navigation = navigation3;
				}
			}
			if (_notificationHistoryItems.Count > 0)
			{
				Navigation navigation4 = _notificationHistoryItems[0].Selectable.navigation;
				navigation4.mode = Navigation.Mode.Explicit;
				if (_notificationItems.Count > 0)
				{
					navigation4.selectOnUp = _notificationItems[0].Selectable;
				}
				else if (_traitItems.Count > 0)
				{
					navigation4.selectOnUp = _traitItems[0].Selectable;
				}
				else
				{
					navigation4.selectOnUp = _followerButton;
				}
				if (_notificationHistoryItems.Count > 1)
				{
					navigation4.selectOnDown = _notificationHistoryItems[1].Selectable;
				}
				_notificationHistoryItems[0].Selectable.navigation = navigation4;
			}
			_notificationsHeader.SetActive(_notificationItems.Count + _notificationHistoryItems.Count > 0);
			_scrollRect.enabled = true;
		}

		private void OnCultButtonPressed()
		{
			UIFollowerSelectMenuController uIFollowerSelectMenuController = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
			uIFollowerSelectMenuController.AllowsVoting = false;
			uIFollowerSelectMenuController.Show(DataManager.Instance.Followers, null, false, UpgradeSystem.Type.Count, false, true, false);
			PushInstance(uIFollowerSelectMenuController);
		}
	}
}
