using System;
using System.Linq;
using src.UI;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.MainMenu
{
	public class DeleteMenu : UISubmenuBase
	{
		public Action OnBackButtonPressed;

		[Header("Delete Menu")]
		[SerializeField]
		private SaveSlotButtonBase[] _saveSlots;

		[SerializeField]
		private Button _backButton;

		[SerializeField]
		private ButtonHighlightController _buttonHighlight;

		private int _slotMarkedForDeletion;

		public void Start()
		{
			_backButton.onClick.AddListener(OnBackButtonClicked);
		}

		public void SetupSlot(int slot)
		{
			_saveSlots[slot].SetupSaveSlot(slot);
		}

		public void SetupSlot(int slot, MetaData metaData)
		{
			_saveSlots[slot].SetupSaveSlot(slot, metaData);
		}

		protected override void OnShowStarted()
		{
			SaveSlotButtonBase[] saveSlots = _saveSlots;
			foreach (SaveSlotButtonBase obj in saveSlots)
			{
				obj.OnSaveSlotPressed = (Action<int>)Delegate.Combine(obj.OnSaveSlotPressed, new Action<int>(OnTryDeleteSaveSlot));
			}
		}

		protected override void OnHideStarted()
		{
			SaveSlotButtonBase[] saveSlots = _saveSlots;
			foreach (SaveSlotButtonBase obj in saveSlots)
			{
				obj.OnSaveSlotPressed = (Action<int>)Delegate.Remove(obj.OnSaveSlotPressed, new Action<int>(OnTryDeleteSaveSlot));
			}
		}

		public void OnEnable()
		{
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			instance.OnDefaultSetComplete = (Action<Selectable>)Delegate.Combine(instance.OnDefaultSetComplete, new Action<Selectable>(UpdateButtonHighlight));
			UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
			instance2.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Combine(instance2.OnSelectionChanged, new Action<Selectable, Selectable>(OnNavigatorSelectionChanged));
		}

		public void OnDisable()
		{
			if (!(MonoSingleton<UINavigatorNew>.Instance == null))
			{
				UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
				instance.OnDefaultSetComplete = (Action<Selectable>)Delegate.Remove(instance.OnDefaultSetComplete, new Action<Selectable>(UpdateButtonHighlight));
				UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
				instance2.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Remove(instance2.OnSelectionChanged, new Action<Selectable, Selectable>(OnNavigatorSelectionChanged));
			}
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				OnBackButtonClicked();
			}
		}

		private void OnTryDeleteSaveSlot(int slotIndex)
		{
			_slotMarkedForDeletion = slotIndex;
			UIMenuConfirmationWindow uIMenuConfirmationWindow = Push(MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate);
			uIMenuConfirmationWindow.OnConfirm = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnConfirm, new Action(ConfirmDeletion));
			uIMenuConfirmationWindow.OnCancel = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnCancel, new Action(CancelDeletion));
		}

		private void ConfirmDeletion()
		{
			SaveAndLoad.DeleteSaveSlot(_slotMarkedForDeletion);
			_saveSlots[_slotMarkedForDeletion].UpdateSaveSlot();
			OnBackButtonClicked();
		}

		private void CancelDeletion()
		{
		}

		private void OnBackButtonClicked()
		{
			_buttonHighlight.SetAsBlack();
			Action onBackButtonPressed = OnBackButtonPressed;
			if (onBackButtonPressed != null)
			{
				onBackButtonPressed();
			}
		}

		private void OnNavigatorSelectionChanged(Selectable current, Selectable previous)
		{
			UpdateButtonHighlight(current);
		}

		private void UpdateButtonHighlight(Selectable target)
		{
			if (_canvasGroup.interactable)
			{
				if (_saveSlots.Any((SaveSlotButtonBase s) => s.Button == target))
				{
					_buttonHighlight.SetAsRed();
				}
				else
				{
					_buttonHighlight.SetAsBlack();
				}
			}
		}
	}
}
