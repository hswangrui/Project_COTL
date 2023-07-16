using System;
using System.Collections.Generic;
using src.Extensions;
using src.UINavigator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.UpgradeMenu
{
	public class UIUpgradeShopMenuController : UIMenuBase
	{
		public enum CheckUnlockedType
		{
			None,
			HideIfUnlocked,
			ShowIfUnlocked
		}

		[Serializable]
		public class AvailableUpgrades
		{
			public UpgradeSystem.Type Type;

			public CheckUnlockedType CheckUnlocked;

			public bool RequireUnlocked;

			public UpgradeSystem.Type RequireUnlockedType;
		}

		public Action<UpgradeSystem.Type> OnUpgradeChosen;

		[SerializeField]
		private List<AvailableUpgrades> _upgrades = new List<AvailableUpgrades>();

		[SerializeField]
		private UpgradeShopItem _upgradeItemTemplate;

		[SerializeField]
		private Transform _contentContainer;

		[SerializeField]
		private TextMeshProUGUI _noAvailableUpgradesText;

		private List<UpgradeShopItem> _items = new List<UpgradeShopItem>();

		private void OnEnable()
		{
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Combine(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnChangeSelection));
			UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
			instance2.OnDefaultSetComplete = (Action<Selectable>)Delegate.Combine(instance2.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
		}

		private void OnDisable()
		{
			if (MonoSingleton<UINavigatorNew>.Instance != null)
			{
				UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
				instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Remove(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnChangeSelection));
				UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
				instance2.OnDefaultSetComplete = (Action<Selectable>)Delegate.Remove(instance2.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
			}
		}

		protected override void OnShowStarted()
		{
			float num = -0.03f;
			foreach (AvailableUpgrades upgrade in _upgrades)
			{
				switch (upgrade.CheckUnlocked)
				{
				case CheckUnlockedType.HideIfUnlocked:
					if (UpgradeSystem.GetUnlocked(upgrade.Type))
					{
						continue;
					}
					break;
				case CheckUnlockedType.ShowIfUnlocked:
					if (!UpgradeSystem.GetUnlocked(upgrade.Type))
					{
						continue;
					}
					break;
				}
				if (!upgrade.RequireUnlocked || UpgradeSystem.GetUnlocked(upgrade.RequireUnlockedType))
				{
					DataManager.Instance.Alerts.Upgrades.AddOnce(upgrade.Type);
					UpgradeShopItem upgradeShopItem = GameObjectExtensions.Instantiate(_upgradeItemTemplate, _contentContainer);
					upgradeShopItem.Configure(upgrade.Type, num += 0.03f);
					upgradeShopItem.OnUpgradeSelected = (Action<UpgradeSystem.Type>)Delegate.Combine(upgradeShopItem.OnUpgradeSelected, new Action<UpgradeSystem.Type>(OnSelect));
					_items.Add(upgradeShopItem);
				}
			}
			if (_items.Count > 0)
			{
				OverrideDefaultOnce(_items[0].Button);
				ActivateNavigation();
			}
			_noAvailableUpgradesText.gameObject.SetActive(_items.Count == 0);
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideStarted()
		{
			UIManager.PlayAudio("event:/upgrade_statue/upgrade_statue_close");
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private void OnChangeSelection(Selectable currentSelectable, Selectable previousSelectable)
		{
			OnSelection(currentSelectable);
		}

		private void OnSelection(Selectable selectable)
		{
			if (selectable != null)
			{
				UIManager.PlayAudio("event:/upgrade_statue/upgrade_statue_scroll");
			}
		}

		private void OnSelect(UpgradeSystem.Type type)
		{
			if (!(UpgradeSystem.GetCoolDownNormalised(type) <= 0f))
			{
				return;
			}
			foreach (StructuresData.ItemCost item in UpgradeSystem.GetCost(type))
			{
				Inventory.ChangeItemQuantity((int)item.CostItem, -item.CostValue);
			}
			Action<UpgradeSystem.Type> onUpgradeChosen = OnUpgradeChosen;
			if (onUpgradeChosen != null)
			{
				onUpgradeChosen(type);
			}
			OnCancelButtonInput();
		}
	}
}
