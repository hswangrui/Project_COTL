using System;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class DropdownItem : MonoBehaviour
	{
		public Action<DropdownItem> OnItemSelected;

		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private GameObject _currentSelection;

		private int _index;

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public int Index
		{
			get
			{
				return _index;
			}
		}

		public void Configure(int index, string content, bool currentSelection)
		{
			_index = index;
			_text.text = content;
			_currentSelection.SetActive(currentSelection);
			_button.onClick.AddListener(OnButtonClicked);
		}

		private void OnButtonClicked()
		{
			Action<DropdownItem> onItemSelected = OnItemSelected;
			if (onItemSelected != null)
			{
				onItemSelected(this);
			}
		}
	}
}
