using System;
using I2.Loc;
using Lamb.UI.Alerts;
using TMPro;
using UnityEngine;

namespace Lamb.UI.AltarMenu
{
	public class UIAltarMenuController : UIMenuBase
	{
		private static int _defaultIndex;

		public Action OnSermonSelected;

		public Action OnRitualsSelected;

		public Action OnDoctrineSelected;

		public Action OnPlayerUpgradesSelected;

		[Header("Buttons")]
		[SerializeField]
		private MMButton _sermonButton;

		[SerializeField]
		private MMButton _ritualsButton;

		[SerializeField]
		private MMButton _playerUpgradesButton;

		[SerializeField]
		private MMButton _doctrineButton;

		[Header("Alerts")]
		[SerializeField]
		private GameObject _sermonAlert;

		[SerializeField]
		private GameObject _ugpradeAlert;

		[SerializeField]
		private RitualAlert _ritualAlert;

		[Header("Misc")]
		[SerializeField]
		private TextMeshProUGUI _description;

		private bool _didCancel;

		public void OnEnable()
		{
			_playerUpgradesButton.gameObject.SetActive(DataManager.Instance.FirstDoctrineStone);
			_doctrineButton.gameObject.SetActive(DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory.Afterlife) > 0 || DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory.Food) > 0 || DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory.Possession) > 0 || DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory.LawAndOrder) > 0 || DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory.WorkAndWorship) > 0);
			_ritualsButton.gameObject.SetActive(UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Ritual_FirePit) || UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Ritual_Brainwashing));
			_ugpradeAlert.SetActive(false);
			if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.MONSTER_HEART) >= 1)
			{
				_ugpradeAlert.SetActive(true);
			}
			if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.TALISMAN) >= 1)
			{
				_ugpradeAlert.SetActive(true);
			}
			if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.DOCTRINE_STONE) >= 1 && UpgradeSystem.GetCoolDownNormalised(UpgradeSystem.PrimaryRitual1) <= 0f && DoctrineUpgradeSystem.TrySermonsStillAvailable())
			{
				_ugpradeAlert.SetActive(true);
			}
			if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE) >= 1 && UpgradeSystem.GetCoolDownNormalised(UpgradeSystem.Type.Ritual_CrystalDoctrine) <= 0f && DoctrineUpgradeSystem.GetAllRemainingDoctrines().Count > 0)
			{
				_ugpradeAlert.SetActive(true);
			}
			if (!DataManager.Instance.PostGameFleecesOnboarded && DataManager.Instance.DeathCatBeaten)
			{
				_ugpradeAlert.SetActive(true);
			}
			_sermonAlert.SetActive(true);
			if (DataManager.Instance.PreviousSermonDayIndex >= TimeManager.CurrentDay)
			{
				_sermonAlert.SetActive(false);
			}
			if (DataManager.Instance.Followers.Count <= 0)
			{
				_sermonAlert.SetActive(false);
			}
			if (Ritual.FollowersAvailableToAttendSermon() <= 0)
			{
				_sermonAlert.SetActive(false);
			}
		}

		public void Start()
		{
			_sermonButton.onClick.AddListener(OnSermonButtonClicked);
			_ritualsButton.onClick.AddListener(OnRitualsButtonClicked);
			_playerUpgradesButton.onClick.AddListener(OnPlayerUpgradesClicked);
			_doctrineButton.onClick.AddListener(OnDoctrineButtonCLicked);
			MMButton sermonButton = _sermonButton;
			sermonButton.OnSelected = (Action)Delegate.Combine(sermonButton.OnSelected, new Action(OnSermonButtonSelected));
			MMButton ritualsButton = _ritualsButton;
			ritualsButton.OnSelected = (Action)Delegate.Combine(ritualsButton.OnSelected, new Action(OnRitualsButtonSelected));
			MMButton playerUpgradesButton = _playerUpgradesButton;
			playerUpgradesButton.OnSelected = (Action)Delegate.Combine(playerUpgradesButton.OnSelected, new Action(OnPlayerUpgradesButtonSelected));
			MMButton doctrineButton = _doctrineButton;
			doctrineButton.OnSelected = (Action)Delegate.Combine(doctrineButton.OnSelected, new Action(OnDoctrineButtonSelected));
		}

		private void OnSermonButtonSelected()
		{
			_description.text = ScriptLocalization.UI_Altar.Sermon;
		}

		private void OnSermonButtonClicked()
		{
			Hide();
			Action onSermonSelected = OnSermonSelected;
			if (onSermonSelected != null)
			{
				onSermonSelected();
			}
			_defaultIndex = 0;
		}

		private void OnRitualsButtonSelected()
		{
			_description.text = ScriptLocalization.UI_Altar.Rituals;
		}

		private void OnRitualsButtonClicked()
		{
			Hide();
			Action onRitualsSelected = OnRitualsSelected;
			if (onRitualsSelected != null)
			{
				onRitualsSelected();
			}
			_defaultIndex = 2;
		}

		private void OnPlayerUpgradesButtonSelected()
		{
			_description.text = ScriptLocalization.UI_Altar.Crown;
		}

		private void OnPlayerUpgradesClicked()
		{
			Hide();
			Action onPlayerUpgradesSelected = OnPlayerUpgradesSelected;
			if (onPlayerUpgradesSelected != null)
			{
				onPlayerUpgradesSelected();
			}
			_defaultIndex = 1;
		}

		private void OnDoctrineButtonSelected()
		{
			_description.text = ScriptLocalization.UI_Altar.Doctrine;
		}

		private void OnDoctrineButtonCLicked()
		{
			Hide();
			Action onDoctrineSelected = OnDoctrineSelected;
			if (onDoctrineSelected != null)
			{
				onDoctrineSelected();
			}
			_defaultIndex = 3;
		}

		protected override void OnShowStarted()
		{
			base.OnShowStarted();
			UIManager.PlayAudio("event:/ui/open_menu");
			switch (_defaultIndex)
			{
			case 0:
				OverrideDefault(_sermonButton);
				break;
			case 1:
				OverrideDefault(_playerUpgradesButton);
				break;
			case 2:
				OverrideDefault(_ritualsButton);
				break;
			case 3:
				OverrideDefault(_doctrineButton);
				break;
			}
			ActivateNavigation();
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			UIManager.PlayAudio("event:/ui/close_menu");
		}

		protected override void OnHideCompleted()
		{
			if (_didCancel)
			{
				Action onCancel = OnCancel;
				if (onCancel != null)
				{
					onCancel();
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				_defaultIndex = 0;
				_didCancel = true;
				Hide();
			}
		}
	}
}
