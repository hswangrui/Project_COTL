using System;
using src.Extensions;
using UnityEngine;

namespace Lamb.UI
{
	public class UIDropdownOverlayController : UIMenuBase
	{
		public Action<int> OnItemChosen;

		[SerializeField]
		private RectTransform _dropdownRectTransform;

		[SerializeField]
		private MMButton _backgroundButton;

		[SerializeField]
		private MMScrollRect _scrollRect;

		[SerializeField]
		private RectTransform _contentContainer;

		[SerializeField]
		private DropdownItem _itemTemplate;

		public override void Awake()
		{
			base.Awake();
			_canvasGroup.alpha = 0f;
		}

		public void Show(MMDropdown dropdown, bool instant = false)
		{
			_dropdownRectTransform.position = dropdown.transform.position;
			_dropdownRectTransform.anchoredPosition -= new Vector2(0f, dropdown.GetComponent<RectTransform>().rect.height * 0.5f);
			_backgroundButton.onClick.AddListener(OnCancelButtonInput);
			for (int i = 0; i < dropdown.Content.Length; i++)
			{
				DropdownItem dropdownItem = _itemTemplate.Instantiate();
				dropdownItem.transform.SetParent(_contentContainer);
				dropdownItem.transform.localScale = Vector3.one;
				dropdownItem.OnItemSelected = (Action<DropdownItem>)Delegate.Combine(dropdownItem.OnItemSelected, new Action<DropdownItem>(OnItemClicked));
				dropdownItem.Configure(i, dropdown.Content[i], i == dropdown.ContentIndex);
				if (i == dropdown.ContentIndex)
				{
					OverrideDefault(dropdownItem.Button);
				}
			}
			Show(instant);
		}

		private void OnItemClicked(DropdownItem item)
		{
			Action<int> onItemChosen = OnItemChosen;
			if (onItemChosen != null)
			{
				onItemChosen(item.Index);
			}
			Hide();
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
