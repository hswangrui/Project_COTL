using System;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.MainMenu
{
	public abstract class SaveSlotButtonBase : MonoBehaviour
	{
		public Action<int> OnSaveSlotPressed;

		[SerializeField]
		protected Button _button;

		[SerializeField]
		protected TextMeshProUGUI _text;

		[SerializeField]
		protected GameObject _completionBadge;

		[SerializeField]
		protected GameObject _sandboxCompletionBadge;

		private int _saveIndex;

		private bool _occupied;

		protected MetaData? _metaData;

		protected COTLDataReadWriter<MetaData> _metaDataReader = new COTLDataReadWriter<MetaData>();

		public Button Button
		{
			get
			{
				return _button;
			}
		}

		public MetaData? MetaData
		{
			get
			{
				return _metaData;
			}
		}

		public bool Occupied
		{
			get
			{
				return _occupied;
			}
		}

		private void Awake()
		{
			LocalizationManager.OnLocalizeEvent += Localize;
		}

		public void SetupSaveSlot(int index)
		{
			_saveIndex = index;
			UpdateSaveSlot();
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(OnSaveSlotButtonClicked);
		}

		public void SetupSaveSlot(int index, MetaData metaData)
		{
			SaveAndLoad.OnSaveSlotDeleted = (Action<int>)Delegate.Combine(SaveAndLoad.OnSaveSlotDeleted, new Action<int>(OnSaveSlotDeleted));
			_metaData = metaData;
			SetupSaveSlot(index);
		}

		public void UpdateSaveSlot()
		{
			if (_metaData.HasValue)
			{
				SetupOccupiedSlot();
				LocalizeOccupied();
				_occupied = true;
			}
			else
			{
				SetupEmptySlot();
				LocalizeEmpty();
				_occupied = false;
			}
		}

		private void Localize()
		{
			if (_occupied)
			{
				LocalizeOccupied();
			}
			else
			{
				LocalizeEmpty();
			}
		}

		protected abstract void LocalizeOccupied();

		protected abstract void LocalizeEmpty();

		private void OnSaveSlotButtonClicked()
		{
			Action<int> onSaveSlotPressed = OnSaveSlotPressed;
			if (onSaveSlotPressed != null)
			{
				onSaveSlotPressed(_saveIndex);
			}
		}

		private void OnDestroy()
		{
			SaveAndLoad.OnSaveSlotDeleted = (Action<int>)Delegate.Remove(SaveAndLoad.OnSaveSlotDeleted, new Action<int>(OnSaveSlotDeleted));
			LocalizationManager.OnLocalizeEvent -= Localize;
		}

		private void OnSaveSlotDeleted(int slot)
		{
			if (slot == _saveIndex)
			{
				_metaData = null;
				_occupied = false;
				SetupEmptySlot();
				LocalizeEmpty();
			}
		}

		protected abstract void SetupOccupiedSlot();

		protected abstract void SetupEmptySlot();
	}
}
