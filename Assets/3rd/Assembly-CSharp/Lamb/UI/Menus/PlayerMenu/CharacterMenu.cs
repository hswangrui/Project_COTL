using System.Collections.Generic;
using Lamb.UI.PauseDetails;
using MMBiomeGeneration;
using Spine.Unity;
using src.Extensions;
using src.UI.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.Menus.PlayerMenu
{
	public class CharacterMenu : UISubmenuBase
	{
		[Header("Character Menu")]
		[SerializeField]
		private MMScrollRect _scrollRect;

		[SerializeField]
		private SkeletonGraphic _skeletonGraphic;

		[SerializeField]
		private GameObject _heartsContainer;

		[Header("Items")]
		[SerializeField]
		private WeaponItem _weaponItem;

		[SerializeField]
		private CurseItem _curseItem;

		[SerializeField]
		private RelicPlayerMenuItem _relicItem;

		[SerializeField]
		private FleeceItem _fleeceItem;

		[SerializeField]
		private TalismanPiecesItem _talismanPiecesItem;

		[SerializeField]
		private DoctrineFragmentsItem _doctrineFragmentsItem;

		[SerializeField]
		private CrownAbilityItem _crownAbilityItem1;

		[SerializeField]
		private CrownAbilityItem _crownAbilityItem2;

		[SerializeField]
		private CrownAbilityItem _crownAbilityItem3;

		[SerializeField]
		private CrownAbilityItem _crownAbilityItem4;

		[Header("Tarot")]
		[SerializeField]
		private RectTransform _tarotCardContentContainer;

		[SerializeField]
		private TarotCardItem_Run _tarotCardItemRunTemplate;

		[SerializeField]
		private GameObject _noTarotText;

		[Header("Relics")]
		[SerializeField]
		private RectTransform _relicEffectsContentContainer;

		[SerializeField]
		private ActiveRelicItem _activeRelicItemTemplate;

		[SerializeField]
		private GameObject _noRelicsText;

		private List<TarotCardItem_Run> _tarotCardItems = new List<TarotCardItem_Run>();

		private List<ActiveRelicItem> _activeRelicItems = new List<ActiveRelicItem>();

		private void Start()
		{
			_skeletonGraphic.Skeleton.SetSkin("Lamb_" + DataManager.Instance.PlayerFleece);
			_weaponItem.Configure(DataManager.Instance.CurrentWeapon);
			_curseItem.Configure(DataManager.Instance.CurrentCurse);
			_relicItem.Configure(PlayerFarming.Instance.playerRelic.CurrentRelic);
			_fleeceItem.Configure(DataManager.Instance.PlayerFleece);
			_talismanPiecesItem.Configure(DataManager.Instance.CurrentKeyPieces);
			_doctrineFragmentsItem.Configure(DataManager.Instance.DoctrineCurrentCount);
			_crownAbilityItem1.Configure(UpgradeSystem.Type.Ability_TeleportHome);
			_crownAbilityItem2.Configure(UpgradeSystem.Type.Ability_BlackHeart);
			_crownAbilityItem3.Configure(UpgradeSystem.Type.Ability_Eat);
			_crownAbilityItem4.Configure(UpgradeSystem.Type.Ability_Resurrection);
		}

		protected override void OnShowStarted()
		{
			_scrollRect.enabled = false;
			_scrollRect.normalizedPosition = Vector2.one;
			_heartsContainer.SetActive(DataManager.Instance.PlayerHasBeenGivenHearts);
			if (_tarotCardItems.Count == 0 && DataManager.Instance.PlayerRunTrinkets.Count > 0)
			{
				foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
				{
					TarotCardItem_Run tarotCardItem_Run = GameObjectExtensions.Instantiate(_tarotCardItemRunTemplate, _tarotCardContentContainer);
					tarotCardItem_Run.Configure(playerRunTrinket);
					_tarotCardItems.Add(tarotCardItem_Run);
				}
				_noTarotText.SetActive(false);
				Navigation navigation = _talismanPiecesItem.Selectable.navigation;
				navigation.selectOnDown = _tarotCardItems[0].Selectable;
				_talismanPiecesItem.Selectable.navigation = navigation;
				Navigation navigation2 = _doctrineFragmentsItem.Selectable.navigation;
				navigation2.selectOnDown = _tarotCardItems[0].Selectable;
				_doctrineFragmentsItem.Selectable.navigation = navigation2;
			}
			if (BiomeGenerator.Instance != null)
			{
				List<RelicType> list = new List<RelicType>();
				foreach (Familiar familiar in Familiar.Familiars)
				{
					RelicData relicData = EquipmentManager.GetRelicData(familiar.GetRelicType());
					if (!list.Contains(relicData.RelicType))
					{
						AddActiveRelic(relicData);
						list.Add(relicData.RelicType);
					}
				}
				if (DataManager.Instance.PlayerScaleModifier < 1)
				{
					AddActiveRelic(EquipmentManager.GetRelicData(RelicType.Shrink));
				}
				else if (DataManager.Instance.PlayerScaleModifier > 1)
				{
					AddActiveRelic(EquipmentManager.GetRelicData(RelicType.Enlarge));
				}
				if (_tarotCardItems.Count == 0 && _activeRelicItems.Count > 0)
				{
					Navigation navigation3 = _talismanPiecesItem.Selectable.navigation;
					navigation3.selectOnDown = _activeRelicItems[0].Selectable;
					_talismanPiecesItem.Selectable.navigation = navigation3;
					Navigation navigation4 = _doctrineFragmentsItem.Selectable.navigation;
					navigation4.selectOnDown = _activeRelicItems[0].Selectable;
					_doctrineFragmentsItem.Selectable.navigation = navigation4;
				}
			}
			_noRelicsText.SetActive(_activeRelicItems.Count == 0);
			_scrollRect.enabled = true;
		}

		private ActiveRelicItem AddActiveRelic(RelicData relicData)
		{
			ActiveRelicItem activeRelicItem = _activeRelicItemTemplate.Instantiate();
			activeRelicItem.transform.SetParent(_relicEffectsContentContainer);
			activeRelicItem.transform.localScale = Vector3.one;
			activeRelicItem.Configure(relicData);
			_activeRelicItems.Add(activeRelicItem);
			return activeRelicItem;
		}
	}
}
